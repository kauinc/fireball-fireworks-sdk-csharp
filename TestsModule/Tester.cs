using System.Threading.Tasks;
using Fireball.Fireworks.Core;
using Fireball.Fireworks.Models;
using Microsoft.Extensions.Logging;

namespace Fireball.Fireworks.TestsModule
{
    public interface ITester
    {
        Task<MessageResult> SendRTPResult(RTPResult resultMessage);
    }

    internal class Tester : ITester
    {
        public const string URL_TESTER = "https://cloud.fireballserver.com/tester";
        public const string URL_RTP_START = URL_TESTER + "/rtp";
        public const string URL_RTP_RESULT = URL_TESTER + "/rtp/result";

        private readonly IFireballLogger _logger;
        private readonly ICommunicator _communicator;

        public Tester(ICommunicator communicator, ILogger<Tester> logger)
        {
            _logger = new FireballLogger(nameof(Tester), logger);
            _communicator = communicator;
        }

        public async Task<MessageResult> SendRTPResult(RTPResult resultMessage)
        {
            _logger.LogDebug($"RTP Result: {resultMessage.ToJson()}");
            var result = await _communicator.Post<MessageResult>(URL_RTP_RESULT, resultMessage.ToJson());
            if (result.IsSuccess)
            {
                return result?.Response;
            }
            return null;
        }
    }
}
