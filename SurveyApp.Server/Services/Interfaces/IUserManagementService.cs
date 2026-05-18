using SurveyApp.Server.DTOs.Users;

namespace SurveyApp.Server.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<UserStatisticsSummaryDto> GetSummaryAsync();
        Task<List<UserListItemDto>> GetUsersAsync();
        Task<UserDetailsDto?> GetUserDetailsAsync(string userId);
        Task<UserRoleUpdateResultDto> SetUserRoleAsync(
            string userId,
            string roleName,
            bool assign,
            string? currentAdminUserId);
    }
}
