using SurveyApp.Server.DTOs.Responses;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IResponseService
    {
        Task<SurveyResponseResultDto?> SubmitResponseAsync(SubmitSurveyResponseDto dto, string? userId);
    }
}