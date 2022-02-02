using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace Epsilon.Messaging.Sql
{
    public class SqlEventHandlerBase
    {
        private readonly IBus _bus;
        private readonly string _connectionString;

        public SqlEventHandlerBase(IBus bus, string connectionString)
        {
            _bus = bus;
            _connectionString = connectionString;
        }

        protected void Handle<T>(IMessageContext c, T ev, string spName)
            where T : IEvent
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();
                {
                    var sqlCmd = cnx.CreateCommand();
                    sqlCmd.CommandTimeout = 120;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandText = spName;
                    SetCommandParams(c, ev, sqlCmd);
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }


        private void SetCommandParams<TEvent>(IMessageContext c, TEvent ev, SqlCommand sqlCmd)
            where TEvent : IEvent
        {
            sqlCmd.Parameters.AddWithValue("@_ActorId", c.ActorId);
            sqlCmd.Parameters.AddWithValue("@_CultureId", c.CultureId);
            sqlCmd.Parameters.AddWithValue("@_CommandId", ev.CommandId);

            foreach (var p in typeof(TEvent).GetProperties().Where(p => p.Name != "CommandId"))
            {
                SqlParameter t = null;
                object v;

                if (p.PropertyType.IsArray)
                {
                    var a = (string[])p.GetValue(ev, null);
                    v = a != null ? string.Join(",", a) : string.Empty;

                    t = TypeConvertor.GetSqlParam("@" + p.Name, p.PropertyType);
                }
                else
                {
                    v = p.GetValue(ev, null);
                    t = TypeConvertor.GetSqlParam("@" + p.Name, p.PropertyType);
                }

                sqlCmd.Parameters.Add(t).Value = v;
            }
        }
    }
}