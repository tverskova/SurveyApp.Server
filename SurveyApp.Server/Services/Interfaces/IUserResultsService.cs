using SurveyApp.Server.DTOs.UserResults;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IUserResultsService
    {
        Task<UserResultsSummaryDto> GetSummaryAsync(string userId);
        Task<List<UserSurveyResultListItemDto>> GetResultsAsync(string userId);
        Task<UserSurveyResultDetailsDto?> GetResultDetailsAsync(string userId, int responseId);
    }
}
