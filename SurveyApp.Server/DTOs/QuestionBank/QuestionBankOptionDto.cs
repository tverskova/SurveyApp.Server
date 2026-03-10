namespace SurveyApp.Server.DTOs.QuestionBank
{
    public class QuestionBankOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
    }
}