using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Statistics;
using SurveyApp.Server.Services.Interfaces;

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


    }
}