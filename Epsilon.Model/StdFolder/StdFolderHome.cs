using Epsilon.Model.StdFolder.Commands;
using Epsilon.Model.StdFolder.Queries;
using Epsilon.Messaging;
using Epsilon.Messaging.Sql;

namespace Epsilon.Model.StdFolder
{
    


    public class StdFolderCommandHandler : SqlCommandHandlerBase, ICommandHandler<Create>, ICommandHandler<Delete>, ICommandHandler<Rename>, ICommandHandler<Move>
    {
        public StdFolderCommandHandler(IConfig config, IBus bus)
            : base(bus, config.ConnectionString)
        {
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Create command)
        {
            Handle(d, context, commandId, command, "Ep.scStdFolderCreate");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Delete command)
        {
            Handle(d, context, commandId, command, "Ep.scStdFolderDelete");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Rename command)
        {
            Handle(d, context, commandId, command, "Ep.scStdFolderRename");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Move command)
        {
            Handle(d, context, commandId, command, "Ep.scStdFolderMove");
        }
    }

    public class StdFolderReader : SqlQueryJSonReaderBase, IQueryJSonReader<AllByType>
    {
        public StdFolderReader(IConfig config)
            : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, AllByType query)
        {
            return Read(context, query, "Ep.svStdFolderAllByType");
        }
    }
}
