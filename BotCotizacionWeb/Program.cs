using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using BotCotizacionWeb.Data;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=suscripciones.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<SuscripcionesService>();
builder.Services.AddScoped<CotizacionService>();

builder.Services.AddHostedService<CotizacionHostedService>(); // bot activo siempre

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var config = builder.Configuration;
    var token = config["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
    return new TelegramBotClient(token);
});

var app = builder.Build();

app.MapGet("/ping", () => "Bot activo");

// Start bot polling
var bot = app.Services.GetRequiredService<ITelegramBotClient>();
var scope = app.Services.CreateScope();
var suscripciones = scope.ServiceProvider.GetRequiredService<SuscripcionesService>();
var cotizacionService = scope.ServiceProvider.GetRequiredService<CotizacionService>();

var cts = new CancellationTokenSource();

bot.StartReceiving(
    new DefaultUpdateHandler(
        async (botClient, update, token) =>
            await UpdateHandler.HandleUpdateAsync(botClient, update, token, suscripciones, cotizacionService),
        async (botClient, exception, token) =>
            await UpdateHandler.HandleErrorAsync(botClient, exception, token)),
    cancellationToken: cts.Token);

Console.WriteLine("Bot en ejecución...");
app.Run();
