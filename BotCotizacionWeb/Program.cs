using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Obtener token de variables de entorno o appsettings.json
var botToken = builder.Configuration["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
if (string.IsNullOrEmpty(botToken))
{
    Console.WriteLine("No se pudo obtener el token del bot.");
    return;
}

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddSingleton<SuscripcionesService>();
builder.Services.AddSingleton<CotizacionService>();

var app = builder.Build();

var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
var suscripcionesService = app.Services.GetRequiredService<SuscripcionesService>();
var cotizacionService = new CotizacionService(botClient, suscripcionesService);

// Inicia el chequeo automático cada 1 minuto
cotizacionService.IniciarChequeoCotizacion(System.TimeSpan.FromMinutes(1));

// Configurar el manejador de updates para recibir mensajes
var cts = new CancellationTokenSource();
botClient.StartReceiving(
    updateHandler: new Telegram.Bot.Polling.DefaultUpdateHandler(
        async (bot, update, token) => await UpdateHandler.HandleUpdateAsync(bot, update, token, suscripcionesService, cotizacionService),
        async (bot, ex, token) => await UpdateHandler.HandleErrorAsync(bot, ex, token)),
    cancellationToken: cts.Token);

// Configurar un endpoint simple para que el servicio web responda (puede ser útil para Render)
app.MapGet("/", () => "Bot Cotización corriendo.");

app.Run();
