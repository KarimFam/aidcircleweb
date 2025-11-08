using H4H.Domain.Enums;

namespace H4H.Application.DTOs
{
    public class ChatSessionDto
    {
        public Guid? ChatSessionId { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public ChatLanguage PreferredLanguage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<ChatMessageDto>? Messages { get; set; }
    }
}
