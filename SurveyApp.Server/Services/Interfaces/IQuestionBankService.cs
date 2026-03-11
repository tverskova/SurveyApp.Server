using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.DTOs.QuestionBank;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IQuestionBankService
    {
        Task<List<QuestionBankItemDto>> GetAllAsync();
        Task<QuestionBankItemDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateQuestionBankItemDto dto);
        Task<bool> UpdateAsync(int id, CreateQuestionBankItemDto dto);
        Task<bool> DeleteAsync(int id);

        Task<CreateQuestionDto?> ConvertToSurveyQuestionAsync(int id);

        Task<List<ImportQuestionBankPreviewDto>> PreviewImportFromExcelAsync(Stream stream);
        Task<List<ImportQuestionBankPreviewDto>> PreviewImportFromXmlAsync(Stream stream);
        Task<int> SaveImportedQuestionsAsync(List<ImportQuestionBankPreviewDto> items);
        Task<byte[]> ExportToExcelAsync();
        Task<byte[]> ExportToXmlAsync();
    }
}