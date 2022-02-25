using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

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
        
        public static dynamic ToExpando(dynamic obj)
        {
            IDictionary<string, object> anonymousDictionary = AnonymousObjectToDictionary(obj);
            var expando = new Flexpando();
            foreach (var kvp in anonymousDictionary)
                expando.Dictionary.Add(kvp.Key, kvp.Value);
            return expando;
        }

        public static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && type.Attributes.HasFlag(TypeAttributes.NotPublic);
        }


    }

    public class Flexpando : DynamicObject
    {
        public Dictionary<string, object> Dictionary
            = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Dictionary.TryGetValue(binder.Name, out result);
        }
    }
    
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random _local;
        public static Random ThisThreadsRandom => _local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
    }
}
