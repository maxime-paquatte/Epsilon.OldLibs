using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class ContentChange : ICommand
    {
        public int RepoValueId { get; set; }

        public int ResCultureId { get; set; }

        public string Content { get; set; }

    }
}
