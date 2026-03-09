using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace PerformansSitesi.Infrastructure;

public class MemoryCacheTicketStore : ITicketStore
{
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore(IMemoryCache cache)  // ← AYNI KALDI
    {
        _cache = cache;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)  // ← async kaldır
    {
        var key = $"AuthTicket_{Guid.NewGuid()}";
        _cache.Set(key, ticket, TimeSpan.FromMinutes(30));
        return Task.FromResult(key);  // ← await kaldır
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        _cache.Set(key, ticket, TimeSpan.FromMinutes(30));
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}