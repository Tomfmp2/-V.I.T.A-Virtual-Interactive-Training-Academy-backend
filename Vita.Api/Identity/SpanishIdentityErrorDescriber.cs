using Microsoft.AspNetCore.Identity;

namespace Vita.Api.Identity;

/// <summary>
/// Mensajes de Identity en español para que la API nunca devuelva errores en inglés.
/// </summary>
public sealed class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Ha ocurrido un error inesperado." };

    public override IdentityError ConcurrencyFailure() =>
        new() { Code = nameof(ConcurrencyFailure), Description = "Error de concurrencia. El registro ya fue modificado." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "Contraseña incorrecta." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "Token inválido." };

    public override IdentityError LoginAlreadyAssociated() =>
        new() { Code = nameof(LoginAlreadyAssociated), Description = "Este inicio de sesión ya está asociado a otra cuenta." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = $"El nombre de usuario '{userName}' no es válido." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = $"El correo '{email}' no es válido." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"El nombre de usuario '{userName}' ya está en uso." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"El correo '{email}' ya está registrado." };

    public override IdentityError InvalidRoleName(string? role) =>
        new() { Code = nameof(InvalidRoleName), Description = $"El rol '{role}' no es válido." };

    public override IdentityError DuplicateRoleName(string role) =>
        new() { Code = nameof(DuplicateRoleName), Description = $"El rol '{role}' ya existe." };

    public override IdentityError UserAlreadyHasPassword() =>
        new() { Code = nameof(UserAlreadyHasPassword), Description = "El usuario ya tiene una contraseña configurada." };

    public override IdentityError UserLockoutNotEnabled() =>
        new() { Code = nameof(UserLockoutNotEnabled), Description = "El bloqueo de cuenta no está habilitado." };

    public override IdentityError UserAlreadyInRole(string role) =>
        new() { Code = nameof(UserAlreadyInRole), Description = $"El usuario ya tiene el rol '{role}'." };

    public override IdentityError UserNotInRole(string role) =>
        new() { Code = nameof(UserNotInRole), Description = $"El usuario no tiene el rol '{role}'." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        new()
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"La contraseña debe contener al menos {uniqueChars} caracteres distintos."
        };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new()
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "La contraseña debe incluir al menos un carácter no alfanumérico."
        };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe incluir al menos un dígito (0-9)." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe incluir al menos una letra minúscula (a-z)." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe incluir al menos una letra mayúscula (A-Z)." };
}
