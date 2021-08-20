namespace Epsilon.Messaging.Host
{
    public interface IBusLogger
    {
        void Log(string cmdId, string message);
    }
}
