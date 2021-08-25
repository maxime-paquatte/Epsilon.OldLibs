using System;
using System.Reflection;

namespace Epsilon.Messaging
{
    public interface ICommandPropertyValidationAttribute
    {
        void Validate(IMessageContext context, ICommand cmd, IErrorsCollector errors, PropertyInfo prop);
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute, ICommandPropertyValidationAttribute
    {
        public void Validate(IMessageContext context, ICommand cmd, IErrorsCollector errors, PropertyInfo prop)
        {
            var v = prop.GetValue(cmd);
            if (v == null || (v is string && (string)v == string.Empty))
                errors.AddErrorMessage(prop.Name, "Res.Std.Required");
        }
    }
}