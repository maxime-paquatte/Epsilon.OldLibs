
using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class ById : IQuery
    {
        public int RepositoryId { get; set; }
    }
}
