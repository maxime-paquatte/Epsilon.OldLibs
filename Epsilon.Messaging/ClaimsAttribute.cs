using System;
using System.Collections.Generic;

namespace Epsilon.Messaging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AnyClaimsAttribute : Attribute
    {
        public string RequiredClaims { get; private set; }

        public AnyClaimsAttribute(string requiredClaims)
        {
            RequiredClaims = requiredClaims;
        }
    }
}