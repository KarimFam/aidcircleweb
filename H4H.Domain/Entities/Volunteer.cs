using H4H.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Volunteer : User
{
    [JsonPropertyName("skills")]
    public string Skills { get; set; }

    [JsonPropertyName("organizations")]
    [InverseProperty("Volunteers")]
    public virtual ICollection<Organization> Organizations { get; set; }

    //[JsonPropertyName("assignedItems")]
    //[InverseProperty("AssignedVolunteers")]
    //public virtual ICollection<Item> AssignedItems { get; set; }

    public Volunteer()
    {
        Organizations = new HashSet<Organization>();
        //AssignedItems = new HashSet<Item>();
    }
}
