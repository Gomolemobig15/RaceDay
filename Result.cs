
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    public int ResultID { get; set; }

    public int EnrolmentID { get; set; }
    public Enrolment? Enrolment { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int? Position { get; set; }

    [Required, MaxLength(20)]
    public string ResultStatus { get; set; } = "Finished";

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
