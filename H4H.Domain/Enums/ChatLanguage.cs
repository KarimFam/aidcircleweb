using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace H4H.Domain.Enums
{
    public enum ChatLanguage
    {
        [EnumMember(Value = "English")]
        [JsonPropertyName("en")]
        English,

        [EnumMember(Value = "Spanish")]
        [JsonPropertyName("es")]
        Spanish,

        [EnumMember(Value = "French")]
        [JsonPropertyName("fr")]
        French,

        [EnumMember(Value = "German")]
        [JsonPropertyName("de")]
        German,

        [EnumMember(Value = "Chinese")]
        [JsonPropertyName("zh")]
        Chinese,

        [EnumMember(Value = "Arabic")]
        [JsonPropertyName("ar")]
        Arabic,

        [EnumMember(Value = "Portuguese")]
        [JsonPropertyName("pt")]
        Portuguese,

        [EnumMember(Value = "Russian")]
        [JsonPropertyName("ru")]
        Russian,

        [EnumMember(Value = "Japanese")]
        [JsonPropertyName("ja")]
        Japanese,

        [EnumMember(Value = "Korean")]
        [JsonPropertyName("ko")]
        Korean,

        [EnumMember(Value = "Hindi")]
        [JsonPropertyName("hi")]
        Hindi,

        [EnumMember(Value = "Auto")]
        [JsonPropertyName("auto")]
        Auto // Auto-detect language
    }
}
