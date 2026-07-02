using System;
using System.Collections.Generic;
using Fireball.Fireworks.IntegrationModule;
using Fireball.Fireworks.JackpotsModule;
using Fireball.Fireworks.SessionModule;
using Fireball.Fireworks.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fireball.Fireworks.Models
{
    public abstract class JsonMessage
    {
        public string ToJson() =>
            JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        public string ToJson_Legacy() =>
            JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
    }

    public class BaseMessage : JsonMessage
    {
        [RequiredSet(IsRequired = true)]
        public string Name { get; set; }
        [RequiredSet(IsRequired = true), UUID]
        public string ActionId { get; set; }
        [RequiredSet(IsRequired = true)]
        public string Environment { get; set; }
        [RequiredSet(IsRequired = true), UUID]
        public string OperatorId { get; set; }
        [RequiredSet(IsRequired = true)]
        public string OperatorPlayerSession { get; set; }
        [RequiredSet(IsRequired = true)]
        public string OperatorPlayerId { get; set; }
        [RequiredSet(IsRequired = true), UUID]
        public string GameId { get; set; }
        [RequiredSet(IsRequired = true), UUID]
        public string PlayerId { get; set; }
        [RequiredSet(IsRequired = true)]
        public string GameSession { get; set; }
        [RequiredSet(IsRequired = true)]
        public string GameMode { get; set; }

        public string ConnectionId { get; set; }
        [RequiredSet(IsRequired = true)]
        public long MessageTimestamp { get; set; }

        public string Currency { get; set; }

        public string ReplayId { get; set; }

        public string MessageId { get; set; }

        public int MessageClientDeviceSequence { get; set; }

        public int MessageServerDeviceSequence { get; set; }

        public Dictionary<string, string> Extra { get; set; }

        public Dictionary<string, object> server_side { get; set; }

        public Dictionary<string, object> client_side { get; set; }

        public int Variant { get; set; }

        public string CopyBaseParams<T>(T otherMessage) where T : BaseMessage
        {
            ActionId = otherMessage.ActionId;
            Environment = otherMessage.Environment;
            OperatorId = otherMessage.OperatorId;
            OperatorPlayerSession = otherMessage.OperatorPlayerSession;
            OperatorPlayerId = otherMessage.OperatorPlayerId;
            GameId = otherMessage.GameId;
            PlayerId = otherMessage.PlayerId;
            GameSession = otherMessage.GameSession;
            GameMode = otherMessage.GameMode;
            ConnectionId = otherMessage.ConnectionId;
            MessageTimestamp = otherMessage.MessageTimestamp;

            Currency = otherMessage.Currency;
            Extra = otherMessage.Extra;
            ReplayId = otherMessage.ReplayId;
            MessageId = otherMessage.MessageId;
            MessageClientDeviceSequence = otherMessage.MessageClientDeviceSequence;
            MessageServerDeviceSequence = otherMessage.MessageServerDeviceSequence;

            server_side = otherMessage.server_side;
            client_side = otherMessage.client_side;
            Variant = otherMessage.Variant;

            return otherMessage.ActionId;
        }

        public T GetServerVariables<T>() where T : class
        {
            T vars = null;
            try
            {
                var varsJson = JsonConvert.SerializeObject(this.server_side);
                vars = JsonConvert.DeserializeObject<T>(varsJson);
            }
            catch (Exception)
            {
                //_logger.Error($"[Fireball] Can't serialize Server Variables to type = {typeof(T).Name}. Error: {e.Message}");
            }
            return vars;
        }

        public T GetClientVariables<T>() where T : class
        {
            T vars = null;
            try
            {
                var varsJson = JsonConvert.SerializeObject(this.client_side);
                vars = JsonConvert.DeserializeObject<T>(varsJson);
            }
            catch (Exception)
            {
                //_logger.LogError($"[Fireball] Can't serialize Client Variables to type = {typeof(T).Name}. Error: {e.Message}");
            }
            return vars;
        }
    }

    public class PingMessage : BaseMessage
    {
        [RequiredSet(IsRequired = false)]
        public new string GameSession { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string ConnectionId { get; set; }
    }

    public class AuthMessage : BaseMessage
    {
        [RequiredSet(IsRequired = true)]
        public string Token { get; set; }
        [RequiredSet(IsRequired = true)]
        public new string ConnectionId { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string GameSession { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string OperatorPlayerSession { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string OperatorPlayerId { get; set; }
        [RequiredSet(IsRequired = false)]
        public new string PlayerId { get; set; }

        public AuthMessage()
        {
            Name = FireballConstants.MessagesNames.AUTHENTICATE;
        }
    }

    public class SessionMessage : BaseMessage
    {
        public long Balance { get; set; }
        public long? Multiplier { get; set; }
        public Dictionary<string, object> GameState { get; set; }
        public List<JackpotDetail> Jackpots { get; set; }
        public List<FreeBetCampaign> FreeBetCampaigns { get; set; }

        public SessionMessage()
        {
            Name = FireballConstants.MessagesNames.SESSION;
        }

        public SessionMessage UpdateGameSession(GameSession gameSession)
        {
            this.GameSession = gameSession.Id;
            this.PlayerId = gameSession?.GetPlayer(this.OperatorPlayerId)?.PlayerId;
            this.GameState = gameSession?.ParseGameState();
            return this;
        }
    }

    public class SessionMessage<T> : SessionMessage where T : class
    {
        public new T GameState { get; set; }

        public SessionMessage()
        {
            Name = FireballConstants.MessagesNames.SESSION;
        }

        public new SessionMessage<T> UpdateGameSession(GameSession gameSession)
        {
            this.GameSession = gameSession.Id;
            this.PlayerId = gameSession?.GetPlayer(this.OperatorPlayerId)?.PlayerId;
            this.GameState = gameSession?.ParseGameState<T>();

            return this;
        }
    }

    public class CoreWarning : BaseMessage
    {
        [RequiredSet(IsRequired = false)]
        public string Sender { get; set; }
        [RequiredSet(IsRequired = false)]
        public string Message { get; set; }

        public CoreWarning()
        {
            Name = FireballConstants.MessagesNames.WARNING;
        }
    }
}
