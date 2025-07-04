﻿using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Threading;

// Carga configuración desde JSON (solo local) y variables de entorno (para Render)
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true) // optional:true evita error en producción
    .AddEnvironmentVariables()
    .Build();

// Intenta obtener el token desde JSON y luego desde las variables de entorno
var botToken = config["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("BOT_TOKEN");
var chatId = config["Telegram:ChatId"] ?? Environment.GetEnvironmentVariable("CHAT_ID");


if (string.IsNullOrEmpty(botToken))
{
    Console.WriteLine("No se pudo obtener el token del bot.");
    return;
}

var bot = new TelegramBotClient(botToken);
var suscripcionesService = new SuscripcionesService();
var cotizacionService = new CotizacionService(bot, suscripcionesService);
cotizacionService.IniciarChequeoCotizacion(TimeSpan.FromMinutes(1));

using var cts = new CancellationTokenSource();

bot.StartReceiving(
    updateHandler: new DefaultUpdateHandler(
        async (botClient, update, token) =>
            await UpdateHandler.HandleUpdateAsync(botClient, update, token, suscripcionesService, cotizacionService),
        async (botClient, exception, token) =>
            await UpdateHandler.HandleErrorAsync(botClient, exception, token)),
    receiverOptions: new ReceiverOptions(),
    cancellationToken: cts.Token);

Console.WriteLine("Bot en ejecución...");
await Task.Delay(Timeout.Infinite); // mantiene el proceso vivo