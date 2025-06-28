// Archivo: MensajeUtils.cs
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public static class MensajeUtils
{
    public static async Task EnviarMensajeInicio(ITelegramBotClient botClient, long chatId)
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
            "▶️ Usá los *botones* para interactuar.";

        await botClient.SendMessage(
            chatId: chatId,
            text: texto,
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    public static async Task EnviarMensajeMenuPrincipal(ITelegramBotClient botClient, long chatId)
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
            "📋 *Menú principal*\n\nSeleccione una opción para consultar la cotización, activar notificaciones o utilizar el conversor.";

        await botClient.SendMessage(
            chatId: chatId,
            text: texto,
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons));

    }
    public static async Task EnviarMensajeConversor(ITelegramBotClient botClient, long chatId)
    {
        var conversionButtons = new InlineKeyboardMarkup(new[]
        {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(" 💵 ARS a USD Blue", "pesos-a-dolar"),
            InlineKeyboardButton.WithCallbackData(" 💵 USD Blue a ARS", "dolar-a-pesos")
        },
        new[]
        {
            InlineKeyboardButton.WithCallbackData("⬅️ Volver al inicio", "menu")
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
    }

}
