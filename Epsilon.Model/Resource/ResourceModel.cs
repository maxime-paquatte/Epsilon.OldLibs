using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Epsilon.Utils;

namespace Epsilon.Model.Resource
{
    public class ResourceModel : IModel
    {
        private readonly IConfig _config;

        public ResourceModel(IConfig config)
        {
            _config = config;
        }

        internal IEnumerable<Value> ForPrefixes(int cultureId, string prefixes)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.Query<Value>(@"
                SELECT r.*, rv.ResValue
                from EpRes.tRes r
                inner join STRING_SPLIT(@prefixes, '|')  pref
	                on r.ResName like pref.value + '%' COLLATE Latin1_General_CI_AS 
                left outer join EpRes.tResValue rv 
	                on rv.ResId = r.ResId AND rv.CultureId = @cultureId
                ", new { cultureId, prefixes });
        }

        internal Value GetResValue(string resName, int cultureId)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.QuerySingleOrDefault<Value>(@"
                select r.ResId, rv.ResValue
                from EpRes.tRes r
                left outer join EpRes.tResValue rv on rv.ResId = r.ResId AND rv.CultureId = @cultureId
                where r.ResName = @resName  COLLATE Latin1_General_CI_AS
                ", new { resName, cultureId });
        }

        internal void Create(string resName, string templateKeys, string comment)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"insert into EpRes.tRes(ResName, Args, Comment) VALUES(@resName, @templateKeys, @comment)
                ", new { resName, templateKeys, comment });
        }

        internal void SetResTemplateKeys(string resName, string args)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"Update [EpRes].[tRes] set [Args] = @args where [ResName] = @resName  COLLATE Latin1_General_CI_AS", new { resName, args });
        }

        internal string GetResTemplateKeys(string resName)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.ExecuteScalar<string>(@"select [Args] from [EpRes].[tRes] where [ResName] = @resName  COLLATE Latin1_General_CI_AS", new { resName });
        }


    }

    public class Value
    {
        public string ResName { get; set; }

        public string ResValue { get; set; }
    }

}
