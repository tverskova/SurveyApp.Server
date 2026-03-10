namespace SurveyApp.Server.DTOs.QuestionBank
{
    public class QuestionBankItemDto
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool HasCorrectAnswer { get; set; }

        public int? RatingMin { get; set; }
        public int? RatingMax { get; set; }

        public byte[]? Image { get; set; }

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<QuestionBankOptionDto> Options { get; set; } = new();
    }
}