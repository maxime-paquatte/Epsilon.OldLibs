
using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class Values : IQuery
    {
        public int RepositoryId { get; set; }

        public int ResCultureId { get; set; }
    }
}
