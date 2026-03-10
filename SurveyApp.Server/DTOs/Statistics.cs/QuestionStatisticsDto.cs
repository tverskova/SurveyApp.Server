namespace SurveyApp.Server.DTOs.Statistics
{
    public class QuestionStatisticsDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public List<string> Labels { get; set; } = new();
        public List<int> Values { get; set; } = new();

        public List<string> TextAnswers { get; set; } = new();

        public double? AverageValue { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
    }
}