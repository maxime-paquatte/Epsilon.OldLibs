using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Epsilon.Utils;

namespace Epsilon.Model.Resource
{
    public class ResourceService : IService
    {
        private readonly ResourceModel _model;

        public ResourceService(ResourceModel model)
        {
            _model = model;
        }

        public IEnumerable<Value> ForPrefixes(int cultureId, string prefixes)
        {
            return _model.ForPrefixes(cultureId, prefixes);
        }

        string GetResTemplate(string resName, int lcid, object templateValues = null)
        {
            resName = resName.Trim();
            var res = _model.GetResValue(resName, lcid);

            IDictionary<string, object> values = templateValues as Dictionary<string, object>
                         ?? ObjectHelper.AnonymousObjectToDictionary(templateValues)
                         ?? new Dictionary<string, object>();

            string[] keys = values.Select(p => p.Key).ToArray();

            //Resource doesn't exists
            if (res == null) _model.Create(resName, string.Join(",", keys), string.Empty);

            //Resource exists have no values
            if (res==null || string.IsNullOrEmpty(res.ResValue))
            {
                if (templateValues != null)
                {
                    var templatesKeys = _model.GetResTemplateKeys(resName);
                    if (string.IsNullOrEmpty(templatesKeys)) _model.SetResTemplateKeys(resName, string.Join(",", keys));
                    return "?" + resName + " " + string.Join(" ", keys.Select(p => "{{ " + p + " }}"));
                }
                return "?" + resName;
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

        public static int GetBestLCID(IDictionary<string, double> languages)
        {
            var l = languages.OrderByDescending(p => p.Value)
                .Select(p =>
                {
                    var c = CultureInfo.GetCultureInfo(p.Key.ToString());
                    if (!c.IsNeutralCulture) c = c.Parent;
                    return c.LCID;
                })
                .FirstOrDefault(p => p == 9 || p == 12);
            return l == 0 ? 9 : l;
        }

    }


    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class ResPrefixAttribute : Attribute
    {
        public string Prefix { get; set; }
    }
}
