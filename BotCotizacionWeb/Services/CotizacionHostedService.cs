using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Threading;
using System.Threading.Tasks;
using BotCotizacionWeb.Data;

public class CotizacionHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private ITelegramBotClient _bot;
    private CancellationTokenSource _cts;


    public CotizacionHostedService(IServiceProvider services, ITelegramBotClient bot)
    {
        _services = services;
        _bot = bot;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Crear un scope para obtener servicios scoped
        var scope = _services.CreateScope();
        var suscripciones = scope.ServiceProvider.GetRequiredService<SuscripcionesService>();
        var cotizacionService = scope.ServiceProvider.GetRequiredService<CotizacionService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        cotizacionService.IniciarChequeoCotizacion(TimeSpan.FromMinutes(1));

        _bot.StartReceiving(
            new DefaultUpdateHandler(
                async (botClient, update, token) =>
                    await UpdateHandler.HandleUpdateAsync(botClient, update, token, suscripciones, cotizacionService,dbContext),
                async (botClient, exception, token) =>
                    await UpdateHandler.HandleErrorAsync(botClient, exception, token)),
            cancellationToken: _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}
