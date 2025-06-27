using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public static class ConversorHandler
{
    public static async Task HandleConversorAsync(
        ITelegramBotClient botClient,
        long chatId,
        string text,
        string tipoConversion,
        CotizacionService cotizacionService,
        CancellationToken token)
    {
        if (decimal.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto))
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
    }
}
