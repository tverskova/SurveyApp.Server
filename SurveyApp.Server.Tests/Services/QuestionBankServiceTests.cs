using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.DTOs.QuestionBank;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class QuestionBankServiceTests
{
    [Fact]
    public async Task CreateAsync_WithTextQuestion_ShouldSaveQuestionBankItem()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new QuestionBankService(context);

        // Act
        var id = await service.CreateAsync(new CreateQuestionBankItemDto
        {
            Text = "Текстовый вопрос",
            QuestionType = "Text",
            Category = "Общие"
        });

        // Assert
        var item = await context.QuestionBankItems.SingleAsync();
        item.Id.Should().Be(id);
        item.Text.Should().Be("Текстовый вопрос");
        item.Options.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithItems_ShouldReturnItemsOrderedByCreatedAtDescending()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        context.QuestionBankItems.Add(new() { Text = "Старый", QuestionType = "Text", CreatedAt = new DateTime(2026, 1, 1) });
        context.QuestionBankItems.Add(new() { Text = "Новый", QuestionType = "Text", CreatedAt = new DateTime(2026, 1, 2) });
        await context.SaveChangesAsync();
        var service = new QuestionBankService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Text.Should().Be("Новый");
    }

    [Fact]
    public async Task ConvertToSurveyQuestionAsync_WithBankItem_ShouldReturnCreateQuestionDto()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new QuestionBankService(context);
        var id = await service.CreateAsync(new CreateQuestionBankItemDto
        {
            Text = "Выберите вариант",
            QuestionType = "SingleChoice",
            IsRequired = true,
            Options =
            {
                new CreateQuestionBankOptionDto { Text = "A", Order = 1 },
                new CreateQuestionBankOptionDto { Text = "B", Order = 2 }
            }
        });

        // Act
        var result = await service.ConvertToSurveyQuestionAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Text.Should().Be("Выберите вариант");
        result.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingItem_ShouldRemoveItem()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new QuestionBankService(context);
        var id = await service.CreateAsync(new CreateQuestionBankItemDto { Text = "Удалить", QuestionType = "Text" });

        // Act
        var deleted = await service.DeleteAsync(id);

        // Assert
        deleted.Should().BeTrue();
        (await context.QuestionBankItems.CountAsync()).Should().Be(0);
    }
}
