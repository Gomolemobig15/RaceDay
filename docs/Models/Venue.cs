
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Venue
{
    public int VenueID { get; set; }

    [Required, MaxLength(120)]
    public string VenueName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Province { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
