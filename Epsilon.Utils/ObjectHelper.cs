using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Epsilon.Utils
{
    public static class ObjectHelper
    {
        public static void FillFromString(object o, Func<string, string> str)
        {
            FillFromString(o, str, CultureInfo.InvariantCulture);
        }

        public static void FillFromString(object o, Func<string, string> str, CultureInfo ci)
        {
            var t = o.GetType();

            foreach (var p in t.GetProperties())
            {
                try
                {
                    if (p.PropertyType.IsArray)
                    {
                        var pVal = str(p.Name + "[]");
                        var v = !string.IsNullOrEmpty(pVal) ? pVal.Split(',') : new string[0];
                        p.SetValue(o, v, null);
                    }
                    else
                    {
                        var pVal = str(p.Name);
                        string val = string.IsNullOrEmpty(pVal) || pVal == "null" ? null : pVal;

                        var type = p.PropertyType;
                        var ult = Nullable.GetUnderlyingType(p.PropertyType);
                        if (ult != null) type = ult;


                        if (val == null)
                        {
                            if (p.PropertyType.IsValueType) p.SetValue(o, Activator.CreateInstance(p.PropertyType), null);
                            else p.SetValue(o, null, null);
                        }
                        else
                        {
                            object v;
                            if (typeof(DateTime).IsAssignableFrom(type))
                                v = DateTime.Parse(val, ci, DateTimeStyles.AdjustToUniversal);
                            else if (typeof(Guid).IsAssignableFrom(type))
                                v = Guid.Parse(val);
                            else v = Convert.ChangeType(val, type, ci);

                            p.SetValue(o, v, null);
                        }
                    }
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Format Exception " + p.Name + " (" + str(p.Name) + ")", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception while convert field : " + p.Name + " (" + str(p.Name) + ")", ex);
                }
            }
        }

        public static Dictionary<string, object> AnonymousObjectToDictionary(object values)
        {
            var dictionary = new Dictionary<string, object>();
            if (values != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    object value = propertyDescriptor.GetValue(values);
                    dictionary.Add(propertyDescriptor.Name, value);
                }
            }
            return dictionary;
        }

        public static string SerializeObject<T>(T o)
        {
            if (!typeof(T).IsSerializable) return null;

            using (var stream = new MemoryStream())
            {
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, o);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static T DeserializeObject<T>(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (var stream = new MemoryStream(bytes))
            {
                return (T)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
            }
        }
    }
}
