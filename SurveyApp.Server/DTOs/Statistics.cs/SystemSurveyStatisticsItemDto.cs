namespace SurveyApp.Server.DTOs.Statistics
{
    public class SystemSurveyStatisticsItemDto
    {
        public int SurveyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public int ResponsesCount { get; set; }
    }
}