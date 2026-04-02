using Fireball.Fireworks.Models;

namespace Fireball.Fireworks.Validation
{
    public class MessageValidationResult
    {
        public bool IsValid { get; set; }
        public ErrorMessage Error { get; set; }
        public MessageResult ErrorSentResult { get; set; }
    }
}
