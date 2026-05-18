using Microsoft.AspNetCore.Identity;
using SurveyApp.Server.Data;

namespace SurveyApp.Server.Tests.TestHelpers;

public static class TestDataFactory
{
    public static ApplicationUser CreateUser(
        string id = "user-1",
        string email = "user@test.com")
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true
        };
    }

    public static IdentityRole CreateRole(string roleName)
    {
        return new IdentityRole
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };
    }

    public static UserProfile CreateProfile(
        string userId = "user-1",
        string firstName = "Иван",
        string lastName = "Иванов",
        string city = "Москва")
    {
        return new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            City = city,
            BirthDate = new DateTime(1995, 5, 10),
            Gender = "Мужской"
        };
    }

    public static Survey CreateSurvey(
        string title = "Тестовый опрос",
        bool isPublished = true,
        string? createdByUserId = null)
    {
        return new Survey
        {
            Title = title,
            Description = "Описание опроса",
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0),
            IsPublished = isPublished,
            IsAnonymous = false,
            CreatedByUserId = createdByUserId
        };
    }

    public static Question CreateQuestion(
        int surveyId,
        string text = "Вопрос",
        string questionType = "Text",
        int order = 1,
        bool isRequired = false)
    {
        return new Question
        {
            SurveyId = surveyId,
            Text = text,
            QuestionType = questionType,
            Order = order,
            IsRequired = isRequired,
            RatingMin = questionType == "Rating" ? 1 : null,
            RatingMax = questionType == "Rating" ? 5 : null
        };
    }

    public static QuestionOption CreateQuestionOption(
        int questionId,
        string text = "Вариант",
        int order = 1,
        bool isCorrect = false)
    {
        return new QuestionOption
        {
            QuestionId = questionId,
            Text = text,
            Order = order,
            IsCorrect = isCorrect
        };
    }

    public static SurveyResponse CreateSurveyResponse(
        int surveyId,
        string? userId = "user-1")
    {
        return new SurveyResponse
        {
            SurveyId = surveyId,
            UserId = userId,
            StartedAt = new DateTime(2026, 1, 2, 10, 0, 0),
            SubmittedAt = new DateTime(2026, 1, 2, 10, 5, 0)
        };
    }

    public static Answer CreateAnswer(
        int responseId,
        int questionId,
        string? textAnswer = null)
    {
        return new Answer
        {
            SurveyResponseId = responseId,
            QuestionId = questionId,
            TextAnswer = textAnswer
        };
    }
}
