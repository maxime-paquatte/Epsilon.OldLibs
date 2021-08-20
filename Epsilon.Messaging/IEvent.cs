namespace Epsilon.Messaging
{
    public interface IEvent : IMessage
    {
        string CommandId { get; set; }
    }
}
