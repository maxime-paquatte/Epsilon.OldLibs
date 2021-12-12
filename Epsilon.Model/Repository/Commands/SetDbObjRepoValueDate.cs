using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class SetDbObjRepoValueDate : ICommand
    {
        public int DbObjId { get; set; }

        public int RepoValueId { get; set; }

        public System.DateTime DateAdded { get; set; }
    }
}
