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
        [JsonPropertyName("name")]
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        [MaxLength(1000)]
        public string Description { get; set; }

        //[JsonPropertyName("createdById")]
        //public int CreatedById { get; set; }

  //    [JsonPropertyName("createdByType")]
  //    [Required, MaxLength(50)]
     // public string CreatedByType { get; set; } // Possible values: "User", "Volunteer", "Organization"

        [JsonPropertyName("addressId")]
        public int AddressId { get; set; }

        [JsonPropertyName("address")]
        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }



        [JsonPropertyName("orderId")]
        public int OrderId { get; set; }


        [JsonPropertyName("order")]
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("user")]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [JsonPropertyName("organizationId")]
        public int OrganizationId { get; set; }

        [JsonPropertyName("organization")]
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }
    }

}
