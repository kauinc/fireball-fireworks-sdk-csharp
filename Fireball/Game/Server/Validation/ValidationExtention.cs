using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Fireball.Fireworks.Models;

namespace Fireball.Fireworks.Validation
{
    public static class ValidationExtention
    {
        public static ValidationResult Validate<T>(this T message) where T : BaseMessage
        {
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(message, new ValidationContext(message), validationResults))
            {
                return validationResults.First();
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ValidateAll<T>(this T message) where T : BaseMessage
        {
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(message, new ValidationContext(message), validationResults, true))
            {
                return new ValidationResult(string.Join(",", validationResults.Select(v => v.ErrorMessage)));
            }
            return ValidationResult.Success;
        }
    }
}
