namespace Epsilon.Messaging
{
    public interface IQueryJSonBus
    {
        string Read<T>(T query)
            where T : IQuery;
    }

    public interface IQueryJSonReader
    {
    }

    public interface IQueryJSonReader<T> : IQueryJSonReader
        where T : IQuery
    {
        string Read(IMessageContext context, T query);
    }
}
