namespace Epsilon.Messaging
{
    public interface IQuery : IMessage
    {
    }
    
    public interface IPagedQuery : IMessage
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Total { get; set; }
    }
}
