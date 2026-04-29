using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Statistics;
using SurveyApp.Server.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace SurveyApp.Server.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemStatisticsDto> GetSystemStatisticsAsync()
        {
            var surveys = await _context.Surveys
                .Select(s => new SystemSurveyStatisticsItemDto
                {
                    SurveyId = s.Id,
                    Title = s.Title,
                    IsPublished = s.IsPublished,
                    ResponsesCount = s.Responses.Count
                })
                .OrderByDescending(s => s.ResponsesCount)
                .ThenBy(s => s.Title)
                .ToListAsync();

            return new SystemStatisticsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalSurveys = await _context.Surveys.CountAsync(),
                PublishedSurveys = await _context.Surveys.CountAsync(s => s.IsPublished),
                TotalResponses = await _context.SurveyResponses.CountAsync(),
                DraftSurveys = await _context.Surveys.CountAsync(s => !s.IsPublished),
                ActiveSurveys = await _context.Surveys.CountAsync(s =>
                    s.IsPublished &&
                    (s.StartDate == null || s.StartDate <= DateTime.UtcNow) &&
                    (s.EndDate == null || s.EndDate >= DateTime.UtcNow)),
                Surveys = surveys
            };
        }

        public async Task<SurveyStatisticsDto?> GetSurveyStatisticsAsync(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                        .ThenInclude(a => a.AnswerOptions)
                            .ThenInclude(ao => ao.QuestionOption)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return null;

            var result = new SurveyStatisticsDto
            {
                SurveyId = survey.Id,
                Title = survey.Title,
                TotalResponses = survey.Responses.Count,
                Questions = new List<QuestionStatisticsDto>()
            };

            var orderedQuestions = survey.Questions
                .OrderBy(q => q.Order)
                .ToList();

            foreach (var question in orderedQuestions)
            {
                var questionAnswers = survey.Responses
                    .SelectMany(r => r.Answers)
                    .Where(a => a.QuestionId == question.Id)
                    .ToList();

                var questionStat = new QuestionStatisticsDto
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.QuestionType
                };

                switch (question.QuestionType)
                {
                    case "SingleChoice":
                    case "MultipleChoice":
                        BuildChoiceStatistics(question, questionAnswers, questionStat);
                        break;

                    case "YesNo":
                        BuildYesNoStatistics(questionAnswers, questionStat);
                        break;

                    case "Rating":
                        BuildRatingStatistics(question, questionAnswers, questionStat);
                        break;

                    case "Number":
                        BuildNumberStatistics(questionAnswers, questionStat);
                        break;

                    case "Text":
                        BuildTextStatistics(questionAnswers, questionStat);
                        break;
                }

                result.Questions.Add(questionStat);
            }

            var responses = survey.Responses;

            if (responses.Any())
            {
                result.FirstResponseDate = responses.Min(r => r.StartedAt);
                result.LastResponseDate = responses.Max(r => r.SubmittedAt);

                var durations = responses
                    .Where(r => r.SubmittedAt.HasValue)
                    .Select(r => (r.SubmittedAt!.Value - r.StartedAt).TotalSeconds)
                    .ToList();

                if (durations.Any())
                    result.AverageCompletionTimeSeconds = durations.Average();
            }

            return result;
        }

        private static void BuildChoiceStatistics(
            Question question,
            List<Answer> questionAnswers,
            QuestionStatisticsDto stat)
        {
            var options = question.Options
                .OrderBy(o => o.Order)
                .ToList();

            foreach (var option in options)
            {
                var count = questionAnswers
                    .SelectMany(a => a.AnswerOptions)
                    .Count(ao => ao.QuestionOptionId == option.Id);

                stat.Labels.Add(option.Text);
                stat.Values.Add(count);
            }
        }

        private static void BuildYesNoStatistics(
            List<Answer> questionAnswers,
            QuestionStatisticsDto stat)
        {
            var yesCount = questionAnswers.Count(a => a.YesNoAnswer == true);
            var noCount = questionAnswers.Count(a => a.YesNoAnswer == false);

            stat.Labels.Add("Да");
            stat.Values.Add(yesCount);

            stat.Labels.Add("Нет");
            stat.Values.Add(noCount);
        }

        private static void BuildRatingStatistics(
            Question question,
            List<Answer> questionAnswers,
            QuestionStatisticsDto stat)
        {
            var min = question.RatingMin ?? 1;
            var max = question.RatingMax ?? 5;

            for (int value = min; value <= max; value++)
            {
                var count = questionAnswers.Count(a => a.RatingAnswer == value);
                stat.Labels.Add(value.ToString());
                stat.Values.Add(count);
            }

            var ratings = questionAnswers
                .Where(a => a.RatingAnswer.HasValue)
                .Select(a => (double)a.RatingAnswer!.Value)
                .ToList();

            if (ratings.Count > 0)
            {
                stat.AverageValue = ratings.Average();
                stat.MinValue = ratings.Min();
                stat.MaxValue = ratings.Max();
            }
        }

        private static void BuildNumberStatistics(
            List<Answer> questionAnswers,
            QuestionStatisticsDto stat)
        {
            var numbers = questionAnswers
                .Where(a => a.NumberAnswer.HasValue)
                .Select(a => a.NumberAnswer!.Value)
                .ToList();

            if (numbers.Count > 0)
            {
                stat.AverageValue = numbers.Average();
                stat.MinValue = numbers.Min();
                stat.MaxValue = numbers.Max();

                stat.Labels.Add("Минимум");
                stat.Values.Add((int)Math.Round(stat.MinValue.Value));

                stat.Labels.Add("Среднее");
                stat.Values.Add((int)Math.Round(stat.AverageValue.Value));

                stat.Labels.Add("Максимум");
                stat.Values.Add((int)Math.Round(stat.MaxValue.Value));
            }
        }

        private static void BuildTextStatistics(
            List<Answer> questionAnswers,
            QuestionStatisticsDto stat)
        {
            stat.TextAnswers = questionAnswers
                .Where(a => !string.IsNullOrWhiteSpace(a.TextAnswer))
                .Select(a => a.TextAnswer!)
                .ToList();
        }


        public async Task<byte[]> ExportSurveyStatisticsToPdfAsync(int surveyId)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var statistics = await GetSurveyStatisticsAsync(surveyId);

            if (statistics == null)
                throw new Exception("Опрос не найден.");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().Text($"Статистика опроса: {statistics.Title}")
                                .FontSize(18)
                                .Bold();

                            col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Border(1).Padding(10).Column(summary =>
                        {
                            summary.Spacing(4);
                            summary.Item().Text($"Всего прохождений: {statistics.TotalResponses}");

                            if (statistics.AverageCompletionTimeSeconds > 0)
                                summary.Item().Text($"Среднее время прохождения: {Math.Round(statistics.AverageCompletionTimeSeconds)} сек.");

                            if (statistics.FirstResponseDate.HasValue)
                                summary.Item().Text($"Первый ответ: {statistics.FirstResponseDate.Value.ToLocalTime():dd.MM.yyyy HH:mm}");

                            if (statistics.LastResponseDate.HasValue)
                                summary.Item().Text($"Последний ответ: {statistics.LastResponseDate.Value.ToLocalTime():dd.MM.yyyy HH:mm}");
                        });

                        for (int i = 0; i < statistics.Questions.Count; i++)
                        {
                            var question = statistics.Questions[i];

                            col.Item().Border(1).Padding(10).Column(q =>
                            {
                                q.Spacing(8);

                                q.Item().Text($"{i + 1}. {question.Text}")
                                    .Bold()
                                    .FontSize(13);

                                q.Item().Text($"Тип: {question.Type}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);

                                if (question.Type == "Text")
                                {
                                    if (question.TextAnswers.Any())
                                    {
                                        foreach (var answer in question.TextAnswers)
                                            q.Item().Text($"• {answer}");
                                    }
                                    else
                                    {
                                        q.Item().Text("Нет текстовых ответов.");
                                    }
                                }
                                else
                                {
                                    if (question.Labels.Any() && question.Values.Any())
                                    {
                                        var svg = question.Type == "YesNo"
                                            ? GeneratePieChartSvg(question.Labels, question.Values)
                                            : GenerateBarChartSvg(question.Labels, question.Values);

                                        q.Item()
                                            .AlignCenter()
                                            .Width(480)
                                            .Svg(svg);

                                        q.Item().PaddingTop(4).Column(statsCol =>
                                        {
                                            statsCol.Spacing(2);

                                            for (int j = 0; j < question.Labels.Count; j++)
                                                statsCol.Item().Text($"{question.Labels[j]}: {question.Values[j]}");

                                            if (question.AverageValue.HasValue)
                                                statsCol.Item().Text($"Среднее: {question.AverageValue:F2}");

                                            if (question.MinValue.HasValue)
                                                statsCol.Item().Text($"Минимум: {question.MinValue}");

                                            if (question.MaxValue.HasValue)
                                                statsCol.Item().Text($"Максимум: {question.MaxValue}");
                                        });
                                    }
                                    else
                                    {
                                        q.Item().Text("Нет данных для построения диаграммы.");
                                    }
                                }
                            });
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Отчёт по статистике опроса");
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> ExportSurveyStatisticsToExcelAsync(int surveyId)
        {
            var statistics = await GetSurveyStatisticsAsync(surveyId);

            if (statistics == null)
                throw new Exception("Опрос не найден.");

            using var workbook = new XLWorkbook();

            // 🔹 Лист 1 — Общая информация
            var summarySheet = workbook.Worksheets.Add("Общая информация");

            summarySheet.Cell(1, 1).Value = "Название опроса";
            summarySheet.Cell(1, 2).Value = statistics.Title;

            summarySheet.Cell(2, 1).Value = "Всего прохождений";
            summarySheet.Cell(2, 2).Value = statistics.TotalResponses;

            summarySheet.Cell(3, 1).Value = "Среднее время (сек)";
            summarySheet.Cell(3, 2).Value = Math.Round(statistics.AverageCompletionTimeSeconds);

            summarySheet.Cell(4, 1).Value = "Первый ответ";
            summarySheet.Cell(4, 2).Value = statistics.FirstResponseDate?.ToLocalTime();

            summarySheet.Cell(5, 1).Value = "Последний ответ";
            summarySheet.Cell(5, 2).Value = statistics.LastResponseDate?.ToLocalTime();

            summarySheet.Columns().AdjustToContents();

            // 🔹 Листы по каждому вопросу
            for (int i = 0; i < statistics.Questions.Count; i++)
            {
                var question = statistics.Questions[i];
                var sheetName = $"Вопрос_{i + 1}";
                var ws = workbook.Worksheets.Add(sheetName);

                ws.Cell(1, 1).Value = "Вопрос";
                ws.Cell(1, 2).Value = question.Text;

                ws.Cell(2, 1).Value = "Тип";
                ws.Cell(2, 2).Value = question.Type;

                int row = 4;

                if (question.Type == "Text")
                {
                    ws.Cell(row, 1).Value = "Текстовые ответы";
                    row++;

                    if (question.TextAnswers.Any())
                    {
                        foreach (var answer in question.TextAnswers)
                        {
                            ws.Cell(row, 1).Value = answer;
                            row++;
                        }
                    }
                    else
                    {
                        ws.Cell(row, 1).Value = "Нет ответов";
                    }
                }
                else
                {
                    ws.Cell(row, 1).Value = "Вариант";
                    ws.Cell(row, 2).Value = "Количество";
                    row++;

                    for (int j = 0; j < question.Labels.Count; j++)
                    {
                        ws.Cell(row, 1).Value = question.Labels[j];
                        ws.Cell(row, 2).Value = question.Values[j];
                        row++;
                    }

                    if (question.AverageValue.HasValue)
                    {
                        row++;
                        ws.Cell(row, 1).Value = "Среднее";
                        ws.Cell(row, 2).Value = question.AverageValue.Value;
                    }
                }

                ws.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }        
        private static string GenerateBarChartSvg(List<string> labels, List<int> values)
        {
            const int width = 480;
            const int height = 170;
            const int marginLeft = 50;
            const int marginRight = 20;
            const int marginTop = 20;
            const int marginBottom = 60;

            int chartWidth = width - marginLeft - marginRight;
            int chartHeight = height - marginTop - marginBottom;

            int maxValue = Math.Max(values.DefaultIfEmpty(0).Max(), 1);
            double barWidth = labels.Count > 0 ? chartWidth / (double)labels.Count : chartWidth;

            var sb = new StringBuilder();

            sb.AppendLine($"""
<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}">
  <rect width="100%" height="100%" fill="white"/>
  <line x1="{marginLeft}" y1="{marginTop + chartHeight}" x2="{marginLeft + chartWidth}" y2="{marginTop + chartHeight}" stroke="black" stroke-width="1"/>
  <line x1="{marginLeft}" y1="{marginTop}" x2="{marginLeft}" y2="{marginTop + chartHeight}" stroke="black" stroke-width="1"/>
""");

            for (int i = 0; i < labels.Count; i++)
            {
                double value = values[i];
                double normalized = value / maxValue;
                double h = normalized * chartHeight;
                double x = marginLeft + i * barWidth + 10;
                double y = marginTop + chartHeight - h;
                double w = Math.Max(barWidth - 20, 10);

                string safeLabel = EscapeXml(TrimLabel(labels[i], 18));

                sb.AppendLine($"""
  <rect x="{x.ToString(CultureInfo.InvariantCulture)}"
        y="{y.ToString(CultureInfo.InvariantCulture)}"
        width="{w.ToString(CultureInfo.InvariantCulture)}"
        height="{h.ToString(CultureInfo.InvariantCulture)}"
        fill="#4e73df"/>
  <text x="{(x + w / 2).ToString(CultureInfo.InvariantCulture)}"
        y="{(y - 5).ToString(CultureInfo.InvariantCulture)}"
        text-anchor="middle"
        font-size="11"
        fill="black">{value}</text>
  <text x="{(x + w / 2).ToString(CultureInfo.InvariantCulture)}"
        y="{(marginTop + chartHeight + 18).ToString(CultureInfo.InvariantCulture)}"
        text-anchor="middle"
        font-size="10"
        fill="black">{safeLabel}</text>
""");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }
        private static string GeneratePieChartSvg(List<string> labels, List<int> values)
        {
            const int width = 480;
            const int height = 170;
            const int cx = 110;
            const int cy = 85;
            const int radius = 50;

            var colors = new[]
            {
        "#4e73df",
        "#1cc88a",
        "#36b9cc",
        "#f6c23e",
        "#e74a3b",
        "#858796"
    };

            int total = values.Sum();
            if (total <= 0)
                total = 1;

            double startAngle = -90;

            var sb = new StringBuilder();

            sb.AppendLine($"""
<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}">
  <rect width="100%" height="100%" fill="white"/>
""");

            for (int i = 0; i < values.Count; i++)
            {
                double sweep = values[i] * 360.0 / total;
                double endAngle = startAngle + sweep;

                string path = DescribePieSlice(cx, cy, radius, startAngle, endAngle);
                string color = colors[i % colors.Length];

                sb.AppendLine($"""  <path d="{path}" fill="{color}" stroke="white" stroke-width="1"/>""");

                startAngle = endAngle;
            }

            int legendX = 210;
            int legendY = 30;

            for (int i = 0; i < labels.Count; i++)
            {
                string color = colors[i % colors.Length];
                string label = EscapeXml(labels[i]);
                int y = legendY + i * 24;

                sb.AppendLine($"""
  <rect x="{legendX}" y="{y}" width="14" height="14" fill="{color}"/>
  <text x="{legendX + 22}" y="{y + 12}" font-size="12" fill="black">{label}: {values[i]}</text>
""");
            }

            sb.AppendLine("</svg>");
            return sb.ToString();
        }
        private static string DescribePieSlice(double cx, double cy, double r, double startAngle, double endAngle)
        {
            var start = PolarToCartesian(cx, cy, r, endAngle);
            var end = PolarToCartesian(cx, cy, r, startAngle);

            int largeArcFlag = endAngle - startAngle <= 180 ? 0 : 1;

            return string.Format(
                CultureInfo.InvariantCulture,
                "M {0} {1} L {2} {3} A {4} {4} 0 {5} 0 {6} {7} Z",
                cx, cy,
                start.X, start.Y,
                r,
                largeArcFlag,
                end.X, end.Y
            );
        }

        private static (double X, double Y) PolarToCartesian(double cx, double cy, double r, double angleInDegrees)
        {
            double angleInRadians = (angleInDegrees - 90) * Math.PI / 180.0;
            return (
                cx + (r * Math.Cos(angleInRadians)),
                cy + (r * Math.Sin(angleInRadians))
            );
        }
        private static string EscapeXml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string TrimLabel(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return text.Length <= maxLength ? text : text[..maxLength] + "...";
        }

        public async Task<List<SurveyParticipantDto>> GetSurveyParticipantsAsync(int surveyId)
        {
            var responses = await _context.SurveyResponses
                .AsNoTracking()
                .Include(r => r.User)
                    .ThenInclude(u => u!.UserProfile)
                .Where(r => r.SurveyId == surveyId && r.SubmittedAt.HasValue)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return responses.Select(r =>
            {
                var profile = r.User?.UserProfile;

                var fullName = string.Join(" ", new[]
                {
            profile?.LastName,
            profile?.FirstName
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    fullName = r.User?.UserName ?? "Анонимный пользователь";
                }

                return new SurveyParticipantDto
                {
                    ResponseId = r.Id,
                    UserId = r.UserId ?? string.Empty,
                    FullName = fullName,
                    Email = r.User?.Email,
                    StartedAt = r.StartedAt,
                    SubmittedAt = r.SubmittedAt
                };
            }).ToList();
        }

        public async Task<UserSurveyAnswersDto?> GetUserSurveyAnswersAsync(int responseId)
        {
            var response = await _context.SurveyResponses
                .AsNoTracking()
                .Include(r => r.Survey)
                .Include(r => r.User)
                    .ThenInclude(u => u!.UserProfile)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.Question)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerOptions)
                        .ThenInclude(ao => ao.QuestionOption)
                .FirstOrDefaultAsync(r => r.Id == responseId);

            if (response is null)
                return null;

            var profile = response.User?.UserProfile;

            var fullName = string.Join(" ", new[]
            {
        profile?.LastName,
        profile?.FirstName
    }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = response.User?.UserName ?? "Анонимный пользователь";
            }

            return new UserSurveyAnswersDto
            {
                ResponseId = response.Id,
                SurveyId = response.SurveyId,
                SurveyTitle = response.Survey.Title,
                UserFullName = fullName,
                UserEmail = response.User?.Email,
                StartedAt = response.StartedAt,
                SubmittedAt = response.SubmittedAt,
                Answers = response.Answers
                    .OrderBy(a => a.Question.Order)
                    .Select(a => new UserAnswerDto
                    {
                        QuestionId = a.QuestionId,
                        QuestionText = a.Question.Text,
                        QuestionType = a.Question.QuestionType,
                        QuestionOrder = a.Question.Order,
                        AnswerText = FormatAnswer(a)
                    })
                    .ToList()
            };
        }

        private static string FormatAnswer(Answer answer)
        {
            return answer.Question.QuestionType switch
            {
                "Text" => string.IsNullOrWhiteSpace(answer.TextAnswer)
                    ? "—"
                    : answer.TextAnswer,

                "Number" => answer.NumberAnswer.HasValue
                    ? answer.NumberAnswer.Value.ToString("G", CultureInfo.CurrentCulture)
                    : "—",

                "Rating" => answer.RatingAnswer.HasValue
                    ? answer.RatingAnswer.Value.ToString()
                    : "—",

                "YesNo" => answer.YesNoAnswer.HasValue
                    ? answer.YesNoAnswer.Value ? "Да" : "Нет"
                    : "—",

                "SingleChoice" or "MultipleChoice" => answer.AnswerOptions.Any()
                    ? string.Join(", ", answer.AnswerOptions
                        .OrderBy(ao => ao.QuestionOption.Order)
                        .Select(ao => ao.QuestionOption.Text))
                    : "—",

                _ => "—"
            };
        }

    }

}