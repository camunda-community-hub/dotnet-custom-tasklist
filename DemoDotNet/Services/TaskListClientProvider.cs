using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DemoDotNet.Models;
using Google.Apis.Auth.OAuth2;
using GraphQL.Client.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using GraphQL.Client.Serializer.Newtonsoft;

namespace DemoDotNet.Services
{
    public class TaskListClientProvider
    {
        public class credentials
        {
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string grant_type { get; set; }
            public string audience { get; set; }
        }

        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private const string AuthToken_CacheKey = "TaskList_AuthToken";
        private String clientId;
        private String clientSecret;
        private String keycloakUrl = "http://localhost:18080";
        private String keycloakRealm = "camunda-platform";
        private String saasTokenUrl = "https://login.cloud.camunda.io/oauth/token";

        public TaskListClientProvider(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<GraphQLHttpClient> GetTaskListClientAsync()
        {
            //TASKLIST API
            var tasklistBaseUrl = _configuration.GetValue<string>("TasklistUrl");
            var taskListUrl = @tasklistBaseUrl + "/graphql";
            var graphQLHttpClientOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(taskListUrl)
            };

            var bearerValue = await GetTaskListAuthenticationAsync();
            var token = $"Bearer {bearerValue}";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", token);

            var graphQLClient = new GraphQLHttpClient(graphQLHttpClientOptions, new NewtonsoftJsonSerializer(), httpClient);

            return graphQLClient;
        }

        private async Task<string> GetTaskListAuthenticationAsync()
        {
            if (!_memoryCache.TryGetValue(AuthToken_CacheKey, out string bearerToken))
            {
                bearerToken = await GetBearerTokenForTaskList();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddSeconds(280));
                _memoryCache.Set(AuthToken_CacheKey, bearerToken, cacheEntryOptions);
            }
            return bearerToken;
        }

        private async Task<string> GetBearerTokenForTaskList()
        {
            var isCloudInstance = _configuration.GetValue<bool>("ZeebeConfiguration:Saas");
            var gatewayAddress = _configuration.GetValue<string>("ZeebeConfiguration:GatewayAddress");

            if (isCloudInstance)
            {
                return await GetBearerTokenForSaas();
            }
            else
            {
                return await GetBearerTokenForSelfManagedAsync();
            }

        }

        private async Task<string> GetBearerTokenForSaas()
        {
            var client = new HttpClient();

            var cloudClientid = _configuration.GetValue<string>("ZeebeConfiguration:Client:Cloud:ClientId");
            var cloudClientSecret = _configuration.GetValue<string>("ZeebeConfiguration:Client:Cloud:ClientSecret");

            var collection = new credentials { client_id = cloudClientid, client_secret = cloudClientSecret, grant_type = "client_credentials", audience = "tasklist.camunda.io" };

            var stringPayload = JsonConvert.SerializeObject(collection);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(saasTokenUrl, httpContent);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jsonString = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskListAuthDto>(result);
            return jsonString.access_token;
        }

        private async Task<string> GetBearerTokenForSelfManagedAsync()
        {
            keycloakUrl = _configuration.GetValue<string>("KeycloakUrl");

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl + "/auth/realms/camunda-platform/protocol/openid-connect/token");

            var identityClientId = _configuration.GetValue<string>("Identity:ClientId");
            var identityClientSecret = _configuration.GetValue<string>("Identity:ClientSecret");

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("client_id", identityClientId));
            collection.Add(new KeyValuePair<string, string>("client_secret", identityClientSecret));
            collection.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jsonString = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskListAuthDto>(result);
            return jsonString.access_token;
        }        
    }
}

