using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class Children : IQuery
    {
        public int RepoId { get; set; }

        public string SystemKey { get; set; }

        public string Path { get; set; }
        
        public bool AllDepth { get; set; }
    }
}
