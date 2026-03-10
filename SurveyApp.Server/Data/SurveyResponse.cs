namespace SurveyApp.Server.Data
{
    public class SurveyResponse
    {
        public int Id { get; set; }

        public int SurveyId { get; set; }
        public Survey Survey { get; set; } = null!;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}