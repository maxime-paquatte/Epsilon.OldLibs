using System;

namespace Epsilon.Messaging
{
    public interface IBus : IEventDispatcher
    {
        CommandResult Send<T>(T command) where T : ICommand;

        CommandResult Send<T>(T command, IEventDispatcher d) where T : ICommand;

        CommandResult Send<T>(T command, string commandId, IEventDispatcher d) where T : ICommand;

        CommandResult Send<T>(T command, string commandId, IMessageContext ctx, IEventDispatcher d = null) where T : ICommand;

        Type ResolveMessage(string fullName);

        DisposableCallback On<T>(Action<T> e);
        void Off<T>(Action<T> e);

    }

    public class DisposableCallback : IDisposable
    {
        public Action Callback { get; private set; }

        public DisposableCallback(Action callback)
        {
            Callback = callback;
        }

        public void Dispose()
        {
            Callback();
        }
    }
}
