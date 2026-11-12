namespace DzentoPOS2.Core.Helpers;

/// <summary>
/// Luhn algorithm validation for IMEI numbers.
/// </summary>
public static class LuhnValidator
{
    public static bool IsValidImei(string? imei)
    {
        if (string.IsNullOrWhiteSpace(imei)) return false;
        var digits = imei.Trim().Replace(" ", "").Replace("-", "");
        if (digits.Length != 15) return false;
        if (!digits.All(char.IsDigit)) return false;

        int sum = 0;
        bool alternate = false;
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int d = digits[i] - '0';
            if (alternate)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }
}
