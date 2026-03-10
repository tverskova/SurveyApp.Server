using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Responses;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class ResponseService : IResponseService
    {
        private readonly ApplicationDbContext _context;

        public ResponseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SurveyResponseResultDto?> SubmitResponseAsync(SubmitSurveyResponseDto dto, string? userId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == dto.SurveyId);

            if (survey == null)
                return null;

            var response = new SurveyResponse
            {
                SurveyId = dto.SurveyId,
                UserId = survey.IsAnonymous ? null : userId,
                StartedAt = DateTime.UtcNow,
                SubmittedAt = DateTime.UtcNow
            };

            foreach (var answerDto in dto.Answers)
            {
                var question = survey.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                if (question == null)
                    continue;

                var answer = new Answer
                {
                    QuestionId = question.Id,
                    TextAnswer = answerDto.TextAnswer,
                    NumberAnswer = answerDto.NumberAnswer,
                    RatingAnswer = answerDto.RatingAnswer,
                    YesNoAnswer = answerDto.YesNoAnswer
                };

                if (answerDto.SelectedOptionIds != null && answerDto.SelectedOptionIds.Any())
                {
                    var validOptionIds = question.Options
                        .Where(o => answerDto.SelectedOptionIds.Contains(o.Id))
                        .Select(o => o.Id)
                        .ToList();

                    foreach (var optionId in validOptionIds)
                    {
                        answer.AnswerOptions.Add(new AnswerOption
                        {
                            QuestionOptionId = optionId
                        });
                    }
                }

                response.Answers.Add(answer);
            }

            _context.SurveyResponses.Add(response);
            await _context.SaveChangesAsync();

            return new SurveyResponseResultDto
            {
                ResponseId = response.Id,
                SurveyId = response.SurveyId,
                SubmittedAt = response.SubmittedAt ?? DateTime.UtcNow,
                Message = "Ответы успешно сохранены."
            };
        }
    }
}