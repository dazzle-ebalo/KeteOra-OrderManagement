var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.UseDefaultFiles();   // serves wwwroot/index.html at /
app.UseStaticFiles();

// Sample orders: stands in for KeteOra's order database
var orders = new[]
{
    new { Id = 1001, Customer = "Aroha T.", Total = 89.50m,  Status = "Shipped" },
    new { Id = 1002, Customer = "Liam K.",  Total = 42.00m,  Status = "Processing" },
    new { Id = 1003, Customer = "Mere P.",  Total = 130.25m, Status = "Delivered" }
};

app.MapGet("/api/orders", () => orders);

// In Kubernetes the machine name is the pod name, so the page shows which replica answered
app.MapGet("/api/info", () => new { Server = Environment.MachineName });

app.MapHealthChecks("/health");
app.Run();