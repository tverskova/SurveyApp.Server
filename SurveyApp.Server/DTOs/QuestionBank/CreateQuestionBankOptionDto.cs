namespace SurveyApp.Server.DTOs.QuestionBank
{
    public class CreateQuestionBankOptionDto
    {
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCorrect { get; set; }
    }
}