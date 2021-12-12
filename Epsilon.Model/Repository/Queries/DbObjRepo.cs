using Epsilon.Messaging;

namespace Epsilon.Model.Repository.Queries
{
    public class DbObjRepo : IQuery
    {
        public int DbObjId { get; set; }

        public string SystemKeys { get; set; }
    }
}
