using System;
using GraphQL;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using static DemoDotNet.Models.Models;
using System.Collections.Generic;

namespace DemoDotNet.Services
{
	public class TaskListService
	{
        private readonly TaskListAuthentication _taskListAuthentication;
        private readonly IConfiguration _configuration;

        public TaskListService(TaskListAuthentication taskListAuthentication, IMemoryCache memoryCache, IConfiguration configuration)
		{
            _taskListAuthentication = taskListAuthentication;
            _configuration = configuration;
        }

        public async Task<List<Models.Models.Task>> FetchAllTasks()
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
        
        public async Task<Models.Models.Task> GetTask(string taskId)
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

            return graphQLResponse.Data.task;

        }

        public async Task<List<Models.Models.Task>> GetTaskByProcessInstanceId(string processInstanceId)
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

            return graphQLResponse.Data.tasks;

        }

        public async Task<List<Models.Models.Task>> GetTasksByUser(string user)
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

                Variables = new { assignee = user, state = "CREATED" }

            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<Models.Models.TaskListResponse>(tasksRequest);

            return graphQLResponse.Data.tasks;
        }

        public async Task<List<Models.Models.Task>> GetCompletedTasks(string user)
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

            return graphQLResponse.Data.tasks;
        }


        public async Task<Models.Models.Task> ClaimTask(string taskId, string user)
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

            return  mutationResponse.Data.claimTask;
        }


        public async Task<Models.Models.Task> UnClaimTask(string taskId)
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

            return mutationResponse.Data.unClaimTask;
        }

        public async Task<Models.Models.Task> CompleteTask(string taskId)
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
                    variables = new List<VariableInput> { new VariableInput { name = "1", value = "1" }, new VariableInput { name = "2", value = 2 } }
                }
            };

            var mutationResponse = await graphQLClient.SendMutationAsync<Models.Models.CompleteMutationResponse>(mutationRequest);

            return mutationResponse.Data.completeTask;
        }

        private async Task<GraphQLHttpClient> GetTaskListClientAsync()
        {
            //TASKLIST API
            var tasklistBaseUrl = _configuration.GetValue<string>("tasklistUrl");
            var taskListUrl = @tasklistBaseUrl + "/graphql";
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
    }
}

