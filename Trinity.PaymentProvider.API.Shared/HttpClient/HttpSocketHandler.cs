using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Trinity.PaymentProvider.API.Shared.HttpClient;

public class HttpSocketHandler
{
    public static HttpClientHandler CreateBindToSpecificIpHandler(string ipAddress)
    {
        var handler = new HttpClientHandler();

#if !DEBUG

        var socketsHandler = (SocketsHttpHandler)GetUnderlyingSocketsHttpHandler(handler);
        
        socketsHandler.ConnectCallback = async (context, token) =>
        {
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), 0));
            await s.ConnectAsync(context.DnsEndPoint, token);
            s.NoDelay = true;
            return new NetworkStream(s, ownsSocket: true);
        };
#endif

        handler.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;

        return handler;
    }

    protected static object GetUnderlyingSocketsHttpHandler(HttpClientHandler handler)
    {
        var field = typeof(HttpClientHandler).GetField("_underlyingHandler", BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(handler);
    }
}