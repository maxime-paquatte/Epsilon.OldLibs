using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class Changed : IEvent
    {
        public string CommandId { get; set; }

        public int RepositoryId { get; set; }
    }
}
