namespace SurveyApp.Server.DTOs.QuestionBank
{
    public class ImportQuestionBankPreviewDto
    {
        public Guid TempId { get; set; } = Guid.NewGuid();

        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool HasCorrectAnswer { get; set; }

        public int? RatingMin { get; set; }
        public int? RatingMax { get; set; }

        public string? Category { get; set; }

        public List<CreateQuestionBankOptionDto> Options { get; set; } = new();
    }
}