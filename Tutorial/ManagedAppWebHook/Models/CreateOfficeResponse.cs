using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HttpTrigger
{
    public class CreateOfficeResponse
    {
        [JsonPropertyName("officeId")]
        public int OfficeId { get; set; }

        [JsonPropertyName("resourceId")]
        public int ResourceId { get; set; }
    }
}
