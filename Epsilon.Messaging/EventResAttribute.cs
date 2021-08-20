using System;

namespace Epsilon.Messaging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventResAttribute : Attribute
    {
        public string ResName { get; set; }
    }
}
