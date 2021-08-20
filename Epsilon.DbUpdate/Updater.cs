using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Epsilon.DbUpdate
{
    public class Updater
    {
        private const int TransactionNameMaxLength = 32;
        private const string AssemblyFilter = "*Epsilon*.dll";

        private readonly string _connectionString;
        private readonly string _assembliesPath;
        private readonly Action<string> _log;

        private readonly Dictionary<string, string> _versions = new Dictionary<string, string>();
        
        private readonly Dictionary<string, List<MessageStoredProcedure>> _spDependencies = new Dictionary<string, List<MessageStoredProcedure>>();
        private readonly HashSet<string> _changedProcedures = new HashSet<string>();
        private readonly Regex _findChangedSp = new Regex(@"(?:alter|create)\s*procedure\s*(?<spName>\w*\.\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _findSpCall = new Regex(@"exec\s+(?<spName>\w*\.\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        
        public Updater(string connectionString, string assembliesPath, Action<string> log)
        {
            _connectionString = connectionString;
            _assembliesPath = assembliesPath;
            _log = log;
        }

        public void Update()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                EnsureDbVersionTable(connection);
                FetchAllVersions(connection);
            }

            var version = long.Parse(_versions["Model"]);
            _log("Current Version: " + version);

            var sqlRes = FindSqlResources().ToArray();
            var migrations = FindNewerMigrationScripts(sqlRes, version);
            foreach (var migration in migrations.OrderBy(p => p.Time))
                ApplyMigration(migration);

            foreach (var storedProcedure in FindMessageStoredProcedure(sqlRes))
            {
                if (!_versions.TryGetValue(storedProcedure.Fullname, out var v) || v != storedProcedure.Sha1)
                    UpdateOrCreateSp(storedProcedure, v);
            }

            var changedSp = _changedProcedures.ToArray();
            while (changedSp.Length > 0)
            {
                _changedProcedures.Clear();

                foreach (var sp in changedSp)
                {
                    if (_spDependencies.ContainsKey(sp))
                    {
                        foreach (var spDependency in _spDependencies[sp])
                            UpdateOrCreateSp(spDependency, spDependency.Sha1);
                    }
                }

                changedSp = _changedProcedures.ToArray();
            }
        }

        private void ApplyMigration(Migration migration)
        {
            _log(migration.Time + "\t" + migration.Item + "\t" + migration.Name);
            PlayScript("Model", migration.Content, migration.Time.ToString(CultureInfo.InvariantCulture));
        }

        private void UpdateOrCreateSp(MessageStoredProcedure sp, string lastVersion)
        {
            _log(sp.Fullname);
            var content = string.IsNullOrEmpty(lastVersion)
                ? sp.Content.Replace("ALTER procedure", "CREATE procedure")
                : sp.Content.Replace("CREATE procedure", "ALTER procedure");

            PlayScript(sp.Fullname, content, sp.Sha1);
        }

        private void PlayScript(string name, string content, string version)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction(name.Length > TransactionNameMaxLength ? name.Substring(0, TransactionNameMaxLength) : name);

            try
            {
                foreach (var subContent in content.Split("go"))
                {
                    var cmd = new SqlCommand(subContent, connection, transaction);
                    cmd.ExecuteNonQuery();
                }

                var updateVersionQuery = @"
UPDATE Ep.tDbVersion set Ver = @Version WHERE Name = @Name
IF @@ROWCOUNT = 0  INSERT INTO Ep.tDbVersion (Name, Ver) VALUES(@Name, @Version)
";
                var cmd2 = new SqlCommand(updateVersionQuery, connection, transaction);
                cmd2.Parameters.AddWithValue("@Name", name);
                cmd2.Parameters.AddWithValue("@Version", version);
                cmd2.ExecuteNonQuery();

                transaction.Commit();

                foreach (Match match in _findChangedSp.Matches(content).Where(m=> m.Success))
                    _changedProcedures.Add(match.Groups["spName"].Value);
                
            }
            catch (Exception ex)
            {
                _log($"\tMessage: {ex.Message}");

                // Attempt to roll back the transaction.
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    _log($"\tMessage: {ex2.Message}");
                }
                throw;
            }
            
        }
        
        private void FetchAllVersions(SqlConnection connection)
        {
            

            var cmd = new SqlCommand($"select Name, Ver from Ep.tDbVersion", connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
                _versions.Add(reader.GetString(0), reader.GetString(1));
        }

        void EnsureDbVersionTable(SqlConnection connection)
        {
            var query = @"
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Ep].[tDbVersion]') AND type in (N'U'))
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Ep')
	  EXEC('CREATE SCHEMA [Ep] AUTHORIZATION [db_owner]')

	CREATE TABLE [Ep].[tDbVersion](Name varchar(256) not null, Ver varchar(256) not null, constraint PK_tDbVersion primary key clustered ( Name ) )
	insert into Ep.tDbVersion(Name, Ver)Values('Model', 0);
	
END
	";

            var cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        private IEnumerable<Migration> FindNewerMigrationScripts(SqlResource[] resources, long version)
        {
            var regex = new Regex(@"(?<Item>(?:\w|\.)*)\.Migrations\.(?<Time>\d*)_(?<Name>\w*)\.sql", RegexOptions.Compiled);
            foreach (var resource in resources)
            {
                var r = regex.Match(resource.ResName);
                if (r.Success && long.TryParse(r.Groups["Time"].Value, out var time) && time > version)
                {
                    yield return new Migration
                    {
                        Item = r.Groups["Item"].Value,
                        Name = r.Groups["Name"].Value,
                        Time = time,
                        Content = resource.GetContent()
                    };
                }
            }
        }

        private IEnumerable<MessageStoredProcedure> FindMessageStoredProcedure(SqlResource[] resources)
        {
            var regex = new Regex(@"(?<Item>(?:\w|\.)*)\.(?:Commands|Event|Queries)\.(?<Name>\w*)\.sql", RegexOptions.Compiled);
            foreach (var resource in resources)
            {
                var r = regex.Match(resource.ResName);
                if (r.Success)
                {
                    var content = resource.GetContent();

                    var sp = new MessageStoredProcedure
                    {
                        Item = r.Groups["Item"].Value,
                        Name = r.Groups["Name"].Value,
                        Content = content,
                        Sha1 = Hash(content)
                    };


                    foreach (Match match in _findSpCall.Matches(content).Where(m => m.Success))
                    {
                        var spName = match.Groups["spName"].Value;
                        if (_spDependencies.ContainsKey(spName))
                            _spDependencies[spName].Add(sp);
                        else _spDependencies.Add(spName, new List<MessageStoredProcedure>{ sp });
                    }

                    yield return sp;
                }
            }
        }

        private IEnumerable<SqlResource> FindSqlResources()
        {
            foreach (var file in Directory.EnumerateFiles(_assembliesPath, AssemblyFilter, SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFile(file);
                foreach (var res in assembly.GetManifestResourceNames()
                    .Where(p => p.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return new SqlResource { Assembly = assembly, ResName = res };
                }
            }
        }

        static string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }

    class MessageStoredProcedure
    {
        public string Name { get; set; }
        public string Item { get; set; }
        public string Content { get; set; }
        public string Sha1 { get; set; }

        public string Fullname => Item + "." + Name;
    }

    class SqlResource
    {
        public Assembly Assembly { get; set; }
        public string ResName { get; set; }

        public string GetContent()
        {
            using Stream stream = Assembly.GetManifestResourceStream(ResName);
            Debug.Assert(stream != null, nameof(stream) + " != null");

            using StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
