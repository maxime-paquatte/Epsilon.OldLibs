using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class SetSortable : ICommand
    {
        public int RepositoryId { get; set; }

        public bool Sortable { get; set; }
    }
}
