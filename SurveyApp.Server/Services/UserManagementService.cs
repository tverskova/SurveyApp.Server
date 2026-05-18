using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.DTOs.Users;
using SurveyApp.Server.Services.Interfaces;

namespace SurveyApp.Server.Services
{
    public class UserManagementService : IUserManagementService
    {
        private static readonly string[] AllowedRoles = { "Admin", "User" };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserStatisticsSummaryDto> GetSummaryAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalCompletions = await _context.SurveyResponses
                .CountAsync(r => r.SubmittedAt.HasValue);
            var usersWithCompletedSurveys = await _context.SurveyResponses
                .Where(r => r.SubmittedAt.HasValue && r.UserId != null)
                .Select(r => r.UserId!)
                .Distinct()
                .CountAsync();

            return new UserStatisticsSummaryDto
            {
                TotalUsers = totalUsers,
                AdminUsers = await CountUsersInRoleAsync("Admin"),
                RegularUsers = await CountUsersInRoleAsync("User"),
                UsersWithCompletedSurveys = usersWithCompletedSurveys,
                TotalSurveyCompletions = totalCompletions,
                AverageCompletedSurveysPerUser = totalUsers == 0
                    ? 0
                    : (double)totalCompletions / totalUsers
            };
        }

        public async Task<List<UserListItemDto>> GetUsersAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserProfile)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var responseStats = await _context.SurveyResponses
                .AsNoTracking()
                .Where(r => r.UserId != null && userIds.Contains(r.UserId))
                .GroupBy(r => r.UserId!)
                .Select(g => new
                {
                    UserId = g.Key,
                    CompletedSurveysCount = g.Count(r => r.SubmittedAt.HasValue),
                    AnswersCount = g.SelectMany(r => r.Answers).Count(),
                    LastSubmittedAt = g.Max(r => r.SubmittedAt)
                })
                .ToDictionaryAsync(x => x.UserId);

            var result = new List<UserListItemDto>();

            foreach (var user in users)
            {
                responseStats.TryGetValue(user.Id, out var stats);
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserListItemDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? "Не указано",
                    FirstName = user.UserProfile?.FirstName,
                    LastName = user.UserProfile?.LastName,
                    City = user.UserProfile?.City,
                    BirthDate = user.UserProfile?.BirthDate,
                    Roles = roles.OrderBy(r => r).ToList(),
                    CompletedSurveysCount = stats?.CompletedSurveysCount ?? 0,
                    AnswersCount = stats?.AnswersCount ?? 0,
                    LastSubmittedAt = stats?.LastSubmittedAt
                });
            }

            return result;
        }

        public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var responses = await _context.SurveyResponses
                .AsNoTracking()
                .Include(r => r.Survey)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.Question)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerOptions)
                        .ThenInclude(ao => ao.QuestionOption)
                .Where(r => r.UserId == userId && r.SubmittedAt.HasValue)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

            return new UserDetailsDto
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "Не указано",
                FirstName = user.UserProfile?.FirstName,
                LastName = user.UserProfile?.LastName,
                City = user.UserProfile?.City,
                BirthDate = user.UserProfile?.BirthDate,
                Roles = roles.OrderBy(r => r).ToList(),
                SurveyResults = responses.Select(r => new UserSurveyResultDto
                {
                    ResponseId = r.Id,
                    SurveyId = r.SurveyId,
                    SurveyTitle = r.Survey.Title,
                    StartedAt = r.StartedAt,
                    SubmittedAt = r.SubmittedAt,
                    AnswersCount = r.Answers.Count,
                    Answers = r.Answers
                        .OrderBy(a => a.Question.Order)
                        .Select(a => new UserAnswerDto
                        {
                            QuestionId = a.QuestionId,
                            QuestionText = a.Question.Text,
                            QuestionType = a.Question.QuestionType,
                            QuestionOrder = a.Question.Order,
                            AnswerText = FormatAnswer(a)
                        })
                        .ToList()
                }).ToList()
            };
        }

        public async Task<UserRoleUpdateResultDto> SetUserRoleAsync(
            string userId,
            string roleName,
            bool assign,
            string? currentAdminUserId)
        {
            if (!AllowedRoles.Contains(roleName))
                return Error("Недопустимая роль.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Error("Пользователь не найден.");

            if (!await _roleManager.RoleExistsAsync(roleName))
                return Error("Роль не найдена.");

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);

            if (assign)
            {
                if (isInRole)
                    return Success("Роль уже назначена.");

                var addResult = await _userManager.AddToRoleAsync(user, roleName);
                return addResult.Succeeded
                    ? Success("Роль успешно назначена.")
                    : Error(BuildIdentityError(addResult));
            }

            if (!isInRole)
                return Success("У пользователя уже нет этой роли.");

            if (roleName == "Admin")
            {
                if (!string.IsNullOrWhiteSpace(currentAdminUserId) && currentAdminUserId == userId)
                    return Error("Вы не можете снять роль Admin у своей учётной записи. Обратитесь к другому администратору.");

                var adminCount = await CountUsersInRoleAsync("Admin");
                if (adminCount <= 1)
                    return Error("Нельзя снять роль Admin у последнего администратора.");
            }

            var removeResult = await _userManager.RemoveFromRoleAsync(user, roleName);
            return removeResult.Succeeded
                ? Success("Роль успешно снята.")
                : Error(BuildIdentityError(removeResult));
        }

        private async Task<int> CountUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.Count;
        }

        private static UserRoleUpdateResultDto Success(string message)
        {
            return new UserRoleUpdateResultDto { Succeeded = true, Message = message };
        }

        private static UserRoleUpdateResultDto Error(string message)
        {
            return new UserRoleUpdateResultDto { Succeeded = false, Message = message };
        }

        private static string BuildIdentityError(IdentityResult result)
        {
            var errors = result.Errors.Select(e => e.Description).Where(e => !string.IsNullOrWhiteSpace(e));
            return errors.Any()
                ? string.Join(" ", errors)
                : "Не удалось изменить роль пользователя.";
        }

        private static string FormatAnswer(Answer answer)
        {
            return answer.Question.QuestionType switch
            {
                "Text" => string.IsNullOrWhiteSpace(answer.TextAnswer) ? "-" : answer.TextAnswer,
                "Number" => answer.NumberAnswer.HasValue
                    ? answer.NumberAnswer.Value.ToString("G", CultureInfo.CurrentCulture)
                    : "-",
                "Rating" => answer.RatingAnswer.HasValue ? answer.RatingAnswer.Value.ToString() : "-",
                "YesNo" => answer.YesNoAnswer.HasValue
                    ? answer.YesNoAnswer.Value ? "Да" : "Нет"
                    : "-",
                "SingleChoice" or "MultipleChoice" => answer.AnswerOptions.Any()
                    ? string.Join(", ", answer.AnswerOptions
                        .OrderBy(ao => ao.QuestionOption.Order)
                        .Select(ao => ao.QuestionOption.Text))
                    : "-",
                _ => "-"
            };
        }
    }
}

