using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PerformansSitesi.Application.Services;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using PerformansSitesi.Infrastructure.Interceptors;
using PerformansSitesi.Infrastructure.Seed;
using PerformansSitesi.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

// Data Protection (IIS + çoklu instance için şart)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\DataProtectionKeys"))
    .SetApplicationName("PerformansSitesi");

// Rate limiting
builder.Services.AddSingleton<IRateLimitService>(new RateLimitService(maxRequests: 100, windowSeconds: 60));

// Memory cache ve session
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>();
});

// Database
builder.Services.AddSingleton<AutoSyncSaveChangesInterceptor>();
builder.Services.AddDbContext<PerformansDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AutoSyncSaveChangesInterceptor>();

    var connectionString = builder.Environment.IsProduction()
        ? Environment.GetEnvironmentVariable("PERFORMANS_DB_CONNECTION")
          ?? builder.Configuration.GetConnectionString("PerformansDb")
        : builder.Configuration.GetConnectionString("PerformansDb");

    options.UseSqlServer(connectionString)
           .AddInterceptors(interceptor);
});

// HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// ✅ FIX: MemoryCacheTicketStore'u DI container'a kaydet (anti-pattern düzeltildi)
builder.Services.AddSingleton<ITicketStore>(provider =>
    new PerformansSitesi.Infrastructure.MemoryCacheTicketStore(
        provider.GetRequiredService<IMemoryCache>()));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        if (builder.Environment.IsProduction())
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;

            // ✅ Cookie boyutunu küçült - DI'dan al
            options.SessionStore = builder.Services.BuildServiceProvider()
                .GetRequiredService<ITicketStore>();
        }
        else
        {
            // Development - HTTP üzerinde çalışabilir
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
        }

        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddScoped<IPasswordHasher<Kullanici>, PasswordHasher<Kullanici>>();
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<DatabaseSyncService>();
builder.Services.AddScoped<KilavuzService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventLog();

var app = builder.Build();

// Türkçe karakter desteği
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Database seeding
try
{
    await DbSeeder.SeedAsync(app.Services);
    app.Logger.LogInformation("Database initialized successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database seeding error: {Message}", ex.Message);
}

// Error handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/Handle");
    app.UseHsts();
}

// Global exception logging middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception! Path: {Path}, Method: {Method}, User: {User}",
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous");
        throw;
    }
});

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://cdn.jsdelivr.net;";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

    await next();
});

// HTTPS redirection (sadece production'da)
if (builder.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseSession();

// Middleware pipeline
app.UseMiddleware<RateLimitingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint eşlemeleri
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ===============================
// TEK SEFERLİK TÜRKÇE DÜZELTME ENDPOINT'İ (SADECE DEVELOPMENT)
// ===============================
if (app.Environment.IsDevelopment())
{
    app.MapGet("/__fix-tr", () =>
    {
        var root = app.Environment.ContentRootPath;

        var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".cshtml", ".json", ".config", ".sql", ".txt"
        };

        bool LooksBroken(string s) => s.Contains(' ');

        var encUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        var enc1254 = Encoding.GetEncoding(1254); // Turkish
        var enc1252 = Encoding.GetEncoding(1252); // Western
        var enc28599 = Encoding.GetEncoding(28599); // ISO-8859-9 (Turkish)

        int scanned = 0, changed = 0, skipped = 0, failed = 0;

        foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file);
            if (!exts.Contains(ext))
                continue;

            var lower = file.ToLowerInvariant();
            if (lower.Contains(@"\bin\") || lower.Contains(@"\obj\") ||
                lower.Contains(@"\.git\") || lower.Contains(@"\publish\"))
            {
                skipped++;
                continue;
            }

            scanned++;

            try
            {
                byte[] bytes = File.ReadAllBytes(file);

                string textUtf8;
                try
                {
                    textUtf8 = encUtf8.GetString(bytes);
                }
                catch
                {
                    textUtf8 = "";
                }

                if (!string.IsNullOrEmpty(textUtf8) && !LooksBroken(textUtf8))
                {
                    continue;
                }

                string t1254 = enc1254.GetString(bytes);
                string t1252 = enc1252.GetString(bytes);
                string t28599 = enc28599.GetString(bytes);

                string PickBest(params string[] candidates)
                {
                    return candidates
                        .OrderBy(c => c.Count(ch => ch == '�'))
                        .ThenByDescending(c => c.Count(ch => ch >= 0x20))
                        .First();
                }

                var best = PickBest(t1254, t28599, t1252, textUtf8);

                if (!string.IsNullOrEmpty(best) && best != textUtf8)
                {
                    var beforeBad = string.IsNullOrEmpty(textUtf8) ? int.MaxValue : textUtf8.Count(ch => ch == ' ');
                    var afterBad = best.Count(ch => ch == ' ');

                    if (afterBad < beforeBad)
                    {
                        File.WriteAllText(file, best, new UTF8Encoding(false));
                        changed++;
                    }
                }
            }
            catch
            {
                failed++;
            }
        }

        return Results.Ok(new
        {
            Root = root,
            Scanned = scanned,
            Changed = changed,
            Skipped = skipped,
            Failed = failed,
            Note = "Bitti. Eğer istediğin düzeldiyse /__fix-tr endpoint'ini Program.cs'den sil."
        });
    });

}

// Dashboard route'ları (özel route'lar ÖNCE)
app.MapControllerRoute(
    name: "dashboardWithUser",
    pattern: "dashboard/{kullaniciAdi}",
    defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "dashboardRoot",
    pattern: "dashboard",
    defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Logger.LogInformation("Application started - Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();