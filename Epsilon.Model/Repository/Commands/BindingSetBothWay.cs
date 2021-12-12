using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class BindingSetBothWay : ICommand
    {
        public int RepoValueSourceId { get; set; }

        public int RepoValueTargetId { get; set; }

        public bool BothWay { get; set; }
    }
}
