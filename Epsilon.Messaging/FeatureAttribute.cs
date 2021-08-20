using System;

namespace Epsilon.Messaging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureAttribute : Attribute
    {
        public string Feature { get; private set; }

        public FeatureAttribute(string feature)
        {
            Feature = feature;
        }
    }
}