using System.Collections.Generic;

namespace Epsilon.Messaging.Host
{
    public interface IClaimsValidator
    {
        bool ValidateAny(IMessageContext ctx, IEnumerable<string> claims);
        bool ValidateFeature(IMessageContext ctx, string feature);
    }
}
