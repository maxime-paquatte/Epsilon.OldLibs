namespace Epsilon.Messaging
{
    public class CommandValidationError
    {
        public string Key { get; set; }

        public string Message { get; set; }

        public CommandValidationError(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public CommandValidationError(string message)
            : this("", message)
        {
        }
    }
}
