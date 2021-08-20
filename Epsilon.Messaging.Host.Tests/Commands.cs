using System;
using System.Linq;
using Epsilon.Messaging.Host.Tests.Impl;
using Xunit;

namespace Epsilon.Messaging.Host.Tests
{
    public class Commands
    {
        Bus GetBus(Action<Store> initStore)
        {
            var activator = new TestActivator();
            var store = new Store(activator);
            var ctxFactory = new MessageContextFactory();
            var claimsValidator = new ClaimsValidator();
            var logger = new BusLogger();

            var bus = new Bus(store, ctxFactory, claimsValidator, logger);
            initStore(store);

            return bus;
        }

        [Fact]
        public void Command()
        {
            var bus = GetBus(store =>
            {
                store.RegisterMessageType(typeof(TestCmd));
                store.RegisterMessageType(typeof(TestEvent));
                store.RegisterCommandHandler(typeof(TestCmd), typeof(CmdHandler));
            });
            
            var eventWrapper = new EventDispatcherWrapper(bus);
            bus.Send(new TestCmd { Name = "TestOK" }, eventWrapper);
            var e = eventWrapper.Events.OfType<TestEvent>().FirstOrDefault();
            Assert.True(e != null && e.Name == "TestOK");
        }

        [Fact]
        public void ValidatorOk()
        {
            var bus = GetBus(store =>
            {
                store.RegisterMessageType(typeof(TestCmd));
                store.RegisterMessageType(typeof(TestEvent));
                store.RegisterValidator(typeof(TestCmd), typeof(Validator));
                store.RegisterCommandHandler(typeof(TestCmd), typeof(CmdHandler));
            });

            var eventWrapper = new EventDispatcherWrapper(bus);
            var r = bus.Send(new TestCmd { Name = "TestOK" }, eventWrapper);
            Assert.True(!r.CommandValidationErrors.Any());
            var e = eventWrapper.Events.OfType<TestEvent>().FirstOrDefault();
            Assert.True(e != null && e.Name == "TestOK");
        }

        [Fact]
        public void ValidatorKo()
        {
            var bus = GetBus(store =>
            {
                store.RegisterMessageType(typeof(TestCmd));
                store.RegisterMessageType(typeof(TestEvent));
                store.RegisterValidator(typeof(TestCmd), typeof(Validator));
                store.RegisterCommandHandler(typeof(TestCmd), typeof(CmdHandler));
            });

            var eventWrapper = new EventDispatcherWrapper(bus);
            var r = bus.Send(new TestCmd { Name = "TestKO" }, eventWrapper);
            Assert.True(r.CommandValidationErrors.Any());

            var e = eventWrapper.Events.OfType<TestEvent>().FirstOrDefault();
            Assert.True(e == null);
        }

        [Fact]
        public void AllCommandHandler()
        {
            var bus = GetBus(store =>
            {
                store.RegisterMessageType(typeof(TestCmd));
                store.RegisterMessageType(typeof(TestEvent));
                store.RegisterCommandHandler(typeof(ICommand), typeof(AllCmdHandler));
            });

            var eventWrapper = new EventDispatcherWrapper(bus);
            bus.Send(new TestCmd { Name = "TestOK" }, eventWrapper);
            var e = eventWrapper.Events.OfType<TestEvent>().FirstOrDefault();
            Assert.True(e != null && e.Name == "All");
        }


        [Fact]
        public void AllValidator()
        {
            var bus = GetBus(store =>
            {
                store.RegisterMessageType(typeof(TestCmd));
                store.RegisterMessageType(typeof(TestEvent));
                store.RegisterValidator(typeof(TestCmd), typeof(AllValidator));
                store.RegisterCommandHandler(typeof(TestCmd), typeof(CmdHandler));
            });

            var eventWrapper = new EventDispatcherWrapper(bus);
            var r = bus.Send(new TestCmd { Name = "TestKO" }, eventWrapper);
            Assert.True(r.CommandValidationErrors.Any());

            var e = eventWrapper.Events.OfType<TestEvent>().FirstOrDefault();
            Assert.True(e == null);
        }
    }

    public class TestCmd : ICommand
    {
        public string Name { get; set; }
    }

    public class TestEvent : IEvent
    {
        public string CommandId { get; set; }
        public string Name { get; set; }
    }

    public class CmdHandler : ICommandHandler<TestCmd>
    {
        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, TestCmd command)
        {
            d.Fire(new TestEvent{CommandId = commandId, Name = command.Name});
        }
    }

    public class AllCmdHandler : ICommandHandler<ICommand>
    {
        public void Handle(IEventDispatcher d, IMessageContext context, string commandId, ICommand command)
        {
            d.Fire(new TestEvent { CommandId = commandId, Name = "All" });
        }
    }

    public class Validator : ICommandValidator<TestCmd>
    {
        public void Validate(IMessageContext context, TestCmd cmd, IErrorsCollector errors)
        {
            if(cmd.Name != "TestOK")
                errors.AddErrorMessage("Name", "Name should be TestOK");
        }
    }

    public class AllValidator : ICommandValidator<ICommand>
    {
        public void Validate(IMessageContext context, ICommand cmd, IErrorsCollector errors)
        {
            var c = cmd as TestCmd;
            if (c.Name != "TestOK")
                errors.AddErrorMessage("Name", "Name should be TestOK");
        }
    }
}
