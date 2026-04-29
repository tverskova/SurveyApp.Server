namespace SurveyApp.Server.DTOs.Statistics;

public class UserSurveyAnswersDto
{
    public int ResponseId { get; set; }

    public int SurveyId { get; set; }

    public string SurveyTitle { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public List<UserAnswerDto> Answers { get; set; } = new();
}