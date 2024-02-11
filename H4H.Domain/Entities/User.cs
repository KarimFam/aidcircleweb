using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Entities
{
    public class User : BaseEntity
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        // External Authentication
        [JsonPropertyName("externalAuthProvider")]
        public string ExternalAuthProvider { get; set; }

        [JsonPropertyName("externalAuthId")]
        public string ExternalAuthId { get; set; }

        // User-specific properties (if any)
    }


}
