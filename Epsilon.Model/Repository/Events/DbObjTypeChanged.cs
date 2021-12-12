using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Events
{
    public class DbObjTypeChanged : IEvent
    {
        public string CommandId { get; set; }

        public int RepositoryId { get; set; }
    }
}
