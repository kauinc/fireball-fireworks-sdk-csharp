using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fireball.Fireworks.Models;
using Microsoft.Extensions.Logging;

namespace Fireball.Fireworks.MultiplayerModule
{
    internal interface IMatchMaker
    {
        Task<List<BetTier>> GetBetTiers(string currency);
        Task<PlayerMatchesResponse> GetPlayerMatches(string operatorPlayerId, BaseMessage playerMessage);
        Task<MessageResult> AddPlayer(AddPlayerRequest playerMessage);
        Task<MessageResult> RemovePlayer(CancelPlayerRequest playerMessage);

        Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, DateTime callbackTime) where T : BaseMessage;
        Task<MessageResult> DeleteCallback(string environment, string gameId, string callbackId);
    }

    internal class MatchMaker : IMatchMaker
    {
        private const string URL_MATCHMAKER = "https://cloud.fireballserver.com/matchmaker";
        private const string URL_BET_TIERS = FireballConstants.URL_FIREBALL_SERVER_API + "/bet-tiers";
        private const string URL_PLAYERS = URL_MATCHMAKER + "/player";
        private const string URL_SCHEDULER = URL_MATCHMAKER + "/callback";

        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;

        public MatchMaker(ICommunicator communicator, ILogger<MatchMaker> logger)
        {
            _logger = new FireballLogger(nameof(MatchMaker), logger);
            _communicator = communicator;
        }

        public async Task<List<BetTier>> GetBetTiers(string currency)
        {
            _logger.LogDebug($"GetBetTiers: currency = {currency}");
            var result = await _communicator.Get<BetTiersData>(URL_BET_TIERS + $"?currencyIsoCode={currency}");
            if (result.IsSuccess)
            {
                var betTierList = result.Response?.Data?.Select(b => b.ToBetTier())?.ToList();
                if (betTierList != null)
                {
                    betTierList = betTierList.OrderBy(b => b.ValueDefault).ToList();
                    betTierList.ForEach(b => b.Tier = betTierList.IndexOf(b) + 1);
                    return betTierList;
                }
            }
            return new List<BetTier>();
        }

        public async Task<PlayerMatchesResponse> GetPlayerMatches(string operatorPlayerId, BaseMessage playerMessage)
        {
            _logger.LogDebug($"CheckPlayer: operatorPlayerId = {operatorPlayerId}, message = {playerMessage.ToJson()}");
            var result = await _communicator.Get<PlayerMatchesResponse>(URL_PLAYERS +
                $"?{nameof(BaseMessage.Environment)}={playerMessage.Environment}" +
                $"&{nameof(BaseMessage.GameMode)}={playerMessage.GameMode}" + 
                $"&{nameof(BaseMessage.OperatorId)}={playerMessage.OperatorId}" + 
                $"&{nameof(BaseMessage.GameId)}={playerMessage.GameId}" + 
                $"&{nameof(BaseMessage.OperatorPlayerId)}={playerMessage.OperatorPlayerId}");

            if (result.IsSuccess)
            {
                return result.Response;
            }
            return new PlayerMatchesResponse();
        }

        public async Task<MessageResult> AddPlayer(AddPlayerRequest playerMessage)
        {
            _logger.LogDebug($"AddPlayer: {playerMessage.ToJson()}");
            var result = await _communicator.Post<string>(URL_PLAYERS, playerMessage.ToJson());
            if (result.IsSuccess)
            {
                return MessageResult.SuccessResult(result.Response);
            }
            return MessageResult.ErrorResult(result.Error?.errorMessage);
        }

        public async Task<MessageResult> RemovePlayer(CancelPlayerRequest playerMessage)
        {
            _logger.LogDebug($"RemovePlayer: {playerMessage.ToJson()}");
            var result = await _communicator.Delete<string>(URL_PLAYERS, playerMessage.ToJson());
            if (result.IsSuccess)
            {
                return MessageResult.SuccessResult(result.Response);
            }
            return MessageResult.ErrorResult(result.Error?.errorMessage);
        }



        public async Task<MessageResult> ScheduleCallback<T>(T message, string callbackId, DateTime callbackTime) where T : BaseMessage
        {
            var scheduleMessage = new ScheduleCallbackMessage<T>(message, callbackId, callbackTime);

            _logger.LogDebug($"Schedule Callback: {scheduleMessage.ToJson()}");
            var result = await _communicator.Post<string>(URL_SCHEDULER, scheduleMessage.ToJson());
            if (result.IsSuccess)
            {
                return MessageResult.SuccessResult(result?.Response);
            }
            return MessageResult.ErrorResult(result?.Error?.errorMessage);
        }

        public async Task<MessageResult> DeleteCallback(string environment, string gameId, string callbackId)
        {
            var deleteMessage = new DeleteCallbackMessage(environment, gameId, callbackId);

            _logger.LogDebug($"Delete Callback: {deleteMessage.ToJson()}");
            var result = await _communicator.Delete<string>(URL_SCHEDULER, deleteMessage.ToJson());
            if (result.IsSuccess)
            {
                return MessageResult.SuccessResult(result?.Response);
            }
            return MessageResult.ErrorResult(result?.Error?.errorMessage);
        }
    }
}

