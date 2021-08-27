using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Epsilon.Utils;

namespace Epsilon.Messaging.Sql
{
 
    public class SqlCommandHandlerBase
    {
        private static ConcurrentDictionary<string, SqlTransaction> _transactions = new ConcurrentDictionary<string, SqlTransaction>();

        private readonly IBus _bus;
        private readonly string _connectionString;

        public SqlCommandHandlerBase(IBus bus, string connectionString)
        {
            _bus = bus;
            _connectionString = connectionString;
        }

        
        protected void Handle(IEventDispatcher d, IMessageContext c, string commandId, ICommand command, string spName)
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();
                var trx = _transactions.GetOrAdd(commandId, id => cnx.BeginTransaction(Guid.NewGuid().ToString("N")));
                try
                {
                    var sqlCmd = cnx.CreateCommand();
                    sqlCmd.Transaction = trx;
                    sqlCmd.CommandTimeout = 120;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandText = spName;
                    SetCommandParams(c, commandId, command, sqlCmd);
                    sqlCmd.ExecuteNonQuery();
                    var eventJson = (string)sqlCmd.Parameters["@_Events"].Value;
                    using (JsonDocument document = JsonDocument.Parse(eventJson))
                    {
                        foreach (JsonElement eventElement in document.RootElement.EnumerateArray())
                        {
                            string eventName = eventElement.GetProperty("EventName").GetString();

                            var type = _bus.ResolveMessage(eventName);
                            if (type == null) throw new NotSupportedException("Event not found " + eventName);

                            var instance = Activator.CreateInstance(type) as IEvent;
                            if (instance == null) throw new InvalidOperationException("Event must be a  IEvent" + eventName);

                            ObjectHelper.FillFromString(instance, str =>
                            {
                                if (str == "CommandId") return commandId;
                                if (eventElement.TryGetProperty(str, out var p))
                                    return p.ValueKind == JsonValueKind.String ? p.GetString() : p.ToString();
                                return null;
                            });

                            MethodInfo method = typeof(IEventDispatcher).GetMethod("Fire");
                            MethodInfo generic = method.MakeGenericMethod(type);
                            generic.Invoke(d, new object[] { instance });
                        }
                    }

                    trx.Commit();
                }
                catch (Exception ex)
                {
                    trx.Rollback();
                    throw ex;
                }
                finally
                {
                    if(_transactions.TryRemove(commandId, out var t))
                        t.Dispose();

                }
            }
        }


        private void SetCommandParams(IMessageContext c, string commandId, ICommand cmd, SqlCommand sqlCmd)
        {
            sqlCmd.Parameters.AddWithValue("@_ActorId", c.ActorId);
            sqlCmd.Parameters.AddWithValue("@_CultureId", c.CultureId);
            sqlCmd.Parameters.AddWithValue("@_CommandId", commandId);

            sqlCmd.Parameters.Add("@_Events", SqlDbType.NVarChar, -1).Value = "[]";
            sqlCmd.Parameters["@_Events"].Direction = ParameterDirection.InputOutput;

            foreach (var p in cmd.GetType().GetProperties())
            {
                SqlParameter t = null;
                object v;

                if (p.PropertyType.IsArray)
                {
                    var a = (string[])p.GetValue(cmd, null);
                    v = a != null ? string.Join(",", a) : string.Empty;

                    t = TypeConvertor.GetSqlParam("@" + p.Name, p.PropertyType);
                }
                else
                {
                    v = p.GetValue(cmd, null);
                    t = TypeConvertor.GetSqlParam("@" + p.Name, p.PropertyType);
                }

                sqlCmd.Parameters.Add(t).Value = v;
            }


        }
    }
}
