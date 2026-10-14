using System.Text.Json.Serialization;

namespace IntervalsMcp.Intervals;

// Rekordy DTO odpowiadające JSON-owi z API intervals.icu.
// Pola są nullable, bo API nie zawsze je zwraca.

public sealed record IntervalsActivity(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("start_date_local")] DateTime StartDateLocal,
    [property: JsonPropertyName("distance")] double? Distance,
    [property: JsonPropertyName("moving_time")] int? MovingTime,
    [property: JsonPropertyName("total_elevation_gain")] double? ElevationGain,
    [property: JsonPropertyName("icu_training_load")] int? TrainingLoad,
    [property: JsonPropertyName("average_heartrate")] int? AverageHeartRate);

public sealed record IntervalsWellness(
    [property: JsonPropertyName("id")] string Date,
    [property: JsonPropertyName("ctl")] double? Fitness,
    [property: JsonPropertyName("atl")] double? Fatigue,
    [property: JsonPropertyName("rampRate")] double? RampRate,
    [property: JsonPropertyName("restingHR")] int? RestingHr,
    [property: JsonPropertyName("hrv")] double? Hrv,
    [property: JsonPropertyName("weight")] double? Weight,
    [property: JsonPropertyName("sleepSecs")] int? SleepSecs)
{
    // Właściwość wyliczana: forma = fitness - zmęczenie
    public double? Form => Fitness - Fatigue;
}

public sealed record IntervalsEvent(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("start_date_local")] DateTime StartDateLocal,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("icu_training_load")] int? TrainingLoad);
