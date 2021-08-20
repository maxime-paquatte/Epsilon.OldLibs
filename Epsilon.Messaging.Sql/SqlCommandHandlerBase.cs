using System;
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
        private readonly IBus _bus;
        private readonly string _connectionString;

        public SqlCommandHandlerBase(IBus bus, string connectionString)
        {
            _bus = bus;
            _connectionString = connectionString;
        }

        protected void Handle<T>(IEventDispatcher d, IMessageContext c, string commandId, T command, string spName)
            where T : ICommand
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();
                var trx = cnx.BeginTransaction(commandId.Substring(0, 32));
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
                                    return p.GetRawText();
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
                
            }
        }


        private void SetCommandParams<TCommand>(IMessageContext c, string commandId, TCommand cmd, SqlCommand sqlCmd)
            where TCommand : ICommand
        {
            sqlCmd.Parameters.AddWithValue("@_ActorId", c.ActorId);
            sqlCmd.Parameters.AddWithValue("@_CultureId", c.CultureId);
            sqlCmd.Parameters.AddWithValue("@_CommandId", commandId);

            sqlCmd.Parameters.Add("@_Events", SqlDbType.NVarChar, -1).Value = "[]";
            sqlCmd.Parameters["@_Events"].Direction = ParameterDirection.InputOutput;

            foreach (var p in typeof(TCommand).GetProperties())
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
