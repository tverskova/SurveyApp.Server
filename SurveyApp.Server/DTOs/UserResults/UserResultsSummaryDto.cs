namespace SurveyApp.Server.DTOs.UserResults
{
    public class UserResultsSummaryDto
    {
        public int CompletedSurveysCount { get; set; }
        public int SentAnswersCount { get; set; }
        public DateTime? LastSubmittedAt { get; set; }
    }
}
