using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;

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

    }

    public class RepoValue
    {
        public int RepoValueId { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
    }
}
