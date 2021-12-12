using Epsilon.Messaging;
using Epsilon.Messaging.Sql;
using Epsilon.Model.Repository.Commands;
using Epsilon.Model.Repository.Queries;

namespace Epsilon.Model.Repository
{
    

    public class RepoCommandHandler : SqlCommandHandlerBase, ICommandHandler<Create>, ICommandHandler<SetShowDate>, ICommandHandler<SetOrderByValue>,
        ICommandHandler<AddDbObjType>, ICommandHandler<RemoveDbObjType>, ICommandHandler<SetSortable>
    {
        public RepoCommandHandler(IConfig config, IBus bus)
            : base(bus, config.ConnectionString)
        {
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, AddDbObjType command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoAddDbObjType");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, RemoveDbObjType command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoRemoveDbObjType");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Create command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoCreate");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, SetShowDate command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoSetShowDate");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, SetOrderByValue command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoSetOrderByValue");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, SetSortable command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoSetSortable");
        }
    }


    public class RepoReader : SqlQueryJSonReaderBase, IQueryJSonReader<ForDbObjType>, IQueryJSonReader<All>, IQueryJSonReader<ById>
    {
        public RepoReader(IConfig config)
            : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, ForDbObjType query)
        {
            return Read(context, query, "Ep.svRepoForDbObjType");
        }

        public string Read(IMessageContext context, All query)
        {
            return Read(context, query, "Ep.svRepoAll");
        }

        public string Read(IMessageContext context, ById query)
        {
            return Read(context, query, "Ep.svRepoById");
        }
    }
}
