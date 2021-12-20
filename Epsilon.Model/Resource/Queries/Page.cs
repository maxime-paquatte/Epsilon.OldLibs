using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Queries
{
    public class Page : IQuery
    {
        public string Prefixes { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}