
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streaming;

namespace pasadena_vistas.Services
{
    public class StreamingService
    {
        private static Streaming.StreamingService.StreamingServiceClient _client;

        public static Streaming.StreamingService.StreamingServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    var channel = GrpcChannel.ForAddress(Config.Config.StreamingURL, new GrpcChannelOptions
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
