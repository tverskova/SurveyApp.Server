namespace SurveyApp.Server.Data
{
    public class AnswerOption
    {
        public int Id { get; set; }

        public int AnswerId { get; set; }
        public Answer Answer { get; set; } = null!;

        public int QuestionOptionId { get; set; }
        public QuestionOption QuestionOption { get; set; } = null!;
    }
}