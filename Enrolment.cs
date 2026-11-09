
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }

    public int ParticipantID { get; set; }
    public User? Participant { get; set; }

    public int CategoryID { get; set; }
    public Category? Category { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Confirmed";

    [MaxLength(20)]
    public string? RaceNumber { get; set; }

    public Result? Result { get; set; }
}
