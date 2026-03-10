namespace SurveyApp.Server.DTOs.Surveys
{
    public class SurveyListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public bool IsPublished { get; set; }
        public bool IsAnonymous { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}