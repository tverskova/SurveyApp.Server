namespace SurveyApp.Server.DTOs.Statistics
{
    public class SurveyStatisticsDto
    {
        public int SurveyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public double AverageCompletionTimeSeconds { get; set; }
        public DateTime? FirstResponseDate { get; set; }
        public DateTime? LastResponseDate { get; set; }
        public List<QuestionStatisticsDto> Questions { get; set; } = new();
    }
}