
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(RaceDayContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = request.Role,
            PhoneNumber = request.PhoneNumber
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created("/api/auth/login", new
        {
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var verification = _hasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(new
        {
            message = "Login successful.",
            user.UserID,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [AllowAnonymous]
    [HttpGet("forbidden")]
    public IActionResult Forbidden()
    {
        return StatusCode(403, new { message = "You are not allowed to access this resource." });
    }
}
