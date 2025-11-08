using H4H.Application.Interfaces;
using H4H.Domain.Entities;
using H4H.Domain.Enums;
using H4H.Domain.Interfaces;
using H4H.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Diagnostics;
using System.Text.Json;

namespace H4H.Infrastructure.Services
{
    public class ChatOrchestrationService : IChatOrchestrationService
    {
        private readonly Kernel _kernel;
        private readonly IAzureTranslatorService _translatorService;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IConfiguration _configuration;

        public ChatOrchestrationService(
            IConfiguration configuration,
            IAzureTranslatorService translatorService,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IOrderRepository orderRepository,
            IItemRepository itemRepository)
        {
            _configuration = configuration;
            _translatorService = translatorService;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _orderRepository = orderRepository;
            _itemRepository = itemRepository;

            // Initialize Semantic Kernel with Azure OpenAI
            var builder = Kernel.CreateBuilder();
            
            var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI Endpoint not configured");
            var apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI ApiKey not configured");
            var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey
            );

            // Add agent plugins for AidCircle domain operations
            builder.Plugins.AddFromType<AidCircleDataPlugin>();

            _kernel = builder.Build();
        }

        public async Task<ChatMessage> ProcessUserMessageAsync(ChatSession session, string userMessage)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Step 1: Detect language and translate if needed
                var targetLangCode = session.PreferredLanguage.ToLanguageCode();
                var (detectedLang, translatedInput) = await _translatorService.DetectAndTranslateAsync(userMessage, "en");

                var detectedLanguage = ChatLanguageExtensions.FromLanguageCode(detectedLang);

                // Step 2: Build conversation history for context
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(GetSystemPrompt(session));

                // Add previous messages for context (last 10 messages)
                foreach (var msg in session.Messages.OrderBy(m => m.CreatedDate).TakeLast(10))
                {
                    if (msg.Role == MessageRole.User)
                        chatHistory.AddUserMessage(msg.Content);
                    else if (msg.Role == MessageRole.Assistant)
                        chatHistory.AddAssistantMessage(msg.Content);
                }

                // Add current user message (in English for processing)
                chatHistory.AddUserMessage(translatedInput);

                // Step 3: Get AI response using Semantic Kernel
                var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
                var executionSettings = new AzureOpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 800,
                    TopP = 0.95
                };

                var result = await chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel
                );

                var responseContent = result.Content ?? "I'm sorry, I couldn't generate a response.";
                
                // Extract token usage from metadata
                var tokensUsed = 0;
                if (result.Metadata?.ContainsKey("Usage") == true)
                {
                    var usage = result.Metadata["Usage"];
                    if (usage != null)
                    {
                        // Try different property names depending on SDK version
                        var usageDict = usage as IDictionary<string, object>;
                        if (usageDict != null)
                        {
                            if (usageDict.ContainsKey("TotalTokenCount"))
                                tokensUsed = Convert.ToInt32(usageDict["TotalTokenCount"]);
                            else if (usageDict.ContainsKey("total_tokens"))
                                tokensUsed = Convert.ToInt32(usageDict["total_tokens"]);
                        }
                    }
                }

                // Step 4: Translate response back to user's preferred language if needed
                var finalResponse = responseContent;
                if (session.PreferredLanguage != ChatLanguage.English)
                {
                    finalResponse = await _translatorService.TranslateAsync(
                        responseContent,
                        "en",
                        targetLangCode
                    );
                }

                stopwatch.Stop();

                // Step 5: Create response message
                var assistantMessage = new ChatMessage
                {
                    ChatSessionId = session.ChatSessionId,
                    Role = MessageRole.Assistant,
                    Content = responseContent, // Store English version
                    TranslatedContent = session.PreferredLanguage != ChatLanguage.English ? finalResponse : null,
                    OriginalLanguage = ChatLanguage.English,
                    TargetLanguage = session.PreferredLanguage,
                    TokensUsed = (int)tokensUsed,
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    AgentMetadata = JsonSerializer.Serialize(new
                    {
                        DetectedInputLanguage = detectedLang,
                        TranslationApplied = session.PreferredLanguage != ChatLanguage.English,
                        Model = _configuration["AzureOpenAI:DeploymentName"]
                    })
                };

                return assistantMessage;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new ChatMessage
                {
                    ChatSessionId = session.ChatSessionId,
                    Role = MessageRole.Assistant,
                    Content = $"I encountered an error processing your message: {ex.Message}",
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    AgentMetadata = JsonSerializer.Serialize(new { Error = ex.Message })
                };
            }
        }

        public async Task<string> GenerateSessionTitleAsync(string firstMessage)
        {
            try
            {
                var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
                var prompt = $"Generate a short, descriptive title (max 5 words) for a conversation that starts with: '{firstMessage}'. Only return the title, nothing else.";
                
                var result = await chatCompletion.GetChatMessageContentAsync(prompt);
                return result.Content?.Trim() ?? "New Chat";
            }
            catch
            {
                return "New Chat";
            }
        }

        private string GetSystemPrompt(ChatSession session)
        {
            return @"You are an AI assistant for AidCircle, a philanthropic aid coordination platform. 
Your role is to help users manage volunteers, organizations, items (requests/events/todos), orders, and addresses.

You can:
- Answer questions about the platform and how to use it
- Help users find information about volunteers, organizations, and aid items
- Provide guidance on coordinating aid and resources
- Assist with creating and managing orders

Be helpful, concise, and empathetic. If asked to perform an action beyond your capabilities, 
politely explain what you can do and suggest alternatives.

Current user context:
- Session Language: " + session.PreferredLanguage + @"
- Session Created: " + session.CreatedDate.ToString("g") + @"

Respond naturally and professionally.";
        }
    }

    // Semantic Kernel Plugin for AidCircle domain operations
    public class AidCircleDataPlugin
    {
        // These methods can be called by the AI to retrieve data
        // For now, placeholder methods - can be expanded with actual data access

        [Microsoft.SemanticKernel.KernelFunction, System.ComponentModel.Description("Get summary statistics about the AidCircle platform")]
        public string GetPlatformStats()
        {
            return "AidCircle helps coordinate volunteers, organizations, and aid resources efficiently.";
        }

        [Microsoft.SemanticKernel.KernelFunction, System.ComponentModel.Description("Provide guidance on how to create a new volunteer profile")]
        public string GetVolunteerCreationGuidance()
        {
            return "To create a volunteer profile, navigate to the Volunteers section and click 'Add New Volunteer'. You'll need to provide name, email, skills, and availability.";
        }

        [Microsoft.SemanticKernel.KernelFunction, System.ComponentModel.Description("Explain how orders work in AidCircle")]
        public string ExplainOrders()
        {
            return "Orders in AidCircle aggregate multiple items (requests, events, or todos) that need to be fulfilled. Orders can involve multiple volunteers and organizations, with statuses like Pending, InProgress, Completed, or Cancelled.";
        }
    }
}
