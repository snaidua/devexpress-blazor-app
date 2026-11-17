using DevExpress.Blazor;
using DevExpressBlazorApp.Components;
using DevExpressBlazorApp.Data;
using DevExpressBlazorApp.Data.Entities;
using DevExpressBlazorApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");

// 2. Hybrid Data Access Layer
// EF Core DbContextFactory (thread-safe for Blazor Server circuits)
builder.Services.AddDbContextFactory<ErpDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Dapper High-Speed Connection Factory & DapperContext
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddSingleton<DapperContext>();

// 3. DevExpress Blazor Suite
builder.Services.AddDevExpressBlazor();

// 4. Identity & Security Services
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<SecurityService>();

// 5. Global State & Dapper Query Services
builder.Services.AddScoped<DashboardQueryService>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<DatabaseSeeder>();

// 6. Razor Components & Interactive Server Mode
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// 7. Seed Database at Startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// 8. Configure HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
