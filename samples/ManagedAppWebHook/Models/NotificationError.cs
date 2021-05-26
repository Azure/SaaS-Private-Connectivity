using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ManagedAppWebHook.Models
{

    public class NotificationError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public NotificationErrorDetails[] Details { get; set; }
    }

    public class NotificationErrorDetails
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}

