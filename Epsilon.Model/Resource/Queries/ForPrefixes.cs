using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Queries
{
    public class All : IQuery
    {
        public int CultureId { get; set; }
        public string Prefixes { get; set; }
    }
}