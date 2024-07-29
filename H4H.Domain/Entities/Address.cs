using H4H.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace H4H.Domain.Entities
{
    public class Address : BaseEntity
    {
        [JsonPropertyName("line1")]
        [Required, MaxLength(255)]
        public string Line1 { get; set; }

        [JsonPropertyName("line2")]
        [MaxLength(255)]
        public string Line2 { get; set; }

        [JsonPropertyName("city")]
        [Required, MaxLength(100)]
        public string City { get; set; }

        [JsonPropertyName("state")]
        [MaxLength(100)]
        public string State { get; set; }

        [JsonPropertyName("postalCode")]
        [Required, MaxLength(20)]
        public string PostalCode { get; set; }

        [JsonPropertyName("country")]
        [Required, MaxLength(100)]
        public string Country { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Item Item { get; set; }
        public virtual Order Order { get; set; }
        public virtual User User { get; set; }  
        public virtual Volunteer Volunteer { get; set; }

       //JsonPropertyName("addressableId")] dont need this, not being used anywhere else in the code
     // public int AddressableId { get; set; }

     // [JsonPropertyName("addressableType")]
  //    public string AddressableType { get; set; } // User, Volunteer, Organization, Item
    }
}
