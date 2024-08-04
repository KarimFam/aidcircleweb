using H4H.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Entities
{
    public class Item : BaseEntity
    {
        [JsonPropertyName("itemId")]
        public Guid ItemId { get; set; }

        [JsonPropertyName("name")]
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        //public Guid? AddressId { get; set; }

        public Address? Address { get; set; }

        public Guid? OrderId  { get; set; }

        public Order? Order { get; set; }

        public Guid? UserId { get; set; }   

        public User? User { get; set; }

        public Item()
        {
            ItemId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

    }
   

}
