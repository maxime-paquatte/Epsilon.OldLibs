using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Epsilon.DbUpdate
{
    public class Updater
    {
        private readonly string _connectionString;
        private readonly string _assembliesPath;
        private readonly Action<string> _log;

        public Updater(string connectionString, string assembliesPath, Action<string> log)
        {
            _connectionString = connectionString;
            _assembliesPath = assembliesPath;
            _log = log;
        }

        public void Update()
        {
            var version = GetDbVersion();
            _log("Current Version: " + version);
            var migrations = FindNewerMigrationScripts(version);
            foreach (var migration in migrations.OrderBy(p => p.Time))
            {
                _log(migration.Time + "\t" + migration.Item + "\t" + migration.Name);
                var r = ApplyMigration(migration);
                if (!r) break;
            }
        }

        private bool ApplyMigration(Migration migration)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction("migration-" + migration.Time);

                try
                {
                    var cmd = new SqlCommand(migration.Content, connection, transaction);
                    cmd.ExecuteNonQuery();

                    var cmd2 = new SqlCommand("UPDATE Ep.DbVersion set Ver = " + migration.Time, connection, transaction);
                    cmd2.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
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
                }
            }
            return false;
        }

        private long GetDbVersion()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                EnsureDbVersionTable(connection);

                var cmd = new SqlCommand("select Ver from Ep.DbVersion", connection);
                return (long)cmd.ExecuteScalar();
            }
        }

        void EnsureDbVersionTable(SqlConnection connection)
        {
            var query = @"
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Ep].[DbVersion]') AND type in (N'U'))
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Ep')
	  EXEC('CREATE SCHEMA [Ep] AUTHORIZATION [db_owner]')

	CREATE TABLE [Ep].[DbVersion]( Ver bigint not null )
	insert into Ep.DbVersion(Ver)Values(0);
	
END
	";

            var cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        private IEnumerable<Migration> FindNewerMigrationScripts(long version)
        {
            foreach (var file in Directory.EnumerateFiles(_assembliesPath, "*Epsilon*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFile(file);
                foreach (var res in assembly.GetManifestResourceNames()
                    .Where(p => p.Contains("Migrations", StringComparison.InvariantCultureIgnoreCase)
                               && p.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase)))
                {
                    _log(res);

                    var migrationIdx = res.IndexOf("Migrations", StringComparison.InvariantCultureIgnoreCase);
                    var resName = res.Substring(migrationIdx + "Migrations".Length + 1).Replace(".sql", "");
                    var underscoreIdx = resName.IndexOf('_');
                    var time = long.Parse(resName.Substring(0, underscoreIdx));
                    if (time > version)
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(res))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = reader.ReadToEnd();
                            yield return new Migration
                            {
                                Item = res.Substring(0, migrationIdx),
                                Name = resName.Substring(underscoreIdx + 1),
                                Time = time,
                                Content = content
                            };
                        }
                    }
                }
            }
        }
    }
}
