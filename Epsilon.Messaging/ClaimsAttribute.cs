using System;
using System.Collections.Generic;

namespace Epsilon.Messaging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AnyClaimsAttribute : Attribute
    {
        public IEnumerable<string> Claims { get; private set; }

        public AnyClaimsAttribute(params string[] claims)
        {
            Claims = claims;
        }
    }
}