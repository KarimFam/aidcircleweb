using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json.Serialization;


namespace H4H.Domain.Entities
{
    public class Organization : BaseEntity
    {
        [JsonPropertyName("organizationId")]
        public Guid OrganizationId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        // Relationships
        [JsonPropertyName("addresses")]
        public Guid? AddressId { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }

        [JsonPropertyName("users")]
        public Guid? UserId { get; set; }

        public virtual ICollection<User> Users { get; set; }

        [JsonPropertyName("items")]
        public Guid? ItemId { get; set; }

        public virtual ICollection<Item> Items { get; set; }

        public Organization()
        {
            Addresses = new HashSet<Address>();
          
            Items = new HashSet<Item>();
            Users = new HashSet<User>();    
        }
    }

}
