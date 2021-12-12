using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class ValueDelete : ICommand
    {
        public int RepoValueId { get; set; }
    }
}
