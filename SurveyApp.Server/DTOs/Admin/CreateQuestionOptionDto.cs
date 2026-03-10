namespace SurveyApp.Server.DTOs.Admin
{
    public class CreateQuestionOptionDto
    {

        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
    }
}