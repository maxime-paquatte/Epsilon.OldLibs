using System;
using System.Collections.Generic;
using System.Text;

namespace Epsilon.Messaging.Host.Tests.Impl
{
    public class TestActivator : IActivator
    {
        public object Create(Type t)
        {
            return Activator.CreateInstance(t);
        }
    }
}
