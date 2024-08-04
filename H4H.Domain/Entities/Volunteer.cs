using H4H.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Volunteer : User
{
    [JsonPropertyName("volunteerId")]
    public Guid VolunteerId { get; set; }

    [JsonPropertyName("skills")]
    public string? Skills { get; set; }

    public Volunteer()
    {
        VolunteerId = Guid.NewGuid();
        CreatedDate = DateTime.Now;
        ModifiedDate = DateTime.Now;
    }

}
