using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class Typeahead : IQuery
    {
        public int RepoId { get; set; }

        public string SystemKey { get; set; }

        public string Pattern { get; set; }
    }
}
