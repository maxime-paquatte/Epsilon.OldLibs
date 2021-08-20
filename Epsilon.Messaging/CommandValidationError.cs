namespace Epsilon.Messaging
{
    public class CommandValidationError
    {
        public string Key { get; set; }

        public string Messsage { get; set; }

        public CommandValidationError(string key, string message)
        {
            Key = key;
            Messsage = message;
        }

        public CommandValidationError(string message)
            : this("", message)
        {
        }
    }
}
