using Microsoft.AspNetCore.Identity;

namespace SurveyApp.Server
{
    public class RussianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => Error(nameof(DefaultError), "Произошла ошибка. Повторите действие.");

        public override IdentityError ConcurrencyFailure()
            => Error(nameof(ConcurrencyFailure), "Данные были изменены другим процессом. Обновите страницу и повторите действие.");

        public override IdentityError PasswordMismatch()
            => Error(nameof(PasswordMismatch), "Текущий пароль указан неверно.");

        public override IdentityError InvalidToken()
            => Error(nameof(InvalidToken), "Недействительный код подтверждения.");

        public override IdentityError LoginAlreadyAssociated()
            => Error(nameof(LoginAlreadyAssociated), "Этот внешний вход уже связан с другой учётной записью.");

        public override IdentityError InvalidUserName(string? userName)
            => Error(nameof(InvalidUserName), $"Имя пользователя '{userName}' недопустимо.");

        public override IdentityError InvalidEmail(string? email)
            => Error(nameof(InvalidEmail), $"Адрес электронной почты '{email}' недопустим.");

        public override IdentityError DuplicateUserName(string userName)
            => Error(nameof(DuplicateUserName), $"Имя пользователя '{userName}' уже используется.");

        public override IdentityError DuplicateEmail(string email)
            => Error(nameof(DuplicateEmail), $"Адрес электронной почты '{email}' уже используется.");

        public override IdentityError InvalidRoleName(string? role)
            => Error(nameof(InvalidRoleName), $"Роль '{role}' недопустима.");

        public override IdentityError DuplicateRoleName(string role)
            => Error(nameof(DuplicateRoleName), $"Роль '{role}' уже существует.");

        public override IdentityError UserAlreadyHasPassword()
            => Error(nameof(UserAlreadyHasPassword), "Для пользователя уже задан пароль.");

        public override IdentityError UserLockoutNotEnabled()
            => Error(nameof(UserLockoutNotEnabled), "Блокировка для этого пользователя не включена.");

        public override IdentityError UserAlreadyInRole(string role)
            => Error(nameof(UserAlreadyInRole), $"Пользователь уже состоит в роли '{role}'.");

        public override IdentityError UserNotInRole(string role)
            => Error(nameof(UserNotInRole), $"Пользователь не состоит в роли '{role}'.");

        public override IdentityError PasswordTooShort(int length)
            => Error(nameof(PasswordTooShort), $"Пароль должен содержать минимум {length} символов.");

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => Error(nameof(PasswordRequiresNonAlphanumeric), "Пароль должен содержать хотя бы один специальный символ.");

        public override IdentityError PasswordRequiresDigit()
            => Error(nameof(PasswordRequiresDigit), "Пароль должен содержать хотя бы одну цифру.");

        public override IdentityError PasswordRequiresLower()
            => Error(nameof(PasswordRequiresLower), "Пароль должен содержать хотя бы одну строчную букву.");

        public override IdentityError PasswordRequiresUpper()
            => Error(nameof(PasswordRequiresUpper), "Пароль должен содержать хотя бы одну заглавную букву.");

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => Error(nameof(PasswordRequiresUniqueChars), $"Пароль должен содержать минимум {uniqueChars} разных символов.");

        public override IdentityError RecoveryCodeRedemptionFailed()
            => Error(nameof(RecoveryCodeRedemptionFailed), "Код восстановления недействителен.");

        private static IdentityError Error(string code, string description)
        {
            return new IdentityError { Code = code, Description = description };
        }
    }
}
