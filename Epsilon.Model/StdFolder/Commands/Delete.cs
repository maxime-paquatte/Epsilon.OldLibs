using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Commands
{
    public class Delete : ICommand
    {
        public int StdFolderId { get; set; }
    }
}
