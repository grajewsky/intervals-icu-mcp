using System.ComponentModel.DataAnnotations;

namespace IntervalsMcp.Intervals;

public sealed class IntervalsOptions
{
    public const string SectionName = "Intervals";

    [Required, Url]
    public string BaseUrl { get; set; } = "https://intervals.icu/api/v1/";

    // "0" = atleta, do którego należy klucz API
    [Required]
    public string AthleteId { get; set; } = "0";

    // Klucz z intervals.icu → Settings → Developer Settings
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
