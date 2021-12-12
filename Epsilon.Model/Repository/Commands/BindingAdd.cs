using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class BindingAdd : ICommand
    {
        public int RepoValueSourceId { get; set; }

        public int RepoValueTargetId { get; set; }
    }
}
