namespace Fireball.Fireworks.Models
{
    public enum ErrorCode
    {
        Default = 0,
        Integration = 1,
        Firestore = 2,
        Authentication = 3,
        InGame = 4,
        Validation = 5,
    }

    public class ErrorMessage : BaseMessage
    {
        public ErrorCode Code { get; set; }
        public string Reason { get; set; }

        public ErrorBase Error { get; set; }
        public long? Balance { get; set; }

        public ErrorMessage() { }

        public ErrorMessage(string name, ErrorCode code, string reason, BaseMessage internalMessage)
        {
            Name = name;
            Code = code;
            Reason = reason;

            CopyBaseParams(internalMessage);
        }

        public ErrorMessage(string name, ErrorCode code, string reason, ErrorDialog errorDialog, BaseMessage internalMessage)
        {
            Name = name;
            Code = code;
            Reason = reason;
            Error = new ErrorBase()
            {
                Dialog = errorDialog,
            };
            CopyBaseParams(internalMessage);
        }
    }

    public class ErrorMessage<T> : ErrorMessage where T : BaseMessage
    {
        public T MessagePayload { get; set; }

        public ErrorMessage() { }

        public ErrorMessage(string name, ErrorCode code, string reason, T messagePayload)
            : base(name, code, reason, messagePayload)
        {
            MessagePayload = messagePayload;
        }
    }

    public class ErrorBase
    {
        public ErrorDialog Dialog { get; set; }
        public ErrorClientScript ClientScript { get; set; }
    }

    // INTEGRATION ERROR EVENT MESSAGE

    public class ErrorClientScript
    {
        public object Value { get; set; }

        public ErrorClientScript() { }
    }
}
