using System.Net.Http;
using System.Net.Http.Headers;

public static class HttpClientFactory
{
    private static HttpClient? _client;

    public static HttpClient Client
    {
        get
        {
            if (_client == null)
            {
                var handler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    SslOptions = { RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true } // dev ONLY
                };

                _client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30)
                };
            }

            return _client;
        }
    }
}
