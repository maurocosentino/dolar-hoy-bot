using BotCotizacionWeb.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=suscripciones.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<SuscripcionesService>();
builder.Services.AddScoped<CotizacionService>();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var token = builder.Configuration["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
    return new TelegramBotClient(token);
});

builder.Services.AddHostedService<CotizacionHostedService>();

var app = builder.Build();

app.MapGet("/ping", () => "Bot activo");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}


app.Run();
