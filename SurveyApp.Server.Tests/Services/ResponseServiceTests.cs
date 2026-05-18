using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.DTOs.Responses;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class ResponseServiceTests
{
    [Fact]
    public async Task SubmitResponseAsync_WithMissingSurvey_ShouldReturnNull()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new ResponseService(context);

        // Act
        var result = await service.SubmitResponseAsync(new SubmitSurveyResponseDto { SurveyId = 404 }, "user-1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SubmitResponseAsync_WithTextNumberRatingAndYesNo_ShouldSaveAnswers()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = CreateSurveyWithQuestionTypes();
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var service = new ResponseService(context);

        var dto = new SubmitSurveyResponseDto
        {
            SurveyId = survey.Id,
            Answers =
            {
                new() { QuestionId = survey.Questions.ElementAt(0).Id, TextAnswer = "Текст" },
                new() { QuestionId = survey.Questions.ElementAt(1).Id, NumberAnswer = 42 },
                new() { QuestionId = survey.Questions.ElementAt(2).Id, RatingAnswer = 5 },
                new() { QuestionId = survey.Questions.ElementAt(3).Id, YesNoAnswer = true }
            }
        };

        // Act
        var result = await service.SubmitResponseAsync(dto, "user-1");

        // Assert
        result.Should().NotBeNull();
        var response = await context.SurveyResponses.Include(r => r.Answers).SingleAsync();
        response.UserId.Should().Be("user-1");
        response.Answers.Should().HaveCount(4);
        response.Answers.Should().Contain(a => a.TextAnswer == "Текст");
        response.Answers.Should().Contain(a => a.NumberAnswer == 42);
        response.Answers.Should().Contain(a => a.RatingAnswer == 5);
        response.Answers.Should().Contain(a => a.YesNoAnswer == true);
    }

    [Fact]
    public async Task SubmitResponseAsync_WithSingleChoice_ShouldSaveSelectedOption()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = CreateChoiceSurvey("SingleChoice");
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var optionId = survey.Questions.Single().Options.First().Id;
        var service = new ResponseService(context);

        // Act
        await service.SubmitResponseAsync(new SubmitSurveyResponseDto
        {
            SurveyId = survey.Id,
            Answers = { new() { QuestionId = survey.Questions.Single().Id, SelectedOptionIds = [optionId] } }
        }, "user-1");

        // Assert
        var answer = await context.Answers.Include(a => a.AnswerOptions).SingleAsync();
        answer.AnswerOptions.Should().ContainSingle();
        answer.AnswerOptions.Single().QuestionOptionId.Should().Be(optionId);
    }

    [Fact]
    public async Task SubmitResponseAsync_WithMultipleChoice_ShouldSaveAllSelectedOptions()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = CreateChoiceSurvey("MultipleChoice");
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var optionIds = survey.Questions.Single().Options.Select(o => o.Id).ToList();
        var service = new ResponseService(context);

        // Act
        await service.SubmitResponseAsync(new SubmitSurveyResponseDto
        {
            SurveyId = survey.Id,
            Answers = { new() { QuestionId = survey.Questions.Single().Id, SelectedOptionIds = optionIds } }
        }, "user-1");

        // Assert
        var answer = await context.Answers.Include(a => a.AnswerOptions).SingleAsync();
        answer.AnswerOptions.Should().HaveCount(2);
        answer.AnswerOptions.Select(ao => ao.QuestionOptionId).Should().BeEquivalentTo(optionIds);
    }

    [Fact]
    public async Task SubmitResponseAsync_WithAnonymousSurvey_ShouldNotSaveUserId()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = CreateChoiceSurvey("SingleChoice");
        survey.IsAnonymous = true;
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var service = new ResponseService(context);

        // Act
        await service.SubmitResponseAsync(new SubmitSurveyResponseDto
        {
            SurveyId = survey.Id,
            Answers = { new() { QuestionId = survey.Questions.Single().Id } }
        }, "user-1");

        // Assert
        var response = await context.SurveyResponses.SingleAsync();
        response.UserId.Should().BeNull();
    }

    private static SurveyApp.Server.Data.Survey CreateSurveyWithQuestionTypes()
    {
        var survey = TestDataFactory.CreateSurvey();
        survey.Questions.Add(new() { Text = "Text", QuestionType = "Text", Order = 1 });
        survey.Questions.Add(new() { Text = "Number", QuestionType = "Number", Order = 2 });
        survey.Questions.Add(new() { Text = "Rating", QuestionType = "Rating", Order = 3, RatingMin = 1, RatingMax = 5 });
        survey.Questions.Add(new() { Text = "YesNo", QuestionType = "YesNo", Order = 4 });
        return survey;
    }

    private static SurveyApp.Server.Data.Survey CreateChoiceSurvey(string questionType)
    {
        var survey = TestDataFactory.CreateSurvey();
        survey.Questions.Add(new()
        {
            Text = "Choice",
            QuestionType = questionType,
            Order = 1,
            Options =
            {
                new() { Text = "A", Order = 1 },
                new() { Text = "B", Order = 2 }
            }
        });
        return survey;
    }
}
