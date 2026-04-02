using System;
using Fireball.Game.Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fireball.Game.Server
{
    public class ParseResult
    {
        private IFireballLogger _logger;

        public string MessageName;
        public string GameSession;
        public long? MessageTimestamp;
        public JObject MessageObject;

        public bool IsSuccess => !string.IsNullOrEmpty(MessageName) && MessageObject != null;

        public ParseResult(string name, string gameSession, long? timestamp, JObject jobject, IFireballLogger logger)
        {
            _logger = logger;
            MessageName = name;
            GameSession = gameSession;
            MessageTimestamp = timestamp;
            MessageObject = jobject;
        }

        public T ToMessage<T>() where T : BaseMessage
        {
            T message = null;
            try
            {
                message = MessageObject.ToObject<T>(new JsonSerializer()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                if (message is null)
                {
                    _logger.LogError($"Message is null. Type = {typeof(T).Name}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Can't deserialize Message to type = {typeof(T).Name}. Error: {e.Message}");
            }
            return message;
        }

        public T GetServerVariables<T>() where T : class
        {
            T vars = null;
            try
            {
                vars = MessageObject[nameof(BaseMessage.server_side)]?.ToObject<T>();
            }
            catch(Exception e)
            {
                _logger.LogError($"Can't deserialize Server Variables to type = {typeof(T).Name}. Error: {e.Message}");
            }
            return vars;
        }
    }
}
