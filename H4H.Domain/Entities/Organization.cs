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
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        // Relationships
        [JsonPropertyName("addresses")]
        public virtual ICollection<Address> Addresses { get; set; }

        [JsonPropertyName("volunteers")]
        public virtual ICollection<Volunteer> Volunteers { get; set; }

        [JsonPropertyName("items")]
        public virtual ICollection<Item> Items { get; set; }

        public Organization()
        {
            Addresses = new HashSet<Address>();
            Volunteers = new HashSet<Volunteer>();
            Items = new HashSet<Item>();
        }
    }

}
