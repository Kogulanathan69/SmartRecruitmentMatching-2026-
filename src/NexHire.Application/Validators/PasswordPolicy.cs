using System.Text.RegularExpressions;
namespace NexHire.Application.Validators;
public static class PasswordPolicy
{
    public const int MinLength = 8; public const int MaxLength = 64;
    public static bool IsStrong(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length is >= MinLength and <= MaxLength && Regex.IsMatch(value, "[A-Z]") && Regex.IsMatch(value, "[a-z]") && Regex.IsMatch(value, "[0-9]") && Regex.IsMatch(value, "[^A-Za-z0-9]");
}
