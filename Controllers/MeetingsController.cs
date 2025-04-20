using IEEE.Data;
using IEEE.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IEEE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _context;


        public MeetingsController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<meetings>>> GetMeetings()
        {
            return await _context.meetings
                .Include(m => m.Creator)
                .Include(m => m.Committee)
                .ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<meetings>> GetMeeting(int id)
        {
            var meeting = await _context.meetings
                .Include(m => m.Creator)
                .Include(m => m.Committee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
                return NotFound();

            return meeting;
        }

        
        [HttpPost]
        public async Task<ActionResult<meetings>> PostMeeting(meetings meeting)
        {
            _context.meetings.Add(meeting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, meeting);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeeting(int id, meetings meeting)
        {
            if (id != meeting.Id)
                return BadRequest();

            _context.Entry(meeting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.meetings.Any(m => m.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.meetings.FindAsync(id);
            if (meeting == null)
                return NotFound();

            _context.meetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
