using System;
using System.Collections.Generic;
using System.Linq;
using Fireball.Game.Server.Models;
using Fireball.Game.Server.SessionModule;

namespace Fireball.Game.Server.MultiplayerModule
{
    internal class BetTiersData
    {
        public List<BetTierFull> Data { get; set; }
    }

    internal class BetTierFull
    {
        public string Id { get; set; }
        public string CurrencyIsoCode { get; set; }
        public long Value { get; set; }
        public BetTierDefault BetTier { get; set; }

        public BetTier ToBetTier()
        {
            if (BetTier == null)
            {
                return new BetTier()
                {
                    Tier = 0,
                    Id = this.Id,
                    Value = this.Value,
                    ValueDefault = this.Value,
                    Currency = this.CurrencyIsoCode,
                    CurrencyDefault = this.CurrencyIsoCode,
                };
            }

            return new BetTier()
            {
                Tier = 0,
                Id = this.BetTier.Id,
                Value = this.Value,
                ValueDefault = this.BetTier.Value,
                Currency = this.CurrencyIsoCode,
                CurrencyDefault = this.BetTier.CurrencyIsoCode,
            };
        }
    }

    internal class BetTierDefault
    {
        public string Id { get; set; }
        public long Value { get; set; }
        public string CurrencyIsoCode { get; set; }
    }

    public class BetTier
    {
        public int Tier { get; set; }
        public string Id { get; set; }
        public long Value { get; set; }
        public long ValueDefault { get; set; }
        public string Currency { get; set; }
        public string CurrencyDefault { get; set; }
    }

    public class AddPlayerRequest : JsonMessage
    {
        public List<int> Tiers { get; set; }
        public List<string> MatchCriterias { get; set; }
        public Dictionary<string, string> GameSettings { get; set; }
        public int MinimumPlayers { get; set; }
        public int MaximumPlayers { get; set; }
        public int PlayersBackoffInterval { get; set; }

        public string OperatorId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string ConnectionId { get; set; }
        public string Currency { get; set; }
        public string GameId { get; set; }
        public string Environment { get; set; }
        public string GameMode { get; set; }
        public Dictionary<string, string> Extra { get; set; }

        public AddPlayerRequest(BaseMessage message, List<int> betTiers, List<string> matchCriterias, int minimumPlayers = 2, int maximumPlayers = 2, int playersBackoffInterval = 60, Dictionary<string, string> gameSettings = null)
        {
            Tiers = betTiers;
            MatchCriterias = matchCriterias;
            MinimumPlayers = minimumPlayers;
            MaximumPlayers = maximumPlayers;
            PlayersBackoffInterval = playersBackoffInterval;
            GameSettings = gameSettings;

            OperatorId = message.OperatorId;
            OperatorPlayerId = message.OperatorPlayerId;
            OperatorPlayerSession = message.OperatorPlayerSession;
            ConnectionId = message.ConnectionId;
            Currency = message.Currency;
            GameId = message.GameId;
            Environment = message.Environment;
            GameMode = message.GameMode;
            Extra = message.Extra;
        }
    }

    public class CancelPlayerRequest : JsonMessage
    {
        public string OperatorId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string GameId { get; set; }
        public string Environment { get; set; }
        public string GameMode { get; set; }

        public CancelPlayerRequest(BaseMessage message)
        {
            OperatorId = message.OperatorId;
            OperatorPlayerId = message.OperatorPlayerId;
            OperatorPlayerSession = message.OperatorPlayerSession;
            GameId = message.GameId;
            Environment = message.Environment;
            GameMode = message.GameMode;
        }
    }

    public class PlayerMatchesResponse : JsonMessage
    {
        public List<PlayerMatchData> Matches { get; set; }
    }

    public class PlayerMatchData : JsonMessage
    {
        public List<int> Tiers { get; set; }
        public List<string> MatchCriterias { get; set; }
        public Dictionary<string, string> GameSettings { get; set; }

        public string OperatorId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorPlayerSession { get; set; }
        public string ConnectionId { get; set; }
        public string Currency { get; set; }
        public string GameId { get; set; }
        public string Environment { get; set; }
        public string GameMode { get; set; }
        public Dictionary<string, string> Extra { get; set; }
    }

    public class MatchStartMessage : BaseMessage
    {
        public int Tier { get; set; }
        public string MatchCriteria { get; set; }
        public List<PlayerDetails> PlayerDetails { get; set; }

        public MatchStartMessage() { }

        public List<Player> GetPlayers()
        {
            return PlayerDetails.Select(p => p.Player.ToSessionPlayer()).ToList();
        }

        public SessionMessage<T> CreateSessionMessage<T>(GameSession gameSession, string operatorPlayerId, T gameState) where T : class
        {
            if (gameSession == null)
                throw new NullReferenceException("GameSession == null");

            if (operatorPlayerId == null)
                throw new NullReferenceException("OperatorPlayerId == null");

            var player = gameSession.Players?.Find(p => p.OperatorPlayerId == operatorPlayerId);
            if (player == null)
                throw new NullReferenceException($"GameSession not contain player with operatorPlayerId = {operatorPlayerId}");

            var playerDetails = PlayerDetails?.Find(d => d.Player?.OperatorPlayerId == operatorPlayerId);
            if (playerDetails == null)
                throw new NullReferenceException($"PlayerDetails not contain player with operatorPlayerId = {operatorPlayerId}");

            var sessionMessage = new SessionMessage<T>();
            sessionMessage.Name = FireballConstants.MessagesNames.SESSION;
            sessionMessage.ActionId = FireballTools.GenerateGUID();
            sessionMessage.ReplayId = FireballTools.GenerateGUID();
            sessionMessage.MessageTimestamp = FireballTools.TimestampNow();

            sessionMessage.Environment = gameSession.Environment;
            sessionMessage.GameMode = gameSession.GameMode;
            sessionMessage.GameId = gameSession.GameId;
            sessionMessage.GameSession = gameSession.Id;
            sessionMessage.GameState = gameState;

            sessionMessage.PlayerId = player.PlayerId;
            sessionMessage.OperatorId = player.OperatorId;
            sessionMessage.OperatorPlayerId = player.OperatorPlayerId;
            sessionMessage.OperatorPlayerSession = player.OperatorSessionId;

            sessionMessage.ConnectionId = playerDetails.ConnectionId;
            sessionMessage.Currency = playerDetails.Currency;
            sessionMessage.Extra = playerDetails.Extra;

            // Balance = 0;
            // Coins = 0;
            // LastActionId = null;
            // server_side = null;
            // client_side = null;
            // sessionMessage.Jackpots = null;

            return sessionMessage;
        }
    }

    public class PlayerDetails
    {
        public string Currency { get; set; }
        public string ConnectionId { get; set; }
        public PlayerIds Player { get; set; }
        public Dictionary<string, string> Extra { get; set; }
        public Dictionary<string, string> GameSettings { get; set; }
    }

    public class PlayerIds
    {
        public string OperatorId { get; set; }
        public string OperatorPlayerId { get; set; }
        public string OperatorPlayerSession { get; set; }

        public Player ToSessionPlayer()
        {
            return new Player()
            {
                OperatorId = this.OperatorId,
                OperatorPlayerId = this.OperatorPlayerId,
                OperatorSessionId = this.OperatorPlayerSession,
            };
        }
    }

    public class ScheduleCallbackMessage : JsonMessage
    {
        public string Environment { get; set; }
        public string GameId { get; set; }
        public string CallbackId { get; set; }
        public DateTime CallbackTime { get; set; }
        public string JsonMessage { get; set; }

        public ScheduleCallbackMessage() { }

        public ScheduleCallbackMessage(string environment, string gameId, string callbackId, DateTime time, string jsonMessage)
        {
            Environment = environment;
            GameId = gameId;
            CallbackId = callbackId;
            CallbackTime = time;
            JsonMessage = jsonMessage;
        }
    }

    public class ScheduleCallbackMessage<T> : ScheduleCallbackMessage where T : BaseMessage
    {
        public ScheduleCallbackMessage(T message, string callbackId, DateTime time)
        {
            Environment = message.Environment;
            GameId = message.GameId;
            CallbackId = callbackId;
            CallbackTime = time;
            JsonMessage = message.ToJson();
        }
    }

    public class DeleteCallbackMessage : JsonMessage
    {
        public string Environment { get; set; }
        public string GameId { get; set; }
        public string CallbackId { get; set; }

        public DeleteCallbackMessage() { }

        public DeleteCallbackMessage(string environment, string gameId, string callbackId)
        {
            Environment = environment;
            GameId = gameId;
            CallbackId = callbackId;
        }
    }
}

