namespace SurveyApp.Server.DTOs.UserResults
{
    public class UserResultAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
        public byte[]? QuestionImage { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
