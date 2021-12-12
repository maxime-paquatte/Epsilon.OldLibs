using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class BindingChanged : IEvent
    {
        public string CommandId { get; set; }

        public int RepoValueSourceId { get; set; }
        public int RepoValueTargetId { get; set; }
    }
}
