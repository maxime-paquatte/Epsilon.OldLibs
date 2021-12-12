using Epsilon.Messaging;

namespace Epsilon.Model.StdFolder.Commands
{
    public class Create : ICommand
    {
        public string FolderType { get; set; }

        public string StdFolderName { get; set; }
        
        public int ParentId { get; set; }


    }
}
