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
using GraphQL.Client.Http;
using System.Net.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using GraphQL.Client.Abstractions;
using DemoDotNet.Models;

using static DemoDotNet.Models.Models;
using System.Collections;
using DemoDotNet.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace DemoDotNet.Controllers
{  

    public class TaskController : Controller
    {       
        private readonly ILogger<TaskController> _logger;
        private readonly TaskListAuthentication _taskListAuthentication;
        private readonly IConfiguration _configuration;


        public TaskController(ILogger<TaskController> logger, TaskListAuthentication taskListAuthentication, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _logger = logger;
            _taskListAuthentication = taskListAuthentication;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetAllTasks()
        {
            var alltasks = await FetchAllTasks();

            return new JsonResult(new { tasks = alltasks });
        }
               
        public async Task<JsonResult> GetOpenTasks()
        {
            var alltasks = await FetchAllTasks();            

            return new JsonResult(new { tasks = alltasks.Where(i=>i.assignee is null).ToList() });
        }

        public async Task<JsonResult> GetTask(string taskId)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var tasksRequest = new GraphQLRequest
            {
                Query = @"
                 query task ($id: String!) {
                    task (id: $id) {
                        id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                        candidateGroups
                    }
                }
                ",
                OperationName = "task",
                Variables = new { id = taskId }
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.GetTaskResponse>(tasksRequest);

            return new JsonResult(new { task = graphQLResponse.Data.task });

        }

        public async Task<JsonResult> GetTaskByProcessInstanceId([FromQuery] string processInstanceId)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var tasksRequest = new GraphQLRequest
            {
                Query = @"
                 query getTasksByProcessInstanceId ($processInstanceId: String!) {
                    tasks (query:{processInstanceId: $processInstanceId}) {
                        id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                        candidateGroups
                    }
                }
                ",
                OperationName = "getTasksByProcessInstanceId",
                Variables = new { processInstanceId = processInstanceId }
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.TaskListResponse>(tasksRequest);

            return new JsonResult(new { task = graphQLResponse.Data.tasks });

        }

        public async Task<JsonResult> GetTasksByUser([FromQuery] string user)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var tasksRequest = new GraphQLRequest
            {
                Query = @"

                 query getTasksByAssignee($assignee: String!, $state: TaskState!){
                    tasks(query:{assignee: $assignee, state: $state }){

                    id,
                    formKey 
                    processDefinitionId 
                    assignee 
                    name  
                    candidateGroups 
                    processName 
                    creationTime 
                    completionTime 
                    }
                }
                ",
                OperationName = "getTasksByAssignee",

                Variables = new { assignee =  user, state="CREATED"}

            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.TaskListResponse>(tasksRequest);

            return new JsonResult(new { tasks = graphQLResponse.Data.tasks });
        }

        public async Task<JsonResult> GetCompletedTasks([FromQuery] string user)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var tasksRequest = new GraphQLRequest
            {
                Query = @"
                 query getTasksByAssignee($state: TaskState!){
                    tasks(query:{state: $state}){
                    id,
                    formKey 
                    processDefinitionId 
                    assignee 
                    name  
                    candidateGroups 
                    processName 
                    creationTime 
                    completionTime 
                    }
                }
                ",
                OperationName = "getTasksByAssignee",
                Variables = new { state = "COMPLETED" }


            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.TaskListResponse>(tasksRequest);

            return new JsonResult(new { tasks = graphQLResponse.Data.tasks });
        }

        [HttpPost]
        public  async Task<JsonResult> ClaimTask([FromQuery]string taskId, [FromQuery] string user)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var mutationRequest = new GraphQLRequest
            {
                Query = @"
                mutation claimTask ($taskId: String!, $assignee: String)  {
                claimTask (taskId: $taskId, assignee: $assignee)
                {
                        id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        taskState
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                        candidateGroups
                    }
                }
                ",
                OperationName = "claimTask",
                Variables = new
                {
                    taskId = taskId,
                    assignee = user
                }
            };

            var mutationResponse = await graphQLClient.SendMutationAsync<Models.Models.ClaimMutationResponse>(mutationRequest);

            return new JsonResult(new { claimedTask = mutationResponse.Data.claimTask });
        }


        [HttpPost]
        public async Task<JsonResult> UnClaimTask([FromQuery] string taskId)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var mutationRequest = new GraphQLRequest
            {
                Query = @"
                mutation unclaimTask ($taskId: String!)  {
                unclaimTask (taskId: $taskId)
                {
                        id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        taskState
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                        candidateGroups
                    }
                }
                ",
                OperationName = "unclaimTask",
                Variables = new
                {
                    taskId = taskId
                }
            };

            var mutationResponse = await graphQLClient.SendMutationAsync<Models.Models.UnClaimMutationResponse>(mutationRequest);

            return new JsonResult(new { claimedTask = mutationResponse.Data.unClaimTask });
        }

        [HttpPost]
        public async Task<JsonResult> CompleteTask([FromQuery] string taskId)
        {
            var graphQLClient = await GetTaskListClientAsync();

            var mutationRequest = new GraphQLRequest
            {
                Query = @"
                mutation completeTask ($taskId: String!, $variables: [VariableInput!]!) {
                completeTask (taskId: $taskId, variables: $variables) {
                        id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                        candidateGroups
                    }
                }   
                ",
                OperationName = "completeTask",
                Variables = new
                {
                    taskId = taskId,
                    variables = new List<VariableInput> { new VariableInput { name="1", value="1" }, new VariableInput { name="2", value=2} }
                }
            };

            var mutationResponse = await graphQLClient.SendMutationAsync<Models.Models.CompleteMutationResponse>(mutationRequest);

            return new JsonResult(new { claimedTask = mutationResponse.Data.completeTask });
        }

        private async Task<GraphQLHttpClient> GetTaskListClientAsync()
        {
            //TASKLIST API
            var tasklistBaseUrl = _configuration.GetValue<string>("tasklistUrl");
            var taskListUrl = @tasklistBaseUrl +"/graphql";
            var graphQLHttpClientOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(taskListUrl)
            };

            var bearerValue = await _taskListAuthentication.GetTaskListAuthenticationAsync();
            var token = $"Bearer {bearerValue}";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", token);

            var graphQLClient = new GraphQLHttpClient(graphQLHttpClientOptions, new NewtonsoftJsonSerializer(), httpClient);

            return graphQLClient;
        }
                
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<List<Models.Models.Task>> FetchAllTasks()
        {
            var graphQLClient = await GetTaskListClientAsync();
            var tasksRequest = new GraphQLRequest
            {
                Query = @"
                query tasks($state: TaskState!)
                {
                    tasks(query:{state: $state}){
                    id
                        name
                        taskDefinitionId
                        processName
                        creationTime
                        completionTime
                        assignee
                        variables {
                            id
                            name
                            value
                            previewValue
                            isValueTruncated
                        }
                        sortValues
                        isFirst
                        formKey
                        processDefinitionId
                    }
                }
            ",
                Variables = new { state = "CREATED" }
            };
            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.TaskListResponse>(tasksRequest);

            return graphQLResponse.Data.tasks;
        }
    }
}

