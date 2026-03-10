namespace SurveyApp.Server.DTOs.Admin
{
    public class CreateSurveyDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public bool IsAnonymous { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<CreateQuestionDto> Questions { get; set; } = new();
    }
}