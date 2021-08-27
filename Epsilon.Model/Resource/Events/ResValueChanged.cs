using Epsilon.Messaging;

namespace Epsilon.Model.Resource.Events
{
    public class ResValueChanged : IEvent
    {
        public int ResId { get; set; }
        public int CultureId { get; set; }
        public string ResValue { get; set; }

        public string CommandId { get; set; }
    }
}
