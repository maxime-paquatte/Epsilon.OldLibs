using System.Collections.Generic;

namespace Epsilon.Messaging
{
    public class CommandResult
    {
        public string CommandId { get; set; }

        public IEnumerable<CommandValidationError> CommandValidationErrors { get; set; }

    }



}
