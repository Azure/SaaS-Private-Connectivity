using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HttpTrigger
{
    public class Office
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("dateFormat")]
        public string DateFormat { get; set; }
        [JsonPropertyName("locale")]
        public string Locale { get; set; }
        [JsonPropertyName("openingDate")]
        public string OpeningDate { get; set; }
        [JsonPropertyName("parentId")]
        public int ParentId { get; set; }
        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }
    }
}
