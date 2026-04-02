using Fireball.Game.Server.Models;

namespace Fireball.Game.Server.Validation
{
    public class MessageValidationResult
    {
        public bool IsValid { get; set; }
        public ErrorMessage Error { get; set; }
        public MessageResult ErrorSentResult { get; set; }
    }
}
