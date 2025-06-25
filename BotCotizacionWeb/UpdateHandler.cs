using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public static class UpdateHandler
{
    public static async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Telegram.Bot.Types.Update update,
        CancellationToken token,
        SuscripcionesService suscripcionesService,
        CotizacionService cotizacionService)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text.ToLower();

            switch (text)
            {
                case "/start":
                    await EnviarMensajeInicio(botClient, chatId);
                    break;

                case "/activar":
                    if (suscripcionesService.EstaActivo(chatId))
                    {
                        await botClient.SendMessage(chatId, "⚠️ El mensaje automático ya está ACTIVADO.", cancellationToken: token);
                    }
                    else
                    {
                        suscripcionesService.Activar(chatId);
                        await botClient.SendMessage(chatId, "✅ Mensaje automático diario ACTIVADO. Recibirás notificaciones cuando la cotización cambie.", cancellationToken: token);
                    }
                    break;

                case "/cancelar":
                    suscripcionesService.Cancelar(chatId);
                    await botClient.SendMessage(chatId, "❌ Mensaje automático diario CANCELADO.", cancellationToken: token);
                    break;

                case "/dolar":
                    var cotizacion = await cotizacionService.ObtenerCotizacionAsync();
                    if (cotizacion != null)
                    {
                        var texto = CotizacionService.FormatearTextoCotizacion(cotizacion);
                        var buttons = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                             InlineKeyboardButton.WithCallbackData("Inicio", "start")
                            }
                        });

                        await botClient.SendMessage(chatId, texto, ParseMode.Markdown, replyMarkup: buttons, cancellationToken: token);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Error al obtener la cotización. Por favor intenta más tarde.", cancellationToken: token);
                    }
                    break;

                default:
                    await botClient.SendMessage(chatId, "No reconozco ese comando. Usa /start para ver opciones.", cancellationToken: token);
                    break;
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var callback = update.CallbackQuery!;
            var chatId = callback.Message!.Chat.Id;
            var data = callback.Data;

            switch (data)
            {
                case "start":
                    await EnviarMensajeInicio(botClient, chatId);
                    break;

                case "activar":
                    if (suscripcionesService.EstaActivo(chatId))
                    {
                        await botClient.AnswerCallbackQuery(callback.Id, "Ya está ACTIVADO");
                        await botClient.SendMessage(chatId, "⚠️ El mensaje automático ya está ACTIVADO.");
                    }
                    else
                    {
                        suscripcionesService.Activar(chatId);
                        await botClient.AnswerCallbackQuery(callback.Id, "Mensaje automático diario ACTIVADO");
                        await botClient.SendMessage(chatId, "✅ Mensaje automático diario ACTIVADO. Recibirás notificaciones cuando la cotización cambie.");
                    }
                    break;

                case "cancelar":
                    suscripcionesService.Cancelar(chatId);
                    await botClient.AnswerCallbackQuery(callback.Id, "Mensaje automático diario CANCELADO");
                    await botClient.SendMessage(chatId, "❌ Mensaje automático diario CANCELADO.");
                    break;

                case "dolar":
                    await botClient.AnswerCallbackQuery(callback.Id);
                    var cotizacionCB = await cotizacionService.ObtenerCotizacionAsync();
                    if (cotizacionCB != null)
                    {
                        var texto = CotizacionService.FormatearTextoCotizacion(cotizacionCB);
                        var buttons = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Inicio", "start")
                            }
                        });

                        await botClient.SendMessage(chatId, texto, ParseMode.Markdown, replyMarkup: buttons);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Error al obtener la cotización. Por favor intenta más tarde.");
                    }
                    break;


                default:
                    await botClient.AnswerCallbackQuery(callback.Id, "Opción desconocida");
                    break;
            }
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, System.Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }


    private static async Task EnviarMensajeInicio(ITelegramBotClient botClient, long chatId)
    {
        var buttons = new[]
        {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Activar automático", "activar"),
            InlineKeyboardButton.WithCallbackData("Cancelar automático", "cancelar"),
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("Cotización ahora", "dolar"),
            InlineKeyboardButton.WithCallbackData("Inicio", "start"),
        }
    };

        string texto =
         "👋 *Bienvenido al Bot de Cotización del Dólar en Argentina.*\n\n" +
         "Este bot te permite conocer la cotización actual del *dólar oficial* y del *dólar blue*.\n\n" +
         "También podés activar notificaciones automáticas para recibir alertas cuando los valores se actualicen.";

        await botClient.SendMessage(
            chatId: chatId,
            text: texto,
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

}
