namespace SurveyApp.Server.DTOs.Responses
{
    public class SubmitSurveyResponseDto
    {
        public int SurveyId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; } = new();
    }
}