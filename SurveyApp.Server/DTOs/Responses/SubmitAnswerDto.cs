namespace SurveyApp.Server.DTOs.Responses
{
    public class SubmitAnswerDto
    {
        public int QuestionId { get; set; }

        public List<int>? SelectedOptionIds { get; set; }

        public string? TextAnswer { get; set; }
        public double? NumberAnswer { get; set; }
        public int? RatingAnswer { get; set; }
        public bool? YesNoAnswer { get; set; }
    }
}