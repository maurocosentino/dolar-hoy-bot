using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCotizacionWeb.Data;
using static Telegram.Bot.TelegramBotClient;

public static class UpdateHandler
{
    public static async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken token,
        SuscripcionesService suscripcionesService,
        CotizacionService cotizacionService,
        AppDbContext dbContext)
    {
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message?.Text != null)
        {
            await MessageHandler.HandleMessageAsync(botClient, update, token, suscripcionesService, cotizacionService, dbContext);
        }
        else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
        {
            await CallbackHandler.HandleCallbackAsync(botClient, update, token, suscripcionesService, cotizacionService);
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, System.Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
