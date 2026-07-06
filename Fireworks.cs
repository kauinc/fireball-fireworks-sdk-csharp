using Fireball.Fireworks.Core;
using Fireball.Fireworks.IntegrationModule;
using Fireball.Fireworks.JackpotsModule;
using Fireball.Fireworks.MessagesModule;
using Fireball.Fireworks.Models;
using Fireball.Fireworks.MultiplayerModule;
using Fireball.Game.Server.Rng;
using Fireball.Fireworks.SessionModule;
using Fireball.Fireworks.TestsModule;
using Fireball.Fireworks.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fireball.Fireworks
{
    public class MessageValidationResult
    {
        public bool IsValid { get; set; }
        public ErrorMessage Error { get; set; }
        public MessageResult ErrorSentResult { get; set; }
    }

    public class FireballConfigs
    {
        public bool AutoDisconnect = true;

        public static FireballConfigs Default => new FireballConfigs();
    }

    public interface IFireworks
    {
        string Environment { get; }

        Task<ParseResult> ParseMessage(string messageJson);
        Task<ParseResult> ParseMessage(string messageJson, FireballConfigs configs);
        Task<MessageValidationResult> ValidateMessage<T>(T message, bool sendErrorToClient = true, string errorName = FireballConstants.MessagesNames.ERROR) where T : BaseMessage;

        Task<MessageResult> Authenticate(AuthMessage message);
        Task<MessageResult> BalanceRequest(BaseMessage message);
        Task<BetResult> PlaceBet(string betType, long amount, BaseMessage message, ParentBet parentBet = null, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null);
        Task<BetResult> PlaceFreeBet(string betType, long amount, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null);
        Task<BetResult> PlaceFreeBetBonus(string betType, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null);

        Task<WinResult> PayWin(string winningType, string operatorBetId, long amount, BaseMessage message, bool noResponse = false, ParentBet parentBet = null, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false);
        Task<WinResult> PayFreeBet(string winningType, string operatorBetId, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, bool noResponse = false, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false);
        Task<WinResult> PayFreeBetBonus(string winningType, string operatorBetId, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, bool noResponse = false, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false);
        Task<WinResult> PayJackpot(List<JackpotEntry> jackpotsEntries, string operatorBetId, BaseMessage message, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null);
        Task<MessageResult> PayDisplay(string displayId, BaseMessage message);

        Task<GameSession> GetGameSession(string gameSessionId);
        Task<List<GameSession>> GetAllGameSessions(BaseMessage message);
        Task<GameSession> CreatePermanentGameSession<T>(T gameState, BaseMessage message) where T : class;
        Task<GameSession> CreateTimedGameSession<T>(T gameState, DateTime timeEnd, BaseMessage message) where T : class;
        Task<bool> CloseGameSession(string sessionId);

        Task<T> GetGameState<T>(string gameSessionId) where T : class;
        Task<bool> UpdateGameState(string gameSessionId, string fieldPath, object fieldValue);
        Task<bool> SaveGameState<T>(string gameSessionId, T gameState) where T : class;
        Task<bool> SaveGameStateForReplay<T>(string gameSessionId, T gameState, string replayId) where T : class;

        Task<MessageResult> SendMessageToClient<T>(T message, bool includeClientVars = false, bool includeServerVars = false) where T : BaseMessage;
        Task<MessageResult> SendSessionToClient<T>(T message, List<string> jackpotTemplateIds = null, bool includeClientVars = true, bool includeServerVars = false) where T : SessionMessage;
        Task<MessageResult> SendErrorToClient<T>(T error, ErrorCode code = ErrorCode.Default) where T : ErrorMessage;
        Task<MessageResult> SendDisconnect(IntegrationDisconnectMessage message, bool integrationEndSession = false);
    }

    internal class Fireworks : IFireworks
    {
        private readonly IFireballLogger _logger;
        private readonly IMessenger _messenger;
        private readonly IIntegration _integration;
        private readonly ISession _session;

        public Fireworks(IIntegration integration, ISession session, IMessenger messenger, ILogger<Fireworks> logger)
        {
            _integration = integration;
            _session = session;
            _messenger = messenger;
            _logger = new FireballLogger("Fireball", logger);
        }

        public string Environment =>
            System.Environment.GetEnvironmentVariable(FireballConstants.Environment.ENV)?.ToLower()
            ?? FireballConstants.Environment.DEVELOPMENT;

        public async Task<ParseResult> ParseMessage(string messageJson)
        {
            return await ParseMessage(messageJson, FireballConfigs.Default);
        }
        public async Task<ParseResult> ParseMessage(string messageJson, FireballConfigs configs)
        {
            if (string.IsNullOrEmpty(messageJson))
            {
                _logger.LogError("Parse Message: Message is empty or null");
                return null;
            }

            if (configs == null)
            {
                configs = FireballConfigs.Default;
            }

            if (TryParseMessage(messageJson, out ParseResult result))
            {
                CheckMaxAge(result);

                if (result.IsSuccess)
                {
                    _logger.LogInfo($"Message - {result.MessageName} - received");
                    if (result.MessageName == FireballConstants.MessagesNames.SESSION)
                    {
                        await CheckSession(result);
                    }
                    else if (result.MessageName == FireballConstants.MessagesNames.DISCONNECTED)
                    {
                        if (configs.AutoDisconnect)
                        {
                            await SendDisconnect(result.ToMessage<IntegrationDisconnectMessage>());
                        }
                    }
                    else if (result.MessageName == FireballConstants.MessagesNames.WARNING)
                    {
                        var warning = result.ToMessage<CoreWarning>();
                        if (warning != null)
                        {
                            _logger.LogWarning($"{warning.Sender} - {warning.Message}");
                        }
                    }
                }
            }
            return result;
        }
        public async Task<MessageValidationResult> ValidateMessage<T>(T message, bool sendErrorToClient = true, string errorName = FireballConstants.MessagesNames.ERROR) where T : BaseMessage
        {
            var validationResult = message.Validate();
            var messageValidation = new MessageValidationResult();
            messageValidation.IsValid = validationResult == ValidationResult.Success;
            if (!messageValidation.IsValid)
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");

                if (sendErrorToClient)
                {
                    messageValidation.Error = new ErrorMessage(errorName, ErrorCode.Validation, validationResult.ErrorMessage, message);
                    messageValidation.ErrorSentResult = await SendErrorToClient(messageValidation.Error);
                }
            }
            _logger.LogDebug($"Validation - {message.Name} - success!");
            return messageValidation;
        }

        #region INTEGRATION
        public async Task<MessageResult> Authenticate(AuthMessage message)
        {
            var validationResult = message.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {message.Name} - success!");
                return await _integration.Authenticate(new IntegrationAuthMessage(message));
            }
            else
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.AUTHENTICATE_REJECT, ErrorCode.Authentication, validationResult.ErrorMessage, message);
                return await SendErrorToClient(error);
            }
        }
        public async Task<MessageResult> BalanceRequest(BaseMessage message)
        {
            var integrationMessage = new IntegrationBalanceRequest(message);
            var validationResult = integrationMessage.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {integrationMessage.Name} - success!");
                return await _integration.BalanceRequest(integrationMessage);
            }
            else
            {
                _logger.LogError($"Validation - {integrationMessage.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.ERROR, ErrorCode.Validation, validationResult.ErrorMessage, message);
                return await SendErrorToClient(error);
            }
        }
        public async Task<BetResult> PlaceBet(string betType, long amount, BaseMessage message, ParentBet parentBet = null, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null)
        {
            if (parentBet == null && (amount == 0 || betType == FireballConstants.BetType.FREESPIN))
            {
                _logger.LogWarning("ParentBet object is required for Bets with zero amount for some operator!");
            }
            return await PlaceBetInternal(betType, amount, parentBet, null, message, roundId, jackpotContributions);
        }
        public async Task<BetResult> PlaceFreeBet(string betType, long amount, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null)
        {
            var details = new FreeBetDetails()
            {
                FreeBetCampaignId = freeBetCampaignId,
                FreeBetId = null,
                IsFreeBetCampaignOver = isCampaignOver,
            };
            return await PlaceBetInternal(betType, amount, null, details, message, roundId, jackpotContributions);
        }
        public async Task<BetResult> PlaceFreeBetBonus(string betType, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, List<JackpotContribution> jackpotContributions = null, Guid? roundId = null)
        {
            var details = new FreeBetDetails()
            {
                FreeBetCampaignId = freeBetCampaignId,
                FreeBetId = freeBetId,
                IsFreeBetCampaignOver = isCampaignOver,
            };
            return await PlaceBetInternal(betType, amount, null, details, message, roundId, jackpotContributions);
        }
        private async Task<BetResult> PlaceBetInternal(string betType, long amount, ParentBet parentBet, FreeBetDetails freeBetDetails, BaseMessage message, Guid? roundId = null, List<JackpotContribution> jackpotContributions = null)
        {
            var validationResult = message.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {message.Name} - success!");
                var integrationBet = new IntegrationBetPlace(betType, amount, message, parentBet, freeBetDetails, jackpotContributions, roundId);
                var result = await _integration.PlaceBet(integrationBet);
                return new BetResult(result, integrationBet.BetId, integrationBet.RoundId);
            }
            else
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.BET_PLACE_REJECTED, ErrorCode.Validation, validationResult.ErrorMessage, message);
                return new BetResult(await SendErrorToClient(error));
            }
        }
        public async Task<WinResult> PayWin(string winningType, string operatorBetId, long amount, BaseMessage message, bool noResponse = false, ParentBet parentBet = null, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false)
        {
            if (parentBet == null && winningType == FireballConstants.WinningType.FREESPIN)
            {
                _logger.LogWarning("ParentBet object is required for Bets with zero amount for some operator!");
            }
            return await PayWinInternal(winningType, operatorBetId, amount, null, message, noResponse, parentBet, displayDelay, betId, roundId, roundClosed);
        }
        public async Task<WinResult> PayFreeBet(string winningType, string operatorBetId, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, bool noResponse = false, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false)
        {
            var details = new FreeBetDetails()
            {
                FreeBetCampaignId = freeBetCampaignId,
                FreeBetId = freeBetId,
                IsFreeBetCampaignOver = isCampaignOver,
            };
            return await PayWinInternal(winningType, operatorBetId, amount, details, message, noResponse, null, displayDelay, betId, roundId, roundClosed);
        }
        public async Task<WinResult> PayFreeBetBonus(string winningType, string operatorBetId, long amount, string freeBetId, string freeBetCampaignId, bool isCampaignOver, BaseMessage message, bool noResponse = false, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false)
        {
            var details = new FreeBetDetails()
            {
                FreeBetCampaignId = freeBetCampaignId,
                FreeBetId = freeBetId,
                IsFreeBetCampaignOver = isCampaignOver,
            };
            return await PayWinInternal(winningType, operatorBetId, amount, details, message, noResponse, null, displayDelay, betId, roundId, roundClosed);
        }
        private async Task<WinResult> PayWinInternal(string winningType, string operatorBetId, long amount, FreeBetDetails freeBetDetails, BaseMessage message, bool noResponse = false, ParentBet parentBet = null, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null, bool roundClosed = false)
        {
            var validationResult = message.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {message.Name} - success!");
                var integrationWin = new IntegrationWinningPay(winningType, operatorBetId, amount, message, noResponse, parentBet, displayDelay, freeBetDetails, betId, roundId, roundClosed);
                var result = await _integration.PayWin(integrationWin);
                return new WinResult(result, integrationWin.WinId, integrationWin.BetId, integrationWin.RoundId);
            }
            else
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.WINNING_PAY_REJECTED, ErrorCode.Validation, validationResult.ErrorMessage, message);
                return new WinResult(await SendErrorToClient(error));
            }
        }
        #endregion INTEGRATION

        #region JACKPOT
        public async Task<WinResult> PayJackpot(List<JackpotEntry> jackpotsEntries, string operatorBetId, BaseMessage message, DisplayDelay displayDelay = null, Guid? betId = null, Guid? roundId = null)
        {
            var validationResult = message.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {message.Name} - success!");
                var integrationJackpot = new IntegrationJackpotPay(jackpotsEntries, operatorBetId, message, displayDelay, betId, roundId);
                var result = await _integration.PayJackpot(integrationJackpot);
                return new WinResult(result, integrationJackpot.WinId, integrationJackpot.BetId, integrationJackpot.RoundId);
            }
            else
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.WINNING_PAY_REJECTED, ErrorCode.Validation, validationResult.ErrorMessage, message);
                return new WinResult(await SendErrorToClient(error));
            }
        }
        public async Task<MessageResult> PayDisplay(string displayId, BaseMessage message)
        {
            var validationResult = message.Validate();
            if (validationResult == ValidationResult.Success)
            {
                _logger.LogDebug($"Validation - {message.Name} - success!");
                return await _integration.PayDisplay(new IntegrationPayDisplay(displayId, message));
            }
            else
            {
                _logger.LogError($"Validation - {message.Name} - error: {validationResult.ErrorMessage}");
                var error = new ErrorMessage(FireballConstants.MessagesNames.ERROR, ErrorCode.Validation, validationResult.ErrorMessage, message);
                return await SendErrorToClient(error);
            }
        }
        #endregion JACKPOT

        #region SESSION
        public async Task<GameSession> GetGameSession(string gameSessionId)
        {
            return await _session.GetSession(gameSessionId);
        }
        public async Task<List<GameSession>> GetAllGameSessions(BaseMessage message)
        {
            return await _session.FindSessions(
                message.GameId,
                message.GameMode,
                message.Environment,
                message.OperatorId,
                message.OperatorPlayerId);
        }
        public async Task<GameSession> CreatePermanentGameSession<T>(T gameState, BaseMessage message) where T : class
        {
            var player = new Player()
            {
                PlayerId = null,
                OperatorId = message.OperatorId,
                OperatorPlayerId = message.OperatorPlayerId,
                OperatorSessionId = message.OperatorPlayerSession,
            };

            return await _session.CreateSession<T>(
                message.GameId,
                message.GameMode,
                message.Environment,
                new List<Player>() { player },
                gameState,
                message.ReplayId);
        }
        public async Task<GameSession> CreateTimedGameSession<T>(T gameState, DateTime timeEnd, BaseMessage message) where T : class
        {
            var player = new Player()
            {
                PlayerId = null,
                OperatorId = message.OperatorId,
                OperatorPlayerId = message.OperatorPlayerId,
                OperatorSessionId = message.OperatorPlayerSession,
            };

            return await _session.CreateSession<T>(
                message.GameId,
                message.GameMode,
                message.Environment,
                new List<Player>() { player },
                gameState,
                message.ReplayId,
                null,
                new AutoComplete(timeEnd, null) { });
        }
        public async Task<bool> CloseGameSession(string sessionId)
        {
            _logger.LogDebug($"CloseGameSession: {sessionId}");
            return await _session.EndSession(sessionId);
        }
        #endregion SESSION

        #region GAME STATE
        public async Task<T> GetGameState<T>(string gameSessionId) where T : class
        {
            var session = await GetGameSession(gameSessionId);
            return session?.ParseGameState<T>();
        }
        public async Task<bool> UpdateGameState(string gameSessionId, string fieldPath, object fieldValue)
        {
            var result = await _session.UpdateSessionState(gameSessionId, fieldPath, fieldValue);
            return result != null;
        }
        public async Task<bool> SaveGameState<T>(string gameSessionId, T gameState) where T : class
        {
            var result = await _session.SaveSession(gameSessionId, JsonConvert.SerializeObject(gameState, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            return result != null;
        }
        public async Task<bool> SaveGameStateForReplay<T>(string gameSessionId, T gameState, string replayId) where T : class
        {
            var result = await _session.SaveSessionForReplay(gameSessionId,
                JsonConvert.SerializeObject(gameState, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                replayId);
            return result != null;
        }
        #endregion GAME STATE

        #region MESSENGER
        public async Task<MessageResult> SendMessageToClient<T>(T message, bool includeClientVars = false, bool includeServerVars = false) where T : BaseMessage
        {
            if (message.Name == FireballConstants.MessagesNames.SESSION)
            {
                _logger.LogWarning($"Tried to send {message.Name} messageto Client... Please, use {nameof(SendSessionToClient)} method for this");
            }

            _logger.LogInfo($"Message - {message.Name} - send to Client: {message.ToJson()}");
            var result = await _messenger.SendMessage(message, null, includeClientVars, includeServerVars);
            _logger.LogDebug($"Message - {message.Name} - sent result: {result}");

            return result;
        }
        public async Task<MessageResult> SendSessionToClient<T>(T message, List<string> jackpotTemplateIds = null, bool includeClientVars = true, bool includeServerVars = false) where T : SessionMessage
        {
            _logger.LogInfo($"Message - {message.Name} - send to Client: {message.ToJson()}");
            var result = await _messenger.SendSession(message, jackpotTemplateIds, includeClientVars, includeServerVars);
            _logger.LogDebug($"Message - {message.Name} - sent result: {result}");

            await CheckOperatorPlayerSession(message, null);
            return result;
        }
        public async Task<MessageResult> SendErrorToClient<T>(T error, ErrorCode code = ErrorCode.Default) where T : ErrorMessage
        {
            if (code != ErrorCode.Default)
            {
                error.Code = code;
            }
            _logger.LogError($"Error Message - {error.Name} - send to Client: {error.ToJson()}");
            var result = await _messenger.SendMessage(error);
            _logger.LogDebug($"Error Message - {error.Name} - sent result: {result}");

            return result;
        }
        public async Task<MessageResult> SendDisconnect(IntegrationDisconnectMessage message, bool integrationEndSession = false)
        {
            if (integrationEndSession)
                return await _integration.EndSession(new IntegrationEndSessionMessage(message));

            return await _integration.DisconnectPlayer(message);
        }
        #endregion MESSENGER

        #region INTERNAL
        private bool TryParseMessage(string messageJson, out ParseResult result)
        {
            try
            {
                JObject jObject = JObject.Parse(messageJson);
                string messageName =
                    jObject[nameof(BaseMessage.Name)] != null ?
                    jObject[nameof(BaseMessage.Name)].ToString() :
                    jObject[nameof(BaseMessage.Name).ToLower()]?.ToString();

                string gameSession =
                    jObject[nameof(BaseMessage.GameSession)] != null ?
                    jObject[nameof(BaseMessage.GameSession)].ToString() :
                    jObject[nameof(BaseMessage.GameSession).ToLower()]?.ToString();

                long? timestamp =
                    jObject[nameof(BaseMessage.MessageTimestamp)] != null ?
                    jObject[nameof(BaseMessage.MessageTimestamp)].ToObject<long>() :
                    jObject[nameof(BaseMessage.MessageTimestamp).ToLower()]?.ToObject<long>();

                result = new ParseResult(messageName, gameSession, timestamp, jObject, _logger);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Parse Message: Error = {e.Message}");
                result = new ParseResult(string.Empty, null, null, null, _logger);
                return false;
            }
        }
        private void CheckMaxAge(ParseResult result)
        {
            try
            {
                if (IsMessageMaxAge(result))
                {
                    result.MessageName = FireballConstants.MessagesNames.ERROR;
                    result.MessageObject = null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Check Max Age: Error = {e.Message}");
            }
        }
        private async Task CheckSession(ParseResult result)
        {
            GameSession gameSession = null;
            var integrationSession = result.ToMessage<IntegrationSessionMessage>();
            var gameSessions = await _session.FindSessions(
                integrationSession.GameId,
                integrationSession.GameMode,
                integrationSession.Environment,
                integrationSession.OperatorId,
                integrationSession.OperatorPlayerId);

            if (gameSessions != null && gameSessions.Count > 0)
            {
                // getting the newest game session
                _logger.LogDebug($"Game sessions count = {gameSessions.Count}");
                gameSession = gameSessions.OrderByDescending(s => s.CreatedAt).First();
                _logger.LogDebug($"Newest gameSession = {gameSession.Id}");
            }
            else
            {
                _logger.LogWarning($"No game sessions found");
            }

            try
            {
                // crete new sesion message
                var sessionMessage = new SessionMessage();
                sessionMessage.CopyBaseParams(integrationSession);

                // set integration data
                sessionMessage.Name = FireballConstants.MessagesNames.SESSION;
                sessionMessage.Balance = integrationSession.Balance;
                sessionMessage.Multiplier = integrationSession.Multiplier;
                sessionMessage.FreeBetCampaigns = integrationSession.FreeBetCampaigns;

                // set game session data
                if (gameSession != null)
                {
                    var playerId = gameSession?.GetPlayer(integrationSession.OperatorPlayerId)?.PlayerId;
                    if (playerId != null)
                    {
                        sessionMessage.PlayerId = playerId;
                    }

                    sessionMessage.GameSession = gameSession?.Id;
                    sessionMessage.GameState = gameSession?.ParseGameState();
                }


                // convert new session message into parse result object
                result.MessageName = FireballConstants.MessagesNames.SESSION;
                result.MessageObject = JObject.Parse(sessionMessage.ToJson());
            }
            catch (Exception e)
            {
                _logger.LogError($"CheckSession Exception: {e}");
            }
        }
        private async Task CheckOperatorPlayerSession(SessionMessage sessionMessage, GameSession gameSession = null)
        {
            if (string.IsNullOrEmpty(sessionMessage.OperatorPlayerSession) ||
                string.IsNullOrEmpty(sessionMessage.GameSession) ||
                string.IsNullOrEmpty(sessionMessage.PlayerId))
                return;

            if (gameSession == null)
            {
                gameSession = await _session.GetSession(sessionMessage.GameSession);
            }

            var existedOperatorSessionId = gameSession?.GetPlayer(sessionMessage.PlayerId)?.OperatorSessionId;
            if (sessionMessage.OperatorPlayerSession != existedOperatorSessionId)
            {
                await _session.UpdateOperatorPlayerSession(sessionMessage.GameSession, sessionMessage.PlayerId, sessionMessage.OperatorPlayerSession);
            }
        }
        private bool IsMessageMaxAge(ParseResult result, long maxAge_ms = 300000)
        {
            if (result?.MessageTimestamp != null && result.MessageTimestamp.Value > 0)
            {
                long messageAge = FireballTools.TimestampNow() - result.MessageTimestamp.Value;
                if (messageAge > maxAge_ms)
                {
                    _logger.LogError($"Message - {result.MessageName} - discarded due max age!");
                    return true;
                }
                else
                {
                    _logger.LogDebug($"Message age = {(messageAge) * 0.001f} sec");
                }
            }
            else
            {
                _logger.LogDebug("Max Age Check: message timestamp is empty");
            }

            return false;
        }
        #endregion INTERNAL
    }

    public static class FireballTools
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }
        public static long TimestampNow(bool milliseconds = true)
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            if (milliseconds)
            {
                return (long)timeSpan.TotalMilliseconds;
            }
            return (long)timeSpan.TotalSeconds;
        }
        public static long GetTimestamp(DateTime dateTime, bool milliseconds = true)
        {
            var timeSpan = dateTime - new DateTime(1970, 1, 1, 0, 0, 0);
            if (milliseconds)
            {
                return (long)timeSpan.TotalMilliseconds;
            }
            return (long)timeSpan.TotalSeconds;
        }
    }

    public static class FireballServerExtensions
    {
        public static IServiceCollection AddFireworks(this IServiceCollection services)
        {
            services.AddRngDependencies();

            var retryPolicy = HttpPolicyExtensions
                      .HandleTransientHttpError()
                      .Or<TimeoutRejectedException>()
                      .Or<TaskCanceledException>()
                      .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(retryCount));

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);
            var wrappedPolicy = retryPolicy.WrapAsync(timeoutPolicy);

            services.AddHttpClient(Communicator.CLIENT_NAME,
                client =>
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("X-SERVICE-ACCOUNT", "optional_for_now");
                })
                .AddHttpMessageHandler<GoogleAccessTokenHandler>()
                .AddPolicyHandler(wrappedPolicy);
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

            services.AddSingleton<IFireworks, Fireworks>();
            services.AddSingleton<IIntegration, Integration>();
            services.AddSingleton<ISession, Session>();
            services.AddSingleton<IMessenger, Messenger>();
            services.AddSingleton<IJackpots, Jackpots>();
            services.AddSingleton<IMultiplayer, Multiplayer>();
            services.AddSingleton<IMatchMaker, MatchMaker>();
            services.AddSingleton<ITester, Tester>();
            services.AddSingleton<ICommunicator, Communicator>();
            services.AddTransient<GoogleAccessTokenHandler>();

            return services;
        }
    }
}