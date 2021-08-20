using System;
using System.Collections.Generic;
using System.Text;

namespace Epsilon.Messaging.Host.Tests.Impl
{
    public class MessageContextFactory : IMessageContextFactory
    {
        public IScopedMessageContext GetScope()
        {
            return new ScopedMessageContext();
        }
    }

    public class ScopedMessageContext : IScopedMessageContext
    {
        public void Dispose()
        {
            
        }

        public IMessageContext GetContext()
        {
            return new MessageContext
            {
                ActorId = 1,
                CultureId = 9
            };
        }
    }

    public class MessageContext : IMessageContext
    {
        public int ActorId { get; set; }
        public int CultureId { get; set; }
    }
}
