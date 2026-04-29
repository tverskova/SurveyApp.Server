namespace SurveyApp.Server.DTOs.Statistics;

public class SurveyParticipantDto
{
    public int ResponseId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }
}