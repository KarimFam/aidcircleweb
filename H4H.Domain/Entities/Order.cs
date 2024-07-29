using H4H.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace H4H.Domain.Entities
{
    public class Order : BaseEntity
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }
   
        [ForeignKey("UserId")]
        [JsonPropertyName("user")]
        public virtual User User { get; set; }

        [JsonPropertyName("items")]
        [InverseProperty("Order")]
        public virtual ICollection<Item> Items { get; set; }
       
        // EF Core does not directly support many-to-many without a joining entity
        // Consider defining a join entity or table configuration in OnModelCreating
   

        [JsonPropertyName("volunteers")]
        public virtual ICollection<Volunteer> Volunteers { get; set; }

        // Same note as for volunteers
        
        public virtual ICollection<Address> Addresses { get; set; }

        public Order()
        {
            Items = new HashSet<Item>();
            Volunteers = new HashSet<Volunteer>();
            Addresses = new HashSet<Address>();
            
        }
    }
}


}

