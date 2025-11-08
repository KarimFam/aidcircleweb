using H4H.Domain.Entities;

namespace H4H.Application.Interfaces
{
    public interface IChatOrchestrationService
    {
        Task<ChatMessage> ProcessUserMessageAsync(ChatSession session, string userMessage);
        Task<string> GenerateSessionTitleAsync(string firstMessage);
    }
}
