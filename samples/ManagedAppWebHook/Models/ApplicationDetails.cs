using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HttpTrigger
{
    public class ApplicationDetails
    {
        [JsonPropertyName("outputs")]
        public Details Outputs { get; set; }
    }

    public class Details
    {
        [JsonPropertyName("customerName")]
        public Property CustomerName { get; set; }


        [JsonPropertyName("presharedKey")]
        public Property PreSharedKey { get; set; }

    }


    public class Property
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

}
