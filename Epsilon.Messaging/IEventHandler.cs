namespace Epsilon.Messaging
{

    public interface IEventHandler
    {
    }

    public interface IEventHandler<in T> : IEventHandler
        where T : IEvent
    {
        void Handle(IMessageContext context, T e);
    }
}
