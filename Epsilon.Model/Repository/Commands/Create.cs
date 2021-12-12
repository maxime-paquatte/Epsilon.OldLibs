using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class Create : ICommand
    {
        public string RepositoryName { get; set; }
    }
}
