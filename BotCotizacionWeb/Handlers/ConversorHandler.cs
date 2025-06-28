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
                respuesta = $"🇦🇷 ${monto:N2} equivale a *USD {resultado:N2}*\n\n _Dólar blue venta → (${cotizacion.BlueVenta})_\n\n_Para realizar otra conversión, escribí_ /convertir";
            }
            else // dolar-a-pesos
            {
                var resultado = monto * cotizacion.BlueCompra;
                respuesta = $"💵 USD {monto:N2} equivale a *${resultado:N2}*\n\n_Dólar blue compra → (${cotizacion.BlueCompra})_\n\n_Para realizar otra conversión, escribí_ /convertir";
            }

            await botClient.SendMessage(chatId, respuesta, ParseMode.Markdown, cancellationToken: token);
        }
        else
        {
            string respuesta;
            respuesta = $"❌ Por favor ingresá un monto válido (solo números)\n\n_Para realizar otra conversión, escribí_ /convertir";
            await botClient.SendMessage(chatId, respuesta, ParseMode.Markdown, cancellationToken: token);
        }
    }
}
