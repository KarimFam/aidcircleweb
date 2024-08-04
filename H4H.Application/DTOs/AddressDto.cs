// Models/AddressDto.cs
using System;

namespace H4H.Application.DTOs
{
    public class AddressDto
    {
      //  public Guid AddressId { get; set; }
       // public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
      //  public string ZipCode { get; set; }

        public string Country {  get; set; }
        public string PostalCode   { get; set; }
        public string Line1     { get; set; }
        public string Line2 { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? ItemId { get; set; }
        public Guid? OrderId { get; set; }
    }
}
