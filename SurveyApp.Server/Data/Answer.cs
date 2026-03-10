namespace SurveyApp.Server.Data
{
    public class Answer
    {
        public int Id { get; set; }

        public int SurveyResponseId { get; set; }
        public SurveyResponse SurveyResponse { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public string? TextAnswer { get; set; }
        public double? NumberAnswer { get; set; }
        public int? RatingAnswer { get; set; }
        public bool? YesNoAnswer { get; set; }

        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    }
}