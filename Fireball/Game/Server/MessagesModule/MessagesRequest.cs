using System.Collections.Generic;
using Fireball.Game.Server.Models;
using Fireball.Game.Server.JackpotsModule;
using Newtonsoft.Json;

namespace Fireball.Game.Server.MessagesModule
{
    public class MessagesBaseRequest<T> where T : BaseMessage
    {
        public T Message { get; set; }
        public string Sender { get; set; }
        public string ActionId { get; set; }

        public MessagesBaseRequest(T message)
        {
            Message = message;
            Sender = $"{message.GameId}-{message.Environment}";
            ActionId = message.ActionId;
        }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }

    public class SendSessionRequest<T> : MessagesBaseRequest<T> where T : SessionMessage
    {
        public string ConnectionId { get; set; }
        public string OperatorId { get; set; }
        public string Environment { get; set; }
        public string GameId { get; set; }
        public string GameSession { get; set; }
        public string PlayerId { get; set; }
        public string GameMode { get; set; }
        public string Currency { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string OperatorPlayerId { get; set; }
        public List<JackpotDetail> Jackpots { get; set; }
        public Dictionary<string, string> Extra { get; set; }

        public SendSessionRequest(T message) : base(message)
        {
            ConnectionId = message.ConnectionId;
            OperatorId = message.OperatorId;
            Environment = message.Environment;
            GameId = message.GameId;
            GameSession = message.GameSession;
            PlayerId = message.PlayerId;
            GameMode = message.GameMode;
            Currency = message.Currency;
            OperatorPlayerSession = message.OperatorPlayerSession;
            OperatorPlayerId = message.OperatorPlayerId;
            Jackpots = message.Jackpots;
            Extra = message.Extra;
        }
    }

    public class SendMessageRequest<T> : MessagesBaseRequest<T> where T : BaseMessage
    {
        public List<ReceiverData> Receivers { get; set; }

        public SendMessageRequest(T message, List<ReceiverData> receivers = null) : base(message)
        {
            Receivers = receivers ?? GetDefaultReceivers(message);
        }

        private List<ReceiverData> GetDefaultReceivers(T message)
        {
            return (!string.IsNullOrEmpty(message.GameSession) ?
                new List<ReceiverData>() { new ReceiverData(ReceiverTypes.GameSession, message.GameSession) } :
                new List<ReceiverData>() { new ReceiverData(ReceiverTypes.Connection, message.ConnectionId) });
        }
    }
}

