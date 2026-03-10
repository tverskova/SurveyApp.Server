namespace SurveyApp.Server.Data
{
    public class QuestionBankOption
    {
        public int Id { get; set; }

        public int QuestionBankItemId { get; set; }
        public QuestionBankItem QuestionBankItem { get; set; } = null!;

        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

        public bool IsCorrect { get; set; }
    }
}