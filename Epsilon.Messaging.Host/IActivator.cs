using System;

namespace Epsilon.Messaging.Host
{
    public interface IActivator
    {
        Object Create(Type t);
    }
}
