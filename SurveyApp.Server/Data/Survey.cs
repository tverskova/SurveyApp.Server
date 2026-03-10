namespace SurveyApp.Server.Data
{
    public class Survey
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPublished { get; set; }
        public bool IsAnonymous { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
    }
}