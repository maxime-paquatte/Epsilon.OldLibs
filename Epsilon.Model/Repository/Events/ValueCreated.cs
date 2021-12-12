using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class ValueCreated : IEvent
    {
        public string CommandId { get; set; }

        public int RepoValueId { get; set; }
    }
}
