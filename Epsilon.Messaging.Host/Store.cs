using System;
using System.Collections.Generic;
using System.Linq;

namespace Epsilon.Messaging.Host
{
    public class Store : IStore
    {
        private readonly IActivator _activator;

        readonly Dictionary<string, Type> _messagesTypes = new Dictionary<string, Type>();
        readonly Dictionary<Type, List<Type>> _commandValidator = new Dictionary<Type, List<Type>>();
        readonly Dictionary<Type, List<Type>> _commandHandlers = new Dictionary<Type, List<Type>>();
        readonly Dictionary<Type, List<Type>> _eventHandlers = new Dictionary<Type, List<Type>>();
        readonly Dictionary<Type, List<Type>> _readers = new Dictionary<Type, List<Type>>();

        public Store(IActivator activator)
        {
            _activator = activator;
        }

        public IEnumerable<string> Messages => _messagesTypes.Keys;
        public IEnumerable<Type> MessageTypes => _messagesTypes.Values;

        #region CommandValidator

        public IEnumerable<ICommandValidator> ResolveValidator(Type commandType)
        {
            return _commandValidator.Where(kvp => kvp.Key.IsAssignableFrom(commandType))
                .SelectMany(kvp => kvp.Value)
                .Select(handlerType => (ICommandValidator)_activator.Create(handlerType));
        }

        public void RegisterValidator(Type commandType, Type validatorType)
        {
            if (!typeof(ICommandValidator).IsAssignableFrom(validatorType))
                throw new ArgumentException("The validator must implement ICommandValidator", "validatorType");

            var key = commandType;
            if (_commandValidator.ContainsKey(key))
                _commandValidator[key].Add(validatorType);
            else
                _commandValidator.Add(key, new List<Type> { validatorType });

        }

        #endregion

        #region CommandHandler
        public IEnumerable<ICommandHandler> ResolveCommandHandlers(Type commandType)
        {
            return _commandHandlers.Where(kvp => kvp.Key.IsAssignableFrom(commandType))
                .SelectMany(kvp => kvp.Value)
                .Select(handlerType => (ICommandHandler)_activator.Create(handlerType));
        }

        public void RegisterCommandHandler(Type commandType, Type handlerType)
        {
            if (!typeof(ICommandHandler).IsAssignableFrom(handlerType))
                throw new ArgumentException("The handler must implement IHandle", "handlerType");

            var key = commandType;
            if (_commandHandlers.ContainsKey(key))
                _commandHandlers[key].Add(handlerType);
            else _commandHandlers.Add(key, new List<Type> { handlerType });
        }
        #endregion

        #region IEventHandler
        public IEnumerable<IEventHandler<TEvent>> ResolveEventHandlers<TEvent>(TEvent eventType)
             where TEvent : IEvent
        {
            List<Type> handlers = _eventHandlers.TryGetValue(typeof(TEvent), out handlers) ? handlers : null;
            if (handlers == null) return Enumerable.Empty<IEventHandler<TEvent>>();

            return handlers.Select(handlerType => (IEventHandler<TEvent>)_activator.Create(handlerType));
        }

        public void RegisterEventHandler(Type eventType, Type handlerType)
        {
            if (!typeof(IEventHandler).IsAssignableFrom(handlerType))
                throw new ArgumentException("The handler must implement IHandle", "handlerType");

            var key = eventType;
            if (_eventHandlers.ContainsKey(key))
                _eventHandlers[key].Add(handlerType);
            else _eventHandlers.Add(key, new List<Type> { handlerType });
        }
        #endregion

        #region Reader
        public IEnumerable<IQueryJSonReader> ResolveReader(Type type)
        {
            return _readers.Where(kvp => kvp.Key.IsAssignableFrom(type))
                .SelectMany(kvp => kvp.Value)
                .Select(handlerType => (IQueryJSonReader)_activator.Create(handlerType));
        }

        public void RegisterReader(Type queryType, Type readerType)
        {
            if (!typeof(IQueryJSonReader).IsAssignableFrom(readerType))
                throw new ArgumentException("The reader must implement IJSonQueryReader : " + readerType.FullName, "readerType");

            var key = queryType;
            if (_readers.ContainsKey(key))
                _readers[key].Add(readerType);
            else
                _readers.Add(key, new List<Type> { readerType });
        }
        #endregion

        public Type ResolveMessageType(string fullName)
        {
            Type type;
            return _messagesTypes.TryGetValue(fullName, out type) ? type : null;
        }

        public void RegisterMessageType(Type messageType)
        {
            if (!_messagesTypes.ContainsKey(messageType.FullName))
                _messagesTypes.Add(messageType.FullName, messageType);
        }
    }
}
