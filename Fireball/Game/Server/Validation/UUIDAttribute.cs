using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Fireball.Fireworks.Validation
{
    /// <summary>Specifies that a data field value is UUID string (in the canonical 8-4-4-4-12 format)</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UUIDAttribute : ValidationAttribute
    {
        private const string PATTERN = @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$";

        public override bool IsValid(object value)
        {
            Regex expression = new Regex(PATTERN);
            var result = expression.Match(value.ToString());
            return result.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be UUIDv4 format";
        }
    }
}
