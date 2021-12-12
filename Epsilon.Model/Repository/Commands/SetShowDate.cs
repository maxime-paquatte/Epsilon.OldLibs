using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class SetShowDate : ICommand
    {
        public int RepositoryId { get; set; }

        public bool ShowDate { get; set; }
    }
}
