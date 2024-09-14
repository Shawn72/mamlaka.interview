using Mamlaka.API.Exceptions;
using System.Net;
using System.Text.RegularExpressions;

namespace Mamlaka.API.Helpers;
public static class Helpers
{
    public static void ValidatedParameter(string parameter, string? value, out string result, bool throwException = false)
    {
        result = value?.Trim() ?? string.Empty;
        if (result.Length < 1 && throwException)
            throw new CustomException($"{parameter} must be provided to complete this request", "ERR403", HttpStatusCode.Forbidden);
    }

    public static void EvaluatePinStrength(string pin)
    {
        bool valid = Regex.IsMatch(pin, @"^[0-9]{4}$", RegexOptions.IgnoreCase);
        if (!valid) throw new CustomException($"PIN must be 4 digits long", "IN024", HttpStatusCode.PreconditionFailed);
    }

    public static void EvaluatePasswordStrength(string password)
    {
        password ??= string.Empty;
        if (password.Length < 6) throw new CustomException($"Password must be at least 6 characters long", "IN025", HttpStatusCode.PreconditionFailed);
    }

    /// <summary>
    /// Uppercase the first letter in the string.
    /// </summary>
    /// <param name="value">your string to cast</param>
    /// <returns></returns>
    public static string ToUpperFirst(this string value)
    {
        value ??= string.Empty;
        char[] array = value.Trim().ToCharArray();
        array[0] = char.ToUpper(array[0]);
        return new string(array);
    }

    /// <summary>
    /// Uppercase the first letter of each word in the string.
    /// </summary>
    /// <param name="value">string to cast</param>
    /// <returns></returns>
    public static string ToTitleCase(this string value)
    {
        value ??= string.Empty;
        char[] array = value.Trim().ToCharArray();
        bool newWord = true;
        for (int i = 0; i < array.Length; i++)
        {
            if (newWord) { array[i] = char.ToUpper(array[i]); newWord = false; } else { array[i] = char.ToLower(array[i]); }
            if (array[i] == ' ') newWord = true;
        }
        return new string(array);
    }
}
