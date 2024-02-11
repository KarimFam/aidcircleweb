using H4H.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H4H.Domain.Entities
{
    public class Item : BaseEntity
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public ItemType Type { get; set; } // Enum for ToDo, Request, Event

        // Relationships
        [JsonPropertyName("organizationId")]
        public int OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [JsonPropertyName("assignedVolunteers")]
        public virtual ICollection<Volunteer> AssignedVolunteers { get; set; }

        public Item()
        {
            AssignedVolunteers = new HashSet<Volunteer>();
        }
    }

}
