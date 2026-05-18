using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SurveyApp.Server.Data;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class UserManagementServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_WithUsersAndRoles_ShouldCountUsersByRole()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        await CreateUserAsync(userManager, "admin-1", "admin@test.com", "Admin");
        await CreateUserAsync(userManager, "user-1", "user@test.com", "User");
        var service = CreateService(provider);

        // Act
        var result = await service.GetSummaryAsync();

        // Assert
        result.TotalUsers.Should().Be(2);
        result.AdminUsers.Should().Be(1);
        result.RegularUsers.Should().Be(1);
    }

    [Fact]
    public async Task GetUsersAsync_WithProfilesAndResponses_ShouldReturnUserActivity()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateUserAsync(userManager, "user-1", "user@test.com", "User");
        context.UserProfiles.Add(TestDataFactory.CreateProfile(user.Id, "Анна", "Сидорова", "Тула"));
        var survey = TestDataFactory.CreateSurvey();
        survey.Questions.Add(new Question { Text = "Text", QuestionType = "Text", Order = 1 });
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var response = TestDataFactory.CreateSurveyResponse(survey.Id, user.Id);
        response.Answers.Add(new Answer { QuestionId = survey.Questions.Single().Id, TextAnswer = "Ответ" });
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        var service = CreateService(provider);

        // Act
        var result = await service.GetUsersAsync();

        // Assert
        var item = result.Single();
        item.Email.Should().Be("user@test.com");
        item.FirstName.Should().Be("Анна");
        item.Roles.Should().Contain("User");
        item.CompletedSurveysCount.Should().Be(1);
        item.AnswersCount.Should().Be(1);
        item.LastSubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SetUserRoleAsync_AssignRole_ShouldAddRole()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateUserAsync(userManager, "user-1", "user@test.com");
        var service = CreateService(provider);

        // Act
        var result = await service.SetUserRoleAsync(user.Id, "User", true, "admin-1");

        // Assert
        result.Succeeded.Should().BeTrue();
        (await userManager.IsInRoleAsync(user, "User")).Should().BeTrue();
    }

    [Fact]
    public async Task SetUserRoleAsync_RemoveRole_ShouldRemoveRole()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateUserAsync(userManager, "user-1", "user@test.com", "User");
        var service = CreateService(provider);

        // Act
        var result = await service.SetUserRoleAsync(user.Id, "User", false, "admin-1");

        // Assert
        result.Succeeded.Should().BeTrue();
        (await userManager.IsInRoleAsync(user, "User")).Should().BeFalse();
    }

    [Fact]
    public async Task SetUserRoleAsync_RemoveLastAdmin_ShouldReturnError()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await CreateUserAsync(userManager, "admin-1", "admin@test.com", "Admin");
        var service = CreateService(provider);

        // Act
        var result = await service.SetUserRoleAsync(admin.Id, "Admin", false, "other-admin");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("последнего администратора");
        (await userManager.IsInRoleAsync(admin, "Admin")).Should().BeTrue();
    }

    [Fact]
    public async Task SetUserRoleAsync_RemoveOwnAdminRole_ShouldReturnError()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await CreateUserAsync(userManager, "admin-1", "admin@test.com", "Admin");
        await CreateUserAsync(userManager, "admin-2", "admin2@test.com", "Admin");
        var service = CreateService(provider);

        // Act
        var result = await service.SetUserRoleAsync(admin.Id, "Admin", false, admin.Id);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Contain("своей учётной записи");
    }

    [Fact]
    public async Task GetUserDetailsAsync_WithResponses_ShouldReturnSurveyAnswers()
    {
        // Arrange
        await using var provider = TestDbContextFactory.CreateServiceProvider();
        await SeedRolesAsync(provider);
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await CreateUserAsync(userManager, "user-1", "user@test.com", "User");
        var survey = TestDataFactory.CreateSurvey("История");
        survey.Questions.Add(new Question { Text = "Text", QuestionType = "Text", Order = 1 });
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();
        var response = TestDataFactory.CreateSurveyResponse(survey.Id, user.Id);
        response.Answers.Add(new Answer { QuestionId = survey.Questions.Single().Id, TextAnswer = "Ответ" });
        context.SurveyResponses.Add(response);
        await context.SaveChangesAsync();
        var service = CreateService(provider);

        // Act
        var result = await service.GetUserDetailsAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.SurveyResults.Should().ContainSingle();
        result.SurveyResults.Single().Answers.Single().AnswerText.Should().Be("Ответ");
    }

    private static UserManagementService CreateService(ServiceProvider provider)
    {
        return new UserManagementService(
            provider.GetRequiredService<ApplicationDbContext>(),
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>());
    }

    private static async Task SeedRolesAsync(ServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(TestDataFactory.CreateRole(role));
        }
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string id,
        string email,
        string? role = null)
    {
        var user = TestDataFactory.CreateUser(id, email);
        var result = await userManager.CreateAsync(user, "User123!");
        result.Succeeded.Should().BeTrue();
        if (role != null)
            (await userManager.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
        return user;
    }
}
