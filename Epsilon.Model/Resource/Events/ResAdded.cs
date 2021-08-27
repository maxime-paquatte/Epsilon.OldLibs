using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Events
{
    public class ResAdded : IEvent
    {
        public int ResId { get; set; }
        public string CommandId { get; set; }
    }
}
