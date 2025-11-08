using H4H.Domain.Enums;

namespace H4H.Application.DTOs
{
    public class ChatMessageDto
    {
        public Guid? ChatMessageId { get; set; }
        public Guid ChatSessionId { get; set; }
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? TranslatedContent { get; set; }
        public ChatLanguage? OriginalLanguage { get; set; }
        public ChatLanguage? TargetLanguage { get; set; }
        public int? TokensUsed { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SendMessageRequest
    {
        public Guid ChatSessionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateChatSessionRequest
    {
        public Guid UserId { get; set; }
        public ChatLanguage PreferredLanguage { get; set; } = ChatLanguage.English;
    }
}
