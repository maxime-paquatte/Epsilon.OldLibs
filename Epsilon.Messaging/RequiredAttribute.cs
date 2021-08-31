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
        public bool AllowZero { get; set; }

        public void Validate(IMessageContext context, ICommand cmd, IErrorsCollector errors, PropertyInfo prop)
        {
            var v = prop.GetValue(cmd);
            if (v == null
                || (v is string && (string)v == string.Empty)
                || !AllowZero && IsNumericType(prop.PropertyType) && Convert.ToDecimal(v) == 0)
                errors.AddErrorMessage(prop.Name, "Res.Std.Required");
        }


        private bool IsNumericType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

    }

}