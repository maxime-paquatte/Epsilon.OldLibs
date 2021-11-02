using System;
using System.Data;
using System.Data.SqlClient;

namespace Epsilon.Model.SharedDic
{
    public class SharedDicModel : IModel
    {
        readonly IConfig _config;

        public SharedDicModel(IConfig config)
        {
            _config = config;
        }

        #region ISharedDicService Members

        public void Create(string key, string value, ExpirationDateRef dateRef, int expirationSecond)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                try
                {
                    var cmd = cnx.CreateCommand();

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "Ep.sSharedDicCreate";
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.Parameters.AddWithValue("@DateRef", dateRef);
                    cmd.Parameters.AddWithValue("@RelativeExpirationSecond", expirationSecond);
                    cmd.ExecuteNonQuery();
                }
                finally { cnx.Close(); }
            }
        }

        public void CreateOrUpdate(string key, string value, ExpirationDateRef dateRef, int expirationSecond)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                try
                {
                    var cmd = cnx.CreateCommand();

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "Ep.sSharedDicCreateOrUpdate";
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.Parameters.AddWithValue("@DateRef", dateRef);
                    cmd.Parameters.AddWithValue("@RelativeExpirationSecond", expirationSecond);
                    cmd.ExecuteNonQuery();
                }
                finally { cnx.Close(); }
            }
        }

        public void Update(string key, string value)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                try
                {
                    var cmd = cnx.CreateCommand();

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "Ep.sSharedDicUpdate";
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.ExecuteNonQuery();
                }
                finally { cnx.Close(); }
            }
        }



        public string Get(string key)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                try
                {
                    var cmd = cnx.CreateCommand();

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Ep.sSharedDicGet";
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, -1)).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    return cmd.Parameters["@Value"].Value as string;
                }
                finally { cnx.Close(); }
            }
        }

        public void Delete(string key)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                try
                {
                    var cmd = cnx.CreateCommand();

                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "delete from Ep.tSharedDic where [Key] = @Key";
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.ExecuteNonQuery();
                }
                finally { cnx.Close(); }
            }
        }

        #endregion

    }

    public enum ExpirationDateRef : short
    {
        Create = 0,
        Write = 1,
        Read = 2,
        Infinite = 99
    }
}
