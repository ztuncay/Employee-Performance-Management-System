using System.Collections.Concurrent;

namespace PerformansSitesi.Application.Services;

/// <summary>
/// Kendi Rate Limiting implementasyonu (AspNetCoreRateLimit yerine)
/// IP adresi ba��na istek say�s� limitlemesi
/// </summary>
public interface IRateLimitService
{
    bool IsAllowed(string clientId);
    void Reset(string clientId);
}

public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, ClientRateLimit> _clientLimits;
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly Timer _cleanupTimer;

    public RateLimitService(int maxRequests = 100, int windowSeconds = 60)
    {
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromSeconds(windowSeconds);
        _clientLimits = new ConcurrentDictionary<string, ClientRateLimit>();

        _cleanupTimer = new Timer(CleanupExpiredLimits, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    public bool IsAllowed(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return true;

        var now = DateTime.UtcNow;
        
        var limit = _clientLimits.AddOrUpdate(clientId, 
            new ClientRateLimit { FirstRequestTime = now, RequestCount = 1 },
            (key, existing) =>
            {
                var timeElapsed = now - existing.FirstRequestTime;
                
                if (timeElapsed > _timeWindow)
                {
                    return new ClientRateLimit { FirstRequestTime = now, RequestCount = 1 };
                }

                if (existing.RequestCount < _maxRequests)
                {
                    existing.RequestCount++;
                    return existing;
                }

                return existing;
            });

        return limit.RequestCount <= _maxRequests;
    }

    public void Reset(string clientId)
    {
        _clientLimits.TryRemove(clientId, out _);
    }

    private void CleanupExpiredLimits(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _clientLimits
            .Where(x => (now - x.Value.FirstRequestTime) > _timeWindow)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _clientLimits.TryRemove(key, out _);
        }
    }

    private class ClientRateLimit
    {
        public DateTime FirstRequestTime { get; set; }
        public int RequestCount { get; set; }
    }
}

/// <summary>
/// Rate Limiting Middleware
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitService _rateLimitService;

    public RateLimitingMiddleware(RequestDelegate next, IRateLimitService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!_rateLimitService.IsAllowed(clientId))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "�ok fazla istek. L�tfen daha sonra tekrar deneyin." });
            return;
        }

        await _next(context);
    }
}
