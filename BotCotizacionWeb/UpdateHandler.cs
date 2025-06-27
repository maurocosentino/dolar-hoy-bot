using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public static class UpdateHandler
{
    private static readonly Dictionary<long, string> _esperandoConversion = new();

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

            if (_esperandoConversion.TryGetValue(chatId, out var tipoConversion))
            {
                if (decimal.TryParse(text.Replace(",", "."), out decimal monto))
                {
                    var cotizacion = await cotizacionService.ObtenerCotizacionAsync();

                    if (cotizacion == null)
                    {
                        await botClient.SendMessage(chatId, "⚠️ No se pudo obtener la cotización del dólar. Intentalo más tarde.", cancellationToken: token);
                        return;
                    }

                    string respuesta;
                    if (tipoConversion == "pesos-a-dolar")
                    {
                        var resultado = monto / cotizacion.BlueVenta;
                        respuesta = $"🇦🇷 ${monto:N2} equivale a *USD {resultado:N2}* al dólar blue venta (${cotizacion.BlueVenta}).";
                    }
                    else // dolar-a-pesos
                    {
                        var resultado = monto * cotizacion.BlueVenta;
                        respuesta = $"💵 USD {monto:N2} equivale a *${resultado:N2}* al dólar blue venta (${cotizacion.BlueVenta}).";
                    }

                    await botClient.SendMessage(chatId, respuesta, ParseMode.Markdown, cancellationToken: token);
                }
                else
                {
                    await botClient.SendMessage(chatId, "❌ Por favor ingresá un monto válido (solo números).", cancellationToken: token);
                }

                _esperandoConversion.Remove(chatId);
                return;
            }


            switch (text)
            {
                case "/start":
                    await EnviarMensajeInicio(botClient, chatId);
                    break;

                case "/activar":
                    if (await suscripcionesService.EstaActivoAsync(chatId))
                    {
                        await botClient.SendMessage(chatId, "⚠️ El mensaje automático ya está ACTIVADO.", cancellationToken: token);
                    }
                    else
                    {
                        await suscripcionesService.ActivarAsync(chatId);
                        await botClient.SendMessage(chatId, "✅ Mensaje automático diario ACTIVADO. Recibirás notificaciones cuando la cotización cambie.", cancellationToken: token);
                    }
                    break;

                case "/cancelar":
                    await suscripcionesService.CancelarAsync(chatId);
                    await botClient.SendMessage(chatId, "❌ Mensaje automático diario CANCELADO.", cancellationToken: token);
                    break;

                case "/dolar":
                    var cotizacion = await cotizacionService.ObtenerCotizacionAsync();
                    if (cotizacion != null)
                    {
                        var texto = CotizacionService.FormatearTextoCotizacion(cotizacion);

                        await botClient.SendMessage(chatId, texto, ParseMode.Markdown,cancellationToken: token);
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
                        var texto = CotizacionService.FormatearTextoCotizacion(cotizacionCB);
                       
                        await botClient.SendMessage(chatId, texto, ParseMode.Markdown);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Error al obtener la cotización. Por favor intenta más tarde.");
                    }
                    break;

                case "pesos-a-dolar":
                    await botClient.SendMessage(chatId, "💰 Ingresá el monto en *pesos argentinos* para convertir a dólar blue.", parseMode: ParseMode.Markdown);
                    _esperandoConversion[chatId] = "pesos-a-dolar";
                    break;

                case "dolar-a-pesos":
                    await botClient.SendMessage(chatId, "💵 Ingresá el monto en *dólares* que querés convertir a pesos (blue).", parseMode: ParseMode.Markdown);
                    _esperandoConversion[chatId] = "dolar-a-pesos";
                    break;
                case "mostrar_conversor":
                    await botClient.AnswerCallbackQuery(callback.Id);

                    var conversionButtons = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {   
                        InlineKeyboardButton.WithCallbackData("Convertir ARS → Blue", "pesos-a-dolar"),
                        InlineKeyboardButton.WithCallbackData("Convertir Blue → ARS", "dolar-a-pesos")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔙 Volver al inicio", "start")
                    }
                    });

                    string conversionTexto =
                        "💱 *Conversor de Moneda*\n\n" +
                        "Seleccioná una opción para ingresar el monto a convertir:\n\n" +
                        "- ARS a Dólar Blue\n" +
                        "- Dólar Blue a ARS";

                    await botClient.SendMessage(
                        chatId,
                        conversionTexto,
                        parseMode: ParseMode.Markdown,
                        replyMarkup: conversionButtons
                    );
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
            InlineKeyboardButton.WithCallbackData("💱 Convertir", "mostrar_conversor"),
            InlineKeyboardButton.WithCallbackData("📊 Cotización ahora", "dolar"),
        }
    };

        string texto =
            "👋 *Bienvenido/a al Bot de Cotización del Dólar en Argentina.*\n\n" +
            "Este bot te permite:\n" +
            "- Ver cotizaciones en tiempo real.\n" +
            "- Convertir entre *ARS* y *Dólar Blue*.\n" +
            "- Recibir actualizaciones automáticas.\n\n" +
            "▶️ Usá los *botones* para interactuar. /start";

        await botClient.SendMessage(
            chatId: chatId,
            text: texto,
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
