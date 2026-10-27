using System.Globalization;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.Application.Common.Validation;

public static class RecipeIngredientQuantityValidator
{
    private static readonly Regex NumericQuantityPattern = new(
        @"^[+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+)$",
        RegexOptions.CultureInvariant);

    private static readonly Regex FractionQuantityPattern = new(
        @"^([+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+))\s*/\s*([+-]?(?:\d+(?:[.,]\d*)?|[.,]\d+))$",
        RegexOptions.CultureInvariant);

    private static readonly Regex NegativeNumericValuePattern = new(
        @"(?:^|\s)[−-]\s*\d",
        RegexOptions.CultureInvariant);

    public static void Validate(string? quantity)
    {
        var value = quantity?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (NumericQuantityPattern.IsMatch(value))
        {
            if ((TryParseNumber(value, out var number) && number < 0)
                || (!TryParseNumber(value, out _) && value.StartsWith('-')))
            {
                throw NegativeQuantityException();
            }

            return;
        }

        var fraction = FractionQuantityPattern.Match(value);
        if (fraction.Success
            && TryParseNumber(fraction.Groups[1].Value, out var numerator)
            && TryParseNumber(fraction.Groups[2].Value, out var denominator))
        {
            if (denominator != 0
                && ((numerator < 0 && denominator > 0)
                    || (numerator > 0 && denominator < 0)))
            {
                throw NegativeQuantityException();
            }

            return;
        }

        if (NegativeNumericValuePattern.IsMatch(value))
        {
            throw NegativeQuantityException();
        }
    }

    private static bool TryParseNumber(string value, out decimal number) =>
        decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out number);

    private static ValidationException NegativeQuantityException() =>
        new("Số lượng nguyên liệu không được nhỏ hơn 0.");
}
