using System.Text.RegularExpressions;

namespace NexHire.Application.Validators;

public static partial class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 64;

    public const string ErrorMessage =
        "Password must be 8-64 characters and contain uppercase, lowercase, number and symbol.";

    public static bool IsStrong(string? password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Length is >= MinLength and <= MaxLength
            && UppercaseRegex().IsMatch(password)
            && LowercaseRegex().IsMatch(password)
            && NumberRegex().IsMatch(password)
            && SymbolRegex().IsMatch(password);
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex NumberRegex();

    [GeneratedRegex("[^A-Za-z0-9]")]
    private static partial Regex SymbolRegex();
}
