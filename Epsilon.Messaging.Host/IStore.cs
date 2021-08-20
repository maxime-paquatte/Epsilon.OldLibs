using System;
using System.Collections.Generic;

namespace Epsilon.Messaging.Host
{
    public interface IStore
    {
        IEnumerable<string> Messages { get; }

        IEnumerable<ICommandValidator> ResolveValidator(Type commandType);
        void RegisterValidator(Type commandType, Type validatorType);


        IEnumerable<ICommandHandler> ResolveCommandHandlers(Type commandType);
        void RegisterCommandHandler(Type commandType, Type handlerType);


        void RegisterEventHandler(Type eventType, Type handlerType);
        IEnumerable<IEventHandler<TEvent>> ResolveEventHandlers<TEvent>(TEvent e)
            where TEvent : IEvent;

        IQueryJSonReader ResolveReader(Type type);
        void RegisterReader(Type queryType, Type readerType);

        Type ResolveMessageType(string fullName);
        void RegisterMessageType(Type messageType);
    }
}
