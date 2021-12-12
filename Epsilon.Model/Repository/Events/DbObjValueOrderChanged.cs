using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class DbObjValueOrderChanged : IEvent
    {
        public string CommandId { get; set; }

        public int DbObjId { get; set; }
    }
}
