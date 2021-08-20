namespace Epsilon.Messaging
{
    public interface IEventDispatcher
    {
        void Fire<T>(T e) where T : IEvent;
    }
}
