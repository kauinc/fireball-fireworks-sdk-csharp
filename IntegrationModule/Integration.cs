using System.Threading.Tasks;
using Fireball.Fireworks.Core;
using Fireball.Fireworks.Models;
using Microsoft.Extensions.Logging;

namespace Fireball.Fireworks.IntegrationModule
{
    internal interface IIntegration
    {
        Task<MessageResult> Authenticate(IntegrationAuthMessage integrationMessage);
        Task<MessageResult> BalanceRequest(IntegrationBalanceRequest integrationMessage);
        Task<MessageResult> PlaceBet(IntegrationBetPlace integrationMessage);
        Task<MessageResult> PayWin(IntegrationWinningPay integrationMessage);
        Task<MessageResult> PayJackpot(IntegrationJackpotPay integrationMessage);
        Task<MessageResult> PayDisplay(IntegrationPayDisplay integrationMessage);
        Task<MessageResult> DisconnectPlayer(IntegrationDisconnectMessage integrationMessage);
        Task<MessageResult> EndSession(IntegrationEndSessionMessage integrationMessage);
    }

    internal class Integration : IIntegration
    {
        private const string URL_INTEGRATIONS = "https://cloud.fireballserver.com/integrations";
        private const string URL_AUTHENTICATE = URL_INTEGRATIONS + "/authenticate";
        private const string URL_BALANCE = URL_INTEGRATIONS + "/balance";
        private const string URL_BET = URL_INTEGRATIONS + "/bet";
        private const string URL_WINNING = URL_INTEGRATIONS + "/winning";
        private const string URL_JACKPOT_PAY = URL_INTEGRATIONS + "/jackpot";
        private const string URL_PAY_DISPLAY = URL_INTEGRATIONS + "/paydisplay";
        private const string URL_DISCONNECTED = URL_INTEGRATIONS + "/disconnected";
        private const string URL_END_SESSION = URL_INTEGRATIONS + "/endsession";

        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;

        public Integration(ICommunicator communicator, ILogger<Integration> logger)
        {
            _logger = new FireballLogger(nameof(Integration), logger);
            _communicator = communicator;
        }

        public async Task<MessageResult> Authenticate(IntegrationAuthMessage integrationMessage)
        {
            _logger.LogDebug($"Authenticate: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_AUTHENTICATE, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> BalanceRequest(IntegrationBalanceRequest integrationMessage)
        {
            _logger.LogDebug($"BalanceRequest: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_BALANCE, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> PlaceBet(IntegrationBetPlace integrationMessage)
        {
            _logger.LogDebug($"PlaceBet: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_BET, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> PayWin(IntegrationWinningPay integrationMessage)
        {
            _logger.LogDebug($"PayWin: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_WINNING, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> PayJackpot(IntegrationJackpotPay integrationMessage)
        {
            _logger.LogDebug($"PayJackpot: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_JACKPOT_PAY, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> PayDisplay(IntegrationPayDisplay integrationMessage)
        {
            _logger.LogDebug($"PayDisplay: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_PAY_DISPLAY, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> DisconnectPlayer(IntegrationDisconnectMessage integrationMessage)
        {
            _logger.LogDebug($"DisconnectPlayer: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_DISCONNECTED, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> EndSession(IntegrationEndSessionMessage integrationMessage)
        {
            _logger.LogDebug($"EndSession: {integrationMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_END_SESSION, integrationMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

    }
}
