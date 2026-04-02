using System;
using System.ComponentModel.DataAnnotations;

namespace Fireball.Fireworks.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredSetAttribute : RequiredAttribute
    {
        public bool IsRequired { get; set; }

        public override bool RequiresValidationContext => IsRequired;

        public override bool IsValid(object value)
        {
            if (IsRequired)
            {
                return base.IsValid(value);
            }
            else
            {
                return true;
            }
        }
    }
}
