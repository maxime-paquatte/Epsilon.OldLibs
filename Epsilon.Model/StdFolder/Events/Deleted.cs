using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Events
{
    public class Deleted : IEvent
    {
        public string CommandId { get; set; }

        public int StdFolderId { get; set; }
    }
}
