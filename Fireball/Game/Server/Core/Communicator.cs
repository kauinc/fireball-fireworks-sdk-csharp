using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fireball.Game.Server
{
    public class ResponseResult<T>
    {
        public T Response { get; set; }
        public Error Error { get; set; }
        public bool IsSuccess => Error is null;

        public ResponseResult()
        {
            Error = null;
        }
    }

    public class Error
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    internal interface ICommunicator
    {
        Task<ResponseResult<T>> Get<T>(string url);
        Task<ResponseResult<T>> Post<T>(string url, string jsonObject);
        Task<ResponseResult<T>> Patch<T>(string url, string jsonObject);
        Task<ResponseResult<T>> Put<T>(string url, string jsonObject);
        Task<ResponseResult<T>> Delete<T>(string url, string jsonObject);
    }

    internal class Communicator : ICommunicator
    {
        public const string CLIENT_NAME = "Communicator";

        private readonly IFireballLogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Communicator(IHttpClientFactory httpClientFactory, ILogger<Communicator> logger)
        {
            _logger = new FireballLogger(nameof(Communicator), logger);
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseResult<T>> Get<T>(string url)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = null,
            };
            return await Send<T>(httpRequest);
        }

        public async Task<ResponseResult<T>> Post<T>(string url, string jsonObject)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonObject ?? string.Empty, Encoding.UTF8, "application/json"),
            };
            return await Send<T>(httpRequest);
        }

        public async Task<ResponseResult<T>> Patch<T>(string url, string jsonObject)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Patch,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonObject ?? string.Empty, Encoding.UTF8, "application/json"),
            };
            return await Send<T>(httpRequest);
        }

        public async Task<ResponseResult<T>> Put<T>(string url, string jsonObject)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonObject ?? string.Empty, Encoding.UTF8, "application/json"),
            };
            return await Send<T>(httpRequest);
        }

        public async Task<ResponseResult<T>> Delete<T>(string url, string jsonObject)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url),
                Content = new StringContent(jsonObject ?? string.Empty, Encoding.UTF8, "application/json"),
            };
            return await Send<T>(httpRequest);
        }

        private async Task<ResponseResult<T>> Send<T>(HttpRequestMessage httpRequest)
        {
            var result = new ResponseResult<T>();

            _logger.LogDebug($"Send {httpRequest.Method.Method} request to: {httpRequest.RequestUri}");
            var httpClient = _httpClientFactory.CreateClient(CLIENT_NAME);
            var httpResponse = await httpClient.SendAsync(httpRequest);

            try
            {
                var contentString = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Send {httpRequest.Method.Method} Success: {contentString} (Code {httpResponse.StatusCode})");
                    result.Response = JsonConvert.DeserializeObject<T>(contentString);
                }
                else
                {
                    result.Error = ParseErrors(contentString);
                    if (result.Error != null && result.Error.errorCode.Equals("M1"))
                    {
                        _logger.LogWarning($"Send {httpRequest.Method.Method} Error: {contentString} (Code {httpResponse.StatusCode})");
                    }
                    else
                    {
                        _logger.LogError($"Send {httpRequest.Method.Method} Error: {contentString} (Code {httpResponse.StatusCode})");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}");
                result.Error = new Error()
                {
                    errorCode = nameof(Exception),
                    errorMessage = e.Message,
                };
            }

            return result;
        }

        private Error ParseErrors(string content)
        {
            Error error = null;
            try
            {
                try
                {
                    error = JsonConvert.DeserializeObject<Error>(content);
                }
                catch (Exception)
                {
                    error = JsonConvert.DeserializeObject<List<Error>>(content)?.First();
                }
            }
            catch
            {
                error = new Error()
                {
                    errorCode = "N/A",
                    errorMessage = content
                };
            }
            return error;
        }
    }
}
