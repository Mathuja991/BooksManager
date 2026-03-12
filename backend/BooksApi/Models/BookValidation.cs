using System.Globalization;

namespace BooksApi.Models;

public static class BookValidation
{
    public static IResult? Validate(BookUpsertDto dto)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(dto.Title))
            errors["title"] = ["Title is required."];

        if (string.IsNullOrWhiteSpace(dto.Author))
            errors["author"] = ["Author is required."];

        if (string.IsNullOrWhiteSpace(dto.Isbn))
            errors["isbn"] = ["ISBN is required."];

        if (string.IsNullOrWhiteSpace(dto.PublicationDate))
        {
            errors["publicationDate"] = ["Publication date is required."];
        }
        else if (!TryNormalizeDate(dto.PublicationDate, out var normalized))
        {
            errors["publicationDate"] = ["Publication date must be a valid date (YYYY-MM-DD)."];
        }
        else
        {
            dto.PublicationDate = normalized;
        }

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    private static bool TryNormalizeDate(string input, out string normalized)
    {
        normalized = "";
        var trimmed = input.Trim();

        if (DateOnly.TryParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            normalized = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return true;
        }

        if (DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
        {
            var asDate = DateOnly.FromDateTime(dto.UtcDateTime);
            normalized = asDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return true;
        }

        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
        {
            var asDate = DateOnly.FromDateTime(dt);
            normalized = asDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }
}
