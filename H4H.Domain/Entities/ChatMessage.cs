using H4H.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H4H.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        [JsonPropertyName("chatMessageId")]
        public Guid ChatMessageId { get; set; }

        [JsonPropertyName("chatSessionId")]
        public Guid ChatSessionId { get; set; }

        [JsonPropertyName("chatSession")]
        public ChatSession? ChatSession { get; set; }

        [JsonPropertyName("role")]
        public MessageRole Role { get; set; }

        [JsonPropertyName("content")]
        [Required]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("translatedContent")]
        public string? TranslatedContent { get; set; }

        [JsonPropertyName("originalLanguage")]
        public ChatLanguage? OriginalLanguage { get; set; }

        [JsonPropertyName("targetLanguage")]
        public ChatLanguage? TargetLanguage { get; set; }

        [JsonPropertyName("tokensUsed")]
        public int? TokensUsed { get; set; }

        [JsonPropertyName("executionTimeMs")]
        public int? ExecutionTimeMs { get; set; }

        [JsonPropertyName("agentMetadata")]
        public string? AgentMetadata { get; set; } // JSON for agent execution details

        public ChatMessage()
        {
            ChatMessageId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}
