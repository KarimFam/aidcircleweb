using H4H.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Volunteer : User
{
    [JsonPropertyName("volunteerId")]
    public Guid VolunteerId { get; set; }

    [JsonPropertyName("skills")]
    public string Skills { get; set; }
  //  public virtual ICollection<Order> Order { get; set; }
    public Volunteer(): base(){
   // Orders = new HashSet<Order>();  
    }


}
