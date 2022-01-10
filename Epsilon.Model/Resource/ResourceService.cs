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
            return _model.ForPrefixes(cultureId, prefixes)
                .Select(p => new Value{ ResName = p.ResName, ResValue = p.ResValue ?? p.DefaultValue});
        }

        string GetResTemplate(string resName, int lcid, object templateValues = null)
        {
            resName = resName.Trim();
            var res = _model.GetResValue(resName, lcid);

            IDictionary<string, object> values = templateValues as Dictionary<string, object>
                         ?? ObjectHelper.AnonymousObjectToDictionary(templateValues)
                         ?? new Dictionary<string, object>();

            string[] keys = values.Select(p => p.Key).ToArray();
            
            //Resource exists and has value
            if (res != null && !string.IsNullOrEmpty(res.ResValue))
                return values.Count == 0 ? res.ResValue : StringHelper.ApplyTemplate(res.ResValue, values);
            
            //Resource exists and has default value
            if (res != null && !string.IsNullOrEmpty(res?.DefaultValue))
                return values.Count == 0 ? res.DefaultValue : StringHelper.ApplyTemplate(res.DefaultValue, values);

            //Resource doesn't exists, create it
            if (res == null)
                _model.Create(resName, string.Join(",", keys), string.Empty);

            if (keys.Length > 0)
            {
                var templatesKeys = _model.GetResTemplateKeys(resName);
                if (string.IsNullOrEmpty(templatesKeys)) _model.SetResTemplateKeys(resName, string.Join(",", keys));
                return "?" + resName + " " + string.Join(" ", keys.Select(p => "{{ " + p + " }}"));
            }
            return "?" + resName;
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
