namespace SurveyApp.Server.DTOs.Statistics
{
    public class SystemStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalSurveys { get; set; }
        public int PublishedSurveys { get; set; }
        public int TotalResponses { get; set; }

        public int DraftSurveys { get; set; }
        public int ActiveSurveys { get; set; }
        public List<SystemSurveyStatisticsItemDto> Surveys { get; set; } = new();
    }
}