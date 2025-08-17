using backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace backend.Services
{
    public class PDFService : IPDFService
    {
        static PDFService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

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
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        // Header
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(headerCol =>
                            {
                                headerCol.Item().Text("DÉVELOPPEUR FULLSTACK").FontSize(14).Bold();
                                headerCol.Item().Text(cv.PersonalInfo.Name).FontSize(22).Bold();

                                var contactLine1 = new List<string>();
                                if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Location)) contactLine1.Add(cv.PersonalInfo.Location);
                                if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Email)) contactLine1.Add(cv.PersonalInfo.Email);
                                if (contactLine1.Count > 0)
                                {
                                    headerCol.Item().Text(string.Join("  |  ", contactLine1)).FontSize(10);
                                }

                                var contactLine2 = new List<string>();
                                if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.LinkedIn)) contactLine2.Add(cv.PersonalInfo.LinkedIn);
                                if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Phone)) contactLine2.Add(cv.PersonalInfo.Phone);
                                if (contactLine2.Count > 0)
                                {
                                    headerCol.Item().Text(string.Join("  |  ", contactLine2)).FontSize(10);
                                }
                            });

                            if (photoData != null)
                            {
                                row.ConstantItem(70).Image(photoData).FitArea();
                            }
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                        col.Item().Height(10);

                        // Summary
                        if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Summary))
                        {
                            col.Item().Text(cv.PersonalInfo.Summary).FontSize(11);
                            col.Item().Height(10);
                        }

                        // Experience
                        var experiences = cv.TimelineItems
                            .Where(t => t.Type == TimelineItemType.Experience)
                            .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                            .ToList();

                        if (experiences.Any())
                        {
                            col.Item().Text("EXPÉRIENCE PROFESSIONNELLE").FontSize(13).Bold();
                            foreach (var exp in experiences)
                            {
                                AddTimelineItemToColumn(col, exp);
                            }
                            col.Item().Height(10);
                        }

                        // Projects
                        var projects = cv.TimelineItems
                            .Where(t => t.Type == TimelineItemType.Project)
                            .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                            .ToList();

                        if (projects.Any())
                        {
                            col.Item().Text("PROJETS").FontSize(13).Bold();
                            foreach (var proj in projects)
                            {
                                AddTimelineItemToColumn(col, proj);
                            }
                            col.Item().Height(10);
                        }

                        // Education
                        var education = cv.TimelineItems
                            .Where(t => t.Type == TimelineItemType.Education)
                            .OrderByDescending(t => t.GraduationYear ?? (t.EndDate?.Year ?? (t.StartDate?.Year ?? int.MinValue)))
                            .ToList();

                        if (education.Any())
                        {
                            col.Item().Text("FORMATION").FontSize(13).Bold();
                            foreach (var edu in education)
                            {
                                AddTimelineItemToColumn(col, edu);
                            }
                            col.Item().Height(10);
                        }

                        // Skills
                        if (cv.Skills.Any())
                        {
                            col.Item().Text("COMPÉTENCES").FontSize(13).Bold();
                            foreach (var skillGroup in cv.Skills)
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(80).Text(skillGroup.Category + ":").FontSize(11).Bold();
                                    row.RelativeItem().Text(string.Join(", ", skillGroup.SkillsList ?? new List<string>())).FontSize(11);
                                });
                            }
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string BuildPeriod(TimelineItem item)
        {
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
                    end = item.Duration;
                }
                else
                {
                    end = string.Empty;
                }

                return string.IsNullOrWhiteSpace(end) ? start : $"{start} – {end}";
            }

            if (!string.IsNullOrWhiteSpace(item.Duration)) return item.Duration;
            if (item.GraduationYear.HasValue) return $"Diplômé {item.GraduationYear}";
            return string.Empty;
        }

        private void AddTimelineItemToColumn(QuestPDF.Fluent.ColumnDescriptor col, TimelineItem item)
        {
            // Title line
            var titleParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.Title)) titleParts.Add(item.Title);
            if (!string.IsNullOrWhiteSpace(item.Organization)) titleParts.Add($"chez {item.Organization}");
            var period = BuildPeriod(item);
            if (!string.IsNullOrWhiteSpace(period)) titleParts.Add(period);

            col.Item().Text(string.Join("  |  ", titleParts)).FontSize(12).Bold();

            // Subtitle
            if (!string.IsNullOrWhiteSpace(item.Subtitle))
            {
                col.Item().Text(item.Subtitle).FontSize(11).Italic();
            }

            // Description
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                col.Item().Text(item.Description).FontSize(11);
            }

            // Bullet points
            if (item.BulletPoints != null && item.BulletPoints.Any())
            {
                foreach (var bullet in item.BulletPoints)
                {
                    if (!string.IsNullOrWhiteSpace(bullet))
                    {
                        col.Item().PaddingLeft(20).Text($"• {bullet}").FontSize(11);
                    }
                }
            }

            // Tech (for projects)
            if (!string.IsNullOrWhiteSpace(item.Tech))
            {
                col.Item().Text($"Technologies : {item.Tech}").FontSize(10);
            }

            // Grade (for education)
            if (!string.IsNullOrWhiteSpace(item.Grade))
            {
                col.Item().Text($"Mention : {item.Grade}").FontSize(10);
            }

            col.Item().Height(5);
        }
    }
}
