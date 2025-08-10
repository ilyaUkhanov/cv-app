using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPDFService _pdfService;

        public PDFController(ApplicationDbContext db, IPDFService pdfService)
        {
            _db = db;
            _pdfService = pdfService;
        }

        [HttpGet("{id:int}/generate")]
        public async Task<IActionResult> GeneratePDF(int id)
        {
            var cv = await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null)
                return NotFound("CV not found");

            try
            {
                var pdfBytes = await _pdfService.GenerateATSCVAsync(cv);

                return File(pdfBytes, "application/pdf", $"CV_{cv.PersonalInfo.Name.Replace(" ", "_")}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

        [HttpPost("{id:int}/generate-with-photo")]
        public async Task<IActionResult> GeneratePDFWithPhoto(int id, IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest("Photo file is required");

            if (photo.Length > 5 * 1024 * 1024) // 5MB limit
                return BadRequest("Photo file size must be less than 5MB");

            var cv = await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null)
                return NotFound("CV not found");

            try
            {
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                var photoBytes = memoryStream.ToArray();

                var pdfBytes = await _pdfService.GenerateATSCVWithPhotoAsync(cv, photoBytes);

                return File(pdfBytes, "application/pdf", $"CV_{cv.PersonalInfo.Name.Replace(" ", "_")}_with_photo.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF with photo: {ex.Message}");
            }
        }

        [HttpGet("{id:int}/preview")]
        public async Task<IActionResult> PreviewCV(int id)
        {
            var cv = await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null)
                return NotFound("CV not found");

            // Return CV data for preview (could be used by a frontend to show before PDF generation)
            return Ok(new
            {
                cv.Id,
                cv.PersonalInfo,
                TimelineItems = cv.TimelineItems.OrderByDescending(t => t.StartDate ?? DateTime.MinValue),
                cv.Skills,
                cv.CreatedAt,
                cv.UpdatedAt
            });
        }
    }
}
