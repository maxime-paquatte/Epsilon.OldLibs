using System.Collections.Generic;

namespace Epsilon.Messaging.Host
{
    public interface IClaimsValidator
    {
        bool ValidateAny(IMessageContext ctx, string requiredClaims);
        bool ValidateFeature(IMessageContext ctx, string feature);
    }
}
