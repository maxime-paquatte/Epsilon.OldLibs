using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Events
{
    public class Deleted : IEvent
    {
        public int ResId { get; set; }

        public string CommandId { get; set; }
    }
}
