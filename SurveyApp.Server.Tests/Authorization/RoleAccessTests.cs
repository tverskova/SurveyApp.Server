using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SurveyApp.Server.Components.Pages;

namespace SurveyApp.Server.Tests.Authorization;

public class RoleAccessTests
{
    [Theory]
    [InlineData(typeof(Admin), "Admin")]
    [InlineData(typeof(AdminUsers), "Admin")]
    [InlineData(typeof(AdminUserDetails), "Admin")]
    [InlineData(typeof(SystemStatistics), "Admin")]
    [InlineData(typeof(SurveyResults), "Admin")]
    [InlineData(typeof(UserResults), "User")]
    [InlineData(typeof(UserResultDetails), "User")]
    public void RazorPage_WithRoleProtectedPage_ShouldHaveExpectedAuthorizeRole(Type componentType, string expectedRole)
    {
        // Arrange
        var attributes = componentType
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .ToList();

        // Act
        var roles = attributes.Select(a => a.Roles).Where(r => r != null);

        // Assert
        roles.Should().Contain(r => r!.Contains(expectedRole, StringComparison.Ordinal));
    }

    [Fact]
    public void UserHome_ShouldAllowUserAndAdminRoles()
    {
        // Arrange
        var attributes = typeof(UserHome)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>();

        // Act
        var roles = attributes.Single().Roles;

        // Assert
        roles.Should().Be("User,Admin");
    }
}
