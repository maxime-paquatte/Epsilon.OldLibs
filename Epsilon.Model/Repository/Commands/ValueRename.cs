using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class ValueRename : ICommand
    {
        public int RepoValueId { get; set; }

        public string RepositoryValueName { get; set; }
    }
}
