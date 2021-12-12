using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Commands
{
    public class Move : ICommand
    {
        public int StdFolderId { get; set; }

        public int ParentId { get; set; }
    }
}
