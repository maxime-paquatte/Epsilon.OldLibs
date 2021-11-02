using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Epsilon.Messaging.Host
{
    public class Bus : IBus
    {

        protected static readonly Dictionary<Type, List<object>> Events = new Dictionary<Type, List<object>>();
        protected readonly IStore _store;
        protected readonly IMessageContextFactory _contextFactory;
        protected readonly IClaimsValidator _claimsValidator;
        protected readonly IBusLogger _logger;

        private readonly JsonSerializerOptions _indentedSerializerOptions = new JsonSerializerOptions { WriteIndented = true };


        public Bus(IStore store, IMessageContextFactory contextFactory,
            IClaimsValidator claimsValidator, IBusLogger logger)
        {
            _store = store;
            _contextFactory = contextFactory;
            _claimsValidator = claimsValidator;
            _logger = logger;
        }

        public Type ResolveMessage(string fullName)
        {
            return _store.ResolveMessageType(fullName);
        }

        public CommandResult Send<T>(T command) where T : ICommand
        {
            return this.Send(command, this);
        }

        public CommandResult Send<T>(T command, IEventDispatcher d) where T : ICommand
        {
            return Send<T>(command, Guid.NewGuid().ToString("N"), d);
        }

        public virtual CommandResult Send<T>(T command, string commandId, IEventDispatcher d) where T : ICommand
        {
            var cmdType = typeof(T);
            using (var scope = _contextFactory.GetScope())
            {
                var ctx = scope.GetContext();

                var claimsAttr = cmdType.GetCustomAttribute<AnyClaimsAttribute>();
                if (claimsAttr != null && !_claimsValidator.ValidateAny(ctx, claimsAttr.Claims))
                    throw new UnauthorizedAccessException("Claims no validated: " + string.Join(", ", claimsAttr.Claims));

                var featureAttr = cmdType.GetCustomAttribute<FeatureAttribute>();
                if (featureAttr != null && !_claimsValidator.ValidateFeature(ctx, featureAttr.Feature))
                    throw new UnauthorizedAccessException("Can not access to feature : " + featureAttr.Feature);


                IErrorsCollector errorCollector = new SimpleErrorsCollector();

                _logger.Log(commandId, "Received command of type : " + cmdType.FullName);
                _logger.Log(commandId, "Command Context was : " + JsonSerializer.Serialize(ctx, _indentedSerializerOptions));
                _logger.Log(commandId, "Command values was : " + JsonSerializer.Serialize(command, _indentedSerializerOptions));

                var validators = _store.ResolveValidator(cmdType);
                StdCmdValidator(ctx, command, errorCollector);
                foreach (var commandValidator in validators)
                {
                    _logger.Log(commandId, "Run validator : " + commandValidator.GetType().FullName);
                    var validator = (ICommandValidator<T>)commandValidator;
                    validator.Validate(ctx, command, errorCollector);
                
                }

                _logger.Log(commandId, "Error count : " + errorCollector.Errors.Count());

                if (!errorCollector.HasError)
                {
                    var handlers = _store.ResolveCommandHandlers(command.GetType());
                    foreach (var handler1 in handlers)
                    {
                        var handler = (ICommandHandler<T>)handler1;
                        _logger.Log(commandId, "Handle command : " + handler.GetType().FullName);
                        handler.Handle(d, ctx, commandId, command);
                    }
                }

                return new CommandResult
                {
                    CommandId = commandId,
                    CommandValidationErrors = errorCollector.Errors
                };
            }
        }

        public virtual void StdCmdValidator(IMessageContext context, ICommand cmd, IErrorsCollector errors)
        {
            foreach (var prop in cmd.GetType().GetProperties())
            {
                var allAttrs = prop.GetCustomAttributes(true);
                foreach (var attr in allAttrs.OfType<ICommandPropertyValidationAttribute>())
                {
                    attr.Validate(context, cmd, errors, prop);
                }
            }
        }

        public virtual void Fire<T>(T e) where T : IEvent
        {
            using (var scope = _contextFactory.GetScope())
            {
                var ctx = scope.GetContext();

                _logger.Log(e.CommandId, "event  Received : " + e.GetType().FullName);
                _logger.Log(e.CommandId, "Event values was : " + _indentedSerializerOptions);

                var handlers = _store.ResolveEventHandlers(e);
                foreach (IEventHandler<T> handler in handlers.Where(p=> p != null))
                {
                    _logger.Log(e.CommandId, "Handle event : " + handler.GetType().FullName);
                    handler.Handle(ctx, e);
                }

                if (Events.ContainsKey(e.GetType()))
                {
                    foreach (Action<T> o in Events[e.GetType()])
                    {
                        o(e);
                    }
                }
            }
        }


        public DisposableCallback On<T>(Action<T> e)
        {
            List<object> l;
            if (!Events.TryGetValue(typeof(T), out l))
                Events.Add(typeof(T), l = new List<object>());

            l.Add(e);
            return new DisposableCallback(() => this.Off(e));
        }

        public void Off<T>(Action<T> e)
        {
            if (Events.ContainsKey(typeof(T)))
                Events[typeof(T)].Remove(e);
        }
    }


}
