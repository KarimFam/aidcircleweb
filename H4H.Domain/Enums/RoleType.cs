using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Enums
{
    public enum RoleType
    {
        [EnumMember(Value = "Admin")]
        [JsonPropertyName("admin")]
        Admin,

        [EnumMember(Value = "User")]
        [JsonPropertyName("user")]
        User,

        [EnumMember(Value = "Volunteer")]
        [JsonPropertyName("volunteer")]
        Volunteer,

        [EnumMember(Value = "Organizer")]
        [JsonPropertyName("organizer")]
        Organizer

        // Additional roles as needed
    }

}
