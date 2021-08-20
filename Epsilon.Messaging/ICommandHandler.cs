namespace Epsilon.Messaging
{
    public interface ICommandHandler
    {
    }

    public interface ICommandHandler<in T> : ICommandHandler
        where T : ICommand
    {
        void Handle(IEventDispatcher d, IMessageContext context, string commandId, T command);
    }
}
