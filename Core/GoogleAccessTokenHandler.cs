using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Fireball.Fireworks.Core
{
    public class GoogleAccessTokenHandler(ILogger<GoogleAccessTokenHandler> logger) : DelegatingHandler
    {
        private readonly Lazy<Task<GoogleCredential>> _credentialFactory = new(CreateCredentialAsync);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is null || !request.RequestUri.IsAbsoluteUri)
            {
                throw new InvalidOperationException("Request URI must be absolute when attaching an OIDC token.");
            }

            var credential = await _credentialFactory.Value;

            var audience = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}";
            var oidcToken = await credential.GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(audience), cancellationToken);
            var token = await oidcToken.GetAccessTokenAsync(cancellationToken);


            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            logger.LogDebug("Attached Google ID token to INTEGRATIONS request for audience {Audience}: {Method} {Uri}", audience, request.Method, request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }

        private static async Task<GoogleCredential> CreateCredentialAsync()
        {
            return await GoogleCredential.GetApplicationDefaultAsync();
        }
    }
}
