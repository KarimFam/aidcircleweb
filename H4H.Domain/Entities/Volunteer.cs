using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Entities
{
    public class Volunteer : User
    {
        // Additional Volunteer-specific properties
        [JsonPropertyName("skills")]
        public string Skills { get; set; }

        // Relationships
        [JsonPropertyName("organizations")]
        public virtual ICollection<Organization> Organizations { get; set; }

        [JsonPropertyName("assignedItems")]
        public virtual ICollection<Item> AssignedItems { get; set; }

        public Volunteer()
        {
            Organizations = new HashSet<Organization>();
            AssignedItems = new HashSet<Item>();
        }
    }

}
