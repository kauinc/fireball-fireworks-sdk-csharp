using System.Collections.Generic;
using System.Threading.Tasks;
using Fireball.Game.Server.Models;
using Fireball.Game.Server.JackpotsModule;
using Microsoft.Extensions.Logging;

namespace Fireball.Game.Server.MessagesModule
{
    internal interface IMessenger
    {
        Task<MessageResult> SendSession<T>(T message, List<string> jackpotTemplateIds = null, bool includeClientVars = true, bool includeServerVars = false) where T : SessionMessage;
        Task<MessageResult> SendMessage<T>(T message, List<ReceiverData> receivers = null, bool includeClientVars = false, bool includeServerVars = false) where T : BaseMessage;
    }

    internal class Messenger : IMessenger
    {
        private const string URL_MESSAGES_DEFAULT = "https://cloud.fireballserver.com/messages";
        private const string FIREBALL_MESSAGES_URL = "FIREBALL_MESSAGES_URL";

        private readonly string URL_MESSAGES;
        private string URL_SEND_SESSION => URL_MESSAGES + "/send/session";
        private string URL_SEND_MESSAGE => URL_MESSAGES + "/send/message";

        private readonly ICommunicator _communicator;
        private readonly IFireballLogger _logger;
        private readonly IJackpots _jackpots;

        public Messenger(IJackpots jackpots, ICommunicator communicator, ILogger<Messenger> logger)
        {
            _logger = new FireballLogger(nameof(Messenger), logger);
            _communicator = communicator;
            _jackpots = jackpots;

            URL_MESSAGES = System.Environment.GetEnvironmentVariable(FIREBALL_MESSAGES_URL) ?? URL_MESSAGES_DEFAULT;
        }

        public async Task<MessageResult> SendSession<T>(T message, List<string> jackpotTemplateIds = null, bool includeClientVars = true, bool includeServerVars = false) where T : SessionMessage
        {
            if(!includeClientVars)
            {
                message.client_side = null;
            }

            if(!includeServerVars)
            {
                message.server_side = null;
            }

            if (jackpotTemplateIds != null && jackpotTemplateIds.Count > 0)
            {
                var jackpotsDetailsResult = await _jackpots.GetDetails(jackpotTemplateIds, message.Currency, message.OperatorId, message.Environment);
                if(jackpotsDetailsResult != null && jackpotsDetailsResult.Jackpots != null)
                {
                    message.Jackpots = jackpotsDetailsResult.Jackpots;
                }
            }

            var request = new SendSessionRequest<T>(message);
            _logger.LogDebug($"SendSession: {request.ToJson()}");

            var result = await _communicator.Post<MessageResult>(URL_SEND_SESSION, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }

        public async Task<MessageResult> SendMessage<T>(T message, List<ReceiverData> receivers = null, bool includeClientVars = false, bool includeServerVars = false) where T : BaseMessage
        {
            if(!includeClientVars)
            {
                message.client_side = null;
            }

            if(!includeServerVars)
            {
                message.server_side = null;
            }

            var request = new SendMessageRequest<T>(message, receivers);
            _logger.LogDebug($"SendMessage: {request.ToJson()}");

            var result = await _communicator.Post<MessageResult>(URL_SEND_MESSAGE, request.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }
    }
}
