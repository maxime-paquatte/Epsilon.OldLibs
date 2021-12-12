using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class Created : IEvent
    {
        public string CommandId { get; set; }

        public int RepositoryId { get; set; }
    }
}
