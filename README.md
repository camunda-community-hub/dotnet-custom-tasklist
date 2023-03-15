# zeebe-dotnet-client

#### A dotnet client for the Camunda 8 zeebe & task list APIs.

A custom tasklist is implemented by the TaskController's Index action. 

Click on the Task Category to expand / collapse the list of tasks. The UI is WIP.

Once you have some user tasks awaiting action, you can see them appearing in the Open tasks section.
Once Claimed they move to the Claimed section.
Once completed, they are then displayed in the COmpleted tasks section.

To connect to a Saas Instance, use the below settings in the appsettings.json -

```
"isCloud": true,
"clientid": "XX",
"clientsecret": "XXXX",
"clusterId": "XX-XX-XX-XX-XX",
"region": "dsm-1",

 ```

Generate client credentials as guided in the link below and get the clientid, secret, clustered, region details for the above settings.
 
https://docs.camunda.io/docs/guides/setup-client-connection-credentials/

To connect to a Self Managed Instance, use the below settings in the appsettings.json -

```
"isCloud": false,
"gatewayAddress": "127.0.0.1:26500",
"plainTextSecurity": true, 
"identityClientid": "xx",
"identityClientsecret": "xx",

 ```

Generate the identityClientid & identityClientsecret, for the tasklist authentication, as outlined below in the documentation -

https://docs.camunda.io/docs/8.0/self-managed/tasklist-deployment/tasklist-authentication/#use-identity-jwt-token-to-access-tasklist-api
