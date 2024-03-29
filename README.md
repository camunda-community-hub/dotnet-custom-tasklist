[![Community Extension](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)
![Compatible with: Camunda Platform 8](https://img.shields.io/badge/Compatible%20with-Camunda%20Platform%208-0072Ce)
[![](https://img.shields.io/badge/Lifecycle-Incubating-blue)](https://github.com/Camunda-Community-Hub/community/blob/main/extension-lifecycle.md#incubating-)

# Camunda 8 with C#

### Custom Task list based on Tasklist's GraphQL Api and MVC.Net / Jquery.

A custom tasklist is implemented by the TaskController's Index action. 

Click on the Task Category to expand / collapse the list of tasks. The UI is WIP.

Once you have some user tasks awaiting action, you can see them appearing in the Open tasks section.
Once Claimed they move to the Claimed section.
Once completed, they are then displayed in the COmpleted tasks section.

### Job Workers based on .Net Hosted Services

This project leverages the [Zeebe csharp accelerator library](https://github.com/camunda-community-hub/zeebe-client-csharp-accelerator) to bootstrap jobworkers. For more details, please refer to this [project's Readme](https://github.com/camunda-community-hub/zeebe-client-csharp-accelerator/blob/main/README.md)

### C# Client for Zeebe gateway

Process controller has examples of Deploying Processes/DMNs, Creating Process Instances & updating Process Variables. This is WIP, to create a UI to deploy processes / decisions & create Process instances / evaluate decisions. 


### Configuration
To connect to a Saas Instance, use the below settings in the appsettings.json -

```
{
  "ZeebeConfiguration": {
    "Saas": true,
    "Client": {
      "GatewayAddress": "your-cluster-id.your-region.zeebe.camunda.io:443", // this is the gateway address format for Saas

      "Cloud": {
        "ClientId": xx,
        "ClientSecret": xx,
        "AuthorizationServerUrl": "https://login.cloud.camunda.io/oauth/token",
        "TokenAudience": "zeebe.camunda.io"
      }
    },

  "TasklistUrl": "https://your-region.tasklist.camunda.io/your-cluster-id"

 ```

Generate client credentials as guided in the link below and get the clientid, secret, clustered, region details for the above settings.
 
https://docs.camunda.io/docs/guides/setup-client-connection-credentials/

To connect to a Self Managed Instance, use the below settings in the appsettings.json -

```
"ZeebeConfiguration": {
    "Saas": false,
    "Client": {
      "GatewayAddress": "127.0.0.1:300",
      "PlainTextSecurity": false
    }

"Identity": {
    "ClientId": "test",
    "ClientSecret": "jTpwR1T5Crz425bse9155Yi71XCA5ava"
  },

"TasklistUrl": "http://localhost:8082",
"KeycloakUrl": "http://localhost:18080",
 ```

Generate the identityClientid & identityClientsecret, for the tasklist authentication, as outlined below in the documentation -

https://docs.camunda.io/docs/8.0/self-managed/tasklist-deployment/tasklist-authentication/#use-identity-jwt-token-to-access-tasklist-api
