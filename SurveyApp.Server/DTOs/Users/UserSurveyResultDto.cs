namespace SurveyApp.Server.DTOs.Users
{
    public class UserSurveyResultDto
    {
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int AnswersCount { get; set; }
        public List<UserAnswerDto> Answers { get; set; } = new();
    }
}
