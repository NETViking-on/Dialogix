using Dialogix.Data;
using Dialogix.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

// === СБОРКА ПРИЛОЖЕНИЯ ===
var builder = WebApplication.CreateBuilder(args);

// === КОНФИГУРАЦИЯ ===
var configuration = builder.Configuration;

// === СЕРВИСЫ ===
builder.Services.AddRazorPages(options =>
{
    // Защита страниц: только авторизованные могут зайти в чат
    options.Conventions.AuthorizePage("/Chat");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Register");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// Сессии (для fallback, если не CookieAuth)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Доступ к HttpContext (для User, Session и т.д.)
builder.Services.AddHttpContextAccessor();

// База данных
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Репозиторий
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// === АУТЕНТИФИКАЦИЯ (ОБЯЗАТЕЛЬНО!) ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// === HUGGING FACE API ===
var apiKey = configuration["HuggingFace:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("HuggingFace:ApiKey is missing in appsettings.json!");

// HttpClient с Bearer токеном
builder.Services.AddHttpClient<HuggingFaceChatService>(client =>
{
    client.BaseAddress = new Uri("https://api-inference.huggingface.co/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient<IBotService, BotService>(client =>
{
    client.BaseAddress = new Uri("https://api-inference.huggingface.co/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// === ЛОГИРОВАНИЕ ===
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// === СБОРКА ПРИЛОЖЕНИЯ ===
var app = builder.Build();

// === MIDDLEWARE (ВАЖЕН ПОРЯДОК!) ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Сессия — ДО авторизации
app.UseSession();

// Аутентификация и авторизация
app.UseAuthentication();  // ← ОБЯЗАТЕЛЬНО ДО UseAuthorization
app.UseAuthorization();

// === ЗАПУСК ===
app.MapRazorPages();

// Инициализация БД (один раз)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate(); // Автоматическая миграция
}

app.Run();