using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class RemoveDbObjType : ICommand
    {
        public int RepositoryId { get; set; }

        public int DbObjTypeId { get; set; }
    }
}
