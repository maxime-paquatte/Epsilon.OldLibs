using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class ValueCreate : ICommand
    {
        public int RepoId { get; set; }

        public int ResCultureId { get; set; }

        public string RepositoryValueName { get; set; }

        public int AlphaId { get; set; }
    }
}
