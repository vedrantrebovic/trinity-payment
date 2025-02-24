using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Contracts;

namespace Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Handlers;

public class AccessTokenHandler(ILogger<AccessTokenHandler> logger,IAccessTokenProvider tokenProvider):DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(tokenProvider.GetToken()))
                await tokenProvider.ObtainToken();

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenProvider.GetToken());
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var authResult = await tokenProvider.ObtainToken();
                if (authResult.IsSuccess)
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", tokenProvider.GetToken());
                    response = await base.SendAsync(request, cancellationToken);
                    return response;
                }
            }

            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "");
            return new HttpResponseMessage(HttpStatusCode.SeeOther);
        }
    }
}