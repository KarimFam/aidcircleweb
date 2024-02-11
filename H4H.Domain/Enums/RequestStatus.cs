using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Enums
{
    public enum RequestStatus
    {
        [EnumMember(Value = "Pending")]
        [JsonPropertyName("pending")]
        Pending,

        [EnumMember(Value = "InProgress")]
        [JsonPropertyName("inProgress")]
        InProgress,

        [EnumMember(Value = "Completed")]
        [JsonPropertyName("completed")]
        Completed,

        [EnumMember(Value = "Cancelled")]
        [JsonPropertyName("cancelled")]
        Cancelled

        // Additional statuses as needed
    }

}
