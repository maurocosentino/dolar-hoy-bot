using System.Threading;
using System.Threading.Tasks;
using BotCotizacionWeb.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public static class CallbackHandler
{
    public static async Task HandleCallbackAsync(
        ITelegramBotClient botClient,
        Telegram.Bot.Types.Update update,
        CancellationToken token,
        SuscripcionesService suscripcionesService,
        CotizacionService cotizacionService)
    {
        var callback = update.CallbackQuery!;
        var chatId = callback.Message!.Chat.Id;
        var data = callback.Data;

        switch (data)
        {
            case "start":
                await MensajeUtils.EnviarMensajeInicio(botClient, chatId);
                break;

            case "activar":
                if (await suscripcionesService.EstaActivoAsync(chatId))
                {
                    await botClient.AnswerCallbackQuery(callback.Id, "Ya está ACTIVADO");
                    await botClient.SendMessage(chatId, "⚠️ El mensaje automático ya está ACTIVADO.");
                }
                else
                {
                    await suscripcionesService.ActivarAsync(chatId);
                    await botClient.AnswerCallbackQuery(callback.Id, "Mensaje automático diario ACTIVADO");
                    await botClient.SendMessage(chatId, "✅ Mensaje automático diario ACTIVADO. Recibirás notificaciones cuando la cotización cambie.");
                }
                break;

            case "cancelar":
                await suscripcionesService.CancelarAsync(chatId);
                await botClient.AnswerCallbackQuery(callback.Id, "Mensaje automático diario CANCELADO");
                await botClient.SendMessage(chatId, "❌ Mensaje automático diario CANCELADO.");
                break;

            case "dolar":
                await botClient.AnswerCallbackQuery(callback.Id);
                var cotizacionCB = await cotizacionService.ObtenerCotizacionAsync();
                if (cotizacionCB != null)
                {
                    var texto = FormateoCotizacionUtils.FormatearTextoCotizacion(cotizacionCB);
                    await botClient.SendMessage(chatId, texto, ParseMode.Markdown);
                }
                else
                {
                    await botClient.SendMessage(chatId, "Error al obtener la cotización. Por favor intenta más tarde.");
                }
                break;

            case "pesos-a-dolar":
                await botClient.SendMessage(chatId, "💰 Ingresá el monto en *pesos argentinos* para convertir a dólar blue.", parseMode: ParseMode.Markdown);
                MessageHandler.EsperandoConversion[chatId] = "pesos-a-dolar";
                break;

            case "dolar-a-pesos":
                await botClient.SendMessage(chatId, "💵 Ingresá el monto en *dólares* que querés convertir a *pesos argentinos*.", parseMode: ParseMode.Markdown);
                MessageHandler.EsperandoConversion[chatId] = "dolar-a-pesos";
                break;

            case "mostrar_conversor":
                await botClient.AnswerCallbackQuery(callback.Id);
                var conversionButtons = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(" 💵 ARS a USD Blue", "pesos-a-dolar"),
                        InlineKeyboardButton.WithCallbackData(" 💵 USD Blue a ARS", "dolar-a-pesos")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("⬅️ Volver al inicio", "start")
                    }
                });

                string conversionTexto =
                    "💱 *Conversor de Moneda*\n\n" +
                    "Seleccioná una opción para convertir:";

                await botClient.SendMessage(
                    chatId,
                    conversionTexto,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: conversionButtons);
                break;

            default:
                await botClient.AnswerCallbackQuery(callback.Id, "Opción desconocida");
                break;
        }
    }
}
