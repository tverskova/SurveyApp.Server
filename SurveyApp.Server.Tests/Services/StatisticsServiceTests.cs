using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class StatisticsServiceTests
{
    [Fact]
    public async Task GetSystemStatisticsAsync_WithSeededData_ShouldCountSurveysAndResponses()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyWithChoiceResponseAsync(context);
        context.Surveys.Add(TestDataFactory.CreateSurvey("Черновик", isPublished: false));
        await context.SaveChangesAsync();
        var service = new StatisticsService(context);

        // Act
        var result = await service.GetSystemStatisticsAsync();

        // Assert
        result.TotalSurveys.Should().Be(2);
        result.PublishedSurveys.Should().Be(1);
        result.DraftSurveys.Should().Be(1);
        result.TotalResponses.Should().Be(1);
        result.Surveys.Should().Contain(s => s.SurveyId == survey.Id && s.ResponsesCount == 1);
    }

    [Fact]
    public async Task GetSurveyStatisticsAsync_WithChoiceAnswers_ShouldBuildOptionCounts()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyWithChoiceResponseAsync(context);
        var service = new StatisticsService(context);

        // Act
        var result = await service.GetSurveyStatisticsAsync(survey.Id);

        // Assert
        result.Should().NotBeNull();
        var question = result!.Questions.Single();
        question.Labels.Should().Equal("A", "B");
        question.Values.Should().Equal(1, 0);
    }

    [Fact]
    public async Task GetSurveyStatisticsAsync_WithRatingAnswers_ShouldCalculateAverageMinMax()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = TestDataFactory.CreateSurvey();
        var question = new Question { Text = "Rate", QuestionType = "Rating", Order = 1, RatingMin = 1, RatingMax = 5 };
        survey.Questions.Add(question);
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();

        var response1 = TestDataFactory.CreateSurveyResponse(survey.Id, "user-1");
        response1.Answers.Add(new Answer { QuestionId = question.Id, RatingAnswer = 3 });
        var response2 = TestDataFactory.CreateSurveyResponse(survey.Id, "user-2");
        response2.Answers.Add(new Answer { QuestionId = question.Id, RatingAnswer = 5 });
        context.SurveyResponses.AddRange(response1, response2);
        await context.SaveChangesAsync();
        var service = new StatisticsService(context);

        // Act
        var result = await service.GetSurveyStatisticsAsync(survey.Id);

        // Assert
        var stat = result!.Questions.Single();
        stat.AverageValue.Should().Be(4);
        stat.MinValue.Should().Be(3);
        stat.MaxValue.Should().Be(5);
    }

    [Fact]
    public async Task GetSurveyStatisticsAsync_WithNoResponses_ShouldReturnZeroTotals()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = TestDataFactory.CreateSurvey();
        survey.Questions.Add(new Question { Text = "Text", QuestionType = "Text", Order = 1 });
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var service = new StatisticsService(context);

        // Act
        var result = await service.GetSurveyStatisticsAsync(survey.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TotalResponses.Should().Be(0);
        result.Questions.Single().TextAnswers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSurveyParticipantsAsync_WithNonAnonymousResponse_ShouldReturnParticipant()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser("user-1", "user@test.com");
        user.UserProfile = TestDataFactory.CreateProfile(user.Id, "Иван", "Петров", "Москва");
        var survey = TestDataFactory.CreateSurvey();
        context.Users.Add(user);
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        context.SurveyResponses.Add(TestDataFactory.CreateSurveyResponse(survey.Id, user.Id));
        await context.SaveChangesAsync();
        var service = new StatisticsService(context);

        // Act
        var result = await service.GetSurveyParticipantsAsync(survey.Id);

        // Assert
        result.Should().ContainSingle();
        result.Single().FullName.Should().Be("Петров Иван");
        result.Single().Email.Should().Be("user@test.com");
    }

    private static async Task<Survey> SeedSurveyWithChoiceResponseAsync(ApplicationDbContext context)
    {
        var survey = TestDataFactory.CreateSurvey();
        var question = new Question
        {
            Text = "Choice",
            QuestionType = "SingleChoice",
            Order = 1,
            Options =
            {
                new QuestionOption { Text = "A", Order = 1 },
                new QuestionOption { Text = "B", Order = 2 }
            }
        };
        survey.Questions.Add(question);
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();

        var selectedOptionId = question.Options.First().Id;
        var response = TestDataFactory.CreateSurveyResponse(survey.Id);
        response.Answers.Add(new Answer
        {
            QuestionId = question.Id,
            AnswerOptions = { new AnswerOption { QuestionOptionId = selectedOptionId } }
        });
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        return survey;
    }
}
