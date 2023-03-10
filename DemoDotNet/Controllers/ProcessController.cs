using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DemoDotNet.Models;
using Google.Apis.Auth.OAuth2;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Responses;
using Zeebe.Client;
using Microsoft.Extensions.Configuration;

namespace DemoDotNet.Controllers
{

    public class ProcessController : Controller
    {
        private readonly ILogger<ProcessController> _logger;
        private readonly IConfiguration _configuration;

        public ProcessController(ILogger<ProcessController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetClusterTopology()
        {
            // Get cluster topology
            var topology = await GetZeebeClient().TopologyRequest().Send();

            Console.WriteLine("Cluster connected: " + topology);

            return new JsonResult(new { topology = topology });
        }

        [HttpPost]
        public async Task<JsonResult> DeployProcess([FromQuery] string processName)
        {
            var processPath = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["resourcePath"];
            var demoProcessPath = $"{processPath}/{processName}.bpmn";

            var deployResponse = await GetZeebeClient().NewDeployCommand()
                .AddResourceFile(demoProcessPath)
                .Send();

            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            return new JsonResult(new { processDefinitionKey = processDefinitionKey });
        }

        [HttpPost]
        public async Task<JsonResult> CreateProcessInstance([FromQuery] string processId)
        {
            //sample - create trade sample process
            var processInstanceVariables = "{\"applicationNumber\":\"12424\", \"name\":\"Applicant1\" }";
            var processInstance = await GetZeebeClient()
                .NewCreateProcessInstanceCommand()
                .BpmnProcessId("customer_onboarding_en")
                .LatestVersion()
                .Variables(processInstanceVariables)
                .Send();


            //Refer below - setting new procecss variables
            await GetZeebeClient().NewSetVariablesCommand(processInstance.ProcessInstanceKey).Variables("{\"email\":\"jothikiruthika.viswanathan@camunda.com\" }").Local().Send();

            return new JsonResult(new { ProcessInstanceKey = processInstance.ProcessInstanceKey });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IZeebeClient GetZeebeClient()
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

