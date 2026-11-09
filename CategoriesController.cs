
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
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
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        if (!await _context.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _context.Categories
            .Where(c => c.EventID == eventId)
            .Select(c => new
            {
                c.CategoryID,
                c.EventID,
                c.CategoryName,
                c.DistanceKm,
                c.EntryFee,
                c.MaximumParticipants
            })
            .ToListAsync();

        return Ok(categories);
    }

    [AllowAnonymous]
    [HttpGet("categories/{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryID == id)
            .Select(c => new
            {
                c.CategoryID,
                c.EventID,
                c.CategoryName,
                c.DistanceKm,
                c.EntryFee,
                c.MaximumParticipants
            })
            .SingleOrDefaultAsync();

        if (category == null)
            return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("events/{eventId:int}/categories")]
    public async Task<IActionResult> CreateCategory(
        int eventId, CategoryRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (!await OwnsEvent(eventId, userId))
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            return Forbid();
        }

        if (request.DistanceKm <= 0 || request.EntryFee < 0)
            return BadRequest(new
            {
                message = "Distance must be greater than zero and entry fee cannot be negative."
            });

        if (request.MaximumParticipants.HasValue &&
            request.MaximumParticipants.Value <= 0)
            return BadRequest(new
            {
                message = "Maximum participants must be greater than zero."
            });

        var name = request.CategoryName.Trim();

        if (await _context.Categories.AnyAsync(c =>
            c.EventID == eventId && c.CategoryName == name))
            return Conflict(new
            {
                message = "This category name already exists for the event."
            });

        var category = new Category
        {
            EventID = eventId,
            CategoryName = name,
            DistanceKm = request.DistanceKm,
            EntryFee = request.EntryFee,
            MaximumParticipants = request.MaximumParticipants
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryID },
            new
            {
                category.CategoryID,
                category.EventID,
                category.CategoryName,
                category.DistanceKm,
                category.EntryFee,
                category.MaximumParticipants
            });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id, CategoryRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (!await OwnsEvent(category.EventID, userId))
            return Forbid();

        if (request.DistanceKm <= 0 || request.EntryFee < 0)
            return BadRequest(new
            {
                message = "Distance must be greater than zero and entry fee cannot be negative."
            });

        if (request.MaximumParticipants.HasValue &&
            request.MaximumParticipants.Value <= 0)
            return BadRequest(new
            {
                message = "Maximum participants must be greater than zero."
            });

        var name = request.CategoryName.Trim();

        if (await _context.Categories.AnyAsync(c =>
            c.EventID == category.EventID &&
            c.CategoryName == name &&
            c.CategoryID != id))
            return Conflict(new
            {
                message = "This category name already exists for the event."
            });

        category.CategoryName = name;
        category.DistanceKm = request.DistanceKm;
        category.EntryFee = request.EntryFee;
        category.MaximumParticipants = request.MaximumParticipants;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Category updated successfully." });
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (!await OwnsEvent(category.EventID, userId))
            return Forbid();

        if (await _context.Enrolments.AnyAsync(e => e.CategoryID == id))
            return Conflict(new
            {
                message = "This category has enrolments and cannot be deleted."
            });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully." });
    }
}

public class CategoryRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int? MaximumParticipants { get; set; }
}
