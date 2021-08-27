using System;
using System.Collections.Generic;
using System.Text;
using Epsilon.Messaging;
using Epsilon.Messaging.Sql;
using Epsilon.Model.Resource.Commands;
using Epsilon.Model.Resource.Queries;

namespace Epsilon.Model.Resource
{
    public class Handlers : SqlCommandHandlerBase, ICommandHandler<Add>, ICommandHandler<ValueSet>
    {
        public Handlers(IBus bus, IConfig config) : base(bus, config.ConnectionString)
        {
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, Add command)
        {
            Handle(d, context, commandId, command, "EpRes.scAdd");
        }

        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ValueSet command)
        {
            Handle(d, context, commandId, command, "EpRes.scValueSet");
        }
    }


    public class SqlQueryJSonReader : SqlQueryJSonReaderBase, IQueryJSonReader<Page>, IQueryJSonReader<All>
    {
        public SqlQueryJSonReader(IConfig config) : base(config.ConnectionString)
        {
        }

        public string Read(IMessageContext context, Page query)
        {
            return Read(context, query, "EpRes.svPage");
        }

        public string Read(IMessageContext context, All query)
        {
            return Read(context, query, "EpRes.svForPrefixes");
        }
    }
}
