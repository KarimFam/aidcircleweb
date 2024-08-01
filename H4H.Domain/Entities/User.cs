using H4H.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class User : BaseEntity
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("username")]
    [Required, MaxLength(50)]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [JsonPropertyName("firstName")]
    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    [Required, MaxLength(50)]
    public string LastName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    [Required]
    public DateTime DateOfBirth { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("externalAuthProvider")]
    [MaxLength(50)]
    public string ExternalAuthProvider { get; set; }

    [JsonPropertyName("externalAuthId")]
    [MaxLength(255)]
    public string ExternalAuthId { get; set; }

    // Relationships
    public Guid AddressId { get; set; }
    [JsonPropertyName("addresses")]
    public virtual ICollection<Address> Addresses { get; set; }
    public Guid? OrderId {  get; set; }
    //[JsonPropertyName("orders")]
    //[InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; }

    //[JsonPropertyName("organizationId")]
    //public Guid? OrganizationId { get; set; }
    public Guid? ItemId { get; set; }
    [JsonPropertyName("items")]
    public virtual ICollection<Item> Items { get; set; }

    
    //[ForeignKey("OrganizationId")]
    //public virtual Organization Organization { get; set; }

    public User()
    {
        Addresses = new HashSet<Address>();
        Orders = new HashSet<Order>();
        Items = new HashSet<Item>();
    }
}
