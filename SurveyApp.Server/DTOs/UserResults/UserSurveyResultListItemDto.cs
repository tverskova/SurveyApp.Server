namespace SurveyApp.Server.DTOs.UserResults
{
    public class UserSurveyResultListItemDto
    {
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; } = string.Empty;
        public string? SurveyDescription { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int QuestionsCount { get; set; }
        public int AnswersCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
