namespace SurveyApp.Server.DTOs.Users
{
    public class UserStatisticsSummaryDto
    {
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int RegularUsers { get; set; }
        public int UsersWithCompletedSurveys { get; set; }
        public int TotalSurveyCompletions { get; set; }
        public double AverageCompletedSurveysPerUser { get; set; }
    }
}
