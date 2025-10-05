using Dialogix.Data;
using Dialogix.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Ошибка: строка подключения не найдена в конфигурации!");
}
else
{
    DatabaseTest.TestConnection(connectionString);
}


builder.Services.AddScoped<IChatRepository, ChatRepository>();


var apiKey = builder.Configuration["HuggingFace:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new ArgumentException("HuggingFace API key not found!");

builder.Services.AddHttpClient<HuggingFaceChatService>()
    .ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    });


builder.Services.AddHttpClient<IBotService, BotService>()
    .ConfigureHttpClient(client =>
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
