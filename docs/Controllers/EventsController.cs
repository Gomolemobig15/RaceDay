
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);
    }

    private async Task<bool> OwnsEvent(int eventId, int userId)
    {
        return await _context.Events.AnyAsync(e =>
            e.EventID == eventId && e.OrganiserID == userId);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Categories)
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventType,
                e.EventDate,
                e.StartTime,
                e.RegistrationDeadline,
                e.Status,
                Venue = e.Venue == null ? null : new
                {
                    e.Venue.VenueID,
                    e.Venue.VenueName,
                    e.Venue.Address,
                    e.Venue.City,
                    e.Venue.Province
                },
                Categories = e.Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.DistanceKm,
                    c.EntryFee,
                    c.MaximumParticipants
                })
            })
            .ToListAsync();

        return Ok(events);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Categories)
            .Where(e => e.EventID == id)
            .Select(e => new
            {
                e.EventID,
                e.EventName,
                e.Description,
                e.EventType,
                e.EventDate,
                e.StartTime,
                e.RegistrationDeadline,
                e.Status,
                Venue = e.Venue == null ? null : new
                {
                    e.Venue.VenueID,
                    e.Venue.VenueName,
                    e.Venue.Address,
                    e.Venue.City,
                    e.Venue.Province
                },
                Categories = e.Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.DistanceKm,
                    c.EntryFee,
                    c.MaximumParticipants
                })
            })
            .SingleOrDefaultAsync();

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        return Ok(eventItem);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (!await _context.Venues.AnyAsync(v => v.VenueID == request.VenueID))
            return BadRequest(new { message = "The selected venue does not exist." });

        if (request.RegistrationDeadline > request.EventDate)
            return BadRequest(new
            {
                message = "The registration deadline cannot be after the event date."
            });

        var eventItem = new Event
        {
            OrganiserID = userId,
            VenueID = request.VenueID,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventType = request.EventType.Trim(),
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            RegistrationDeadline = request.RegistrationDeadline,
            Status = "Open"
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = eventItem.EventID },
            new
            {
                eventItem.EventID,
                eventItem.EventName,
                eventItem.OrganiserID,
                eventItem.VenueID,
                eventItem.EventDate,
                eventItem.Status
            });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(
        int id, UpdateEventRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(id);

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        if (eventItem.OrganiserID != userId)
            return Forbid();

        if (!await _context.Venues.AnyAsync(v => v.VenueID == request.VenueID))
            return BadRequest(new { message = "The selected venue does not exist." });

        if (request.RegistrationDeadline > request.EventDate)
            return BadRequest(new
            {
                message = "The registration deadline cannot be after the event date."
            });

        eventItem.VenueID = request.VenueID;
        eventItem.EventName = request.EventName.Trim();
        eventItem.Description = request.Description?.Trim();
        eventItem.EventType = request.EventType.Trim();
        eventItem.EventDate = request.EventDate;
        eventItem.StartTime = request.StartTime;
        eventItem.RegistrationDeadline = request.RegistrationDeadline;
        eventItem.Status = request.Status.Trim();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Event updated successfully." });
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(id);

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        if (eventItem.OrganiserID != userId)
            return Forbid();

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(en => en.Category != null &&
                            en.Category.EventID == id);

        if (hasEnrolments)
            return Conflict(new
            {
                message = "This event has enrolments and cannot be deleted. Close the event instead."
            });

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Event deleted successfully." });
    }
}

public class CreateEventRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string EventType { get; set; } = "Running";

    public int VenueID { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime RegistrationDeadline { get; set; }
}

public class UpdateEventRequest : CreateEventRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string Status { get; set; } = "Open";
}
