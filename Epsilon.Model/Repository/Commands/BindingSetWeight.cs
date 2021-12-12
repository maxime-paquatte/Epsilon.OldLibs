using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class BindingSetWeight : ICommand
    {
        public int RepoValueSourceId { get; set; }

        public int RepoValueTargetId { get; set; }

        public int Weight { get; set; }
    }
}
