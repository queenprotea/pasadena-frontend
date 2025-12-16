
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streaming;



namespace pasadena_vistas.Services
{
    public static class StreamingService
    {
        private static Streaming.StreamingService.StreamingServiceClient? _client;

        public static Streaming.StreamingService.StreamingServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    // Validar que la URL sea válida
                    if (!Uri.TryCreate(Config.Config.StreamingURL, UriKind.Absolute, out var grpcUri))
                        throw new InvalidOperationException($"La URL de StreamingService no es válida: {Config.Config.StreamingURL}");

                    // Usar SocketsHttpHandler para compatibilidad con Android
                    var handler = new SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                        SslOptions =
                        {
                            RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true // solo DEV
                        }
                    };

                    var channel = GrpcChannel.ForAddress(grpcUri, new GrpcChannelOptions
                    {
                        HttpHandler = handler
                    });

                    _client = new Streaming.StreamingService.StreamingServiceClient(channel);
                }

                return _client;
            }
        }
    }
}
