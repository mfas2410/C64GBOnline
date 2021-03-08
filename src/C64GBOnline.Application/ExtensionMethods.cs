namespace C64GBOnline.Application;

public static class ExtensionMethods
{
    /// <summary>
    ///     Converts special characters to normal ones, so that e.g. "a" == "á" == "à".
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The converted string.</returns>
    public static string RemoveDiacritics(this string text)
        => string.Concat(text.Normalize(NormalizationForm.FormD).Where(x => CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark)).Normalize(NormalizationForm.FormC);
}