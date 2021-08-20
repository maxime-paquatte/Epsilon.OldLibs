using System.Collections.Generic;

namespace Epsilon.Messaging
{

    public interface IErrorsCollector
    {
        bool HasError { get; }

        void AddErrorMessage(string message);

        void AddErrorMessage(string key, string message);

        IEnumerable<CommandValidationError> Errors { get; }
    }
}
