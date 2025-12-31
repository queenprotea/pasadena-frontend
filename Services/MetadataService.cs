    using Grpc.Net.Client;
using Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Net.Http;

namespace pasadena_vistas.Services
{
    public static class MetadataService
    {
        private static Metadata.MetadataService.MetadataServiceClient? _client;

        public static Metadata.MetadataService.MetadataServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    // Validar que la URL sea válida
                    if (!Uri.TryCreate(Config.Config.MetadataURL, UriKind.Absolute, out var grpcUri))
                        throw new InvalidOperationException($"La URL de MetadataService no es válida: {Config.Config.MetadataURL}");

                    // Usar SocketsHttpHandler para Android y compatibilidad con gRPC
                    var handler = new SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                        SslOptions = { RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true } // acepta cualquier certificado si es dev
                    };

                    var channel = GrpcChannel.ForAddress(grpcUri, new GrpcChannelOptions
                    {
                        HttpHandler = handler
                    });

                    _client = new Metadata.MetadataService.MetadataServiceClient(channel);
                }

                return _client;
            }
        }
    }
}
