namespace Epsilon.Messaging
{
    public interface ICommandValidator
    {

    }

    public interface ICommandValidator<in T> : ICommandValidator
        where T : ICommand
    {
        void Validate(IMessageContext context, T cmd, IErrorsCollector errors);
    }


}
