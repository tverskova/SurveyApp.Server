using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.DTOs.Surveys;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface ISurveyService
    {
        
        Task<int> CreateSurveyAsync(CreateSurveyDto dto, string? createdByUserId);
        Task<List<SurveyListItemDto>> GetAllSurveysAsync();
        Task<bool> PublishSurveyAsync(int surveyId);
        Task<bool> UnpublishSurveyAsync(int surveyId);
        Task<bool> DeleteSurveyAsync(int surveyId);
        Task<CreateSurveyDto?> GetSurveyForEditAsync(int surveyId);
        Task<bool> UpdateSurveyAsync(int surveyId, CreateSurveyDto dto);

        Task<List<SurveyListItemDto>> GetPublishedSurveysAsync();
        Task<SurveyDetailsDto?> GetSurveyDetailsAsync(int surveyId);
    }
}