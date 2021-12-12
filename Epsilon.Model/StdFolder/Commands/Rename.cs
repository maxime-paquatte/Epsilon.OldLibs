using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Commands
{
    public class Rename : ICommand
    {
        public int StdFolderId { get; set; }
        
        public string StdFolderName { get; set; }
    }
}
