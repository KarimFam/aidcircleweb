using H4H.Application.DTOs;
using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Domain.Enums;
using H4H.Domain.Interfaces;
using AutoMapper;

namespace H4H.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatSessionRepository _sessionRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatOrchestrationService _orchestrationService;
        private readonly IMapper _mapper;

        public ChatService(
            IChatSessionRepository sessionRepository,
            IChatMessageRepository messageRepository,
            IChatOrchestrationService orchestrationService,
            IMapper mapper)
        {
            _sessionRepository = sessionRepository;
            _messageRepository = messageRepository;
            _orchestrationService = orchestrationService;
            _mapper = mapper;
        }

        public async Task<ChatSessionDto> CreateSessionAsync(Guid userId, ChatLanguage preferredLanguage)
        {
            var session = new ChatSession
            {
                UserId = userId,
                PreferredLanguage = preferredLanguage,
                Title = "New Chat",
                IsActive = true
            };

            await _sessionRepository.AddAsync(session);
            return _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<ChatSessionDto?> GetSessionByIdAsync(Guid chatSessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(chatSessionId);
            return session == null ? null : _mapper.Map<ChatSessionDto>(session);
        }

        public async Task<List<ChatSessionDto>> GetUserSessionsAsync(Guid userId)
        {
            var sessions = await _sessionRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ChatSessionDto>>(sessions);
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid chatSessionId, string message)
        {
            // Get the session
            var session = await _sessionRepository.GetByIdAsync(chatSessionId);
            if (session == null)
                throw new InvalidOperationException($"Chat session {chatSessionId} not found.");

            // Save user message
            var userMessage = new ChatMessage
            {
                ChatSessionId = chatSessionId,
                Role = MessageRole.User,
                Content = message
            };
            await _messageRepository.AddAsync(userMessage);

            // Add to session's message collection for context
            session.Messages.Add(userMessage);

            // Process with AI orchestration
            var assistantMessage = await _orchestrationService.ProcessUserMessageAsync(session, message);
            
            // Save assistant response
            await _messageRepository.AddAsync(assistantMessage);

            // Update session title if it's the first message
            if (session.Messages.Count >= 2 && session.Title == "New Chat")
            {
                session.Title = await _orchestrationService.GenerateSessionTitleAsync(message);
                session.ModifiedDate = DateTime.Now;
                await _sessionRepository.UpdateAsync(session);
            }

            return _mapper.Map<ChatMessageDto>(assistantMessage);
        }

        public async Task<List<ChatMessageDto>> GetSessionMessagesAsync(Guid chatSessionId)
        {
            var messages = await _messageRepository.GetBySessionIdAsync(chatSessionId);
            return _mapper.Map<List<ChatMessageDto>>(messages);
        }

        public async Task DeleteSessionAsync(Guid chatSessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(chatSessionId);
            if (session != null)
            {
                await _sessionRepository.DeleteAsync(session);
            }
        }

        public async Task<ChatSessionDto> UpdateSessionLanguageAsync(Guid chatSessionId, ChatLanguage language)
        {
            var session = await _sessionRepository.GetByIdAsync(chatSessionId);
            if (session == null)
                throw new InvalidOperationException($"Chat session {chatSessionId} not found.");

            session.PreferredLanguage = language;
            session.ModifiedDate = DateTime.Now;
            await _sessionRepository.UpdateAsync(session);
            
            return _mapper.Map<ChatSessionDto>(session);
        }
    }
}
