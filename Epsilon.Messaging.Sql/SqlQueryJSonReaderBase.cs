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
    public class SqlQueryJSonReaderBase
    {
        private readonly string _connectionString;

        public SqlQueryJSonReaderBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected string Read(IMessageContext c, IQuery query, string spName, int timeout = 120)
        {
            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();
                var sqlCmd = cnx.CreateCommand();
                sqlCmd.CommandTimeout = timeout;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = spName;
                SetCommandParams(c, query, sqlCmd);

                var sb = new StringBuilder();
                using var reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                    sb.Append(reader[0]);
                return sb.ToString();
            }
        }

        private void SetCommandParams(IMessageContext c, IQuery q, SqlCommand sqlCmd)
        {
            sqlCmd.Parameters.AddWithValue("@_ActorId", c.ActorId);
            sqlCmd.Parameters.AddWithValue("@_CultureId", c.CultureId);

            foreach (var p in q.GetType().GetProperties())
            {
                object v = null;
                if (p.PropertyType.IsArray)
                {
                    string[] a = (string[])p.GetValue(q, null);
                    if (a != null) v = string.Join(",", a);
                    else v = string.Empty;
                }
                else v = p.GetValue(q, null);

                sqlCmd.Parameters.Add(TypeConvertor.GetSqlParam("@" + p.Name, p.PropertyType)).Value = v;
            }
        }

    }
}
