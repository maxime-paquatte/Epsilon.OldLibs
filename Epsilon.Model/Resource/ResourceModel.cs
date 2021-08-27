using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Dapper;

namespace Epsilon.Model.Resource
{
    public class ResourceModel : IModel
    {
        private readonly IConfig _config;

        public ResourceModel(IConfig config)
        {
            _config = config;
        }

        public IEnumerable<Value> ForPrefixes(int cultureId, string prefixes)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.Query<Value>(@"
                SELECT r.*, rv.ResValue
                from EpRes.tRes r
                inner join STRING_SPLIT(@prefixes, '|')  pref
	                on r.ResName like pref.value + '%'
                left outer join EpRes.tResValue rv 
	                on rv.ResId = r.ResId AND rv.CultureId = @cultureId
                ", new { cultureId, prefixes });
        }

    }

    public class Value
    {
        public string ResName { get; set; }

        public string ResValue { get; set; }
    }
}
