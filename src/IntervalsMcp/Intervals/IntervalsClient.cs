using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace IntervalsMcp.Intervals;

/// <summary>
/// Typowany klient HTTP do API intervals.icu.
/// Instancję tworzy IHttpClientFactory (AddHttpClient w Program.cs).
/// </summary>
public sealed class IntervalsClient
{
    private readonly HttpClient _http;
    private readonly string _athleteId;

    public IntervalsClient(HttpClient http, IOptions<IntervalsOptions> options)
    {
        var o = options.Value;
        _http = http;
        _athleteId = o.AthleteId;

        _http.BaseAddress = new Uri(o.BaseUrl);

        // Basic Auth: użytkownik "API_KEY", hasło = klucz
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"API_KEY:{o.ApiKey}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    public Task<List<IntervalsActivity>> GetActivitiesAsync(DateOnly oldest, DateOnly newest, CancellationToken ct = default) =>
        GetListAsync<IntervalsActivity>("activities", oldest, newest, ct);

    public Task<List<IntervalsWellness>> GetWellnessAsync(DateOnly oldest, DateOnly newest, CancellationToken ct = default) =>
        GetListAsync<IntervalsWellness>("wellness", oldest, newest, ct);

    public Task<List<IntervalsEvent>> GetEventsAsync(DateOnly oldest, DateOnly newest, CancellationToken ct = default) =>
        GetListAsync<IntervalsEvent>("events", oldest, newest, ct);

    // Metoda generyczna — jeden kod dla wszystkich endpointów z zakresem dat
    private async Task<List<T>> GetListAsync<T>(string resource, DateOnly oldest, DateOnly newest, CancellationToken ct)
    {
        var url = $"athlete/{_athleteId}/{resource}?oldest={oldest:yyyy-MM-dd}&newest={newest:yyyy-MM-dd}";
        var result = await _http.GetFromJsonAsync<List<T>>(url, ct);
        return result ?? [];
    }
}
