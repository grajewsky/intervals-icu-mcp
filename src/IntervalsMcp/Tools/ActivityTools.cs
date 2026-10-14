using System.ComponentModel;
using IntervalsMcp.Data;
using IntervalsMcp.Intervals;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace IntervalsMcp.Tools;

[McpServerToolType]
public sealed class ActivityTools(IntervalsClient intervals, AppDbContext db)
{
    [McpServerTool(Name = "sync_activities")]
    [Description("Pobiera aktywności z intervals.icu z ostatnich N dni i zapisuje je w lokalnej bazie.")]
    public async Task<string> SyncActivities(
        [Description("Liczba dni wstecz (domyślnie 30)")] int days = 30,
        CancellationToken ct = default)
    {
        var newest = DateOnly.FromDateTime(DateTime.Today);
        var oldest = newest.AddDays(-days);

        var activities = await intervals.GetActivitiesAsync(oldest, newest, ct);
        var ids = activities.Select(a => a.Id).ToList();

        // Jedno zapytanie zamiast N: pobierz istniejące rekordy do słownika
        var existing = await db.Activities
            .Where(a => ids.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        foreach (var a in activities)
        {
            if (!existing.TryGetValue(a.Id, out var entity))
            {
                entity = new ActivityEntity { Id = a.Id };
                db.Activities.Add(entity);
            }

            entity.Name = a.Name;
            entity.Type = a.Type;
            entity.StartDateLocal = a.StartDateLocal;
            entity.DistanceMeters = a.Distance;
            entity.MovingTimeSeconds = a.MovingTime;
            entity.ElevationGain = a.ElevationGain;
            entity.TrainingLoad = a.TrainingLoad;
            entity.AverageHeartRate = a.AverageHeartRate;
            entity.SyncedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return $"Zsynchronizowano {activities.Count} aktywności ({oldest} – {newest}).";
    }

    [McpServerTool(Name = "get_activities")]
    [Description("Zwraca aktywności z lokalnej bazy (najpierw uruchom sync_activities).")]
    public async Task<List<ActivitySummary>> GetActivities(
        [Description("Liczba dni wstecz (domyślnie 14)")] int days = 14,
        [Description("Opcjonalny filtr typu, np. Ride, Run, Swim")] string? type = null,
        CancellationToken ct = default)
    {
        var from = DateTime.Today.AddDays(-days);

        var query = db.Activities.AsNoTracking().Where(a => a.StartDateLocal >= from);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(a => a.Type == type);

        return await query
            .OrderByDescending(a => a.StartDateLocal)
            .Select(a => new ActivitySummary(
                a.Id,
                a.Name,
                a.Type,
                a.StartDateLocal,
                a.DistanceMeters / 1000,
                a.MovingTimeSeconds / 60,
                a.TrainingLoad))
            .ToListAsync(ct);
    }

    [McpServerTool(Name = "weekly_summary")]
    [Description("Podsumowanie tygodniowe (dystans, czas, obciążenie) na podstawie lokalnej bazy.")]
    public async Task<List<WeeklySummary>> WeeklySummary(
        [Description("Liczba tygodni wstecz (domyślnie 8)")] int weeks = 8,
        CancellationToken ct = default)
    {
        var from = DateTime.Today.AddDays(-7 * weeks);

        var rows = await db.Activities
            .AsNoTracking()
            .Where(a => a.StartDateLocal >= from)
            .ToListAsync(ct);

        // Grupowanie w pamięci (LINQ to Objects) — prościej niż tłumaczenie tygodni na SQL
        return rows
            .GroupBy(a => StartOfWeek(a.StartDateLocal))
            .OrderBy(g => g.Key)
            .Select(g => new WeeklySummary(
                WeekStart: DateOnly.FromDateTime(g.Key),
                Activities: g.Count(),
                DistanceKm: Math.Round(g.Sum(a => a.DistanceMeters ?? 0) / 1000, 1),
                Hours: Math.Round(g.Sum(a => a.MovingTimeSeconds ?? 0) / 3600.0, 1),
                TrainingLoad: g.Sum(a => a.TrainingLoad ?? 0)))
            .ToList();
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        // Poniedziałek jako początek tygodnia
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-diff);
    }
}

public sealed record ActivitySummary(
    string Id, string? Name, string? Type, DateTime Start,
    double? DistanceKm, int? Minutes, int? TrainingLoad);

public sealed record WeeklySummary(
    DateOnly WeekStart, int Activities, double DistanceKm, double Hours, int TrainingLoad);
