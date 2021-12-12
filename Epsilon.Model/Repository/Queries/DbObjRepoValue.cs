using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class DbObjRepoValue : IQuery
    {
        public int DbObjId { get; set; }

        public int RepoId { get; set; }
    }
}
