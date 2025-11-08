using H4H.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H4H.Domain.Entities
{
    public class ChatSession : BaseEntity
    {
        [JsonPropertyName("chatSessionId")]
        public Guid ChatSessionId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("user")]
        public User? User { get; set; }

        [JsonPropertyName("title")]
        [MaxLength(200)]
        public string? Title { get; set; }

        [JsonPropertyName("preferredLanguage")]
        public ChatLanguage PreferredLanguage { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatSession()
        {
            ChatSessionId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            IsActive = true;
            PreferredLanguage = ChatLanguage.English;
        }
    }
}
