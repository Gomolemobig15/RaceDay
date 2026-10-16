
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Route
{
    public int RouteID { get; set; }

    public int EventID { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(150)]
    public string RouteName { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }

    [MaxLength(1000)]
    public string? RouteDescription { get; set; }

    [MaxLength(500)]
    public string? RouteURL { get; set; }
}
