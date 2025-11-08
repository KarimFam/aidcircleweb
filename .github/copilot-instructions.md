# AidCircle - Comprehensive AI Coding Agent Instructions

## Project Overview & Purpose
AidCircle is a full-stack philanthropic aid coordination platform built on .NET 8 with clean architecture principles. The system connects volunteers, organizations, and users to streamline resource management, order fulfillment, and community-driven aid coordination. Enhanced with multi-language AI chat capabilities powered by Azure OpenAI and Microsoft Semantic Kernel.

### Core Mission
- **Facilitate Collaboration**: Connect volunteers, organizations, and end-users on a single platform
- **Streamline Resource Management**: Manage items and orders efficiently, ensuring resources reach those in need
- **Enhance Communication**: Provide clear channels for collaboration with AI-powered assistance
- **Promote Accountability**: Transparent tracking of items, orders, and volunteer efforts

### Domain Model
The platform manages the following core entities:
- **Users**: Base entity with authentication, addresses, items, orders, and chat sessions
- **Volunteers**: Extends User with skills tracking and organization assignments
- **Organizations**: Manages multiple addresses, items, and orders
- **Items**: Categorized as Todo/Request/Event (ItemType enum), assigned to volunteers/organizations
- **Orders**: Aggregates items with status tracking (RequestStatus enum), single user ownership
- **Addresses**: Shared across Users, Organizations, Volunteers, and Items (AddressType enum)
- **ChatSessions/ChatMessages**: AI-powered multi-language chat system (NEW)


## Clean Architecture - Layer Responsibilities & Rules

### 1. Domain Layer (`H4H.Domain`) - Pure Business Logic
**Purpose**: Core business entities, enums, repository interfaces - ZERO external dependencies

**Entities**:
- Core: `User`, `Volunteer`, `Organization`, `Item`, `Order`, `Address`
- AI Chat: `ChatSession`, `ChatMessage`
- All inherit from `BaseEntity` (CreatedDate, ModifiedDate)

**Enums**:
- `ItemType`: Todo, Request, Event
- `RequestStatus`: Pending, InProgress, Completed, Cancelled
- `AddressType`: Home, Work, Billing, Shipping, Organization
- `RoleType`: Admin, Volunteer, User
- `ChatLanguage`: 11 languages (English, Spanish, French, German, Chinese, Arabic, Portuguese, Russian, Japanese, Korean, Hindi) with `[EnumMember]` and `[JsonPropertyName]` attributes
- `MessageRole`: User, Assistant, System

**Interfaces**: Repository contracts (e.g., `IUserRepository`, `IChatSessionRepository`)

**Rules**:
- NO references to Application, Infrastructure, or Presentation layers
- NO EF Core, Azure SDK, or external library dependencies
- Pure C# POCOs representing business concepts

### 2. Application Layer (`H4H.Application`) - Orchestration & Contracts
**Purpose**: Services, DTOs, AutoMapper profiles, orchestration interfaces

**Services**:
- Business services: `UserService`, `OrderService`, `ItemService`, `OrganizationService`, `VolunteerService`, `AddressService`
- AI services: `ChatService` - thin wrapper coordinating chat operations
- **Pattern**: Services are pass-through wrappers over repositories - NO complex business logic

**Interfaces**:
- Service contracts: `IUserService`, `IChatService`
- **Critical**: `IChatOrchestrationService` (implemented in Infrastructure but interface here for clean architecture)

**DTOs**:
- All entities have corresponding DTOs (e.g., `UserDto`, `ChatSessionDto`)
- DTOs for requests: `SendMessageRequest`, `CreateChatSessionRequest`

**Mappers**:
- `MappingProfile.cs`: AutoMapper bidirectional mappings for all entities ↔ DTOs

**Rules**:
- References ONLY Domain layer
- Infrastructure implements interfaces defined here
- Services delegate to repositories, minimal logic

### 3. Infrastructure Layer (`H4H.Infrastructure`) - Data & External Services
**Purpose**: EF Core implementations, Azure AI services, orchestration, external integrations

**Data Layer**:
- `H4HDbContext`: EF Core context with all entity DbSets
- Connection string: `"H4HDB-DEV"` → Azure SQL Database
- Migrations: Located in `Migrations/` folder

**Repositories**:
- Implement Domain interfaces (e.g., `UserRepository : IUserRepository`)
- **Critical Pattern**: Call `SaveChangesAsync()` immediately after Add/Update/Delete
- Use `.Include()` for navigation properties in Get methods

**AI Services** (NEW):
- `AzureTranslatorService`: Language detection/translation using Azure AI Translator
  - Methods: `DetectAndTranslateAsync()`, `TranslateAsync()`, `DetectLanguageAsync()`
  - Extension methods: `ToLanguageCode()`, `FromLanguageCode()` for ChatLanguage enum
  
- `ChatOrchestrationService`: Semantic Kernel orchestration layer
  - Methods: `ProcessUserMessageAsync()`, `GenerateSessionTitleAsync()`
  - Workflow: Detect language → Translate to English → Build chat history → Invoke Semantic Kernel → Call agent plugins → Translate response → Track metrics
  - Agent plugins: `AidCircleDataPlugin` with `[KernelFunction]` attributes for domain-specific guidance

**Rules**:
- References both Domain AND Application layers (for orchestration interfaces)
- Implements repository interfaces from Domain
- Implements service interfaces from Application (e.g., `IChatOrchestrationService`)

### 4. Presentation Layer - User Interfaces & APIs
**`H4H.Presentation.API`**: REST API with Swagger
- Controllers: `UserController`, `OrderController`, `ChatController` (NEW)
- Pattern: Controller → Service → Repository
- Endpoints use DTOs for requests/responses

**`H4H.Presentation.Web`**: Blazor Server/WASM Hybrid
- Server components: Pages, shared layouts
- WebAssembly components: Client-side interactive components
- Components: `ChatComponent.razor` (NEW) - interactive AI chat UI
- Authentication: Azure AD B2C with OpenID Connect

**Rules**:
- References Application (services) and Infrastructure (DI setup)
- Uses DTOs for data transfer
- Controllers/Components inject services via DI

### Dependency Flow (CRITICAL)
```
Domain (no dependencies)
   ↑
Application (references Domain only)
   ↑
Infrastructure (references Domain + Application)
   ↑
Presentation (references Application + Infrastructure for DI)
```

**Why this matters**:
- Application can define interfaces (like `IChatOrchestrationService`) that Infrastructure implements
- Presentation depends on Application interfaces, not Infrastructure implementations
- Swappable implementations (e.g., SQL Server ↔ Cosmos DB) without changing Application layer

## AI Chat Architecture

### Multi-Language AI System
AidCircle features an intelligent AI assistant with:
- **Language Detection**: Automatic detection of input language via Azure Translator
- **Multi-Language Support**: 11 languages (English, Spanish, French, German, Chinese, Arabic, Portuguese, Russian, Japanese, Korean, Hindi)
- **Real-Time Translation**: Seamless translation between user's preferred language and English (processing language)
- **Context-Aware Responses**: AI maintains conversation history for contextual understanding
- **Domain-Specific Agents**: Semantic Kernel plugins provide AidCircle-specific guidance

### Orchestration with Microsoft Semantic Kernel
The `ChatOrchestrationService` acts as the AI orchestration layer:
```csharp
// Semantic Kernel setup with Azure OpenAI
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o-mini",  // Cost-effective model
    endpoint: azureEndpoint,
    apiKey: apiKey
);
builder.Plugins.AddFromType<AidCircleDataPlugin>();  // Domain agents
```

**Agent Capabilities** (via Semantic Kernel Functions):
- Platform statistics and guidance
- Volunteer creation workflows
- Order management explanations
- Extensible plugin architecture for new capabilities

### Chat Data Flow
1. User sends message in any language → `ChatComponent.razor`
2. `ChatService.SendMessageAsync()` stores user message
3. `ChatOrchestrationService.ProcessUserMessageAsync()`:
   - Detects language → Azure Translator
   - Translates to English (if needed)
   - Builds chat history for context
   - Invokes Semantic Kernel with GPT-4o-mini
   - AI can call agent plugins for domain data
   - Translates response back to user's language
   - Tracks tokens, execution time, metadata
4. Response stored and returned to UI

### Chat Entities
**ChatSession**:
- User-specific chat sessions
- Preferred language setting
- Auto-generated title from first message
- Multiple messages per session

**ChatMessage**:
- Role (User/Assistant/System)
- Original content (always English for Assistant)
- Translated content (user's language)
- Performance metrics (tokens, execution time)
- Agent metadata (JSON for execution details)

## Critical Patterns & Conventions

### Entity Design
All entities inherit from `BaseEntity` with `CreatedDate` and `ModifiedDate`:
```csharp
public abstract class BaseEntity {
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
```

Key entities auto-generate GUIDs in constructors and set timestamps:
```csharp
public ChatSession() {
    ChatSessionId = Guid.NewGuid();
    CreatedDate = DateTime.Now;
    ModifiedDate = DateTime.Now;
    IsActive = true;
    PreferredLanguage = ChatLanguage.English;
}
```

### Service Pattern (Application Layer)
Services are thin wrappers over repositories - NO complex business logic:
```csharp
public class ChatService : IChatService {
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatOrchestrationService _orchestrationService;
    
    public async Task<ChatMessage> SendMessageAsync(Guid sessionId, string message) {
        var userMessage = new ChatMessage { Content = message, Role = MessageRole.User };
        await _messageRepository.AddAsync(userMessage);
        
        var response = await _orchestrationService.ProcessUserMessageAsync(session, message);
        await _messageRepository.AddAsync(response);
        return response;
    }
}
```

### Repository Pattern (Infrastructure Layer)
Repositories call `SaveChangesAsync()` immediately after modifications:
```csharp
public async Task AddAsync(ChatMessage entity) {
    await _context.ChatMessages.AddAsync(entity);
    await _context.SaveChangesAsync(); // Always called in repository
}
```

### Azure Service Configuration
Required in `appsettings.json`:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR-KEY",
    "DeploymentName": "gpt-4o-mini"
  },
  "AzureTranslator": {
    "Key": "YOUR-KEY",
    "Endpoint": "https://api.cognitive.microsofttranslator.com",
    "Region": "eastus"
  }
}
```

### Dependency Injection for AI Services
```csharp
// Singleton for stateless translation
builder.Services.AddSingleton<IAzureTranslatorService, AzureTranslatorService>();

// Scoped for per-request orchestration
builder.Services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Repositories
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
```

## Naming Conventions
- **Classes/Methods/Properties**: PascalCase
- **Parameters/Variables**: camelCase
- **Interfaces**: Prefix with `I` (e.g., `IChatService`, `IChatOrchestrationService`)
- **Primary Keys**: `{EntityName}Id` (e.g., `ChatSessionId`, `ChatMessageId`)
- **Async Methods**: Suffix with `Async`

## Database & Configuration

### Connection String
Located in `appsettings.json` as `"H4HDB-DEV"` pointing to Azure SQL Database.

### DbContext Setup
`H4HDbContext` registers all entities as DbSets:
```csharp
public DbSet<ChatSession> ChatSessions { get; set; }
public DbSet<ChatMessage> ChatMessages { get; set; }
```

### Migrations for AI Features
```powershell
# Add new migration
dotnet ef migrations add AddChatEntities --project H4H.Infrastructure --startup-project H4H.Presentation.API

# Apply to database
dotnet ef database update --project H4H.Infrastructure --startup-project H4H.Presentation.API
```

## Domain Model Relationships

### Core Entities
- **User**: Base entity with addresses, items, orders, and chat sessions
- **Volunteer**: Inherits User, adds skills tracking
- **Organization**: Has multiple addresses and orders
- **Item**: Categorized by `ItemType` enum (Todo/Request/Event)
- **Order**: Aggregates items with `RequestStatus` enum
- **Address**: Shared across Users, Organizations, and Items

### AI Chat Entities
- **ChatSession**: Belongs to User, has many ChatMessages, stores preferred language
- **ChatMessage**: Belongs to ChatSession, stores original + translated content, performance metrics

### Enums with JSON Attributes
Enums use both `[EnumMember]` and `[JsonPropertyName]` for serialization:
```csharp
[EnumMember(Value = "English")]
[JsonPropertyName("en")]
English
```

## Development Workflows

### Adding a New Semantic Kernel Plugin
1. Create class in `H4H.Infrastructure/Services/Plugins`
2. Add `[KernelFunction]` and `[Description]` attributes:
```csharp
public class AidCircleDataPlugin {
    [KernelFunction, Description("Get volunteer statistics")]
    public async Task<string> GetVolunteerStats() {
        // Implementation
    }
}
```
3. Register in `ChatOrchestrationService`:
```csharp
builder.Plugins.AddFromType<YourNewPlugin>();
```

### Testing AI Chat Locally
1. Set Azure OpenAI and Translator keys in `appsettings.json`
2. Run migrations to create chat tables
3. Navigate to `/chat` in Blazor Web app
4. Test multi-language by changing language selector
5. Monitor console for token usage and execution time

### Switching to Cosmos DB (Future)
The repository pattern supports swapping SQL Server for Cosmos DB:
1. Create new CosmosDB repositories implementing same interfaces
2. Update DI registration in `Program.cs`
3. No changes needed in Application or Presentation layers

## Technology Stack
- .NET 8.0 with C# implicit usings and nullable reference types enabled
- Entity Framework Core 8.0.7 with SQL Server
- **Microsoft Semantic Kernel 1.25.0** - AI orchestration and agents
- **Azure OpenAI** - GPT-4o-mini for cost-effective conversational AI
- **Azure AI Translator** - Multi-language support
- AutoMapper 13.0.1
- Blazor Server + WebAssembly (hybrid rendering)
- Microsoft Identity Web (Azure AD B2C)

## Key Files Reference
- **AI Orchestration**: `H4H.Infrastructure/Services/ChatOrchestrationService.cs`
- **Translation**: `H4H.Infrastructure/Services/AzureTranslatorService.cs`
- **Chat Service**: `H4H.Application/Services/ChatService.cs`
- **Chat UI**: `H4H.Presentation.Web/Components/Pages/ChatComponent.razor`
- **API Endpoints**: `H4H.Presentation.API/Controllers/ChatController.cs`
- **DbContext**: `H4H.Infrastructure/Data/Contexts/H4HDbContext.cs`
- **Mapping**: `H4H.Application/Mappers/MappingProfile.cs`

## Authentication & Authorization (Azure AD B2C)

### Azure AD B2C Configuration
AidCircle uses **Azure AD B2C** for identity management with OpenID Connect authentication. This provides secure, scalable user authentication with social login support.

**Configuration Location**: `H4H.Presentation.Web/appsettings.json`
```json
{
  "AzureAdB2C": {
    "Instance": "https://aidcirclenet.b2clogin.com/",
    "ClientId": "36bf28fa-d15c-4762-8c32-8e46c3aa9051",
    "Domain": "aidcirclenet.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1_susi"
  }
}
```

**Key Configuration Elements**:
- **Instance**: B2C tenant login endpoint (`https://{tenant}.b2clogin.com/`)
- **ClientId**: Application (client) ID from Azure AD B2C app registration
- **Domain**: B2C tenant domain (`{tenant}.onmicrosoft.com`)
- **SignUpSignInPolicyId**: User flow name for combined sign-up/sign-in (e.g., `B2C_1_susi`)

### Authentication Setup in Program.cs
Located in `H4H.Presentation.Web/H4H.Presentation.Web/Program.cs` (lines ~48-110):

```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options => {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.Events = new OpenIdConnectEvents {
            OnTicketReceived = async ctxt => {
                // Extract claims from Azure AD B2C token
                var identity = ctxt.Principal.Identity as ClaimsIdentity;
                var claims = await ctxt.Principal.Claims.ToDynamicListAsync();
                
                // Available claims:
                // - identityprovider: Authentication provider (e.g., Google, Facebook)
                // - nameidentifier: Unique user object identifier
                // - emailaddress: User's email
                // - givenname: First name
                // - surname: Last name
                // - authnclassreference: B2C user flow used
                // - name: Display name
            }
        };
    });
```

**Critical OpenIdConnect Events**:
- **OnRedirectToIdentityProvider**: Customize parameters before redirecting to B2C
- **OnAuthenticationFailed**: Handle login failures
- **OnSignedOutCallbackRedirect**: Custom redirect after sign-out
- **OnTicketReceived**: Process claims after successful authentication (extract user profile data, create/update user records)

### Authorization Policies
**Default Policy** (lines ~39-44):
```csharp
builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```
All pages require authentication by default.

**Custom "Volunteer" Policy** (lines ~46-49):
```csharp
builder.Services.AddAuthorization(config => {
    config.AddPolicy("Volunteer", policy => 
        policy.RequireClaim("IsVolunteer", "true"));
});
```
Use `[Authorize(Policy = "Volunteer")]` on pages/controllers requiring volunteer role.

### Blazor Authentication Integration
```csharp
// Enable cascading authentication state (line ~36)
builder.Services.AddCascadingAuthenticationState();

// HTTP context access for authentication (lines ~37-38)
builder.Services.AddHttpContextAccessor();
```

**Usage in Components**:
```razor
@using Microsoft.AspNetCore.Components.Authorization
<AuthorizeView>
    <Authorized>
        <p>Hello, @context.User.Identity?.Name!</p>
    </Authorized>
    <NotAuthorized>
        <p>Please log in.</p>
    </NotAuthorized>
</AuthorizeView>
```

### Common Claims Extraction Pattern
```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var email = User.FindFirstValue(ClaimTypes.Email);
var firstName = User.FindFirstValue(ClaimTypes.GivenName);
var isVolunteer = User.HasClaim("IsVolunteer", "true");
```

**CRITICAL: User Lookup Pattern**
Never use hardcoded user IDs or assume claims contain GUIDs. Azure AD External ID returns string identifiers, not GUIDs. Always look up users in the database:

```csharp
// CORRECT: Look up user by external auth ID
var externalAuthIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
if (externalAuthIdClaim == null)
{
    throw new InvalidOperationException("User claim 'nameidentifier' not found.");
}

var appUser = await UserService.GetByExternalAuthIdAsync(externalAuthIdClaim.Value);
if (appUser == null)
{
    throw new InvalidOperationException("User not found in database.");
}

Guid currentUserId = appUser.UserId; // Now we have the internal GUID

// WRONG: Hardcoded fallback (security vulnerability)
currentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // ❌ NEVER DO THIS
```

**Authentication Requirements**:
- All chat pages require `[Authorize]` attribute
- Unauthenticated users redirected to login
- Users must exist in database (linked to Azure AD External ID)
- No fallback to demo/default user IDs in production code

## Historical Context & Lessons Learned

### Evolution of the Codebase
1. **Initial Architecture**: Started with basic clean architecture (Domain, Application, Infrastructure, Presentation)
2. **Authentication Addition**: Integrated Azure AD B2C for secure user management
3. **Weather Service Experiment**: Temporarily added weather service as proof-of-concept (later removed)
4. **AI Chat Revolution**: Major enhancement adding multi-language AI chat with Azure OpenAI and Semantic Kernel

### Critical Lessons Learned

#### 1. Clean Architecture Dependency Violations
**Problem**: Initially tried to have `ChatService` (Application layer) directly reference `ChatOrchestrationService` (Infrastructure layer), violating clean architecture.

**Solution**: Moved `IChatOrchestrationService` interface to Application layer (`H4H.Application/Interfaces/`), while implementation stays in Infrastructure. Application depends on interface, Infrastructure implements it.

**Rule**: When Infrastructure needs to provide services to Application, define the interface in Application layer. Infrastructure can reference Application for interfaces only.

#### 2. Repository SaveChanges Pattern
**Problem**: Inconsistent `SaveChangesAsync()` calls led to data not persisting.

**Solution**: **ALWAYS** call `SaveChangesAsync()` immediately after `AddAsync()`, `Update()`, or `Remove()` in repository methods. Never defer to service layer.

**Pattern**:
```csharp
public async Task AddAsync(Entity entity) {
    await _context.Entities.AddAsync(entity);
    await _context.SaveChangesAsync(); // REQUIRED
}
```

#### 3. Azure SDK Version Compatibility
**Problem**: `AzureTranslatorService` constructor initially used wrong `TextTranslationClient` signature (3 parameters instead of 2).

**Solution**: Always verify Azure SDK API signatures—they change between versions. Current pattern:
```csharp
var credential = new AzureKeyCredential(apiKey);
var client = new TextTranslationClient(credential, endpoint); // 2 params
```

#### 4. Nullable Reference Type Warnings
**Problem**: Build generates warnings on entity navigation properties (e.g., `User.Addresses`) being potentially null.

**Solution**: Accept warnings on entities (47 warnings currently) or add null-forgiving operators. Focus on fixing nullability in services/controllers where it impacts runtime.

#### 5. JSON Configuration Comments
**Problem**: `appsettings.json` had inline comments (`//`) which are invalid in strict JSON parsers.

**Solution**: Remove all `//` comments from JSON files. Use separate documentation or `appsettings.Development.json` for notes.

#### 6. EF Core Include Pattern for Navigation Properties
**Problem**: Lazy loading not enabled, navigation properties returned null when not explicitly included.

**Solution**: Use `.Include()` in repository Get methods:
```csharp
public async Task<ChatSession?> GetByIdAsync(Guid id) {
    return await _context.ChatSessions
        .Include(s => s.Messages)
        .Include(s => s.User)
        .FirstOrDefaultAsync(s => s.ChatSessionId == id);
}
```

### Architectural Decisions & Rationale

#### Why SQL Server First, Cosmos DB Ready?
- **SQL Server**: Proven relational model for Users → Orders → Items relationships
- **Future Cosmos DB**: If scale demands (millions of chat messages), repository pattern allows swapping without touching Application/Presentation layers
- **Trade-off**: Accepting slight over-engineering now for future flexibility

#### Why Semantic Kernel Over LangChain?
- **Microsoft Ecosystem**: Tight integration with Azure OpenAI, .NET native
- **Agent Plugin Architecture**: `[KernelFunction]` attributes cleaner than LangChain's chains/agents
- **Cost Efficiency**: GPT-4o-mini recommended as cost-effective (vs GPT-4)
- **Trade-off**: Less Python community support than LangChain

#### Why Thin Service Layer?
- **Pattern**: Services are pass-through wrappers over repositories
- **Rationale**: Business logic belongs in Domain entities or orchestration services (like `ChatOrchestrationService`), not in CRUD service classes
- **Trade-off**: Services may seem redundant, but provide DTO mapping and future extension points

#### Why AutoMapper Over Manual Mapping?
- **DRY Principle**: Avoid repetitive entity-to-DTO mapping code
- **Maintainability**: Single `MappingProfile.cs` file for all mappings
- **Trade-off**: Slight performance overhead, "magic" can hide mapping errors

#### Why Blazor Hybrid (Server + WASM)?
- **Server-Side**: Fast initial load, secure server-side operations
- **WASM**: Client-side interactivity where needed (e.g., chat UI)
- **Trade-off**: Complexity of managing two rendering modes

#### Why Multi-Language Translation Instead of Native Multi-Language LLM?
- **User Experience**: Users send/receive messages in their native language
- **Cost**: Azure Translator cheaper than multi-lingual LLM training/prompting
- **Context**: English-only chat history simplifies Semantic Kernel context management
- **Trade-off**: Extra Azure service dependency, translation latency

## Common Pitfalls & Troubleshooting

### Build Errors
1. **"Cannot find IChatOrchestrationService"**: Ensure `H4H.Infrastructure.csproj` references `H4H.Application` project
2. **"Circular dependency"**: Check layer references—Application should NEVER reference Infrastructure
3. **"Missing using directive"**: Add `using H4H.Infrastructure.Services;` in `Program.cs` files

### Runtime Errors
1. **"Navigation property is null"**: Add `.Include()` in repository method
2. **"SaveChangesAsync not called"**: Repository must call it, not service
3. **"Invalid JSON configuration"**: Remove `//` comments from `appsettings.json`

### Migration Issues
1. **"Build failed during migration"**: Run `dotnet build` first to verify code compiles
2. **"Connection string not found"**: Ensure `appsettings.json` has `"H4HDB-DEV"` connection string
3. **"Migration already exists"**: Delete migration file and run `dotnet ef migrations remove` first

### Authentication Issues
1. **"Redirect loop"**: Check `AzureAdB2C:Instance` ends with `/` (e.g., `https://tenant.b2clogin.com/`)
2. **"Invalid client"**: Verify `ClientId` matches Azure AD B2C app registration
3. **"Policy not found"**: Ensure `SignUpSignInPolicyId` matches user flow name in Azure portal (case-sensitive)

### AI Chat Issues
1. **"401 Unauthorized from Azure OpenAI"**: Verify `ApiKey` in `appsettings.json`
2. **"Deployment not found"**: Ensure `DeploymentName` matches Azure OpenAI model deployment name
3. **"Translation failed"**: Check `AzureTranslator:Key` and `Region` are correct
4. **"High token usage"**: Review chat history size—consider truncating old messages in `ChatOrchestrationService.ProcessUserMessageAsync()`

## Development Workflows (Expanded)

### Adding a New Entity
1. Create entity in `H4H.Domain/Entities/` (inherit `BaseEntity`)
2. Add repository interface in `H4H.Domain/Interfaces/`
3. Create repository implementation in `H4H.Infrastructure/Repositories/`
4. Create DTO in `H4H.Application/DTOs/`
5. Add mapping in `H4H.Application/Mappers/MappingProfile.cs`
6. Create service interface in `H4H.Application/Interfaces/`
7. Create service in `H4H.Application/Services/`
8. Register in DI (`Program.cs`): service + repository
9. Add DbSet to `H4HDbContext`
10. Create migration: `dotnet ef migrations add AddNewEntity --project H4H.Infrastructure --startup-project H4H.Presentation.API`
11. Apply migration: `dotnet ef database update --project H4H.Infrastructure --startup-project H4H.Presentation.API`

### Adding a New API Endpoint
1. Add method to service interface (Application layer)
2. Implement method in service class
3. Create controller action in `H4H.Presentation.API/Controllers/`
4. Use DTOs for request/response
5. Test with Swagger UI (`https://localhost:5001/swagger`)

### Adding a New Blazor Page
1. Create `.razor` file in `H4H.Presentation.Web/Components/Pages/`
2. Add `@page "/route"` directive
3. Inject services: `@inject IUserService UserService`
4. Add authentication: `@attribute [Authorize]`
5. Test navigation from existing page

### Debugging AI Chat
1. **Enable verbose logging**: Add breakpoints in `ChatOrchestrationService.ProcessUserMessageAsync()`
2. **Monitor tokens**: Check `result.Metadata["Usage"]` for token consumption
3. **Test translation**: Use `AzureTranslatorService.DetectLanguageAsync()` directly
4. **Verify plugin execution**: Add logging in `AidCircleDataPlugin` methods

### Testing Multi-Language Support
1. Navigate to `/chat` in Blazor Web
2. Select non-English language from dropdown
3. Send message in selected language
4. Verify response is in same language
5. Check database: `Content` should be English, `TranslatedContent` should be user's language

## What's NOT in This Codebase (Current Limitations)
- **No Unit/Integration Tests**: Zero test coverage (future work)
- **No Logging**: `ILogger` imported but not implemented (console output only)
- **No Advanced RAG**: No vector stores, embeddings, or retrieval-augmented generation
- **No Streaming Responses**: Chat waits for full AI response (no real-time streaming)
- **No Session Sharing**: Users can't share chat sessions with others
- **No Fine-Tuned Models**: Using base GPT-4o-mini (no custom training data)
- **No Rate Limiting**: No throttling on API endpoints or AI requests
- **No Caching**: Every AI request hits Azure OpenAI (no response caching)
- **No Offline Mode**: Requires internet for Azure services
- **No Multi-Tenancy**: Single database for all organizations (future: tenant isolation)

## Quick Reference Commands

### EF Core Migrations
```powershell
# Add migration
dotnet ef migrations add MigrationName --project H4H.Infrastructure --startup-project H4H.Presentation.API

# Apply migration
dotnet ef database update --project H4H.Infrastructure --startup-project H4H.Presentation.API

# Rollback migration
dotnet ef database update PreviousMigrationName --project H4H.Infrastructure --startup-project H4H.Presentation.API

# Remove last migration
dotnet ef migrations remove --project H4H.Infrastructure --startup-project H4H.Presentation.API
```

### Build & Run
```powershell
# Build solution
dotnet build H4H.sln

# Run API
dotnet run --project H4H.Presentation.API

# Run Blazor Web
dotnet run --project H4H.Presentation.Web/H4H.Presentation.Web

# Run both (separate terminals)
# Terminal 1: dotnet run --project H4H.Presentation.API
# Terminal 2: dotnet run --project H4H.Presentation.Web/H4H.Presentation.Web
```

### NuGet Package Management
```powershell
# Add package to Infrastructure (example)
dotnet add H4H.Infrastructure/H4H.Infrastructure.csproj package PackageName

# Restore packages
dotnet restore H4H.sln

# List outdated packages
dotnet list package --outdated
```

## Summary for AI Agents
When working on this codebase:
1. **Respect clean architecture layers**: Domain → Application → Infrastructure → Presentation
2. **Always call SaveChangesAsync() in repositories** immediately after modifications
3. **Use Include() for navigation properties** in repository Get methods
4. **Define orchestration interfaces in Application layer** when Infrastructure provides services to Application
5. **Keep services thin** - they're wrappers, not business logic containers
6. **Preserve Azure AD B2C configuration** - authentication is production-ready
7. **Test AI chat with multiple languages** - translation is core feature
8. **Document architectural decisions** - explain WHY, not just WHAT
9. **Learn from past mistakes** - circular dependencies, SaveChanges omissions, SDK version mismatches
10. **Think future-ready** - code is SQL Server now but switchable to Cosmos DB via repository pattern
