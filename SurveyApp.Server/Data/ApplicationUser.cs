using Microsoft.AspNetCore.Identity;

namespace SurveyApp.Server.Data
{
    public class ApplicationUser : IdentityUser
    {
        public UserProfile? UserProfile { get; set; }

        public ICollection<Survey> CreatedSurveys { get; set; } = new List<Survey>();
        public ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    }
}