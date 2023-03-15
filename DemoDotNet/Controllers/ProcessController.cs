using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DemoDotNet.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace DemoDotNet.Controllers
{

    public class ProcessController : Controller
    {
        private readonly ILogger<ProcessController> _logger;
        private readonly Services.ZeebeClientProvider _zeebeClientProvider;
        private readonly IConfiguration _configuration;

        public ProcessController(ILogger<ProcessController> logger, Services.ZeebeClientProvider zeebeClientProvider, IConfiguration configuration)
        {
            _logger = logger;
            _zeebeClientProvider = zeebeClientProvider;
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
            var topology = await _zeebeClientProvider.GetZeebeClient().TopologyRequest().Send();

            Console.WriteLine("Cluster connected: " + topology);

            return new JsonResult(new { topology = topology });
        }

        [HttpPost]
        public async Task<JsonResult> DeployProcess([FromQuery] string processName)
        {
            var processPath = _configuration.GetValue<string>("resourcePath");
            var demoProcessPath = $"{processPath}/{processName}.bpmn";

            var deployResponse = await _zeebeClientProvider.GetZeebeClient().NewDeployCommand()
                .AddResourceFile(demoProcessPath)
                .Send();

            var processDefinitionKey = deployResponse.Processes[0].ProcessDefinitionKey;

            return new JsonResult(new { processDefinitionKey = processDefinitionKey });
        }

        [HttpPost]
        public async Task<JsonResult> CreateProcessInstance([FromQuery] string processId)
        {
            //sample - sample process with some dummy variables
            var processInstanceVariables = "{\"applicationNumber\":\"12424\", \"name\":\"Applicant1\" }";
            var processInstance = await _zeebeClientProvider.GetZeebeClient()
                .NewCreateProcessInstanceCommand()
                .BpmnProcessId(processId)
                .LatestVersion()
                .Variables(processInstanceVariables)
                .Send();


            //Refer below - setting new procecss variables
            await _zeebeClientProvider.GetZeebeClient().NewSetVariablesCommand(processInstance.ProcessInstanceKey).Variables("{\"email\":\"jothikiruthika.viswanathan@camunda.com\" }").Local().Send();

            return new JsonResult(new { ProcessInstanceKey = processInstance.ProcessInstanceKey });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}

