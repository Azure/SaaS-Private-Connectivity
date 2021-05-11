using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
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
        private readonly static string TenantId = Environment.GetEnvironmentVariable("TenantId") ?? throw new ArgumentNullException(nameof(TenantId));
        private readonly static string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret") ?? throw new ArgumentNullException(nameof(ClientSecret));
        private readonly static string ClientId = Environment.GetEnvironmentVariable("ClientId") ?? throw new ArgumentNullException(nameof(ClientId));
        private readonly static string MySqlServer = Environment.GetEnvironmentVariable("MySqlServer") ?? throw new ArgumentNullException(nameof(MySqlServer));
        private readonly static string MySqlDatabase = Environment.GetEnvironmentVariable("MySqlDatabase") ?? throw new ArgumentNullException(nameof(MySqlDatabase));
        private readonly static string MySqlUserId = Environment.GetEnvironmentVariable("MySqlUserId") ?? throw new ArgumentNullException(nameof(MySqlUserId));
        private readonly static string MySqlPassword = Environment.GetEnvironmentVariable("MySqlPassword") ?? throw new ArgumentNullException(nameof(MySqlPassword));
    


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
                        await CreateUserSecretsAsync(data, log);
                        break;
                }
            }
            catch (ArgumentNullException ex)
            {
                responseMessage = "I did not have the right access to get deployment details";
                log.LogError(-1, ex, responseMessage);
                return new InternalServerErrorResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                responseMessage = "Your shared key is not valid";
                log.LogError(-1, ex, responseMessage);
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

        private async Task<ActionResult> ApprovePrivateConnectionAsync(Notification data, ILogger log)
        {
            var parsedString = data?.ApplicationId?.Split('/') ?? throw new ArgumentNullException(nameof(data));
            if (parsedString.Length < 5)
            {
                throw new ArgumentOutOfRangeException(nameof(data.ApplicationId));
            }
            var clientSubscriptionId = parsedString[2];

            //authenticate in client subscription
            var clientAzureAuth = AuthenticateToAzure(clientSubscriptionId);
            var managedAppDetails = await GetManagedAppDetails(data.ApplicationId, clientAzureAuth, log);

            if (managedAppDetails == null)
            {
                var message = "I did not have the right access to get deployment details";
                throw new Exception(message);
            }

            if (await ValidateKeyAsync(managedAppDetails.Outputs.PreSharedKey.Value, log) == false)
            {
                var message = "Your shared key is not valid";
                throw new UnauthorizedAccessException(message);
            }

            //await CreateKeyVaultSecretsAsync(managedAppDetails, log);
            //replace with private link approval
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

       // add connection approval


        private async Task<ApplicationDetails> GetManagedAppDetails(string applicationId, IAzure azure, ILogger log)
        {

            var retryCount = 0;
            ApplicationDetails customerDetails = null;


            while (retryCount < 3 && customerDetails == null)
            {
                log.LogInformation("Trying to authenticate against client subscription again");
                await Task.Delay(20000);
                var managedAppDetails = await azure.GenericResources.GetByIdAsync(applicationId);
                if (managedAppDetails != null)
                {
                    try
                    {
                        customerDetails = JsonSerializer.Deserialize<ApplicationDetails>(managedAppDetails.Properties.ToString());
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
                }
                log.LogInformation("Customer name is " + customerDetails.Outputs.CustomerName.Value);
                retryCount++;
            }

            return customerDetails;
        }

    }
}
