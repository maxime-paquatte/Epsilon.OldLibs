using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Events
{
    public class Changed : IEvent
    {
        public string CommandId { get; set; }

        public int StdFolderId { get; set; }
    }
}
