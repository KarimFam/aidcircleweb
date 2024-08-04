using System;

namespace H4H.Application.DTOs
{
    public class ItemDto
    {
      //  public Guid ItemId { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrganizationId { get; set; }
   //     public Guid? ItemId { get; set; }
        public Guid? OrderId { get; set; }
    
    //    public AddressDto Address { get; set; }


    }
}
