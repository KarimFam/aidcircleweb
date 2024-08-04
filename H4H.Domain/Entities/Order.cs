using H4H.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace H4H.Domain.Entities
{
    public class Order : BaseEntity
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }
        
        public List<Item> Items { get; set; } = new List<Item>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Organization> Organizations { get; set; } = new List<Organization>();
        public Order()
        {
            OrderId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

    } 
}




