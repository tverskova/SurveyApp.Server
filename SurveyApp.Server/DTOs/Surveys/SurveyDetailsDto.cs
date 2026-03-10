namespace SurveyApp.Server.DTOs.Surveys
{
    public class SurveyDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAnonymous { get; set; }

        public List<QuestionDto> Questions { get; set; } = new();
    }
}