using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class SetOrderByValue : ICommand
    {
        public int RepositoryId { get; set; }

        public bool OrderByValue { get; set; }
    }
}
