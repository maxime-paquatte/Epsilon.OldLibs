using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Commands
{
    public class DbObjValueChangeOrder : ICommand
    {
        public int DbObjId { get; set; }
        public int SourceValueId { get; set; }
        public int TargetValueId { get; set; }
    }
}
