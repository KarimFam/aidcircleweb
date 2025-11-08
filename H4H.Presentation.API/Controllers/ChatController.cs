using Microsoft.AspNetCore.Mvc;
using H4H.Application.Interfaces;
using H4H.Application.DTOs;
using AutoMapper;
using H4H.Domain.Enums;

namespace H4H.Presentation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;

        public ChatController(IChatService chatService, IMapper mapper)
        {
            _chatService = chatService;
            _mapper = mapper;
        }

        // POST: api/chat/sessions
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionRequest request)
        {
            if (request == null || request.UserId == Guid.Empty)
            {
                return BadRequest("Invalid request. UserId is required.");
            }

            var session = await _chatService.CreateSessionAsync(request.UserId, request.PreferredLanguage);
            var sessionDto = _mapper.Map<ChatSessionDto>(session);
            return CreatedAtAction(nameof(GetSessionById), new { sessionId = session.ChatSessionId }, sessionDto);
        }

        // GET: api/chat/sessions/{sessionId}
        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionById(Guid sessionId)
        {
            var session = await _chatService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Chat session {sessionId} not found.");
            }

            var sessionDto = _mapper.Map<ChatSessionDto>(session);
            return Ok(sessionDto);
        }

        // GET: api/chat/users/{userId}/sessions
        [HttpGet("users/{userId}/sessions")]
        public async Task<IActionResult> GetUserSessions(Guid userId)
        {
            var sessions = await _chatService.GetUserSessionsAsync(userId);
            var sessionDtos = _mapper.Map<List<ChatSessionDto>>(sessions);
            return Ok(sessionDtos);
        }

        // POST: api/chat/messages
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var assistantMessage = await _chatService.SendMessageAsync(request.ChatSessionId, request.Message);
                var messageDto = _mapper.Map<ChatMessageDto>(assistantMessage);
                return Ok(messageDto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing message: {ex.Message}");
            }
        }

        // GET: api/chat/sessions/{sessionId}/messages
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetSessionMessages(Guid sessionId)
        {
            var messages = await _chatService.GetSessionMessagesAsync(sessionId);
            var messageDtos = _mapper.Map<List<ChatMessageDto>>(messages);
            return Ok(messageDtos);
        }

        // DELETE: api/chat/sessions/{sessionId}
        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            try
            {
                await _chatService.DeleteSessionAsync(sessionId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting session: {ex.Message}");
            }
        }

        // PUT: api/chat/sessions/{sessionId}/language
        [HttpPut("sessions/{sessionId}/language")]
        public async Task<IActionResult> UpdateSessionLanguage(Guid sessionId, [FromBody] ChatLanguage language)
        {
            try
            {
                var updatedSession = await _chatService.UpdateSessionLanguageAsync(sessionId, language);
                return Ok(updatedSession);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating session language: {ex.Message}");
            }
        }
    }
}
