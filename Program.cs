using Microsoft.EntityFrameworkCore;
using TemperatureBlazorApp.Data;
using TemperatureBlazorApp.Hubs;
using TemperatureBlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Поддержка компонентов Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Сервисы бизнес-логики и SignalR из прошлых работ
builder.Services.AddSingleton<TemperatureService>();
builder.Services.AddSingleton<AppDataService>();
builder.Services.AddSignalR();

// Подключение к PostgreSQL через переменную окружения DB_CONNECTION
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? "Host=localhost;Port=5432;Database=appdb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Автоматическое создание БД и тестовых данных при запуске контейнера
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Items.Any())
    {
        db.Items.AddRange(
            new Item { Name = "Датчик Серверная", TemperatureCelsius = 21.5 },
            new Item { Name = "Датчик Цех №1", TemperatureCelsius = 26.0 }
        );
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<TemperatureBlazorApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapHub<TemperatureHub>("/temperaturehub");

// Эндпоинт для проверки взаимодействия с БД в контейнере
app.MapGet("/items", async (AppDbContext db) =>
    await db.Items.ToListAsync());

app.Run();
