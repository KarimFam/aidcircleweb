// MappingProfile.cs
using AutoMapper;
using H4H.Domain.Entities;
using H4H.Presentation.API.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<Item, ItemDto>().ReverseMap();
    }
}
