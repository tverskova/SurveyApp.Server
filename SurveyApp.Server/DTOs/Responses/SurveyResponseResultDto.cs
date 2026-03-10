namespace SurveyApp.Server.DTOs.Responses
{
    public class SurveyResponseResultDto
    {
        public int ResponseId { get; set; }
        public int SurveyId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}