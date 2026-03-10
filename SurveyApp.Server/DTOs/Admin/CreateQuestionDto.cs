namespace SurveyApp.Server.DTOs.Admin
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public bool HasCorrectAnswer { get; set; }

        public int? RatingMin { get; set; }
        public int? RatingMax { get; set; }

        public byte[]? Image { get; set; }

        public List<CreateQuestionOptionDto> Options { get; set; } = new();
    }
}