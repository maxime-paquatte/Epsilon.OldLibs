using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class BindingDeleted : IEvent
    {
        public string CommandId { get; set; }

        public int RepoValueSourceId { get; set; }
        public int RepoValueTargetId { get; set; }
    }
}
