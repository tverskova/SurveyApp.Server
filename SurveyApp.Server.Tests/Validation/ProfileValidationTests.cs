using FluentAssertions;
using SurveyApp.Server.DTOs.Profile;
using SurveyApp.Server.Services;
using SurveyApp.Server.Tests.TestHelpers;

namespace SurveyApp.Server.Tests.Validation;

public class ProfileValidationTests
{
    [Fact]
    public async Task UpdateProfileAsync_WithFutureBirthDate_InServiceLayer_ShouldSaveBecauseValidationIsInRazorPage()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new ProfileService(context);
        var futureDate = DateTime.Today.AddDays(1);

        // Act
        var result = await service.UpdateProfileAsync(user.Id, new UpdateUserProfileDto
        {
            FirstName = "Анна",
            BirthDate = futureDate
        });

        // Assert
        result.Should().BeTrue();
        context.UserProfiles.Single().BirthDate.Should().Be(futureDate);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithPhotoFieldsOnEntity_ShouldPersistPhotoData()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var user = TestDataFactory.CreateUser();
        user.UserProfile = TestDataFactory.CreateProfile(user.Id);
        user.UserProfile.Photo = [1, 2, 3];
        user.UserProfile.PhotoContentType = "image/png";
        context.Users.Add(user);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var profile = context.UserProfiles.Single();
        profile.Photo.Should().Equal(1, 2, 3);
        profile.PhotoContentType.Should().Be("image/png");
    }
}
