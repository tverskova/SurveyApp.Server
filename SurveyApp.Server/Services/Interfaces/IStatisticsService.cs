using SurveyApp.Server.DTOs.Statistics;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<SystemStatisticsDto> GetSystemStatisticsAsync();
        Task<SurveyStatisticsDto?> GetSurveyStatisticsAsync(int surveyId);
    }
}