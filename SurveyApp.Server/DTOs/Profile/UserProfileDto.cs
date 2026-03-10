namespace SurveyApp.Server.DTOs.Profile
{
    public class UserProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
    }
}