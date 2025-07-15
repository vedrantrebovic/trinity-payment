namespace Trinity.PaymentPlatform.Processors.Airtel;

public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestHeaders = string.Join("\n", request.Headers.Select(h => $"{h.Key}: {string.Join("; ", h.Value)}"));
        var requestBody = request.Content != null ? await request.Content.ReadAsStringAsync() : "No Body";

        // Log request
        _logger.LogInformation("Outgoing HTTP Request: {Method} {Url} \nHeaders: \n{Headers} \nBody: {Body}",
            request.Method, request.RequestUri, requestHeaders, requestBody);

        var response = await base.SendAsync(request, cancellationToken);

        var responseHeaders = string.Join("\n", response.Headers.Select(h => $"{h.Key}: {string.Join("; ", h.Value)}"));
        var responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : "No Body";

        // Log response
        _logger.LogInformation("HTTP Response: {StatusCode} \nHeaders: \n{Headers} \nBody: {Body}",
            response.StatusCode, responseHeaders, responseBody);

        return response;
    }
}