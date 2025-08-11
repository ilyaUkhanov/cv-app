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
using System.Linq;

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

            // Margins (kept generous for ATS)
            document.SetMargins(42, 50, 50, 50);

            // Optional: register a readable sans-serif font (falls back to default if not found)
            try
            {
                var font = PdfFontFactory.CreateFont("Helvetica");
                document.SetFont(font);
            }
            catch { /* fallback to default */ }

            // ===== HEADER =====
            AddHeaderBanner(document, cv.PersonalInfo, photoData);

            // ===== SUMMARY (accroche) =====
            if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Summary))
            {
                AddSectionTitle(document, "Résumé professionnel");
                document.Add(new Paragraph(cv.PersonalInfo.Summary)
                    .SetFontSize(11)
                    .SetMarginBottom(12));
            }

            // ===== EXPÉRIENCE =====
            var experiences = cv.TimelineItems
                .Where(t => t.Type == TimelineItemType.Experience)
                .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                .ToList();

            if (experiences.Any())
            {
                AddSectionTitle(document, "Expérience professionnelle");
                foreach (var exp in experiences)
                {
                    AddExperienceBlock(document, exp, cv.Skills);
                }
            }

            // ===== PROJETS =====
            var projects = cv.TimelineItems
                .Where(t => t.Type == TimelineItemType.Project)
                .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                .ToList();

            if (projects.Any())
            {
                AddSectionTitle(document, "Projets");
                foreach (var p in projects)
                {
                    AddProjectBlock(document, p);
                }
            }

            // ===== FORMATION =====
            var educ = cv.TimelineItems
                .Where(t => t.Type == TimelineItemType.Education)
                .OrderByDescending(t => t.GraduationYear ?? (t.EndDate?.Year ?? (t.StartDate?.Year ?? int.MinValue)))
                .ToList();

            if (educ.Any())
            {
                AddSectionTitle(document, "Formation");
                foreach (var ed in educ)
                {
                    AddEducationBlock(document, ed);
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }

        // ===== Header (Template-like) =====
        private void AddHeaderBanner(Document document, PersonalInfo info, byte[]? photoData)
        {
            // Top role title (fixed label – adapt as needed from data)
            document.Add(new Paragraph("DÉVELOPPEUR FULLSTACK")
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginBottom(2));

            // Name big
            document.Add(new Paragraph(info.Name)
                .SetFontSize(22)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginBottom(8));

            // First contact line: city | email
            var line1 = new List<string>();
            if (!string.IsNullOrWhiteSpace(info.Location)) line1.Add(info.Location!);
            if (!string.IsNullOrWhiteSpace(info.Email)) line1.Add(info.Email!);
            if (line1.Count > 0)
            {
                document.Add(new Paragraph(string.Join("  |  ", line1))
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(2));
            }

            // Second contact line: website (use LinkedIn if no website) | LinkedIn | phone
            var line2 = new List<string>();
            // If you have a Website field later, insert it here. For now we print LinkedIn only once.
            if (!string.IsNullOrWhiteSpace(info.LinkedIn)) line2.Add(info.LinkedIn!);
            if (!string.IsNullOrWhiteSpace(info.Phone)) line2.Add(info.Phone!);

            if (line2.Count > 0)
            {
                document.Add(new Paragraph(string.Join("  |  ", line2))
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(12));
            }

            // Optional small photo at the right (kept same as your previous version but placed using float)
            if (photoData != null)
            {
                try
                {
                    var imageData = ImageDataFactory.Create(photoData);
                    var image = new Image(imageData)
                        .SetWidth(70)
                        .SetHeight(88)
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                    document.Add(image);
                }
                catch { /* ignore on failure for ATS simplicity */ }
            }

            AddDivider(document);
        }

        // ===== Blocks =====
        private void AddExperienceBlock(Document document, TimelineItem item, List<Skills> allSkills)
        {
            // Title line: "Rôle | chez ORG | Mois/Année – Présent"
            var titleParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.Title)) titleParts.Add(item.Title!);
            if (!string.IsNullOrWhiteSpace(item.Organization)) titleParts.Add($"chez {item.Organization}");
            var period = BuildPeriod(item);
            if (!string.IsNullOrWhiteSpace(period)) titleParts.Add(period);

            document.Add(new Paragraph(string.Join("  |  ", titleParts))
                .SetFontSize(12)
                .SetMarginTop(6)
                .SetMarginBottom(4));

            // Optional subtitle
            if (!string.IsNullOrWhiteSpace(item.Subtitle))
            {
                document.Add(new Paragraph(item.Subtitle)
                    .SetFontSize(11)
                    .SetMarginBottom(4));
            }

            // Description
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                document.Add(new Paragraph(item.Description)
                    .SetFontSize(11)
                    .SetMarginBottom(4));
            }

            // Bullets
            if (item.BulletPoints != null && item.BulletPoints.Any())
            {
                foreach (var b in item.BulletPoints)
                {
                    if (string.IsNullOrWhiteSpace(b)) continue;
                    document.Add(new Paragraph("• " + b)
                        .SetFontSize(11)
                        .SetMarginLeft(12)
                        .SetMarginBottom(2));
                }
            }

            // Per-role skills (from many-to-many mapping)
            var related = allSkills
                .Where(s => s.RelatedTimelineItems != null && s.RelatedTimelineItems.Any(r => r.Id == item.Id))
                .ToList();

            if (related.Any())
            {
                // A compact single line per category similar to the template
                document.Add(new Paragraph("Compétences")
                    .SetFontSize(11)
                    .SetMarginTop(6)
                    .SetMarginBottom(2));

                foreach (var group in related)
                {
                    var skillsText = (group.SkillsList != null && group.SkillsList.Any())
                        ? string.Join(", ", group.SkillsList)
                        : string.Empty;

                    document.Add(new Paragraph(skillsText)
                        .SetFontSize(10)
                        .SetMarginLeft(12)
                        .SetMarginBottom(2));
                }
            }
        }

        private void AddProjectBlock(Document document, TimelineItem item)
        {
            // Title line: project name; use organization if present for context
            var title = !string.IsNullOrWhiteSpace(item.Title) ? item.Title : "Projet";
            var ctx = !string.IsNullOrWhiteSpace(item.Organization) ? $" – {item.Organization}" : string.Empty;
            document.Add(new Paragraph(title + ctx)
                .SetFontSize(12)
                .SetMarginTop(6)
                .SetMarginBottom(2));

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                document.Add(new Paragraph(item.Description)
                    .SetFontSize(11)
                    .SetMarginBottom(2));
            }

            if (!string.IsNullOrWhiteSpace(item.Tech))
            {
                document.Add(new Paragraph("Technologies : " + item.Tech)
                    .SetFontSize(10)
                    .SetMarginBottom(4));
            }
        }

        private void AddEducationBlock(Document document, TimelineItem item)
        {
            // "Diplôme | École | Années" style
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.Title)) parts.Add(item.Title!);
            if (!string.IsNullOrWhiteSpace(item.Organization)) parts.Add(item.Organization!);

            var years = string.Empty;
            if (item.GraduationYear.HasValue) years = item.GraduationYear.Value.ToString();
            else if (item.StartDate.HasValue || item.EndDate.HasValue) years = BuildYears(item.StartDate, item.EndDate);
            if (!string.IsNullOrWhiteSpace(years)) parts.Add(years);

            document.Add(new Paragraph(string.Join("  |  ", parts))
                .SetFontSize(12)
                .SetMarginTop(6)
                .SetMarginBottom(2));

            if (!string.IsNullOrWhiteSpace(item.Subtitle))
            {
                document.Add(new Paragraph(item.Subtitle)
                    .SetFontSize(11)
                    .SetMarginBottom(2));
            }

            if (!string.IsNullOrWhiteSpace(item.Grade))
            {
                document.Add(new Paragraph("Mention : " + item.Grade)
                    .SetFontSize(10)
                    .SetMarginBottom(2));
            }
        }

        // ===== Helpers =====
        private void AddSectionTitle(Document document, string title)
        {
            document.Add(new Paragraph(title.ToUpperInvariant())
                .SetFontSize(13)
                .SetMarginTop(14)
                .SetMarginBottom(6));
            AddDivider(document);
        }

        private void AddDivider(Document document)
        {
            var sep = new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(0.75f));
            sep.SetMarginBottom(10);
            document.Add(sep);
        }

        private static string BuildPeriod(TimelineItem item)
        {
            // Preferred: Month YYYY – Month YYYY (or Présent)
            if (item.StartDate.HasValue)
            {
                var start = item.StartDate.Value.ToString("MMMM yyyy");
                string end;
                if (item.IsCurrentPosition)
                {
                    end = "Présent";
                }
                else if (item.EndDate.HasValue)
                {
                    end = item.EndDate.Value.ToString("MMMM yyyy");
                }
                else if (!string.IsNullOrWhiteSpace(item.Duration))
                {
                    end = item.Duration!; // fallback textual
                }
                else
                {
                    end = string.Empty;
                }

                return string.IsNullOrWhiteSpace(end) ? start : $"{start} – {end}";
            }

            if (!string.IsNullOrWhiteSpace(item.Duration)) return item.Duration!;
            if (item.GraduationYear.HasValue) return $"Diplômé {item.GraduationYear}";
            return string.Empty;
        }

        private static string BuildYears(DateTime? start, DateTime? end)
        {
            if (start.HasValue && end.HasValue)
            {
                var s = start.Value.Year;
                var e = end.Value.Year;
                return s == e ? s.ToString() : $"{s} – {e}";
            }
            if (start.HasValue) return start.Value.Year.ToString();
            if (end.HasValue) return end.Value.Year.ToString();
            return string.Empty;
        }
    }
}
