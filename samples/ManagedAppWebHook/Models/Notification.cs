using ManagedAppWebHook.Models;
using System.Text.Json.Serialization;

namespace HttpTrigger
{
    public class Notification
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; }

        [JsonPropertyName("applicationId")]
        public string ApplicationId { get; set; }

        [JsonPropertyName("provisioningState")]
        public ProvisioningState ProvisioningState { get; set; }

        [JsonPropertyName("error")]
        public NotificationError Error { get; set; }

    }


}
