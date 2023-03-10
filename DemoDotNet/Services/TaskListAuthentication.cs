using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DemoDotNet.Models;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DemoDotNet.Services
{
	public class TaskListAuthentication
	{
        public class credentials {
            public string client_id { get; set; }
            public string client_secret { get; set; }
            public string grant_type  { get; set; }
        public string audience  { get; set; }
}

        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private const string AuthToken_CacheKey = "TaskList_AuthToken";
        private String clientId;
        private String clientSecret;
        private String keycloakUrl = "http://localhost:18080";
        private String keycloakRealm = "camunda-platform";
        private String saasTokenUrl = "https://login.cloud.camunda.io/oauth/token";

        public TaskListAuthentication(IMemoryCache memoryCache, IConfiguration configuration)
		{
			_memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<string> GetTaskListAuthenticationAsync()
        {
            if (!_memoryCache.TryGetValue(AuthToken_CacheKey, out string bearerToken))
            {
                bearerToken =  await GetBearerTokenForTaskList();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddSeconds(280));
                _memoryCache.Set(AuthToken_CacheKey, bearerToken, cacheEntryOptions);
            }
            return bearerToken;
        }

        public async Task<string> GetBearerTokenForTaskList()
        {
            var isCloudInstance = _configuration.GetValue<bool>("isCloud");
            var gatewayAddress = _configuration.GetValue<string>("gatewayAddress");

            if (isCloudInstance)
            {
                return await GetBearerTokenForSaas();
            }
            else {
               return await GetBearerTokenForSelfManagedAsync();
            }

        }

        public async Task<string> GetBearerTokenForSaas()
        {
            var client = new HttpClient();

            var cloudClientid = _configuration.GetValue<string>("clientid");
            var cloudClientSecret = _configuration.GetValue<string>("clientsecret");

            var collection = new credentials { client_id = cloudClientid, client_secret = cloudClientSecret, grant_type = "client_credentials", audience= "tasklist.camunda.io" };

            var stringPayload = JsonConvert.SerializeObject(collection);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(saasTokenUrl, httpContent);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var jsonString = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskListAuthDto>(result);
            return jsonString.access_token;
        }

        public async Task<string> GetBearerTokenForSelfManagedAsync()
        {
            keycloakUrl = _configuration.GetValue<string>("keycloakUrl");
           
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl+"/auth/realms/camunda-platform/protocol/openid-connect/token");

            var identityClientId = _configuration.GetValue<string>("identityClientid");
            var identityClientSecret = _configuration.GetValue<string>("identityClientsecret");

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

        public HttpContent getContent()
        {
            var identityClientId = _configuration.GetValue<string>("identityClientid");
            var identityClientSecret = _configuration.GetValue<string>("identityClientsecret");

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>("client_id", identityClientId));
            collection.Add(new KeyValuePair<string, string>("client_secret", identityClientSecret));
            collection.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

            return new FormUrlEncodedContent(collection);
        }
	}
}

