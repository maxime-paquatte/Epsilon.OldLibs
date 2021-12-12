using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class ValueChange : ICommand
    {
        public int RepoValueId { get; set; }

        public int ResCultureId { get; set; }

        public string Value { get; set; }

    }
}
