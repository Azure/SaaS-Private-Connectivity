using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HttpTrigger
{
    public class User
    {
        [JsonPropertyName("officeId")]
        public int OfficeId { get; set; }

        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastname")]
        public string LastName { get; set; }

        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("roles")]
        public List<int> Roles { get; set; }

        [JsonPropertyName("sendPasswordToEmail")]
        public bool SendPasswordToEmail { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("repeatPassword")]
        public string RepeatPassword { get; set; }

    }




}
