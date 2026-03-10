using SurveyApp.Server.DTOs.Profile;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto);
    }
}