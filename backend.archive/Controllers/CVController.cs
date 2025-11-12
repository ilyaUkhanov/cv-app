using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CVController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CV>>> GetAll()
        {
            return await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CV>> GetById(int id)
        {
            var cv = await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            return cv is null ? NotFound() : cv;
        }

        [HttpPost]
        public async Task<ActionResult<CV>> Create(CV cv)
        {
            cv.CreatedAt = DateTime.UtcNow;
            cv.UpdatedAt = DateTime.UtcNow;

            _db.CVs.Add(cv);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cv.Id }, cv);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CV cv)
        {
            if (id != cv.Id) return BadRequest();

            cv.UpdatedAt = DateTime.UtcNow;

            _db.Entry(cv).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cv = await _db.CVs.FindAsync(id);
            if (cv is null) return NotFound();

            _db.CVs.Remove(cv);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:int}/timeline")]
        public async Task<ActionResult<IEnumerable<TimelineItem>>> GetTimeline(int id)
        {
            var timelineItems = await _db.TimelineItems
                .Where(t => EF.Property<int>(t, "CVId") == id)
                .ToListAsync();

            return timelineItems;
        }

        [HttpGet("{id:int}/timeline/{type}")]
        public async Task<ActionResult<IEnumerable<TimelineItem>>> GetTimelineByType(int id, TimelineItemType type)
        {
            var timelineItems = await _db.TimelineItems
                .Where(t => EF.Property<int>(t, "CVId") == id && t.Type == type)
                .ToListAsync();

            return timelineItems;
        }

        [HttpGet("{id:int}/skills")]
        public async Task<ActionResult<IEnumerable<Skills>>> GetSkills(int id)
        {
            var skills = await _db.Skills
                .Where(s => EF.Property<int>(s, "CVId") == id)
                .ToListAsync();

            return skills;
        }
    }
}
