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

        public Value GetResValue(string resName, int lcid)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.QuerySingleOrDefault<Value>(@"
                select r.*, rv.CultureId, rv.ResValue
                from EpRes.tRes r
                left outer join EpRes.tResValue rv on rv.ResId = r.ResId
                where r.ResName = @resName AND rv.CultureId = @cultureId
                ", new { resName, lcid });
        }

        private void Create(string resName, string templateKeys, string comment)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"insert into EpRes.tRes(ResName, Args, Comment) VALUES(resName, templateKeys, comment)
                ", new { resName, templateKeys, comment });
        }

        public void SetResTemplateKeys(string resName, string args)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            connection.Execute(@"Update [EpRes].[tRes] set [Args] = @args where [ResName] = @resName", new { resName, args });
        }

        public string GetResTemplateKeys(string resName)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.ExecuteScalar<string>(@"select [Args] from [EpRes].[tRes] where [ResName] = @resName", new { resName });
        }

        string GetResTemplate(string resName, int lcid, object templateValues = null)
        {
            var res = GetResValue(resName, lcid);

            IDictionary<string, object> values = templateValues as Dictionary<string, object>
                         ?? ObjectHelper.AnonymousObjectToDictionary(templateValues)
                         ?? new Dictionary<string, object>();

            string[] keys = values.Select(p => p.Key).ToArray();

            //Resource doesn't exists
            if (res == null) Create(resName, string.Join(",", keys), string.Empty);

            //Resource exists have no values
            if (string.IsNullOrEmpty(res.ResValue))
            {
                var shortName = resName.Substring(resName.LastIndexOf('.') + 1);
                if (templateValues != null)
                {
                    var templatesKeys = GetResTemplateKeys(resName);
                    if (string.IsNullOrEmpty(templatesKeys)) SetResTemplateKeys(resName, string.Join(",", keys));
                    return "?" + shortName + " " + string.Join(" ", keys.Select(p => "{{ " + p + " }}"));
                }
                return "?" + shortName;
            }

            //var cValues = GetContextTemplateValues() as ICollection<KeyValuePair<string, object>>
            //              ?? ObjectHelper.AnonymousObjectToDictionary(GetContextTemplateValues())
            //              ?? new Dictionary<string, object>();

            //var allVals = values.Union(cValues).ToArray();
            return values.Count == 0 ? res.ResValue : StringHelper.ApplyTemplate(res.ResValue, values);
        }

        public string GetRes(string resName, int lcid, object obj = null)
        {
            return GetResTemplate(resName, lcid, obj);
        }


        public string GetRes<T>(T enumVal, int cultureId) where T : struct
        {
            var t = typeof(T);
            if (!t.IsEnum) throw new ArgumentException("T must be an enumerated type");
            var a = t.GetCustomAttribute<ResPrefixAttribute>();
            if (a == null) throw new ArgumentException("T must be decorated by ResPrefixAttribute");

            return GetRes(a.Prefix + Enum.GetName(t, enumVal), cultureId);
        }

    }

    public class Value
    {
        public string ResName { get; set; }

        public string ResValue { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class ResPrefixAttribute : Attribute
    {
        public string Prefix { get; set; }
    }
}
