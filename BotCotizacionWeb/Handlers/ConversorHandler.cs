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
        string textoLimpio = text.Replace(".", "").Replace(",", ".");


        string nuevaConversion = "_Para realizar otra conversión, escribí_ /convertir";

        if (decimal.TryParse(textoLimpio, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto))
        {
            if (monto <= 0)
            {
                await botClient.SendMessage(chatId,
                    $"⚠️ Por favor ingresá un monto mayor a 0.",
                    ParseMode.Markdown, cancellationToken: token);
                return;
            }

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
                respuesta = $"🇦🇷 ${monto:N2} equivale a *USD {resultado:N2}*\n\n_Dólar blue venta → (${cotizacion.BlueVenta:N2})_\n\n{nuevaConversion}";
            }
            else
            {
                var resultado = monto * cotizacion.BlueCompra;
                respuesta = $"💵 USD {monto:N2} equivale a *${resultado:N2}*\n\n_Dólar blue compra → (${cotizacion.BlueCompra:N2})_\n\n{nuevaConversion}";
            }

            await botClient.SendMessage(chatId, respuesta, ParseMode.Markdown, cancellationToken: token);
            MessageHandler.EsperandoConversion.Remove(chatId);
        }
        else
        {
            string respuesta = $"❌ Por favor ingresá un monto válido";
            await botClient.SendMessage(chatId, respuesta, ParseMode.Markdown, cancellationToken: token);
        }
    }
}
