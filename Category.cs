
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryID { get; set; }

    public int EventID { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }

    public decimal EntryFee { get; set; }

    public int? MaximumParticipants { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
