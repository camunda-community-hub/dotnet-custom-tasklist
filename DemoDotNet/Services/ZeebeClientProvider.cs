using System;
using Microsoft.Extensions.Configuration;
using Zeebe.Client;
using Zeebe.Client.Impl.Builder;

namespace DemoDotNet.Services
{
    public class ZeebeClientProvider
    {
        private readonly IConfiguration _configuration;

        public ZeebeClientProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IZeebeClient GetZeebeClient()
        {
            var isCloudInstance = _configuration.GetValue<bool>("isCloud");
            var gatewayAddress = _configuration.GetValue<string>("gatewayAddress");
            if (isCloudInstance)
            {

                var ClientID = _configuration.GetValue<string>("clientid");
                var ClientSecret = _configuration.GetValue<string>("clientsecret");
                var clusterId = _configuration.GetValue<string>("clusterId");
                var region = _configuration.GetValue<string>("region");
                var ClusterAddress = $"{clusterId}.{region}.zeebe.camunda.io:443";

                return CamundaCloudClientBuilder.Builder()
                                  .UseClientId(ClientID)
                                  .UseClientSecret(ClientSecret)
                                  .UseContactPoint(ClusterAddress)
                //.UseLoggerFactory(new NLogLoggerFactory())
                                  .Build();
            }
            else if (_configuration.GetValue<bool>("plainTextSecurity"))
            {

                return ZeebeClient.Builder()
                 .UseGatewayAddress(gatewayAddress)
                 .UsePlainText()
                 .Build();

            }
            else
            {

                return ZeebeClient.Builder()
                 .UseGatewayAddress(gatewayAddress).UseTransportEncryption().Build();

            }
        }
    }
}

