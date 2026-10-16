
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
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
    [HttpPost]
    public async Task<IActionResult> Enrol(EnrolRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var category = await _context.Categories
            .Include(c => c.Event)
            .SingleOrDefaultAsync(c => c.CategoryID == request.CategoryID);

        if (category == null || category.Event == null)
            return NotFound(new { message = "Category or event not found." });

        var eventItem = category.Event;

        if (eventItem.Status != "Open")
            return BadRequest(new { message = "This event is not open for enrolment." });

        if (eventItem.RegistrationDeadline < DateTime.UtcNow)
            return BadRequest(new { message = "The registration deadline has passed." });

        if (await _context.Enrolments.AnyAsync(e =>
            e.ParticipantID == userId &&
            e.CategoryID == request.CategoryID))
            return Conflict(new { message = "You are already enrolled in this category." });

        if (category.MaximumParticipants.HasValue)
        {
            var currentCount = await _context.Enrolments.CountAsync(e =>
                e.CategoryID == request.CategoryID &&
                e.Status == "Confirmed");

            if (currentCount >= category.MaximumParticipants.Value)
                return Conflict(new { message = "This category is full." });
        }

        var enrolment = new Enrolment
        {
            ParticipantID = userId,
            CategoryID = category.CategoryID,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMyEnrolments),
            new { },
            new
            {
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.CategoryID,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .Where(e => e.ParticipantID == userId)
            .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EnrolmentDate,
                e.Status,
                e.RaceNumber,
                Category = e.Category == null ? null : new
                {
                    e.Category.CategoryID,
                    e.Category.CategoryName,
                    e.Category.DistanceKm,
                    Event = e.Category.Event == null ? null : new
                    {
                        e.Category.Event.EventID,
                        e.Category.Event.EventName,
                        e.Category.Event.EventDate,
                        e.Category.Event.Status
                    }
                }
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    [Authorize(Roles = "Organiser")]
    [HttpGet("/api/events/{eventId:int}/enrolments")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(eventId);

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        if (eventItem.OrganiserID != userId)
            return Forbid();

        var enrolments = await _context.Enrolments
            .Where(e => e.Category != null &&
                        e.Category.EventID == eventId)
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Select(e => new
            {
                e.EnrolmentID,
                e.EnrolmentDate,
                e.Status,
                e.RaceNumber,
                Participant = e.Participant == null ? null : new
                {
                    e.Participant.UserID,
                    e.Participant.FirstName,
                    e.Participant.LastName,
                    e.Participant.Email
                },
                Category = e.Category == null ? null : new
                {
                    e.Category.CategoryID,
                    e.Category.CategoryName,
                    e.Category.DistanceKm
                }
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    [Authorize(Roles = "Participant")]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var enrolment = await _context.Enrolments.FindAsync(id);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        if (enrolment.ParticipantID != userId)
            return Forbid();

        if (enrolment.Status == "Cancelled")
            return BadRequest(new { message = "This enrolment is already cancelled." });

        enrolment.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Enrolment cancelled successfully." });
    }
}

public class EnrolRequest
{
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}
