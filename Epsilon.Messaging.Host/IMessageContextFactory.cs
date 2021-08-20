using System;

namespace Epsilon.Messaging.Host
{
    public interface IMessageContextFactory
    {
        IScopedMessageContext GetScope();
    }

    public interface IScopedMessageContext : IDisposable
    {
        IMessageContext GetContext();
    }
}