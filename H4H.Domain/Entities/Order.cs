using H4H.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace H4H.Domain.Entities
{
    public class Order : BaseEntity
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        //[JsonPropertyName("userId")]
        // public Guid UserId { get; set; }

        //[ForeignKey("UserId")]
        // [JsonPropertyName("user")]
        //public virtual User User { get; set; }
        public Guid? ItemId { get; set; }


        [JsonPropertyName("items")]
        [InverseProperty("Order")]
        public virtual ICollection<Item> Items { get; set; }



        //   [JsonPropertyName("volunteers")]
      public virtual ICollection<User> Users { get; set; }

        // Same note as for volunteers

        public virtual ICollection<Address> Addresses { get; set; }

        public Order()
        {
            Items = new HashSet<Item>();
         Users = new HashSet<User>();
            Addresses = new HashSet<Address>();
            
        }
    }
}




