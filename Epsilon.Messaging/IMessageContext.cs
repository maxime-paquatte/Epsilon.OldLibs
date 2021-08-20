namespace Epsilon.Messaging
{
    public interface IMessageContext
    {
        int ActorId { get; }

        int CultureId { get; }
    }
}
