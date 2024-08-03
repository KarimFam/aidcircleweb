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

        public List<Address> Addresses { get; set; }
        public Organization()
        {
            OrganizationId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }

}
