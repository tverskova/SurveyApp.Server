using FluentAssertions;
using SurveyApp.Server.DTOs.Admin;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Validation;

public class SurveyValidationTests
{
    [Fact]
    public async Task CreateSurveyAsync_WithEmptyTitle_InServiceLayer_ShouldSaveBecauseValidationIsInRazorPage()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);

        // Act
        var id = await service.CreateSurveyAsync(new CreateSurveyDto
        {
            Title = string.Empty,
            Questions =
            {
                new CreateQuestionDto { Text = "Вопрос", QuestionType = "Text", Order = 1 }
            }
        }, "admin-1");

        // Assert
        id.Should().BeGreaterThan(0);
        context.Surveys.Single().Title.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithMissingSurvey_ShouldReturnFalse()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new SurveyService(context);

        // Act
        var result = await service.UpdateSurveyAsync(404, new CreateSurveyDto { Title = "Не найден" });

        // Assert
        result.Should().BeFalse();
    }
}
