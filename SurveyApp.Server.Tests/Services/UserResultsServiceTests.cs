using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.Data;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class UserResultsServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_WithNoResults_ShouldReturnZeros()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new UserResultsService(context);

        // Act
        var result = await service.GetSummaryAsync("user-1");

        // Assert
        result.CompletedSurveysCount.Should().Be(0);
        result.SentAnswersCount.Should().Be(0);
        result.LastSubmittedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetResultsAsync_WithCurrentUserResponses_ShouldReturnOnlyOwnResults()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyAsync(context);
        context.SurveyResponses.Add(TestDataFactory.CreateSurveyResponse(survey.Id, "user-1"));
        context.SurveyResponses.Add(TestDataFactory.CreateSurveyResponse(survey.Id, "user-2"));
        await context.SaveChangesAsync();
        var service = new UserResultsService(context);

        // Act
        var result = await service.GetResultsAsync("user-1");

        // Assert
        result.Should().ContainSingle();
        result.Single().SurveyTitle.Should().Be(survey.Title);
    }

    [Fact]
    public async Task GetResultDetailsAsync_WithOwnResponse_ShouldReturnQuestionsAndAnswers()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyAsync(context);
        var response = TestDataFactory.CreateSurveyResponse(survey.Id, "user-1");
        response.Answers.Add(new Answer { QuestionId = survey.Questions.Single().Id, TextAnswer = "Мой ответ" });
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        var service = new UserResultsService(context);

        // Act
        var result = await service.GetResultDetailsAsync("user-1", response.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Answers.Should().ContainSingle();
        result.Answers.Single().AnswerText.Should().Be("Мой ответ");
    }

    [Fact]
    public async Task GetResultDetailsAsync_WithOtherUserResponse_ShouldReturnNull()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyAsync(context);
        var response = TestDataFactory.CreateSurveyResponse(survey.Id, "user-2");
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        var service = new UserResultsService(context);

        // Act
        var result = await service.GetResultDetailsAsync("user-1", response.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetResultDetailsAsync_WithMissingAnswer_ShouldReturnAnswerNotSpecified()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = await SeedSurveyAsync(context);
        var response = TestDataFactory.CreateSurveyResponse(survey.Id, "user-1");
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        var service = new UserResultsService(context);

        // Act
        var result = await service.GetResultDetailsAsync("user-1", response.Id);

        // Assert
        result!.Answers.Single().AnswerText.Should().Be("Ответ не указан");
    }

    private static async Task<Survey> SeedSurveyAsync(ApplicationDbContext context)
    {
        var survey = TestDataFactory.CreateSurvey("Опрос пользователя");
        survey.Questions.Add(new Question { Text = "Text", QuestionType = "Text", Order = 1 });
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        return survey;
    }
}
