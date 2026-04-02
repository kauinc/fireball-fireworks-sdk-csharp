using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fireball.Fireworks.MessagesModule;
using Fireball.Fireworks.Core;
using Fireball.Fireworks.Models;
using Fireball.Fireworks.SessionModule;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fireball.Fireworks.MultiplayerModule
{
    public interface IMultiplayer
	{
        Task<List<BetTier>> GetBetTiers(string currency);

        Task<List<PlayerMatchData>> GetSearchingMatches(string operatorPlayerId, BaseMessage message);
        Task<MessageResult> StartSearchMatch(List<int> bets, List<string> matchCriterias, int minPlayers, int maxPlayers, Dictionary<string, string> gameSettings, BaseMessage message);
        Task<MessageResult> CancelSearchMatch(string operatorPlayerId, BaseMessage message);

        Task<GameSession> CreateGameSession<T>(List<Player> players, T gameState, BaseMessage message) where T : class;
        Task<GameSession> CreateGameSession<T>(List<Player> players, T gameState, DateTime endTime, BaseMessage message) where T : class;
        Task<bool> AddPlayerToGameSession(string gameSessionId, Player player);
        Task<bool> RemovePlayerFromGameSession(string gameSessionId, string playerId);
        Task<bool> CloseGameSession(string sessionId);

        Task<T> GetGameState<T>(string gameSessionId, string lockId, DateTime lockTimeout) where T : class;
        Task<bool> UpdateGameState(string gameSessionId, string fieldPath, object fieldValue, string lockId);
        Task<bool> SaveGameState<T>(string gameSessionId, T gameState, string lockId) where T : class;
        Task<bool> SaveGameStateForReplay<T>(string gameSessionId, T gameState, string replayId, string lockId) where T : class;

        Task<MessageResult> BroadcastMessage<T>(List<string> playerIds, T message) where T : BaseMessage;
        Task<MessageResult> BroadcastMessage<T>(string gameSessionId, T message) where T : BaseMessage;

        Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, DateTime callbackTimeUTC) where T : BaseMessage;
        Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, TimeSpan callbackTimeSpan) where T : BaseMessage;
        Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, double delaySeconds) where T : BaseMessage;
        Task<MessageResult> DeleteCallback(string environment, string gameId, string callbackId);
    }

    internal class Multiplayer : IMultiplayer
	{
        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;
        private readonly IMatchMaker _matchmaker;
        private readonly ISession _session;
        private readonly IMessenger _messenger;

        public Multiplayer(IMessenger messenger, ISession session, IMatchMaker matchmaker, ICommunicator communicator, ILogger<MatchMaker> logger)
		{
            _logger = new FireballLogger(nameof(Multiplayer), logger);
            _communicator = communicator;
            _matchmaker = matchmaker;
            _session = session;
            _messenger = messenger;
        }

        public async Task<List<BetTier>> GetBetTiers(string currency)
        {
            return await _matchmaker.GetBetTiers(currency);
        }

        // MATCH MAKING
        public async Task<List<PlayerMatchData>> GetSearchingMatches(string operatorPlayerId, BaseMessage message)
        {
            var response = await _matchmaker.GetPlayerMatches(operatorPlayerId, message);
            return response?.Matches;
        }
        public async Task<MessageResult> StartSearchMatch(List<int> bets, List<string> matchCriterias, int minPlayers, int maxPlayers, Dictionary<string, string> gameSettings, BaseMessage message)
        {
            return await _matchmaker.AddPlayer(new AddPlayerRequest(message, bets, matchCriterias, minPlayers, maxPlayers, 60, gameSettings));
        }
        public async Task<MessageResult> CancelSearchMatch(string operatorPlayerId, BaseMessage message)
        {
            return await _matchmaker.RemovePlayer(new CancelPlayerRequest(message));
        }


        // GAME SESSION
        public async Task<GameSession> CreateGameSession<T>(List<Player> players, T gameState, BaseMessage message) where T : class
        {
            return await _session.CreateSession<T>(
                message.GameId,
                message.GameMode,
                message.Environment,
                players,
                gameState,
                message.ReplayId);
        }
        public async Task<GameSession> CreateGameSession<T>(List<Player> players, T gameState, DateTime endTime, BaseMessage message) where T : class
        {
            return await _session.CreateSession<T>(
                message.GameId,
                message.GameMode,
                message.Environment,
                players,
                gameState,
                message.ReplayId,
                null,
                new AutoComplete(endTime));
        }
        public async Task<bool> AddPlayerToGameSession(string gameSessionId, Player player)
        {
            _logger.LogInfo($"Add Player = {player.PlayerId} to GameSession = {gameSessionId}");
            return await _session.AddPlayerToGameSession(gameSessionId, player);
        }
        public async Task<bool> RemovePlayerFromGameSession(string gameSessionId, string playerId)
        {
            _logger.LogInfo($"Remove Player = {playerId} from GameSession = {gameSessionId}");
            return await _session.RemovePlayerFromSession(gameSessionId, playerId);
        }
        public async Task<bool> CloseGameSession(string sessionId)
        {
            _logger.LogInfo($"CloseGameSession: {sessionId}");
            return await _session.EndSession(sessionId);
        }

        // GAME STATE
        public async Task<T> GetGameState<T>(string gameSessionId, string lockId, DateTime lockTimeout) where T : class
        {
            var session = await _session.GetSessionWithLock(gameSessionId, lockId, lockTimeout);
            return session?.ParseGameState<T>();
        }
        public async Task<bool> UpdateGameState(string gameSessionId, string fieldPath, object fieldValue, string lockId)
        {
            var result = await _session.UpdateSessionState(gameSessionId, fieldPath, fieldValue, lockId);
            return result != null;
        }
        public async Task<bool> SaveGameState<T>(string gameSessionId, T gameState, string lockId) where T : class
        {
            var result = await _session.SaveSession(gameSessionId, JsonConvert.SerializeObject(gameState, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), lockId);
            return result != null;
        }
        public async Task<bool> SaveGameStateForReplay<T>(string gameSessionId, T gameState, string replayId, string lockId) where T : class
        {
            var result = await _session.SaveSessionForReplay(gameSessionId,
                JsonConvert.SerializeObject(gameState, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                replayId,
                lockId);
            return result != null;
        }

        // MEGAGGING
        public async Task<MessageResult> BroadcastMessage<T>(List<string> playerIds, T message) where T : BaseMessage
        {
            if (playerIds != null && playerIds.Count > 0)
            {
                _logger.LogInfo($"BroadcastMessage: players = [{string.Join(",", playerIds)}]");
                var playerReceiver = new ReceiverData(ReceiverTypes.Player, playerIds);

                if (string.IsNullOrEmpty(message.ActionId))
                {
                    message.ActionId = FireballTools.GenerateGUID();
                }

                return await _messenger.SendMessage<T>(message, new List<ReceiverData>() { playerReceiver });
            }

            var error = "Fail to send broadcast message: playerIds is empty";
            _logger.LogError(error);
            return MessageResult.ErrorResult(error);
        }
        public async Task<MessageResult> BroadcastMessage<T>(string gameSessionId, T message) where T : BaseMessage
        {
            if (!string.IsNullOrEmpty(gameSessionId))
            {
                _logger.LogInfo($"BroadcastMessage: gameSessionId = {gameSessionId}");
                var sessionReceiver = new ReceiverData(ReceiverTypes.GameSession, gameSessionId);

                if (string.IsNullOrEmpty(message.ActionId))
                {
                    message.ActionId = FireballTools.GenerateGUID();
                }

                return await _messenger.SendMessage<T>(message, new List<ReceiverData>() { sessionReceiver });
            }

            var error = "Fail to send broadcast message: gameSessionId is empty";
            _logger.LogError(error);
            return MessageResult.ErrorResult(error);
        }


        // SCHEDULER
        public async Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, DateTime callbackTimeUTC) where T : BaseMessage
        {
            if (callbackTimeUTC <= DateTime.UtcNow)
            {
                return MessageResult.ErrorResult("Wrong date! Callback date must be in future");
            }

            var callbackTimestamp = FireballTools.GetTimestamp(callbackTimeUTC);
            if (message.MessageTimestamp < callbackTimestamp)
            {
                message.MessageTimestamp = callbackTimestamp;
            }
            return await _matchmaker.ScheduleCallback(message, callbackId, callbackTimeUTC);
        }
        public async Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, TimeSpan delayTimeSpan) where T : BaseMessage
        {
            return await ScheduleCallback(message, callbackId, DateTime.UtcNow.Add(delayTimeSpan));
        }
        public async Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, double delaySeconds) where T : BaseMessage
        {
            return await ScheduleCallback(message, callbackId, DateTime.UtcNow.AddSeconds(delaySeconds));
        }
        public async Task<MessageResult> DeleteCallback(string environment, string gameId, string callbackId)
        {
            return await _matchmaker.DeleteCallback(environment, gameId, callbackId);
        }
    }
}

