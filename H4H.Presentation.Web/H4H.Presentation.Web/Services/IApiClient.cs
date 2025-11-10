using H4H.Application.DTOs;
using H4H.Domain.Enums;

namespace H4H.Presentation.Web.Services;

/// <summary>
/// API client for communicating with the backend REST API.
/// Supports dual authentication: Azure AD B2C tokens for user operations and API keys for service operations.
/// </summary>
public interface IApiClient
{
    // Chat endpoints
    Task<ChatSessionDto?> CreateChatSessionAsync(ChatLanguage preferredLanguage);
    Task<List<ChatSessionDto>> GetUserChatSessionsAsync(Guid userId);
    Task<ChatSessionDto?> GetChatSessionAsync(Guid sessionId);
    Task<ChatMessageDto?> SendMessageAsync(Guid sessionId, string content);
    Task<List<ChatMessageDto>> GetChatMessagesAsync(Guid sessionId);

    // User endpoints
    Task<UserDto?> GetUserAsync(Guid userId);
    Task<UserDto?> GetUserByExternalAuthIdAsync(string externalAuthId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> CreateUserAsync(UserDto user);
    Task<UserDto?> UpdateUserAsync(Guid userId, UserDto user);
    Task<bool> DeleteUserAsync(Guid userId);

    // Volunteer endpoints
    Task<List<VolunteerDto>> GetAllVolunteersAsync();
    Task<VolunteerDto?> GetVolunteerAsync(Guid volunteerId);
    Task<List<VolunteerDto>> GetOnlineVolunteersAsync();
    Task<VolunteerDto?> CreateVolunteerAsync(VolunteerDto volunteer);
    Task<VolunteerDto?> UpdateVolunteerAsync(Guid volunteerId, VolunteerDto volunteer);
    Task<bool> DeleteVolunteerAsync(Guid volunteerId);

    // Order endpoints
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<List<OrderDto>> GetLatestOrdersAsync();
    Task<OrderDto?> GetOrderAsync(Guid orderId);
    Task<OrderDto?> CreateOrderAsync(OrderDto order);
    Task<OrderDto?> UpdateOrderAsync(Guid orderId, OrderDto order);
    Task<bool> DeleteOrderAsync(Guid orderId);

    // Item endpoints
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<ItemDto?> GetItemAsync(Guid itemId);
    Task<ItemDto?> CreateItemAsync(ItemDto item);
    Task<ItemDto?> UpdateItemAsync(Guid itemId, ItemDto item);
    Task<bool> DeleteItemAsync(Guid itemId);

    // Organization endpoints
    Task<List<OrganizationDto>> GetAllOrganizationsAsync();
    Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId);
    Task<OrganizationDto?> CreateOrganizationAsync(OrganizationDto organization);
    Task<OrganizationDto?> UpdateOrganizationAsync(Guid organizationId, OrganizationDto organization);
    Task<bool> DeleteOrganizationAsync(Guid organizationId);

    // Address endpoints
    Task<List<AddressDto>> GetAllAddressesAsync();
    Task<AddressDto?> GetAddressAsync(Guid addressId);
    Task<AddressDto?> CreateAddressAsync(AddressDto address);
    Task<AddressDto?> UpdateAddressAsync(Guid addressId, AddressDto address);
    Task<bool> DeleteAddressAsync(Guid addressId);

    // Health check
    Task<bool> CheckHealthAsync();
}
