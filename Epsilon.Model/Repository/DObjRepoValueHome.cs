using Epsilon.Model.Repository.Commands;
using Epsilon.Model.Repository.Queries;
using Epsilon.Messaging;
using Epsilon.Messaging.Sql;

namespace Epsilon.Model.Repository
{
    
    public class CommandHandler : SqlCommandHandlerBase, ICommandHandler<SetDbObjRepoValue>, ICommandHandler<SetDbObjRepoValueDate>, ICommandHandler<DbObjValueChangeOrder>
    {
        public CommandHandler(IConfig config, IBus bus)
            : base(bus, config.ConnectionString)
        {
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, SetDbObjRepoValue command)
        {
            Handle(d, context, commandId, command, "Ep.scSetDbObjRepoValue");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, SetDbObjRepoValueDate command)
        {
            Handle(d, context, commandId, command, "Ep.scSetDbObjRepoValueDate");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, DbObjValueChangeOrder command)
        {
            Handle(d, context, commandId, command, "Ep.scDbObjRepoValueChangeOrder");
        }
    }


    public class DbObjRepoValueReader : SqlQueryJSonReaderBase, IQueryJSonReader<DbObjRepoValue>, IQueryJSonReader<DbObjRepo>
    {
        public DbObjRepoValueReader(IConfig config)
            : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, DbObjRepoValue query)
        {
            return Read(context, query, "Ep.svDbObjRepoValue");
        }

        public string Read(IMessageContext context, DbObjRepo query)
        {
            return Read(context, query, "Ep.svDbObjRepo");
        }
    }
}
