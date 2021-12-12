using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Epsilon.Model.Repository
{
    public class RepositoryModel : IModel
    {
        private readonly IConfig _config;

        public RepositoryModel(IConfig config)
        {
            _config = config;
        }

        public IEnumerable<RepoValue> GetRepoValues(int repoId, int resCultureId)
        {
            using var connection = new SqlConnection(_config.ConnectionString);
            return connection.Query<RepoValue>("select * from Ep.vRepoValue where RepoId = @repoId AND ResCultureId = @resCultureId", new { repoId, resCultureId });
        }


        public IEnumerable<string> GetDbObjRepoValueNames(int dbObjId, string repoSystemKey)
        {
            using (var cnx = new SqlConnection(_config.ConnectionString))
            {
                cnx.Open();
                SqlCommand sqlCmd = cnx.CreateCommand();
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = "select ValueName from Ep.vDbObjRepoValue where DbObjId = @DbObjId AND RepoSystemKey = @RepoSystemKey";
                sqlCmd.Parameters.AddWithValue("@DbObjId", dbObjId);
                sqlCmd.Parameters.AddWithValue("@RepoSystemKey", repoSystemKey);
                using (var r = sqlCmd.ExecuteReader())
                {
                    while (r.Read()) yield return r.GetString(0);
                }
            }

        }

        public int? AlphaValueBinding(int alphaId)
        {
            var db = new SqlConnection(_config.ConnectionString);
            return db.ExecuteScalar<int?>("select RepoValueId from Ep.tRepoValue where AlphaId = @alphaId", new { alphaId });
        }
        public int? AlphaRepoBinding(int alphaId)
        {
            var db = new SqlConnection(_config.ConnectionString);
            return db.ExecuteScalar<int?>("select RepoId from Ep.tRepo where AlphaId = @alphaId", new { alphaId });
        }

    }

    public class RepoValue
    {
        public int RepoValueId { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
    }
}
