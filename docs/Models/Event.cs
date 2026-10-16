
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Event
{
    public int EventID { get; set; }

    public int OrganiserID { get; set; }
    public User? Organiser { get; set; }

    public int VenueID { get; set; }
    public Venue? Venue { get; set; }

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string EventType { get; set; } = "Running";

    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime RegistrationDeadline { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public Route? Route { get; set; }
}
