using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class BindingDelete : ICommand
    {
        public int RepoValueSourceId { get; set; }

        public int RepoValueTargetId { get; set; }
    }
}
