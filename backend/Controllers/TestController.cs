using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TestController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("create-sample-cv")]
        public async Task<ActionResult<CV>> CreateSampleCV()
        {
            var cv = new CV
            {
                PersonalInfo = new PersonalInfo
                {
                    Name = "John Doe",
                    Email = "john.doe@email.com",
                    Phone = "+1-555-0123",
                    Location = "San Francisco, CA",
                    LinkedIn = "linkedin.com/in/johndoe",
                    Summary = "Experienced software engineer with 5+ years in full-stack development, specializing in .NET and React applications."
                },
                TimelineItems = new List<TimelineItem>
                {
                    new TimelineItem
                    {
                        Type = TimelineItemType.Experience,
                        Title = "Senior Software Engineer",
                        Organization = "Tech Corp",
                        Subtitle = "Full-Stack Development",
                        StartDate = new DateTime(2022, 1, 1),
                        EndDate = null,
                        IsCurrentPosition = true,
                        Description = "Leading development of enterprise web applications using .NET Core and React.",
                        BulletPoints = new List<string>
                        {
                            "Developed and maintained 5+ production applications",
                            "Led a team of 3 junior developers",
                            "Improved application performance by 40%",
                            "Implemented CI/CD pipelines using Azure DevOps"
                        }
                    },
                    new TimelineItem
                    {
                        Type = TimelineItemType.Experience,
                        Title = "Software Developer",
                        Organization = "Startup Inc",
                        Subtitle = "Backend Development",
                        StartDate = new DateTime(2020, 6, 1),
                        EndDate = new DateTime(2021, 12, 31),
                        Description = "Built scalable backend services using .NET and PostgreSQL.",
                        BulletPoints = new List<string>
                        {
                            "Developed RESTful APIs for mobile applications",
                            "Optimized database queries reducing response time by 60%",
                            "Implemented authentication and authorization systems"
                        }
                    },
                    new TimelineItem
                    {
                        Type = TimelineItemType.Education,
                        Title = "Bachelor of Science in Computer Science",
                        Organization = "University of Technology",
                        Subtitle = "Computer Science",
                        StartDate = new DateTime(2016, 9, 1),
                        EndDate = new DateTime(2020, 5, 31),
                        Grade = "3.8/4.0",
                        GraduationYear = 2020
                    },
                    new TimelineItem
                    {
                        Type = TimelineItemType.Project,
                        Title = "E-commerce Platform",
                        Organization = "Personal Project",
                        StartDate = new DateTime(2021, 3, 1),
                        EndDate = new DateTime(2021, 8, 31),
                        Description = "Full-stack e-commerce application with payment integration.",
                        Tech = ".NET Core, React, PostgreSQL, Stripe",
                        BulletPoints = new List<string>
                        {
                            "Implemented secure payment processing",
                            "Built responsive admin dashboard",
                            "Integrated with multiple payment gateways"
                        }
                    }
                },
                Skills = new List<Skills>
                {
                    new Skills
                    {
                        Category = "Programming Languages",
                        SkillsList = new List<string> { "C#", "JavaScript", "TypeScript", "Python", "SQL" }
                    },
                    new Skills
                    {
                        Category = "Frameworks & Libraries",
                        SkillsList = new List<string> { ".NET Core", "ASP.NET Core", "React", "Entity Framework", "Dapper" }
                    },
                    new Skills
                    {
                        Category = "Databases",
                        SkillsList = new List<string> { "PostgreSQL", "SQL Server", "MongoDB", "Redis" }
                    },
                    new Skills
                    {
                        Category = "Tools & Platforms",
                        SkillsList = new List<string> { "Git", "Azure DevOps", "Docker", "Kubernetes", "AWS" }
                    }
                }
            };

            _db.CVs.Add(cv);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSampleCV), new { id = cv.Id }, cv);
        }

        [HttpGet("sample-cv/{id:int}")]
        public async Task<ActionResult<CV>> GetSampleCV(int id)
        {
            var cv = await _db.CVs
                .Include(cv => cv.PersonalInfo)
                .Include(cv => cv.TimelineItems)
                .Include(cv => cv.Skills)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (cv == null)
                return NotFound();

            return cv;
        }
    }
}