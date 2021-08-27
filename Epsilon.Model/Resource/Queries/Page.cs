using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Queries
{
    public class Page : IQuery
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}