using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.UserResults;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class UserResultsService : IUserResultsService
    {
        private readonly ApplicationDbContext _context;

        public UserResultsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserResultsSummaryDto> GetSummaryAsync(string userId)
        {
            var responses = _context.SurveyResponses
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.SubmittedAt.HasValue);

            return new UserResultsSummaryDto
            {
                CompletedSurveysCount = await responses.CountAsync(),
                SentAnswersCount = await responses.SelectMany(r => r.Answers).CountAsync(),
                LastSubmittedAt = await responses.MaxAsync(r => r.SubmittedAt)
            };
        }

        public async Task<List<UserSurveyResultListItemDto>> GetResultsAsync(string userId)
        {
            return await _context.SurveyResponses
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.SubmittedAt.HasValue)
                .OrderByDescending(r => r.SubmittedAt)
                .Select(r => new UserSurveyResultListItemDto
                {
                    ResponseId = r.Id,
                    SurveyId = r.SurveyId,
                    SurveyTitle = r.Survey.Title,
                    SurveyDescription = r.Survey.Description,
                    SubmittedAt = r.SubmittedAt,
                    QuestionsCount = r.Survey.Questions.Count,
                    AnswersCount = r.Answers.Count,
                    Status = r.SubmittedAt.HasValue ? "Отправлено" : "Не завершено"
                })
                .ToListAsync();
        }

        public async Task<UserSurveyResultDetailsDto?> GetResultDetailsAsync(string userId, int responseId)
        {
            var response = await _context.SurveyResponses
                .AsNoTracking()
                .Include(r => r.Survey)
                    .ThenInclude(s => s.Questions)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.Question)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerOptions)
                        .ThenInclude(ao => ao.QuestionOption)
                .FirstOrDefaultAsync(r =>
                    r.Id == responseId &&
                    r.UserId == userId &&
                    r.SubmittedAt.HasValue);

            if (response == null)
                return null;

            var answersByQuestionId = response.Answers
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.First());

            var orderedQuestions = response.Survey.Questions
                .OrderBy(q => q.Order)
                .ToList();

            return new UserSurveyResultDetailsDto
            {
                ResponseId = response.Id,
                SurveyId = response.SurveyId,
                SurveyTitle = response.Survey.Title,
                SurveyDescription = response.Survey.Description,
                StartedAt = response.StartedAt,
                SubmittedAt = response.SubmittedAt,
                QuestionsCount = orderedQuestions.Count,
                AnswersCount = response.Answers.Count,
                Status = response.SubmittedAt.HasValue ? "Отправлено" : "Не завершено",
                Answers = orderedQuestions.Select(q =>
                {
                    answersByQuestionId.TryGetValue(q.Id, out var answer);

                    return new UserResultAnswerDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        QuestionType = q.QuestionType,
                        QuestionOrder = q.Order,
                        QuestionImage = q.Image,
                        AnswerText = answer == null ? "Ответ не указан" : FormatAnswer(answer)
                    };
                }).ToList()
            };
        }

        private static string FormatAnswer(Answer answer)
        {
            var value = answer.Question.QuestionType switch
            {
                "Text" => answer.TextAnswer,
                "Number" => answer.NumberAnswer.HasValue
                    ? answer.NumberAnswer.Value.ToString("G", CultureInfo.CurrentCulture)
                    : null,
                "Rating" => answer.RatingAnswer.HasValue
                    ? answer.RatingAnswer.Value.ToString()
                    : null,
                "YesNo" => answer.YesNoAnswer.HasValue
                    ? answer.YesNoAnswer.Value ? "Да" : "Нет"
                    : null,
                "SingleChoice" or "MultipleChoice" => answer.AnswerOptions.Any()
                    ? string.Join(", ", answer.AnswerOptions
                        .OrderBy(ao => ao.QuestionOption.Order)
                        .Select(ao => ao.QuestionOption.Text))
                    : null,
                _ => null
            };

            return string.IsNullOrWhiteSpace(value) ? "Ответ не указан" : value;
        }
    }
}
