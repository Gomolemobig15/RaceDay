
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyResults()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var results = await _context.Results
            .Where(r => r.Enrolment != null &&
                        r.Enrolment.ParticipantID == userId)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                    .ThenInclude(c => c!.Event)
            .Select(r => new
            {
                r.ResultID,
                r.FinishTime,
                r.Position,
                r.ResultStatus,
                r.RecordedAt,
                Enrolment = r.Enrolment == null ? null : new
                {
                    r.Enrolment.EnrolmentID,
                    Category = r.Enrolment.Category == null ? null : new
                    {
                        r.Enrolment.Category.CategoryName,
                        Event = r.Enrolment.Category.Event == null ? null : new
                        {
                            r.Enrolment.Category.Event.EventID,
                            r.Enrolment.Category.Event.EventName,
                            r.Enrolment.Category.Event.EventDate
                        }
                    }
                }
            })
            .ToListAsync();

        return Ok(results);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("/api/enrolments/{enrolmentId:int}/result")]
    public async Task<IActionResult> RecordResult(
        int enrolmentId, RecordResultRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (request.FinishTime < TimeSpan.Zero)
            return BadRequest(new { message = "Finish time cannot be negative." });

        if (request.ResultStatus == "Finished" &&
            request.FinishTime == TimeSpan.Zero)
            return BadRequest(new
            {
                message = "A finished participant must have a finish time greater than zero."
            });

        if (request.Position.HasValue && request.Position.Value <= 0)
            return BadRequest(new { message = "Position must be greater than zero." });

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .SingleOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

        if (enrolment == null || enrolment.Category?.Event == null)
            return NotFound(new { message = "Enrolment or associated event not found." });

        if (enrolment.Category.Event.OrganiserID != userId)
            return Forbid();

        var existingResult = await _context.Results
            .SingleOrDefaultAsync(r => r.EnrolmentID == enrolmentId);

        if (existingResult == null)
        {
            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                ResultStatus = request.ResultStatus,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
        }
        else
        {
            existingResult.FinishTime = request.FinishTime;
            existingResult.Position = request.Position;
            existingResult.ResultStatus = request.ResultStatus;
            existingResult.RecordedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Result saved successfully." });
    }
}

public class RecordResultRequest
{
    public TimeSpan FinishTime { get; set; }

    public int? Position { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.RegularExpression(
        "^(Finished|Disqualified|DidNotFinish)$")]
    public string ResultStatus { get; set; } = "Finished";
}
