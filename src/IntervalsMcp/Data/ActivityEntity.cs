namespace IntervalsMcp.Data;

// Encja EF Core — wiersz tabeli "activities" (lokalny cache aktywności)
public class ActivityEntity
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public DateTime StartDateLocal { get; set; }
    public double? DistanceMeters { get; set; }
    public int? MovingTimeSeconds { get; set; }
    public double? ElevationGain { get; set; }
    public int? TrainingLoad { get; set; }
    public int? AverageHeartRate { get; set; }
    public DateTime SyncedAtUtc { get; set; }
}
