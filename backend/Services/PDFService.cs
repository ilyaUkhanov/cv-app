using backend.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System.Linq;
using static QuestPDF.Infrastructure.IContainer;

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
            // Configure QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20, Unit.Millimetre);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(11).FontFamily("Segoe UI"));

                    page.Header().Element(header => BuildHeader(header, cv.PersonalInfo, photoData));
                    page.Content().Element(content => BuildContent(content, cv));
                });
            });

            return document.GeneratePdf();
        }

        private void BuildHeader(IContainer header, PersonalInfo info, byte[]? photoData)
        {
            header.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    // Name - Large title
                    col.Item().Text(info.Name).FontSize(20).Bold().LetterSpacing(0.2f);
                    
                    // Contact info in two rows
                    var contactLine1 = new List<string>();
                    if (!string.IsNullOrWhiteSpace(info.Location)) contactLine1.Add(info.Location);
                    if (!string.IsNullOrWhiteSpace(info.Email)) contactLine1.Add(info.Email);
                    
                    if (contactLine1.Count > 0)
                    {
                        col.Item().Text(string.Join("  •  ", contactLine1)).FontSize(10).FontColor("#555");
                    }
                    
                    var contactLine2 = new List<string>();
                    if (!string.IsNullOrWhiteSpace(info.Phone)) contactLine2.Add(info.Phone);
                    if (!string.IsNullOrWhiteSpace(info.LinkedIn)) contactLine2.Add(info.LinkedIn);
                    
                    if (contactLine2.Count > 0)
                    {
                        col.Item().Text(string.Join("  •  ", contactLine2)).FontSize(10).FontColor("#555");
                    }
                    
                    // Summary/tagline
                    if (!string.IsNullOrWhiteSpace(info.Summary))
                    {
                        col.Item().PaddingTop(4).Text(info.Summary).FontSize(10).FontColor("#555");
                    }
                });

                // Photo on the right if provided
                if (photoData != null)
                {
                    row.ConstantItem(70).Height(88).Image(photoData).FitArea();
                }
            });
            
            // Bottom border
            header.BorderBottom(1).BorderColor("#ddd").PaddingBottom(8);
        }

        private void BuildContent(IContainer content, CV cv)
        {
            content.Column(col =>
            {
                // Two-column layout
                col.Item().Row(row =>
                {
                    // Left column (1.2fr)
                    var leftContainer = row.RelativeItem(1.2f).Container();
                    BuildSkillsSection(leftContainer, cv.Skills);
                    BuildProjectsSection(leftContainer, cv.TimelineItems);
                    BuildEducationSection(leftContainer, cv.TimelineItems);
                        

                    // Right column (2fr)
                    var rightContainer = row.RelativeItem(2f).Container();
                    BuildExperienceSection(rightContainer, cv.TimelineItems, cv.Skills);
                });
            });
        }

        private void BuildSkillsSection(IContainer container, List<Skills> skills)
        {
            if (skills == null || !skills.Any()) return;

            container.PaddingBottom(10).Column(col =>
            {
                col.Item().Text("COMPÉTENCES").FontSize(12).Bold().LetterSpacing(0.5f).LineHeight(2.0f);
                
                // Skills as chips
                var allSkills = skills.SelectMany(s => s.SkillsList ?? new List<string>()).Distinct().ToList();
                if (allSkills.Any())
                {
                    col.Item().Row(row =>
                    {
                        var skillsPerRow = 3;
                        for (int i = 0; i < allSkills.Count; i += skillsPerRow)
                        {
                            var rowSkills = allSkills.Skip(i).Take(skillsPerRow).ToList();
                            row.RelativeItem().Row(skillRow =>
                            {
                                foreach (var skill in rowSkills)
                                {
                                    skillRow.RelativeItem().PaddingRight(6).PaddingBottom(6).Border(1).BorderColor("#ddd").PaddingHorizontal(6).PaddingVertical(2).Text(skill).FontSize(9.5f);
                                }
                            });
                        }
                    });
                }
            });
        }

        private void BuildProjectsSection(IContainer container, List<TimelineItem> timelineItems)
        {
            var projects = timelineItems?.Where(t => t.Type == TimelineItemType.Project)
                .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                .ToList();

            if (projects == null || !projects.Any()) return;

            container.PaddingBottom(10).Column(col =>
            {
                col.Item().Text("PROJETS").FontSize(12).Bold().LetterSpacing(0.5f).LineHeight(2.0f);
                
                foreach (var project in projects)
                {
                    col.Item().PaddingBottom(6).Column(projectCol =>
                    {
                        projectCol.Item().Text(project.Title ?? "Projet").FontSize(11.5f).Bold();
                        if (!string.IsNullOrWhiteSpace(project.Description))
                        {
                            projectCol.Item().Text(project.Description).FontSize(10).FontColor("#555");
                        }
                    });
                }
            });
        }

        private void BuildEducationSection(IContainer container, List<TimelineItem> timelineItems)
        {
            var education = timelineItems?.Where(t => t.Type == TimelineItemType.Education)
                .OrderByDescending(t => t.GraduationYear ?? (t.EndDate?.Year ?? (t.StartDate?.Year ?? int.MinValue)))
                .ToList();

            if (education == null || !education.Any()) return;

            container.PaddingBottom(10).Column(col =>
            {
                col.Item().Text("FORMATION").FontSize(12).Bold().LetterSpacing(0.5f).LineHeight(2.0f);
                
                foreach (var edu in education)
                {
                    col.Item().PaddingBottom(6).Column(eduCol =>
                    {
                        eduCol.Item().Text(edu.Title ?? "").FontSize(11.5f).Bold();
                        if (!string.IsNullOrWhiteSpace(edu.Organization))
                        {
                            eduCol.Item().Text(edu.Organization).FontSize(10);
                        }
                        if (edu.GraduationYear.HasValue)
                        {
                            eduCol.Item().Text(edu.GraduationYear.Value.ToString()).FontSize(10).FontColor("#555");
                        }
                    });
                }
            });
        }

        private void BuildExperienceSection(IContainer container, List<TimelineItem> timelineItems, List<Skills> skills)
        {
            var experiences = timelineItems?.Where(t => t.Type == TimelineItemType.Experience)
                .OrderByDescending(t => t.StartDate ?? DateTime.MinValue)
                .ToList();

            if (experiences == null || !experiences.Any()) return;

            container.Column(col =>
            {
                col.Item().Text("EXPÉRIENCE").FontSize(12).Bold().LetterSpacing(0.5f).LineHeight(2.0f);
                
                foreach (var exp in experiences)
                {
                    col.Item().PaddingBottom(10).Column(expCol =>
                    {
                        // Title with organization and period
                        var titleParts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(exp.Title)) titleParts.Add(exp.Title);
                        if (!string.IsNullOrWhiteSpace(exp.Organization)) titleParts.Add(exp.Organization);
                        
                        var period = BuildPeriod(exp);
                        if (!string.IsNullOrWhiteSpace(period)) titleParts.Add(period);
                        
                        expCol.Item().Text(string.Join(" — ", titleParts)).FontSize(11.5f).Bold();
                        
                        // Subtitle if available
                        if (!string.IsNullOrWhiteSpace(exp.Subtitle))
                        {
                            expCol.Item().Text(exp.Subtitle).FontSize(10).FontColor("#555");
                        }
                        
                        // Description
                        if (!string.IsNullOrWhiteSpace(exp.Description))
                        {
                            expCol.Item().Text(exp.Description).FontSize(10).LineHeight(2.0f);
                        }
                        
                        // Bullet points
                        if (exp.BulletPoints != null && exp.BulletPoints.Any())
                        {
                            expCol.Item().PaddingLeft(12).Column(bulletCol =>
                            {
                                foreach (var bullet in exp.BulletPoints)
                                {
                                    if (!string.IsNullOrWhiteSpace(bullet))
                                    {
                                        bulletCol.Item().Text($"• {bullet}").FontSize(10).LineHeight(2.0f);
                                    }
                                }
                            });
                        }
                        
                        // Related skills
                        var relatedSkills = skills?.Where(s => s.RelatedTimelineItems != null && 
                            s.RelatedTimelineItems.Any(r => r.Id == exp.Id)).ToList();
                        
                        if (relatedSkills != null && relatedSkills.Any())
                        {
                            expCol.Item().PaddingTop(4).Text("Compétences :").FontSize(10).Bold();
                            var skillsText = relatedSkills.SelectMany(s => s.SkillsList ?? new List<string>()).Distinct();
                            expCol.Item().Text(string.Join(", ", skillsText)).FontSize(9).FontColor("#555");
                        }
                    });
                }
            });
        }

        private static string BuildPeriod(TimelineItem item)
        {
            if (item.StartDate.HasValue)
            {
                var start = item.StartDate.Value.ToString("MM/yyyy");
                string end;
                
                if (item.IsCurrentPosition)
                {
                    end = "Actuel";
                }
                else if (item.EndDate.HasValue)
                {
                    end = item.EndDate.Value.ToString("MM/yyyy");
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
            if (item.GraduationYear.HasValue) return item.GraduationYear.Value.ToString();
            return string.Empty;
        }
    }
}
