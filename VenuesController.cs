
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/venues")]
  
    public class VenuesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public VenuesController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetVenues()
        {
            var totalVenues = await _context.Venues.CountAsync();

            var venues = await _context.Venues
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                Database = _context.Database.GetDbConnection().Database,
                TotalVenues = totalVenues,
                Venues = venues
            });

            return Ok(venues);
        }
    }
}
