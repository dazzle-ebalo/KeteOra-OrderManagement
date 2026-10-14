using System.Security.Claims;
using System.Threading.RateLimiting;
using KeteOra.Orders.Data;
using KeteOra.Orders.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Helper mode: dotnet run --project app -- hash-password
if (args.Contains("hash-password"))
{
    AdminAccount.RunHashPasswordTool();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Demo database: a SQLite file in the temp folder, which the container's non-root user can write to
var defaultDb = $"Data Source={Path.Combine(Path.GetTempPath(), "keteora.db")}";
builder.Services.AddDbContext<KeteOraDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("KeteOra") ?? defaultDb));

// Admin sign-in: an HttpOnly session cookie that browsers only send to this site
builder.Services.AddSingleton<AdminAccount>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "keteora.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // the AKS external IP serves plain HTTP
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/login.html";
        options.Events.OnRedirectToLogin = context =>
        {
            // APIs answer 401 instead of redirecting, so the page scripts can send the user to the login page
            if (context.Request.Path.StartsWithSegments("/api"))
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            else
                context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Slow down password guessing: 5 sign-in attempts per minute per client address
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Rebuild the demo database from the seed data on every start
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KeteOraDb>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    SeedData.Load(db);
}

if (!app.Services.GetRequiredService<AdminAccount>().IsConfigured)
    app.Logger.LogWarning("Admin:Username or Admin:PasswordHash is not set, so nobody can sign in.");

// Security headers on every response
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Content-Security-Policy"] =
        "default-src 'self'; style-src 'self' https://fonts.googleapis.com; font-src https://fonts.gstatic.com; frame-ancestors 'none'";
    await next();
});

app.UseAuthentication();

// Every page except the login page needs a signed-in admin
string[] publicPages = ["/login.html"];
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    var isPage = path == "/" || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
    if (isPage && !publicPages.Contains(path, StringComparer.OrdinalIgnoreCase) &&
        context.User.Identity?.IsAuthenticated != true)
    {
        context.Response.Redirect("/login.html");
        return;
    }
    await next();
});

app.UseAuthorization();
app.UseRateLimiter();
app.UseDefaultFiles(); // makes a request for / return index.html
app.UseStaticFiles();

app.MapPost("/api/login", async (LoginRequest request, AdminAccount admin, HttpContext context) =>
{
    if (!admin.IsConfigured)
        return Results.Problem("The admin account isn't configured on this server.", statusCode: StatusCodes.Status503ServiceUnavailable);
    if (!admin.Verify(request.Username ?? "", request.Password ?? ""))
        return Results.Unauthorized();

    var identity = new ClaimsIdentity(
        [new Claim(ClaimTypes.Name, admin.DisplayName), new Claim(ClaimTypes.Role, "Admin")],
        CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(new ClaimsPrincipal(identity));
    return Results.Ok();
}).RequireRateLimiting("login");

app.MapPost("/api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync();
    return Results.Ok();
});

// Everything under /api below this line needs a signed-in admin
var api = app.MapGroup("/api").RequireAuthorization();

api.MapGet("/me", (ClaimsPrincipal user) => new { Name = user.Identity?.Name });

api.MapGet("/orders", async (KeteOraDb db) =>
    (await db.Orders.Include(o => o.Customer).Include(o => o.Lines).ToListAsync())
        .OrderByDescending(o => o.PlacedAt)
        .Select(o => new
        {
            o.Id,
            o.PlacedAt,
            Customer = o.Customer!.Name,
            o.Customer.City,
            Items = o.Lines.Sum(l => l.Quantity),
            Total = o.Lines.Sum(l => l.Quantity * l.UnitPrice),
            o.Payment,
            o.Fulfillment,
            o.Delivery
        }));

api.MapGet("/products", async (KeteOraDb db) =>
{
    var sold = (await db.OrderLines.ToListAsync())
        .GroupBy(l => l.ProductId)
        .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
    return (await db.Products.ToListAsync())
        .OrderBy(p => p.Id)
        .Select(p => new { p.Id, p.Sku, p.Name, p.Category, p.Price, p.Stock, Sold = sold.GetValueOrDefault(p.Id) });
});

api.MapGet("/customers", async (KeteOraDb db) =>
    (await db.Customers.Include(c => c.Orders).ThenInclude(o => o.Lines).ToListAsync())
        .OrderBy(c => c.Name)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            Address = $"{c.Street}, {c.Suburb}, {c.City} {c.Postcode}",
            c.Region,
            Orders = c.Orders.Count,
            Spent = c.Orders.Where(o => o.Payment == "Paid").Sum(o => o.Lines.Sum(l => l.Quantity * l.UnitPrice))
        }));

api.MapGet("/analytics", async (KeteOraDb db) =>
{
    var orders = await db.Orders.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).ToListAsync();
    var paid = orders.Where(o => o.Payment == "Paid").ToList();
    static decimal Total(Order o) => o.Lines.Sum(l => l.Quantity * l.UnitPrice);

    return new
    {
        Orders = orders.Count,
        PaidRevenue = paid.Sum(Total),
        AverageOrderValue = paid.Count == 0 ? 0 : Math.Round(paid.Sum(Total) / paid.Count, 2),
        ItemsSold = orders.Sum(o => o.Lines.Sum(l => l.Quantity)),
        ByFulfillment = orders.GroupBy(o => o.Fulfillment).Select(g => new { Status = g.Key, Count = g.Count() }),
        RevenueByRegion = paid.GroupBy(o => o.Customer!.Region)
            .Select(g => new { Region = g.Key, Revenue = g.Sum(Total) })
            .OrderByDescending(r => r.Revenue),
        TopProducts = orders.SelectMany(o => o.Lines)
            .GroupBy(l => l.Product!.Name)
            .Select(g => new { Product = g.Key, Quantity = g.Sum(l => l.Quantity), Revenue = g.Sum(l => l.Quantity * l.UnitPrice) })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
    };
});

// In Kubernetes the machine name is the pod name, so the page shows which replica answered
app.MapGet("/api/info", () => new { Server = Environment.MachineName });

// Kubernetes readiness and liveness probes call this endpoint
app.MapHealthChecks("/health");

app.Run();

record LoginRequest(string? Username, string? Password);
