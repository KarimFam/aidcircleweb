using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Enums
{
    public enum ItemType
    {
        [EnumMember(Value = "Todo")]
        [JsonPropertyName("todo")]
        Todo,

        [EnumMember(Value = "Request")]
        [JsonPropertyName("request")]
        Request,

        [EnumMember(Value = "Event")]
        [JsonPropertyName("event")]
        Event

        // Additional item types as needed
    }

}
