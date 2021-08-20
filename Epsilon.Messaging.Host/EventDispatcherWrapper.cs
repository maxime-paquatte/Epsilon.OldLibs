using System.Collections.Generic;

namespace Epsilon.Messaging.Host
{
    public class EventDispatcherWrapper : IEventDispatcher
    {
        private readonly List<IEvent> _events;
        private readonly IBus _bus;

        public IReadOnlyList<IEvent> Events
        {
            get { return _events; }
        }

        public EventDispatcherWrapper(IBus bus)
        {
            _bus = bus;
            _events = new List<IEvent>();
        }

        public void Fire<T>(T e) where T : IEvent
        {
            _events.Add(e);
            _bus.Fire(e);
        }
    }
}
