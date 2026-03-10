using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.DTOs.Surveys;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ApplicationDbContext _context;

        public SurveyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // Admin
        // ==========================

        public async Task<int> CreateSurveyAsync(CreateSurveyDto dto, string? createdByUserId)
        {
            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                IsAnonymous = dto.IsAnonymous,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsPublished = false,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };


            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    Text = questionDto.Text,
                    QuestionType = questionDto.QuestionType,
                    IsRequired = questionDto.IsRequired,
                    HasCorrectAnswer = questionDto.HasCorrectAnswer,
                    Order = questionDto.Order,
                    RatingMin = questionDto.RatingMin,
                    RatingMax = questionDto.RatingMax,
                    Image = questionDto.Image
                };

                foreach (var optionDto in questionDto.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Text = optionDto.Text,
                        Order = optionDto.Order,
                        IsCorrect = optionDto.IsCorrect
                    });
                }

                survey.Questions.Add(question);
            }

            _context.Surveys.Add(survey);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveChanges failed: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            return survey.Id;
        }

        public async Task<List<SurveyListItemDto>> GetAllSurveysAsync()
        {
            return await _context.Surveys
                .Select(s => new SurveyListItemDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsPublished = s.IsPublished,
                    IsAnonymous = s.IsAnonymous,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                })
                .ToListAsync();
        }

        public async Task<bool> PublishSurveyAsync(int surveyId)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return false;

            survey.IsPublished = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishSurveyAsync(int surveyId)
        {
            var survey = await _context.Surveys.FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return false;

            survey.IsPublished = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSurveyAsync(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return false;

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CreateSurveyDto?> GetSurveyForEditAsync(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return null;

            return new CreateSurveyDto
            {
                Title = survey.Title,
                Description = survey.Description,
                IsAnonymous = survey.IsAnonymous,
                StartDate = survey.StartDate,
                EndDate = survey.EndDate,
                Questions = survey.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new CreateQuestionDto
                    {
                        Text = q.Text,
                        QuestionType = q.QuestionType,
                        IsRequired = q.IsRequired,
                        Order = q.Order,
                        RatingMin = q.RatingMin,
                        RatingMax = q.RatingMax,
                        HasCorrectAnswer = q.HasCorrectAnswer,
                        Image = q.Image,
                        Options = q.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new CreateQuestionOptionDto
                            {
                                Text = o.Text,
                                Order = o.Order,
                                IsCorrect = o.IsCorrect
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        public async Task<bool> UpdateSurveyAsync(int surveyId, CreateSurveyDto dto)
        {
            var survey = await _context.Surveys
        .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
        .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return false;

            var hasResponses = await _context.SurveyResponses
                .AnyAsync(r => r.SurveyId == surveyId);

            if (hasResponses)
            {
                throw new Exception("Нельзя изменять структуру опроса, по которому уже есть ответы пользователей.");
            }

            survey.Title = dto.Title;
            survey.Description = dto.Description;
            survey.IsAnonymous = dto.IsAnonymous;
            survey.StartDate = dto.StartDate;
            survey.EndDate = dto.EndDate;

            // Сначала делаем снимки коллекций, чтобы избежать ошибки
            var existingQuestions = survey.Questions.ToList();
            var existingOptions = existingQuestions
                .SelectMany(q => q.Options)
                .ToList();

            // Удаляем старые варианты и вопросы
            _context.QuestionOptions.RemoveRange(existingOptions);
            _context.Questions.RemoveRange(existingQuestions);

            // Очищаем навигационную коллекцию
            survey.Questions = new List<Question>();

            // Добавляем новую структуру вопросов
            foreach (var questionDto in dto.Questions)
            {
                var question = new Question
                {
                    Text = questionDto.Text,
                    QuestionType = questionDto.QuestionType,
                    IsRequired = questionDto.IsRequired,
                    Order = questionDto.Order,
                    RatingMin = questionDto.RatingMin,
                    RatingMax = questionDto.RatingMax,
                    HasCorrectAnswer = questionDto.HasCorrectAnswer,
                    Image = questionDto.Image
                };

                foreach (var optionDto in questionDto.Options)
                {
                    question.Options.Add(new QuestionOption
                    {
                        Text = optionDto.Text,
                        Order = optionDto.Order,
                        IsCorrect = optionDto.IsCorrect
                    });
                }

                survey.Questions.Add(question);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"SaveChanges failed: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            return true;
        }

        // ==========================
        // User
        // ==========================

        public async Task<List<SurveyListItemDto>> GetPublishedSurveysAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Surveys
                .Where(s =>
                    s.IsPublished &&
                    (s.StartDate == null || s.StartDate.Value.Date <= today) &&
                    (s.EndDate == null || s.EndDate.Value.Date >= today))
                .Select(s => new SurveyListItemDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsPublished = s.IsPublished,
                    IsAnonymous = s.IsAnonymous,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                })
                .ToListAsync();
        }

        public async Task<SurveyDetailsDto?> GetSurveyDetailsAsync(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == surveyId);

            if (survey == null)
                return null;

            return new SurveyDetailsDto
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                IsAnonymous = survey.IsAnonymous,
                Questions = survey.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        QuestionType = q.QuestionType,
                        IsRequired = q.IsRequired,
                        Order = q.Order,
                        RatingMin = q.RatingMin,
                        RatingMax = q.RatingMax,
                        Image = q.Image,
                        Options = q.Options
                            .OrderBy(o => o.Order)
                            .Select(o => new QuestionOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text,
                                Order = o.Order
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }
    }
}