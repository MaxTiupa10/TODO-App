using System.Globalization;

namespace TODO_App.Services.Helpers;

public static class TaskQueryDateParser
{
    private static readonly string[] Formats = ["yyyy-MM-dd", "yyyy/MM/dd"];

    public static bool TryParse(string? value, out DateOnly? date, out string? error)
    {
        date = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (DateOnly.TryParseExact(
                value.Trim(),
                Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            date = parsed;
            return true;
        }

        error = $"Invalid date format: '{value}'. Expected yyyy-MM-dd.";
        return false;
    }

    public static (DateOnly? From, DateOnly? To) NormalizeRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            return (to, from);

        return (from, to);
    }
}
