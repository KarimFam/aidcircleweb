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

    public List<Address> Addresses { get; set; }

    public User()
    {
        UserId = Guid.NewGuid();
        CreatedDate = DateTime.Now;
        ModifiedDate = DateTime.Now;
    }

    
}
