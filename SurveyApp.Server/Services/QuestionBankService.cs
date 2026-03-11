using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.DTOs.QuestionBank;
using SurveyApp.Server.Services.Interfaces;
using ClosedXML.Excel;
using System.Xml.Linq;

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
        public async Task<int> ImportFromExcelAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var rows = worksheet.RowsUsed().Skip(1).ToList();
            if (!rows.Any())
                return 0;

            var grouped = rows
                .Select(r => new
                {
                    QuestionCode = r.Cell(1).GetString().Trim(),
                    Category = r.Cell(2).GetString().Trim(),
                    Text = r.Cell(3).GetString().Trim(),
                    QuestionType = r.Cell(4).GetString().Trim(),
                    IsRequired = ParseBool(r.Cell(5).GetString()),
                    HasCorrectAnswer = ParseBool(r.Cell(6).GetString()),
                    RatingMin = ParseNullableInt(r.Cell(7).GetString()),
                    RatingMax = ParseNullableInt(r.Cell(8).GetString()),
                    OptionText = r.Cell(9).GetString().Trim(),
                    OptionOrder = ParseNullableInt(r.Cell(10).GetString()) ?? 0,
                    IsCorrect = ParseBool(r.Cell(11).GetString())
                })
                .GroupBy(x => x.QuestionCode);

            int importedCount = 0;

            foreach (var group in grouped)
            {
                var first = group.First();

                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(first.QuestionType))
                    continue;

                var item = new QuestionBankItem
                {
                    Category = string.IsNullOrWhiteSpace(first.Category) ? null : first.Category,
                    Text = first.Text,
                    QuestionType = first.QuestionType,
                    IsRequired = first.IsRequired,
                    HasCorrectAnswer = first.HasCorrectAnswer,
                    RatingMin = first.RatingMin,
                    RatingMax = first.RatingMax,
                    CreatedAt = DateTime.UtcNow
                };

                if (RequiresOptions(first.QuestionType))
                {
                    foreach (var row in group
                                 .Where(x => !string.IsNullOrWhiteSpace(x.OptionText))
                                 .OrderBy(x => x.OptionOrder))
                    {
                        item.Options.Add(new QuestionBankOption
                        {
                            Text = row.OptionText,
                            Order = row.OptionOrder,
                            IsCorrect = row.IsCorrect
                        });
                    }
                }

                NormalizeQuestionBankItem(item);

                _context.QuestionBankItems.Add(item);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }

        public async Task<int> ImportFromXmlAsync(Stream stream)
        {
            var document = XDocument.Load(stream);

            var questionElements = document.Root?.Elements("Question").ToList();
            if (questionElements == null || !questionElements.Any())
                return 0;

            int importedCount = 0;

            foreach (var q in questionElements)
            {
                var item = new QuestionBankItem
                {
                    Category = q.Element("Category")?.Value,
                    Text = q.Element("Text")?.Value ?? string.Empty,
                    QuestionType = q.Element("QuestionType")?.Value ?? string.Empty,
                    IsRequired = ParseBool(q.Element("IsRequired")?.Value),
                    HasCorrectAnswer = ParseBool(q.Element("HasCorrectAnswer")?.Value),
                    RatingMin = ParseNullableInt(q.Element("RatingMin")?.Value),
                    RatingMax = ParseNullableInt(q.Element("RatingMax")?.Value),
                    CreatedAt = DateTime.UtcNow
                };

                var options = q.Element("Options")?.Elements("Option").ToList() ?? new();

                foreach (var option in options)
                {
                    item.Options.Add(new QuestionBankOption
                    {
                        Text = option.Element("Text")?.Value ?? string.Empty,
                        Order = ParseNullableInt(option.Element("Order")?.Value) ?? 0,
                        IsCorrect = ParseBool(option.Element("IsCorrect")?.Value)
                    });
                }

                if (string.IsNullOrWhiteSpace(item.Text) || string.IsNullOrWhiteSpace(item.QuestionType))
                    continue;

                NormalizeQuestionBankItem(item);

                _context.QuestionBankItems.Add(item);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }

        private static bool ParseBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLowerInvariant();

            return value is "true" or "1" or "да" or "yes";
        }

        private static int? ParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.TryParse(value, out var result) ? result : null;
        }

        private static bool RequiresOptions(string questionType)
        {
            return questionType == "SingleChoice"
                || questionType == "MultipleChoice"
                || questionType == "YesNo";
        }

        private static void NormalizeQuestionBankItem(QuestionBankItem item)
        {
            if (item.QuestionType == "YesNo")
            {
                var yes = item.Options.FirstOrDefault(o => o.Order == 1) ?? new QuestionBankOption();
                var no = item.Options.FirstOrDefault(o => o.Order == 2) ?? new QuestionBankOption();

                yes.Text = "Да";
                yes.Order = 1;

                no.Text = "Нет";
                no.Order = 2;

                item.Options = new List<QuestionBankOption> { yes, no };
                item.RatingMin = null;
                item.RatingMax = null;
                return;
            }

            if (item.QuestionType == "Rating")
            {
                item.Options.Clear();
                item.RatingMin ??= 1;
                item.RatingMax ??= 5;
                return;
            }

            if (item.QuestionType == "Text" || item.QuestionType == "Number")
            {
                item.Options.Clear();
                item.RatingMin = null;
                item.RatingMax = null;
                return;
            }

            item.Options = item.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                .OrderBy(o => o.Order)
                .ToList();
        }
        private static void NormalizePreviewItem(ImportQuestionBankPreviewDto item)
        {
            if (item.QuestionType == "YesNo")
            {
                var yes = item.Options.FirstOrDefault(o => o.Order == 1) ?? new CreateQuestionBankOptionDto();
                var no = item.Options.FirstOrDefault(o => o.Order == 2) ?? new CreateQuestionBankOptionDto();

                yes.Text = "Да";
                yes.Order = 1;

                no.Text = "Нет";
                no.Order = 2;

                item.Options = new List<CreateQuestionBankOptionDto> { yes, no };
                item.RatingMin = null;
                item.RatingMax = null;
                return;
            }

            if (item.QuestionType == "Rating")
            {
                item.Options.Clear();
                item.RatingMin ??= 1;
                item.RatingMax ??= 5;
                return;
            }

            if (item.QuestionType == "Text" || item.QuestionType == "Number")
            {
                item.Options.Clear();
                item.RatingMin = null;
                item.RatingMax = null;
                return;
            }

            item.Options = item.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                .OrderBy(o => o.Order)
                .ToList();
        }
        public async Task<List<ImportQuestionBankPreviewDto>> PreviewImportFromExcelAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheets.First();

            var rows = worksheet.RowsUsed().Skip(1).ToList();
            if (!rows.Any())
                return new List<ImportQuestionBankPreviewDto>();

            var grouped = rows
                .Select(r => new
                {
                    QuestionCode = r.Cell(1).GetString().Trim(),
                    Category = r.Cell(2).GetString().Trim(),
                    Text = r.Cell(3).GetString().Trim(),
                    QuestionType = r.Cell(4).GetString().Trim(),
                    IsRequired = ParseBool(r.Cell(5).GetString()),
                    HasCorrectAnswer = ParseBool(r.Cell(6).GetString()),
                    RatingMin = ParseNullableInt(r.Cell(7).GetString()),
                    RatingMax = ParseNullableInt(r.Cell(8).GetString()),
                    OptionText = r.Cell(9).GetString().Trim(),
                    OptionOrder = ParseNullableInt(r.Cell(10).GetString()) ?? 0,
                    IsCorrect = ParseBool(r.Cell(11).GetString())
                })
                .GroupBy(x => x.QuestionCode);

            var result = new List<ImportQuestionBankPreviewDto>();

            foreach (var group in grouped)
            {
                var first = group.First();

                if (string.IsNullOrWhiteSpace(first.Text) || string.IsNullOrWhiteSpace(first.QuestionType))
                    continue;

                var item = new ImportQuestionBankPreviewDto
                {
                    Category = string.IsNullOrWhiteSpace(first.Category) ? null : first.Category,
                    Text = first.Text,
                    QuestionType = first.QuestionType,
                    IsRequired = first.IsRequired,
                    HasCorrectAnswer = first.HasCorrectAnswer,
                    RatingMin = first.RatingMin,
                    RatingMax = first.RatingMax
                };

                if (RequiresOptions(first.QuestionType))
                {
                    foreach (var row in group
                                 .Where(x => !string.IsNullOrWhiteSpace(x.OptionText))
                                 .OrderBy(x => x.OptionOrder))
                    {
                        item.Options.Add(new CreateQuestionBankOptionDto
                        {
                            Text = row.OptionText,
                            Order = row.OptionOrder,
                            IsCorrect = row.IsCorrect
                        });
                    }
                }

                NormalizePreviewItem(item);
                result.Add(item);
            }

            return result;
        }

        public async Task<List<ImportQuestionBankPreviewDto>> PreviewImportFromXmlAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var document = await XDocument.LoadAsync(
                memoryStream,
                System.Xml.Linq.LoadOptions.None,
                CancellationToken.None);

            var questionElements = document.Root?.Elements("Question").ToList();
            if (questionElements == null || !questionElements.Any())
                return new List<ImportQuestionBankPreviewDto>();

            var result = new List<ImportQuestionBankPreviewDto>();

            foreach (var q in questionElements)
            {
                var item = new ImportQuestionBankPreviewDto
                {
                    Category = q.Element("Category")?.Value,
                    Text = q.Element("Text")?.Value ?? string.Empty,
                    QuestionType = q.Element("QuestionType")?.Value ?? string.Empty,
                    IsRequired = ParseBool(q.Element("IsRequired")?.Value),
                    HasCorrectAnswer = ParseBool(q.Element("HasCorrectAnswer")?.Value),
                    RatingMin = ParseNullableInt(q.Element("RatingMin")?.Value),
                    RatingMax = ParseNullableInt(q.Element("RatingMax")?.Value)
                };

                var options = q.Element("Options")?.Elements("Option").ToList() ?? new();

                foreach (var option in options)
                {
                    item.Options.Add(new CreateQuestionBankOptionDto
                    {
                        Text = option.Element("Text")?.Value ?? string.Empty,
                        Order = ParseNullableInt(option.Element("Order")?.Value) ?? 0,
                        IsCorrect = ParseBool(option.Element("IsCorrect")?.Value)
                    });
                }

                if (string.IsNullOrWhiteSpace(item.Text) || string.IsNullOrWhiteSpace(item.QuestionType))
                    continue;

                NormalizePreviewItem(item);
                result.Add(item);
            }

            return result;
        }

        public async Task<int> SaveImportedQuestionsAsync(List<ImportQuestionBankPreviewDto> items)
        {
            int importedCount = 0;

            foreach (var preview in items)
            {
                var entity = new QuestionBankItem
                {
                    Category = preview.Category,
                    Text = preview.Text,
                    QuestionType = preview.QuestionType,
                    IsRequired = preview.IsRequired,
                    HasCorrectAnswer = preview.HasCorrectAnswer,
                    RatingMin = preview.RatingMin,
                    RatingMax = preview.RatingMax,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var option in preview.Options.OrderBy(o => o.Order))
                {
                    entity.Options.Add(new QuestionBankOption
                    {
                        Text = option.Text,
                        Order = option.Order,
                        IsCorrect = option.IsCorrect
                    });
                }

                _context.QuestionBankItems.Add(entity);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            return importedCount;
        }
        public async Task<byte[]> ExportToExcelAsync()
        {
            var items = await _context.QuestionBankItems
                .Include(q => q.Options)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.Text)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("QuestionBank");

            ws.Cell(1, 1).Value = "QuestionCode";
            ws.Cell(1, 2).Value = "Category";
            ws.Cell(1, 3).Value = "Text";
            ws.Cell(1, 4).Value = "QuestionType";
            ws.Cell(1, 5).Value = "IsRequired";
            ws.Cell(1, 6).Value = "HasCorrectAnswer";
            ws.Cell(1, 7).Value = "RatingMin";
            ws.Cell(1, 8).Value = "RatingMax";
            ws.Cell(1, 9).Value = "OptionText";
            ws.Cell(1, 10).Value = "OptionOrder";
            ws.Cell(1, 11).Value = "IsCorrect";

            int row = 2;

            foreach (var item in items)
            {
                var code = $"Q{item.Id}";

                if (item.Options.Any())
                {
                    foreach (var option in item.Options.OrderBy(o => o.Order))
                    {
                        ws.Cell(row, 1).Value = code;
                        ws.Cell(row, 2).Value = item.Category;
                        ws.Cell(row, 3).Value = item.Text;
                        ws.Cell(row, 4).Value = item.QuestionType;
                        ws.Cell(row, 5).Value = item.IsRequired;
                        ws.Cell(row, 6).Value = item.HasCorrectAnswer;
                        ws.Cell(row, 7).Value = item.RatingMin;
                        ws.Cell(row, 8).Value = item.RatingMax;
                        ws.Cell(row, 9).Value = option.Text;
                        ws.Cell(row, 10).Value = option.Order;
                        ws.Cell(row, 11).Value = option.IsCorrect;
                        row++;
                    }
                }
                else
                {
                    ws.Cell(row, 1).Value = code;
                    ws.Cell(row, 2).Value = item.Category;
                    ws.Cell(row, 3).Value = item.Text;
                    ws.Cell(row, 4).Value = item.QuestionType;
                    ws.Cell(row, 5).Value = item.IsRequired;
                    ws.Cell(row, 6).Value = item.HasCorrectAnswer;
                    ws.Cell(row, 7).Value = item.RatingMin;
                    ws.Cell(row, 8).Value = item.RatingMax;
                    row++;
                }
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportToXmlAsync()
        {
            var items = await _context.QuestionBankItems
                .Include(q => q.Options)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.Text)
                .ToListAsync();

            var document = new XDocument(
                new XElement("Questions",
                    items.Select(item =>
                        new XElement("Question",
                            new XElement("Category", item.Category ?? string.Empty),
                            new XElement("Text", item.Text),
                            new XElement("QuestionType", item.QuestionType),
                            new XElement("IsRequired", item.IsRequired),
                            new XElement("HasCorrectAnswer", item.HasCorrectAnswer),
                            item.RatingMin.HasValue ? new XElement("RatingMin", item.RatingMin.Value) : null,
                            item.RatingMax.HasValue ? new XElement("RatingMax", item.RatingMax.Value) : null,
                            new XElement("Options",
                                item.Options
                                    .OrderBy(o => o.Order)
                                    .Select(option =>
                                        new XElement("Option",
                                            new XElement("Text", option.Text),
                                            new XElement("Order", option.Order),
                                            new XElement("IsCorrect", option.IsCorrect)
                                        )
                                    )
                            )
                        )
                    )
                )
            );

            await using var stream = new MemoryStream();
            await document.SaveAsync(
            stream,
            System.Xml.Linq.SaveOptions.None,
            CancellationToken.None);
            return stream.ToArray();
        }
    }

}