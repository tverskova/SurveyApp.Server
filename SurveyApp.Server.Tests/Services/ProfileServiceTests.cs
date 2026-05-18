using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Server.DTOs.Profile;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Services;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_WithExistingProfile_ShouldReturnProfile()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser();
        user.UserProfile = TestDataFactory.CreateProfile(user.Id);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new ProfileService(context);

        // Act
        var result = await service.GetProfileAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.FirstName.Should().Be("Иван");
        result.City.Should().Be("Москва");
    }

    [Fact]
    public async Task GetProfileAsync_WithMissingUser_ShouldReturnNull()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new ProfileService(context);

        // Act
        var result = await service.GetProfileAsync("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_WithExistingProfile_ShouldUpdateFields()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser();
        user.UserProfile = TestDataFactory.CreateProfile(user.Id);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new ProfileService(context);

        // Act
        var updated = await service.UpdateProfileAsync(user.Id, new UpdateUserProfileDto
        {
            FirstName = "Пётр",
            LastName = "Петров",
            City = "Казань",
            BirthDate = new DateTime(1990, 1, 1),
            Gender = "Мужской"
        });

        // Assert
        updated.Should().BeTrue();
        var profile = await context.UserProfiles.SingleAsync();
        profile.FirstName.Should().Be("Пётр");
        profile.LastName.Should().Be("Петров");
        profile.City.Should().Be("Казань");
        profile.BirthDate.Should().Be(new DateTime(1990, 1, 1));
    }

    [Fact]
    public async Task UpdateProfileAsync_WithoutExistingProfile_ShouldCreateProfile()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new ProfileService(context);

        // Act
        var updated = await service.UpdateProfileAsync(user.Id, new UpdateUserProfileDto
        {
            FirstName = "Анна",
            City = "Тверь"
        });

        // Assert
        updated.Should().BeTrue();
        var profile = await context.UserProfiles.SingleAsync();
        profile.UserId.Should().Be(user.Id);
        profile.FirstName.Should().Be("Анна");
        profile.City.Should().Be("Тверь");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithMissingUser_ShouldReturnFalse()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var service = new ProfileService(context);

        // Act
        var updated = await service.UpdateProfileAsync("missing", new UpdateUserProfileDto { FirstName = "Анна" });

        // Assert
        updated.Should().BeFalse();
    }
}
