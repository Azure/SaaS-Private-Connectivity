using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Identity;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace HttpTrigger
{

    public class ManagedAppWebHook
    {
        private readonly static string TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? throw new ArgumentNullException(nameof(TenantId));
        private readonly static string ClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? throw new ArgumentNullException(nameof(ClientSecret));
        private readonly static string ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? throw new ArgumentNullException(nameof(ClientId));
        private readonly static string MySqlServer = Environment.GetEnvironmentVariable("MySqlServer") ?? throw new ArgumentNullException(nameof(MySqlServer));
        private readonly static string MySqlDatabase = Environment.GetEnvironmentVariable("MySqlDatabase") ?? throw new ArgumentNullException(nameof(MySqlDatabase));
        private readonly static string MySqlUserId = Environment.GetEnvironmentVariable("MySqlUserId") ?? throw new ArgumentNullException(nameof(MySqlUserId));
        private readonly static string MySqlPassword = Environment.GetEnvironmentVariable("MySqlPassword") ?? throw new ArgumentNullException(nameof(MySqlPassword));
        private readonly static string ResourceGroup = Environment.GetEnvironmentVariable("ResourceGroup") ?? throw new ArgumentNullException(nameof(ResourceGroup));
        private readonly static string PrivateLinkService = Environment.GetEnvironmentVariable("PrivateLinkService") ?? throw new ArgumentNullException(nameof(PrivateLinkService));

        [FunctionName("HealthCheck")]
        public static IActionResult HealthCheck(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "health")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = "This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("ManagedAppWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "resource")] HttpRequest req,
            ILogger log)

        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string responseMessage;

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("The request details are" + requestBody);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                };

                Notification data;
                try
                {
                    data = JsonSerializer.Deserialize<Notification>(requestBody, options);
                }
                catch (ArgumentNullException)
                {
                    log.LogError("Null exception when deserializing request body");
                    throw;
                }
                catch (JsonException)
                {
                    log.LogError("Json exception when deserializing request body");
                    throw;
                }

                var provisioningState = data?.ProvisioningState ?? throw new ArgumentNullException(nameof(data.ProvisioningState));
                log.LogInformation("Provisioning state is " + provisioningState);

                switch (provisioningState)
                {
                    case ProvisioningState.Accepted:
                        break;

                    case ProvisioningState.Failed:
                        break;

                    case ProvisioningState.Deleted:
                        break;

                    case ProvisioningState.Succeeded:
                        await ValidateAndApprovePrivateConnectionAsync(data, log);
                        break;
                }
            }
            catch (ArgumentNullException ex)
            {
                log.LogError(-1, ex, ex.Message);
                return new InternalServerErrorResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                log.LogError(-1, ex, ex.Message);
                return new UnauthorizedResult();
            }
            catch (Exception ex)
            {
                responseMessage = "Exception occurred during processing";
                log.LogError(-1, ex, responseMessage);
                return new StatusCodeResult(500);
            }
            return new OkResult();
        }

        private async Task<ActionResult> ValidateAndApprovePrivateConnectionAsync(Notification data, ILogger log)
        {
            var parsedString = data?.ApplicationId?.Split('/') ?? throw new ArgumentNullException(nameof(data));
            if (parsedString.Length < 5)
            {
                throw new ArgumentOutOfRangeException(nameof(data.ApplicationId));
            }
            var clientSubscriptionId = parsedString[2];

            //authenticate in client subscription for marketplace app or same subscription for service catalog app
            var clientAzureAuth = AuthenticateToAzure(clientSubscriptionId);

            var managedAppDetails = await GetAndParseManagedAppDetails(data.ApplicationId, clientAzureAuth, log);

            if (await ValidateKeyAsync(managedAppDetails.Outputs.PreSharedKey.Value, log) == false)
            {
                var message = "Your shared key is not valid";
                throw new UnauthorizedAccessException(message);
            }

            await ApprovePrivateEndpointConnectionAsync(clientSubscriptionId, managedAppDetails.Outputs.CustomerName.Value, log);
            return new OkResult();
        }


        private IAzure AuthenticateToAzure(string subscriptionId)
        {
            var credentials = new AzureCredentials(new ServicePrincipalLoginInformation { ClientId = ClientId, ClientSecret = ClientSecret }, TenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .Authenticate(credentials)
                        .WithSubscription(subscriptionId);
            return azure;
        }


        private async Task<bool> ValidateKeyAsync(string preSharedKey, ILogger log)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = MySqlServer,
                Database = MySqlDatabase,
                UserID = MySqlUserId,
                Password = MySqlPassword,
                SslMode = MySqlSslMode.Preferred,
            };

            using var conn = new MySqlConnection(builder.ConnectionString);
            log.LogInformation("Opening connection");
            await conn.OpenAsync();

            using var command = conn.CreateCommand();

            command.CommandText = "SELECT * FROM customer WHERE SharedKey = @presharedkey;";

            command.Parameters.AddWithValue("@presharedkey", preSharedKey);

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();
                var companyName = reader.GetString("CompanyName");
                log.LogInformation("Found key for " + companyName);
                return true;
            }

            return false;
        }

        private async Task ApprovePrivateEndpointConnectionAsync(string clientSubscriptionId, string customerName, ILogger log)
        {
            try
            {
                var networkManagementClient = new NetworkManagementClient(clientSubscriptionId, new DefaultAzureCredential());
                var privateEndpointConnections =
                    networkManagementClient.PrivateLinkServices.ListPrivateEndpointConnectionsAsync(ResourceGroup, PrivateLinkService);
                var privateEndpointConnectionsList = new List<PrivateEndpointConnection>();

                await foreach (PrivateEndpointConnection privateEndpointConnection in privateEndpointConnections)
                {
                    privateEndpointConnectionsList.Add(privateEndpointConnection);
                }

                var pendingConnection = privateEndpointConnectionsList
                        .FirstOrDefault(pe =>
                        pe.PrivateLinkServiceConnectionState.Status == "Pending" &&
                        pe.PrivateEndpoint.Id.Split('/')[2] == clientSubscriptionId);

                if (pendingConnection == null)
                {
                    throw new ArgumentNullException();
                }

                pendingConnection.PrivateLinkServiceConnectionState.Status = "Approved";
                pendingConnection.PrivateLinkServiceConnectionState.Description = $"Approved connection from customer {customerName}";
                await networkManagementClient.PrivateLinkServices.UpdatePrivateEndpointConnectionAsync(ResourceGroup, PrivateLinkService, pendingConnection.Name, pendingConnection);
                log.LogInformation($"Approved connection from customer {customerName}");
            }
            catch (ArgumentNullException ex)
            {
                log.LogError(ex, "There is no private endpoint connection in pending state to approve");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error approving the connection");
            }
        }
        private async Task<IGenericResource> GetManagedAppDetails(string applicationId, IAzure azure, ILogger log)
        {
            var retryCount = 1;

            IGenericResource managedAppDetails = null;

            // it takes a bit longer sometimes for role to be assigned as part of marketplace/service catalog app deployment, retrying 3 times
            while (retryCount < 4 && managedAppDetails == null)
            {
                log.LogInformation("Trying to get managed app deployment details");
                await Task.Delay(20000);

                try
                {
                    managedAppDetails = await azure.GenericResources.GetByIdAsync(applicationId);

                }
                catch (Exception)
                {
                    log.LogInformation($"You do not have permissions for getting managed app deployment details in {retryCount} try, trying again");
                }
                retryCount++;
            }
            if (managedAppDetails == null)
            {
                throw new UnauthorizedAccessException("You do not have access to get managed app deployment details");
            }
            return managedAppDetails;
        }

        private async Task<ApplicationDetails> GetAndParseManagedAppDetails(string applicationId, IAzure azure, ILogger log)
        {
            var managedAppDetails = await GetManagedAppDetails(applicationId, azure, log);
            ApplicationDetails customerDetails;
            try
            {
                customerDetails = JsonSerializer.Deserialize<ApplicationDetails>(managedAppDetails.Properties.ToString());
                log.LogInformation("Customer name is " + customerDetails.Outputs.CustomerName.Value);
                log.LogInformation("Shared key is " + customerDetails.Outputs.PreSharedKey.Value);
            }
            catch (ArgumentNullException)
            {
                log.LogError("Null exception when deserializing request body");
                throw;
            }
            catch (JsonException)
            {
                log.LogError("Json exception when deserializing request body");
                throw;
            }
            return customerDetails;

        }

    }
}

