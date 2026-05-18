using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class SurveyServiceTests
{
    [Fact]
    public async Task CreateSurveyAsync_WithValidData_ShouldSaveSurvey()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);
        var dto = CreateSurveyDto("Новый опрос");

        // Act
        var surveyId = await service.CreateSurveyAsync(dto, "admin-1");

        // Assert
        var survey = await context.Surveys.Include(s => s.Questions).ThenInclude(q => q.Options).SingleAsync();
        survey.Id.Should().Be(surveyId);
        survey.Title.Should().Be("Новый опрос");
        survey.IsPublished.Should().BeFalse();
        survey.Questions.Should().ContainSingle();
        survey.Questions.Single().Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllSurveysAsync_WithExistingSurveys_ShouldReturnSurveys()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        context.Surveys.Add(TestDataFactory.CreateSurvey("Опрос 1"));
        context.Surveys.Add(TestDataFactory.CreateSurvey("Опрос 2", isPublished: false));
        await context.SaveChangesAsync();
        var service = new SurveyService(context);

        // Act
        var result = await service.GetAllSurveysAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(s => s.Title).Should().Contain(["Опрос 1", "Опрос 2"]);
    }

    [Fact]
    public async Task GetSurveyDetailsAsync_WithExistingSurvey_ShouldReturnQuestionsAndOptions()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = TestDataFactory.CreateSurvey();
        survey.Questions.Add(new()
        {
            Text = "Любимый цвет?",
            QuestionType = "SingleChoice",
            Order = 1,
            Options =
            {
                new() { Text = "Красный", Order = 1 },
                new() { Text = "Синий", Order = 2 }
            }
        });
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var service = new SurveyService(context);

        // Act
        var result = await service.GetSurveyDetailsAsync(survey.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Questions.Should().ContainSingle();
        result.Questions.Single().Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSurveyDetailsAsync_WithMissingSurvey_ShouldReturnNull()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);

        // Act
        var result = await service.GetSurveyDetailsAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithoutResponses_ShouldReplaceSurveyStructure()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);
        var surveyId = await service.CreateSurveyAsync(CreateSurveyDto("До изменения"), "admin-1");
        var updateDto = CreateSurveyDto("После изменения");
        updateDto.Questions[0].Text = "Новый вопрос";

        // Act
        var updated = await service.UpdateSurveyAsync(surveyId, updateDto);

        // Assert
        updated.Should().BeTrue();
        var survey = await context.Surveys.Include(s => s.Questions).SingleAsync(s => s.Id == surveyId);
        survey.Title.Should().Be("После изменения");
        survey.Questions.Single().Text.Should().Be("Новый вопрос");
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithExistingResponses_ShouldThrowException()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);
        var surveyId = await service.CreateSurveyAsync(CreateSurveyDto("Опрос"), "admin-1");
        context.SurveyResponses.Add(TestDataFactory.CreateSurveyResponse(surveyId));
        await context.SaveChangesAsync();

        // Act
        var act = () => service.UpdateSurveyAsync(surveyId, CreateSurveyDto("Изменение"));

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Нельзя изменять структуру опроса*");
    }

    [Fact]
    public async Task DeleteSurveyAsync_WithExistingSurvey_ShouldRemoveSurvey()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var survey = TestDataFactory.CreateSurvey();
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var service = new SurveyService(context);

        // Act
        var deleted = await service.DeleteSurveyAsync(survey.Id);

        // Assert
        deleted.Should().BeTrue();
        var count = await context.Surveys.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetPublishedSurveysAsync_WithDraftAndPublished_ShouldReturnOnlyActivePublished()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        context.Surveys.Add(TestDataFactory.CreateSurvey("Опубликован", isPublished: true));
        context.Surveys.Add(TestDataFactory.CreateSurvey("Черновик", isPublished: false));
        context.Surveys.Add(new()
        {
            Title = "Просрочен",
            IsPublished = true,
            StartDate = DateTime.Today.AddDays(-10),
            EndDate = DateTime.Today.AddDays(-1)
        });
        await context.SaveChangesAsync();
        var service = new SurveyService(context);

        // Act
        var result = await service.GetPublishedSurveysAsync();

        // Assert
        result.Should().ContainSingle();
        result.Single().Title.Should().Be("Опубликован");
    }

    private static CreateSurveyDto CreateSurveyDto(string title)
    {
        return new CreateSurveyDto
        {
            Title = title,
            Description = "Описание",
            Questions =
            {
                new CreateQuestionDto
                {
                    Text = "Вопрос",
                    QuestionType = "SingleChoice",
                    IsRequired = true,
                    Order = 1,
                    Options =
                    {
                        new CreateQuestionOptionDto { Text = "Да", Order = 1 },
                        new CreateQuestionOptionDto { Text = "Нет", Order = 2 }
                    }
                }
            }
        };
    }
}
