using Grpc.Net.Client;
using Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    var channel = GrpcChannel.ForAddress(Config.Config.MetadataURL, new GrpcChannelOptions
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
