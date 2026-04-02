using System.Collections.Generic;
using System.Threading.Tasks;
using Fireball.Game.Server.Models;
using Fireball.Game.Server.SessionModule;
using Microsoft.Extensions.Logging;

namespace Fireball.Game.Server.JackpotsModule
{
    public interface IJackpots
    {
        Task<JackpotDetailsResult> GetDetails(List<string> jackpotTemplateIds, string currency, string operatorId, string environment);
        Task<JackpotContributeResult> Contribute(string jackpotTemplateId, long contributionAmount, BaseMessage message);
        Task<JackpotOutcomeResult> GetOutcome(string jackpotTemplateId, long contributionAmount, BaseMessage message, bool forceWin = false);
        Task<JackpotOutcomeResult> ContributeAndGetOutcome(string jackpotTemplateId, long contributionAmount, BaseMessage message, bool forceWin = false);
    }

    internal class Jackpots : IJackpots
    {
        private const string URL_JACKPOTS = "https://cloud.fireballserver.com/jackpots";
        private const string URL_CONTRIBUTE = URL_JACKPOTS + "/contribute";
        private const string URL_OUTCOME = URL_JACKPOTS + "/outcome";
        private const string URL_CONTRIBUTE_AND_OUTCOME = URL_JACKPOTS + "/contribute/outcome";
        private const string URL_DETAILS = URL_JACKPOTS + "/details";

        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;

        public Jackpots(ICommunicator communicator, ILogger<Jackpots> logger)
        {
            _logger = new FireballLogger(nameof(Jackpots), logger);
            _communicator = communicator;
        }

        public async Task<JackpotDetailsResult> GetDetails(List<string> jackpotTemplateIds, string currency, string operatorId, string environment)
        {
            var request = new JackpotsDetailsMessage()
            {
                Environment = environment,
                OperatorId = operatorId,
                JackpotTemplateIds = jackpotTemplateIds,
                PlayerCurrency = currency,
            };
            _logger.LogDebug($"Get Details: {request.ToJson()}");

            var result = await _communicator.Post<JackpotDetailsResult>(URL_DETAILS, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<JackpotContributeResult> Contribute(string jackpotTemplateId, long contributionAmount, BaseMessage message)
        {
            var request = new JackpotContributeMessage()
            {
                Environment = message.Environment,
                OperatorId = message.OperatorId,
                GameId = message.GameId,
                PlayerId = message.PlayerId,
                GameSession = message.GameSession,
                OperatorPlayerSession = message.OperatorPlayerSession,
                PlayerCurrency = message.Currency,
                ContributionAmount = contributionAmount,
                TemplateId = jackpotTemplateId,
            };
            _logger.LogDebug($"Contribute: {request.ToJson()}");

            var result = await _communicator.Patch<JackpotContributeResult>(URL_CONTRIBUTE, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<JackpotOutcomeResult> GetOutcome(string jackpotTemplateId, long contributionAmount, BaseMessage message, bool forceWin = false)
        {
            var request = new JackpotOutcomeMessage()
            {
                Environment = message.Environment,
                OperatorId = message.OperatorId,
                GameId = message.GameId,
                PlayerId = message.PlayerId,
                GameSession = message.GameSession,
                OperatorPlayerSession = message.OperatorPlayerSession,
                PlayerCurrency = message.Currency,
                ContributionAmount = contributionAmount,
                TemplateId = jackpotTemplateId,
                ForceWin = forceWin,
            };
            _logger.LogDebug($"Outcome: {request.ToJson()}");

            var result = await _communicator.Patch<JackpotOutcomeResult>(URL_OUTCOME, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<JackpotOutcomeResult> ContributeAndGetOutcome(string jackpotTemplateId, long contributionAmount, BaseMessage message, bool forceWin = false)
        {
            var request = new JackpotOutcomeMessage()
            {
                Environment = message.Environment,
                OperatorId = message.OperatorId,
                GameId = message.GameId,
                PlayerId = message.PlayerId,
                GameSession = message.GameSession,
                OperatorPlayerSession = message.OperatorPlayerSession,
                PlayerCurrency = message.Currency,
                ContributionAmount = contributionAmount,
                TemplateId = jackpotTemplateId,
                ForceWin = forceWin,
            };
            _logger.LogDebug($"Contribute and get Outcome: {request.ToJson()}");

            var result = await _communicator.Patch<JackpotOutcomeResult>(URL_CONTRIBUTE_AND_OUTCOME, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }
    }
}
