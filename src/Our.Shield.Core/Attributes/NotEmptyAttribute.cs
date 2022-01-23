using System;
using System.ComponentModel.DataAnnotations;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    internal class NotEmptyAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field must not be empty";

        public NotEmptyAttribute(bool allowNull = false) : base(DefaultErrorMessage)
        {
        }

        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            switch (value)
            {
                case Guid guid:
                    return guid != Guid.Empty;

                case int integer:
                    return integer != default(int);

                default:
                    return true;
            }
        }
    }
}
