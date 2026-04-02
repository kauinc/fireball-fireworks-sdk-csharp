using Newtonsoft.Json;

namespace Fireball.Fireworks.Models
{
    public class MessageResult
    {
        private const string SUCCESS = "Success";
        private const string ERROR = "Error";

        public string Status { get; set; }
        public string ActionId { get; set; }
        public string Message { get; set; }

        public bool IsSuccess() =>
            Status == SUCCESS;

        public bool IsError() =>
            Status == ERROR;

        public string ToJson() =>
            JsonConvert.SerializeObject(this);

        public override string ToString() =>
            ToJson();

        public static MessageResult SuccessResult(string message, string actionId = null)
        {
            return new MessageResult()
            {
                Status = SUCCESS,
                Message = message,
                ActionId = actionId,
            };
        }

        public static MessageResult ErrorResult(string message, string actionId = null)
        {
            return new MessageResult()
            {
                Status = ERROR,
                Message = message,
                ActionId = actionId,
            };
        }
    }
}
