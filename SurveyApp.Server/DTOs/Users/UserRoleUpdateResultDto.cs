namespace SurveyApp.Server.DTOs.Users
{
    public class UserRoleUpdateResultDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
