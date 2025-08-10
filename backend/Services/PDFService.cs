using backend.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font;

namespace backend.Services
{
    public class PDFService : IPDFService
    {
        public async Task<byte[]> GenerateATSCVAsync(CV cv)
        {
            return await Task.Run(() => GenerateATSCVInternal(cv, null));
        }

        public async Task<byte[]> GenerateATSCVWithPhotoAsync(CV cv, byte[] photoData)
        {
            return await Task.Run(() => GenerateATSCVInternal(cv, photoData));
        }

        private byte[] GenerateATSCVInternal(CV cv, byte[]? photoData)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);

            // Set margins for ATS compatibility
            document.SetMargins(50, 50, 50, 50);

            // Header with Personal Info
            AddHeader(document, cv.PersonalInfo, photoData);

            // Summary
            if (!string.IsNullOrEmpty(cv.PersonalInfo.Summary))
            {
                AddSection(document, "PROFESSIONAL SUMMARY", cv.PersonalInfo.Summary);
            }

            // Skills
            if (cv.Skills.Any())
            {
                AddSkillsSection(document, cv.Skills);
            }

            // Timeline Items (Experience, Education, Projects)
            if (cv.TimelineItems.Any())
            {
                AddTimelineSection(document, cv.TimelineItems);
            }

            document.Close();
            return memoryStream.ToArray();
        }

        private void AddHeader(Document document, PersonalInfo personalInfo, byte[]? photoData)
        {
            // Name as main heading
            var nameParagraph = new Paragraph(personalInfo.Name)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(nameParagraph);

            // Contact information
            var contactText = BuildContactText(personalInfo);
            var contactParagraph = new Paragraph(contactText)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(contactParagraph);

            // Add photo if provided (small size, right-aligned)
            if (photoData != null)
            {
                try
                {
                    var imageData = ImageDataFactory.Create(photoData);
                    var image = new Image(imageData);
                    image.SetWidth(80);
                    image.SetHeight(100);
                    image.SetFixedPosition(450, 700); // Position on the right side
                    document.Add(image);
                }
                catch
                {
                    // Silently fail if photo can't be added - ATS compatibility is priority
                }
            }

            document.Add(new Paragraph("").SetMarginBottom(20));
        }

        private string BuildContactText(PersonalInfo personalInfo)
        {
            var contactParts = new List<string>();

            if (!string.IsNullOrEmpty(personalInfo.Email))
                contactParts.Add(personalInfo.Email);

            if (!string.IsNullOrEmpty(personalInfo.Phone))
                contactParts.Add(personalInfo.Phone);

            if (!string.IsNullOrEmpty(personalInfo.Location))
                contactParts.Add(personalInfo.Location);

            if (!string.IsNullOrEmpty(personalInfo.LinkedIn))
                contactParts.Add(personalInfo.LinkedIn);

            return string.Join(" | ", contactParts);
        }

        private void AddSection(Document document, string title, string content)
        {
            // Section title
            var titleParagraph = new Paragraph(title)
                .SetFontSize(14)
                .SetUnderline()
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(titleParagraph);

            // Section content
            var contentParagraph = new Paragraph(content)
                .SetFontSize(11)
                .SetMarginBottom(15);
            document.Add(contentParagraph);
        }

        private void AddSkillsSection(Document document, List<Skills> skills)
        {
            var titleParagraph = new Paragraph("SKILLS")
                .SetFontSize(14)
                .SetUnderline()
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(titleParagraph);

            foreach (var skillGroup in skills)
            {
                var categoryParagraph = new Paragraph(skillGroup.Category)
                    .SetFontSize(12)
                    .SetMarginTop(10)
                    .SetMarginBottom(5);
                document.Add(categoryParagraph);

                var skillsText = string.Join(", ", skillGroup.SkillsList);
                var skillsParagraph = new Paragraph(skillsText)
                    .SetFontSize(11)
                    .SetMarginBottom(10);
                document.Add(skillsParagraph);
            }
        }

        private void AddTimelineSection(Document document, List<TimelineItem> timelineItems)
        {
            var titleParagraph = new Paragraph("PROFESSIONAL EXPERIENCE & EDUCATION")
                .SetFontSize(14)
                .SetUnderline()
                .SetMarginTop(15)
                .SetMarginBottom(10);
            document.Add(titleParagraph);

            // Group by type and sort by date
            var groupedItems = timelineItems
                .GroupBy(t => t.Type)
                .OrderBy(g => g.Key == TimelineItemType.Education ? 1 : 0)
                .ThenBy(g => g.Key == TimelineItemType.Experience ? 0 : 1);

            foreach (var group in groupedItems)
            {
                var groupTitle = group.Key switch
                {
                    TimelineItemType.Experience => "WORK EXPERIENCE",
                    TimelineItemType.Education => "EDUCATION",
                    TimelineItemType.Project => "PROJECTS",
                    _ => "OTHER"
                };

                var groupTitleParagraph = new Paragraph(groupTitle)
                    .SetFontSize(13)
                    .SetMarginTop(15)
                    .SetMarginBottom(10);
                document.Add(groupTitleParagraph);

                foreach (var item in group.OrderByDescending(t => t.StartDate ?? DateTime.MinValue))
                {
                    AddTimelineItem(document, item);
                }
            }
        }

        private void AddTimelineItem(Document document, TimelineItem item)
        {
            // Title and Organization
            var titleOrgParagraph = new Paragraph($"{item.Title} - {item.Organization}")
                .SetFontSize(12)
                .SetMarginTop(10)
                .SetMarginBottom(5);
            document.Add(titleOrgParagraph);

            // Subtitle if exists
            if (!string.IsNullOrEmpty(item.Subtitle))
            {
                var subtitleParagraph = new Paragraph(item.Subtitle)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(subtitleParagraph);
            }

            // Duration/Date
            var durationText = GetDurationText(item);
            if (!string.IsNullOrEmpty(durationText))
            {
                var durationParagraph = new Paragraph(durationText)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(durationParagraph);
            }

            // Description
            if (!string.IsNullOrEmpty(item.Description))
            {
                var descriptionParagraph = new Paragraph(item.Description)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(descriptionParagraph);
            }

            // Tech stack for projects
            if (item.Type == TimelineItemType.Project && !string.IsNullOrEmpty(item.Tech))
            {
                var techParagraph = new Paragraph($"Technologies: {item.Tech}")
                    .SetFontSize(10)
                    .SetMarginBottom(5);
                document.Add(techParagraph);
            }

            // Bullet points
            if (item.BulletPoints.Any())
            {
                foreach (var bullet in item.BulletPoints)
                {
                    var bulletParagraph = new Paragraph($"• {bullet}")
                        .SetFontSize(11)
                        .SetMarginLeft(20)
                        .SetMarginBottom(3);
                    document.Add(bulletParagraph);
                }
            }

            // Grade for education
            if (item.Type == TimelineItemType.Education && !string.IsNullOrEmpty(item.Grade))
            {
                var gradeParagraph = new Paragraph($"Grade: {item.Grade}")
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(gradeParagraph);
            }

            document.Add(new Paragraph("").SetMarginBottom(10));
        }

        private string GetDurationText(TimelineItem item)
        {
            if (item.StartDate.HasValue && item.EndDate.HasValue)
            {
                var startYear = item.StartDate.Value.Year;
                var endYear = item.EndDate.Value.Year;

                if (item.IsCurrentPosition)
                    return $"{startYear} - Present";

                return startYear == endYear ? $"{startYear}" : $"{startYear} - {endYear}";
            }

            if (!string.IsNullOrEmpty(item.Duration))
                return item.Duration;

            if (item.GraduationYear.HasValue)
                return $"Graduated {item.GraduationYear}";

            return string.Empty;
        }
    }
}
