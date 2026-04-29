namespace SurveyApp.Server.DTOs.Statistics;

public class UserAnswerDto
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string QuestionType { get; set; } = string.Empty;

    public int QuestionOrder { get; set; }

    public string AnswerText { get; set; } = string.Empty;
}