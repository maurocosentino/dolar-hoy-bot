using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Timers;
using System.Threading;

// Carga configuración
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string botToken = config["Telegram:BotToken"];
var bot = new TelegramBotClient(botToken);

// Inicia servicio de suscripciones
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

Console.WriteLine("Bot en ejecución. Presioná Enter para salir.");
Console.ReadLine();

cts.Cancel();
