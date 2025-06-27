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
            "▶️ Usá los *botones* para interactuar. /start";

        await botClient.SendMessage(
            chatId: chatId,
            text: texto,
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
