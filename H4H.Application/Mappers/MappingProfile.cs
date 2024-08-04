// MappingProfile.cs
using AutoMapper;
using H4H.Domain.Entities;
using H4H.Application.DTOs; 

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<Item, ItemDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Volunteer, VolunteerDto>().ReverseMap();
        CreateMap<Organization, OrganizationDto>().ReverseMap();
        CreateMap<Order, OrderDto>().ReverseMap();
    }
}
