namespace SurveyApp.Server.DTOs.Surveys
{
    public class QuestionOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}