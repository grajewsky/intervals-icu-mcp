using System.ComponentModel;
using IntervalsMcp.Intervals;
using ModelContextProtocol.Server;

namespace IntervalsMcp.Tools;

[McpServerToolType]
public sealed class WellnessTools(IntervalsClient intervals)
{
    [McpServerTool(Name = "get_wellness")]
    [Description("Pobiera dane wellness (fitness/CTL, zmęczenie/ATL, forma, HRV, tętno spoczynkowe, sen) z intervals.icu.")]
    public async Task<List<IntervalsWellness>> GetWellness(
        [Description("Liczba dni wstecz (domyślnie 7)")] int days = 7,
        CancellationToken ct = default)
    {
        var newest = DateOnly.FromDateTime(DateTime.Today);
        return await intervals.GetWellnessAsync(newest.AddDays(-days), newest, ct);
    }

    [McpServerTool(Name = "get_planned_workouts")]
    [Description("Zwraca zaplanowane treningi i wydarzenia z kalendarza intervals.icu na najbliższe dni.")]
    public async Task<List<IntervalsEvent>> GetPlannedWorkouts(
        [Description("Liczba dni do przodu (domyślnie 7)")] int days = 7,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var events = await intervals.GetEventsAsync(today, today.AddDays(days), ct);
        return events.OrderBy(e => e.StartDateLocal).ToList();
    }
}
