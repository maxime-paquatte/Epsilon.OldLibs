using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class SetDbObjRepoValue : ICommand
    {
        public int DbObjId { get; set; }

        public int RepoId { get; set; }

        public string[] RepoValueIds { get; set; }
    }
}
