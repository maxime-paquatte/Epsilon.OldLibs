using System.Collections.Generic;

namespace Epsilon.Messaging.Host
{
    public class SimpleErrorsCollector : IErrorsCollector
    {
        private readonly List<CommandValidationError> _errors;

        public bool HasError
        {
            get { return _errors.Count > 0; }
        }

        public SimpleErrorsCollector()
        {
            _errors = new List<CommandValidationError>();
        }

        public void AddErrorMessage(string message)
        {
            _errors.Add(new CommandValidationError(message));
        }


        public void AddErrorMessage(string key, string message)
        {
            _errors.Add(new CommandValidationError(key, message));
        }

        IEnumerable<CommandValidationError> IErrorsCollector.Errors
        {
            get { return _errors; }
        }
    }
}
