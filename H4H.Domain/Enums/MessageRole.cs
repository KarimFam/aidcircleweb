using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace H4H.Domain.Enums
{
    public enum MessageRole
    {
        [EnumMember(Value = "User")]
        [JsonPropertyName("user")]
        User,

        [EnumMember(Value = "Assistant")]
        [JsonPropertyName("assistant")]
        Assistant,

        [EnumMember(Value = "System")]
        [JsonPropertyName("system")]
        System
    }
}
