namespace SurveyApp.Server.Data
{
    public class Question
    {
        public int Id { get; set; }

        public int SurveyId { get; set; }
        public Survey Survey { get; set; } = null!;

        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public bool HasCorrectAnswer { get; set; }

        public int? RatingMin { get; set; }
        public int? RatingMax { get; set; }

        public byte[]? Image { get; set; }

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}