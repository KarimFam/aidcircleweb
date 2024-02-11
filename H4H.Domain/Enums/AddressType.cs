using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Enums
{
    public enum AddressType
    {
        [EnumMember(Value = "Home")]
        [JsonPropertyName("home")]
        Home,

        [EnumMember(Value = "Work")]
        [JsonPropertyName("work")]
        Work,

        [EnumMember(Value = "Other")]
        [JsonPropertyName("other")]
        Other

        // Additional address types as needed
    }

}
