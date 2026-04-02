using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fireball.Fireworks.IntegrationModule;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fireball.Fireworks.SessionModule
{
    internal interface ISession
    {
        Task<GameSession> CreateSession<T>(string gameId, string gameMode, string environment, List<Player> players, T defaultGameState = null, string replayId = null, Lock lockObj = null, AutoComplete autoComplete = null) where T : class;
        Task<List<GameSession>> FindSessions(string gameId, string gameMode, string environment, string operatorId, string operatorPlayerId, string operatorSessionId = null);

        Task<GameSession> GetSession(string sessionId);
        Task<GameSession> GetSessionWithLock(string sessionId, string lockId, DateTime lockTimeout);
        Task<GameSession> UpdateOperatorPlayerSession(string sessionId, string playerId, string operatorSessionId);
        Task<SaveSessionResult> UpdateSessionState(string sessionId, string fieldPath, object fieldValue, string lockId = null);

        Task<SaveSessionResult> SaveSession(string sessionId, Dictionary<string, object> gameState, string lockId = null);
        Task<SaveSessionResult> SaveSession<T>(string sessionId, T gameState, string lockId = null) where T : class;
        Task<SaveSessionResult> SaveSession(string sessionId, string gameStateJson, string lockId = null);

        Task<SaveSessionResult> SaveSessionForReplay(string sessionId, Dictionary<string, object> gameState, string replayId, string lockId = null);
        Task<SaveSessionResult> SaveSessionForReplay<T>(string sessionId, T gameState, string replayId, string lockId = null) where T : class;
        Task<SaveSessionResult> SaveSessionForReplay(string sessionId, string gameStateJson, string replayId, string lockId = null);

        Task<bool> AddPlayerToGameSession(string sessionId, Player player);
        Task<bool> RemovePlayerFromSession(string sessionId, string playerId);
        Task<bool> EndSession(string sessionId);
    }

    internal class Session : ISession
    {
        private const string URL_SESSION = "https://cloud.fireballserver.com/sessions/session/";
        private const string URL_SESSION_FIND = URL_SESSION + "find";
        private const string URL_SESSION_STATE = URL_SESSION + "state/v2";
        private const string URL_SESSION_END = URL_SESSION + "end";
        private const string URL_SESSION_OPERATOR = URL_SESSION + "operator";

        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;

        public Session(ICommunicator communicator, ILogger<Session> logger)
        {
            _logger = new FireballLogger(nameof(Session), logger);
            _communicator = communicator;
        }

        public async Task<GameSession> GetOrCreateSession(IntegrationSessionMessage integrationSession)
        {
            return await GetOrCreateSession(integrationSession, string.Empty);
        }
        public async Task<GameSession> GetOrCreateSession(IntegrationSessionMessage integrationSession, Dictionary<string, object> defaultGameState)
        {
            return await GetOrCreateSession(integrationSession, defaultGameState != null ? JsonConvert.SerializeObject(defaultGameState) : null);
        }
        public async Task<GameSession> GetOrCreateSession<T>(IntegrationSessionMessage integrationSession, T defaultGameState) where T : class
        {
            return await GetOrCreateSession(integrationSession, defaultGameState != null ? JsonConvert.SerializeObject(defaultGameState) : null);
        }
        public async Task<GameSession> GetOrCreateSession(IntegrationSessionMessage integrationSession, string defaultGameStateJson)
        {
            var request = new GetSessionMessage()
            {
                Environment = integrationSession.Environment,
                OperatorId = integrationSession.OperatorId,
                GameId = integrationSession.GameId,
                OperatorPlayerId = integrationSession.OperatorPlayerId,
                OperatorPlayerSession = integrationSession.OperatorPlayerSession,
                Type = integrationSession.GameMode,
                GameState = !string.IsNullOrEmpty(defaultGameStateJson) ? defaultGameStateJson : null,
            };
            _logger.LogDebug($"GetOrCreateSession: {request.ToJson()}");

            var result = await _communicator.Post<GameSessionResult>(URL_SESSION, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response?.Session;
            }
            return null;
        }

        public async Task<GameSession> CreateSession<T>(string gameId, string gameMode, string environment, List<Player> players, T defaultGameState = null, string replayId = null, Lock lockObj = null, AutoComplete autoComplete = null) where T : class
        {
            var gameStateJson = defaultGameState != null ? JsonConvert.SerializeObject(defaultGameState) : null;
            var request = new CreateSessionsMessage()
            {
                GameId = gameId,
                GameMode = gameMode,
                Environment = environment,
                GameState = gameStateJson,
                Players = players,
                ReplayId = replayId,
                Lock = lockObj,
                AutoComplete = autoComplete,
            };
            _logger.LogDebug($"CreateSession: {request.ToJson()}");
            var result = await _communicator.Post<GameSessionResult>(URL_SESSION, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response?.Session;
            }
            return null;
        }

        public async Task<List<GameSession>> FindSessions(string gameId, string gameMode, string environment, string operatorId, string operatorPlayerId, string operatorSessionId = null)
        {
            var request = new FindSessionsMessage()
            {
                GameId = gameId,
                GameMode = gameMode,
                Environment = environment,
                OperatorId = operatorId,
                OperatorPlayerId = operatorPlayerId,
                OperatorSessionId = operatorSessionId,
            };
            _logger.LogDebug($"FindSessions: {request.ToJson()}");
            var result = await _communicator.Post<FindSessionsResult>(URL_SESSION_FIND, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response?.Sessions;
            }
            return null;
        }


        public async Task<GameSession> GetSession(string sessionId)
        {
            _logger.LogDebug($"GetSession: {sessionId}");
            var result = await _communicator.Get<GameSessionResult>(URL_SESSION + sessionId);
            if (result.IsSuccess)
            {
                return result?.Response?.Session;
            }
            return null;
        }
        public async Task<GameSession> GetSessionWithLock(string sessionId, string lockId, DateTime lockTimeout)
        {
            _logger.LogDebug($"GetSession: {sessionId}, lockId = {lockId}");
            var result = await _communicator.Get<GameSessionResult>(URL_SESSION + $"{sessionId}/{lockId}/{lockTimeout.ToString("o")}");
            if (result.IsSuccess)
            {
                return result?.Response?.Session;
            }
            return null;
        }
        public async Task<GameSession> UpdateOperatorPlayerSession(string sessionId, string playerId, string operatorSessionId)
        {
            var request = new UpdateOperatorPlayerSessionMessage()
            {
                SessionId = sessionId,
                PlayerId = playerId,
                OperatorSessionId = operatorSessionId,
            };
            var result = await _communicator.Patch<GameSession>(URL_SESSION_OPERATOR, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }
        public async Task<SaveSessionResult> UpdateSessionState(string sessionId, string fieldPath, object fieldValue, string lockId = null)
        {
            if (!fieldPath.StartsWith("$."))
            {
                fieldPath = "$." + fieldPath;
            }

            var request = new UpdateSessionStateMessage()
            {
                SessionId = sessionId,
                GameStateFieldPath = fieldPath,
                GameStateFieldObject = fieldValue,
                LockId = lockId,
            };
            _logger.LogDebug($"UpdateSessionState: {request.ToJson()}");

            var result = await _communicator.Patch<SaveSessionResult>(URL_SESSION_STATE, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<SaveSessionResult> SaveSession(string sessionId, Dictionary<string, object> gameState, string lockId = null)
        {
            return await SaveSessionInternal(sessionId, gameState != null ? JsonConvert.SerializeObject(gameState) : null, null, lockId);
        }
        public async Task<SaveSessionResult> SaveSession<T>(string sessionId, T gameState, string lockId = null) where T : class
        {
            return await SaveSessionInternal(sessionId, gameState != null ? JsonConvert.SerializeObject(gameState) : null, null, lockId);
        }
        public async Task<SaveSessionResult> SaveSession(string sessionId, string gameStateJson, string lockId = null)
        {
            return await SaveSessionInternal(sessionId, gameStateJson, null, lockId);
        }

        public async Task<SaveSessionResult> SaveSessionForReplay(string sessionId, Dictionary<string, object> gameState, string replayId, string lockId = null)
        {
            return await SaveSessionInternal(sessionId, gameState != null ? JsonConvert.SerializeObject(gameState) : null, replayId, lockId);
        }
        public async Task<SaveSessionResult> SaveSessionForReplay<T>(string sessionId, T gameState, string replayId, string lockId = null) where T : class
        {
            return await SaveSessionInternal(sessionId, gameState != null ? JsonConvert.SerializeObject(gameState) : null, replayId, lockId);
        }
        public async Task<SaveSessionResult> SaveSessionForReplay(string sessionId, string gameStateJson, string replayId, string lockId = null)
        {
            return await SaveSessionInternal(sessionId, gameStateJson, replayId, lockId);
        }
        public async Task<bool> AddPlayerToGameSession(string sessionId, Player player)
        {
            // TODO 
            var result = await _communicator.Post<GameSessionResult>(URL_SESSION + $"{sessionId}/add", JsonConvert.SerializeObject(player));
            if (result != null && result.IsSuccess)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> RemovePlayerFromSession(string sessionId, string playerId)
        {
            var result = await _communicator.Patch<GameSessionResult>(URL_SESSION + $"{sessionId}/remove/{playerId}", string.Empty);
            if (result != null && result.IsSuccess)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> EndSession(string sessionId)
        {
            var request = new EndSessionMessage()
            {
                SessionId = sessionId,
            };
            var result = await _communicator.Patch<string>(URL_SESSION_END, request.ToJson());
            if (result != null && result.IsSuccess)
            {
                return true;
            }
            return false;
        }

        private async Task<SaveSessionResult> SaveSessionInternal(string sessionId, string gameStateJson, string replayId = null, string lockId = null)
        {
            var request = new SaveSessionMessage()
            {
                SessionId = sessionId,
                GameState = !string.IsNullOrEmpty(gameStateJson) ? gameStateJson : null,
                ReplayId = replayId,
                LockId = lockId,
            };
            _logger.LogDebug($"SaveSession: {request.ToJson()}");

            var result = await _communicator.Patch<SaveSessionResult>(URL_SESSION, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }
    }
}
