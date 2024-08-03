using H4H.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace H4H.Domain.Entities
{
    public class Address : BaseEntity
    {
        [JsonPropertyName("addressId")]
        public Guid AddressId { get; set; }

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

        public Address()
        {
            AddressId = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

    

        public Guid? ItemId { get; set; }
        public Item? Item { get; set; } 

        public Guid? UserId { get; set; }
        public User? User { get; set; } 

        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }





    }
}
