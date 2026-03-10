using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Profile;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;

        public ProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.UserProfile?.FirstName,
                LastName = user.UserProfile?.LastName,
                BirthDate = user.UserProfile?.BirthDate,
                Gender = user.UserProfile?.Gender,
                City = user.UserProfile?.City
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateUserProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile
                {
                    UserId = userId
                };
            }

            user.UserProfile.FirstName = dto.FirstName;
            user.UserProfile.LastName = dto.LastName;
            user.UserProfile.BirthDate = dto.BirthDate;
            user.UserProfile.Gender = dto.Gender;
            user.UserProfile.City = dto.City;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}