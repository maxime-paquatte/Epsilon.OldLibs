using System;
using System.Collections.Generic;
using System.Text;

namespace Epsilon.Messaging.Host.Tests.Impl
{
    public class ClaimsValidator : IClaimsValidator
    {
        public bool ValidateAny(IMessageContext ctx, string claims)
        {
            return true;
        }

        public bool ValidateFeature(IMessageContext ctx, string feature)
        {
            return true;
        }
    }
}
