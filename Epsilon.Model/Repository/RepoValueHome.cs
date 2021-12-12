using Epsilon.Model.Repository.Commands;
using Epsilon.Model.Repository.Events;
using Epsilon.Model.Repository.Queries;
using Epsilon.Messaging;
using Epsilon.Messaging.Sql;

namespace Epsilon.Model.Repository
{
    
    public class RepoValueCommandHandler : SqlCommandHandlerBase, ICommandHandler<ValueChange>, ICommandHandler<ValueCreate>, 
        ICommandHandler<ValueDelete>, ICommandHandler<ValueRename>, ICommandHandler<ContentChange>
        , ICommandHandler<BindingAdd>, ICommandHandler<BindingDelete>, ICommandHandler<BindingSetBothWay>, ICommandHandler<BindingSetWeight>
    {
        public RepoValueCommandHandler(IConfig config, IBus bus)
            : base(bus, config.ConnectionString)
        {
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ValueChange command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueChange");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ValueCreate command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueCreate");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ValueDelete command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueDelete");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ValueRename command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueRename");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ContentChange command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueContentChange");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, BindingAdd command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueBindingAdd");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, BindingDelete command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueBindingDelete");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, BindingSetBothWay command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueBindingSetBothWay");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, BindingSetWeight command)
        {
            Handle(d, context, commandId, command, "Ep.scRepoValueBindingSetWeight");
        }
    }

    public class RepoValueEventHandler : SqlEventHandlerBase, IEventHandler<ValueChanged>, IEventHandler<ValueCreated>, IEventHandler<ValueDeleted>
    {
        public RepoValueEventHandler(IConfig config, IBus bus)
            : base(bus, config.ConnectionString)
        {
        }


        public void Handle(IMessageContext context, ValueChanged e)
        {
            Handle(context, e, "Ep.seRepoValueChanged");
        }

        public void Handle(IMessageContext context, ValueCreated e)
        {
            Handle(context, e, "Ep.seRepoValueCreated");
        }

        public void Handle(IMessageContext context, ValueDeleted e)
        {
            Handle(context, e, "Ep.seRepoValueDeleted");
        }
    }

    public class RepoValueReader : SqlQueryJSonReaderBase, IQueryJSonReader<Children>, IQueryJSonReader<Values>, IQueryJSonReader<Typeahead>
        , IQueryJSonReader<ValueBySystemKey>, IQueryJSonReader<BindingForSource>
    {
        public RepoValueReader(IConfig config)
            : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, Children query)
        {
            return Read(context, query, "Ep.svRepoValueChildren");
        }
        
        public string Read(IMessageContext context, Values query)
        {
            return Read(context, query, "Ep.svRepoValues");
        }

        public string Read(IMessageContext context, Typeahead query)
        {
            return Read(context, query, "Ep.svRepoValueTypeahead");
        }

        public string Read(IMessageContext context, ValueBySystemKey query)
        {
            return Read(context, query, "Ep.svRepoValueBySystemKey");
        }

        public string Read(IMessageContext context, BindingForSource query)
        {
            return Read(context, query, "Ep.svRepoValueBindingForSource");
        }
    }
}
