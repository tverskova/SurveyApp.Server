namespace SurveyApp.Server.DTOs.Users
{
    public class UserDetailsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? City { get; set; }
        public DateTime? BirthDate { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<UserSurveyResultDto> SurveyResults { get; set; } = new();
    }
}
