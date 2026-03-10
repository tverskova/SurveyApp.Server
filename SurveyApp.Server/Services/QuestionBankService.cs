using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.DTOs.QuestionBank;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class QuestionBankService : IQuestionBankService
    {
        private readonly ApplicationDbContext _context;

        public QuestionBankService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuestionBankItemDto>> GetAllAsync()
        {
            return await _context.QuestionBankItems
                .Include(q => q.Options)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuestionBankItemDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    HasCorrectAnswer = q.HasCorrectAnswer,
                    RatingMin = q.RatingMin,
                    RatingMax = q.RatingMax,
                    Image = q.Image,
                    Category = q.Category,
                    CreatedAt = q.CreatedAt,
                    Options = q.Options
                        .OrderBy(o => o.Order)
                        .Select(o => new QuestionBankOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            Order = o.Order,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<QuestionBankItemDto?> GetByIdAsync(int id)
        {
            return await _context.QuestionBankItems
                .Include(q => q.Options)
                .Where(q => q.Id == id)
                .Select(q => new QuestionBankItemDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    HasCorrectAnswer = q.HasCorrectAnswer,
                    RatingMin = q.RatingMin,
                    RatingMax = q.RatingMax,
                    Image = q.Image,
                    Category = q.Category,
                    CreatedAt = q.CreatedAt,
                    Options = q.Options
                        .OrderBy(o => o.Order)
                        .Select(o => new QuestionBankOptionDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            Order = o.Order,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(CreateQuestionBankItemDto dto)
        {
            var item = new QuestionBankItem
            {
                Text = dto.Text,
                QuestionType = dto.QuestionType,
                IsRequired = dto.IsRequired,
                HasCorrectAnswer = dto.HasCorrectAnswer,
                RatingMin = dto.RatingMin,
                RatingMax = dto.RatingMax,
                Image = dto.Image,
                Category = dto.Category,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var optionDto in dto.Options)
            {
                item.Options.Add(new QuestionBankOption
                {
                    Text = optionDto.Text,
                    Order = optionDto.Order,
                    IsCorrect = optionDto.IsCorrect
                });
            }

            _context.QuestionBankItems.Add(item);
            await _context.SaveChangesAsync();

            return item.Id;
        }

        public async Task<bool> UpdateAsync(int id, CreateQuestionBankItemDto dto)
        {
            var item = await _context.QuestionBankItems
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (item == null)
                return false;

            item.Text = dto.Text;
            item.QuestionType = dto.QuestionType;
            item.IsRequired = dto.IsRequired;
            item.HasCorrectAnswer = dto.HasCorrectAnswer;
            item.RatingMin = dto.RatingMin;
            item.RatingMax = dto.RatingMax;
            item.Image = dto.Image;
            item.Category = dto.Category;

            var existingOptions = item.Options.ToList();
            _context.QuestionBankOptions.RemoveRange(existingOptions);
            item.Options = new List<QuestionBankOption>();

            foreach (var optionDto in dto.Options)
            {
                item.Options.Add(new QuestionBankOption
                {
                    Text = optionDto.Text,
                    Order = optionDto.Order,
                    IsCorrect = optionDto.IsCorrect
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.QuestionBankItems
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (item == null)
                return false;

            _context.QuestionBankItems.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CreateQuestionDto?> ConvertToSurveyQuestionAsync(int id)
        {
            var item = await _context.QuestionBankItems
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (item == null)
                return null;

            return new CreateQuestionDto
            {
                Text = item.Text,
                QuestionType = item.QuestionType,
                IsRequired = item.IsRequired,
                HasCorrectAnswer = item.HasCorrectAnswer,
                RatingMin = item.RatingMin,
                RatingMax = item.RatingMax,
                Image = item.Image,
                Options = item.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new CreateQuestionOptionDto
                    {
                        Text = o.Text,
                        Order = o.Order,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList()
            };
        }
    }
}