using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Timers;
using Timer = System.Timers.Timer;

public class CotizacionService
{
    private readonly ITelegramBotClient _bot;
    private readonly SuscripcionesService _suscripciones;
    private CotizacionUltima? _ultimaCotizacion;
    private Timer? _timer;

    public CotizacionService(ITelegramBotClient bot, SuscripcionesService suscripciones)
    {
        _bot = bot;
        _suscripciones = suscripciones;
    }

    public void IniciarChequeoCotizacion(TimeSpan intervalo)
    {
        _timer = new Timer(intervalo.TotalMilliseconds);
        _timer.AutoReset = true;
        _timer.Elapsed += async (_, __) => await ChequearYNotificarAsync();
        _timer.Start();
    }

    public async Task ChequearYNotificarAsync()
    {
        try
        {
            var cotizacionNueva = await ObtenerCotizacionAsync();

            if (cotizacionNueva == null)
                return;

            if (_ultimaCotizacion == null || !cotizacionNueva.Equals(_ultimaCotizacion))
            {
                var cotizacionAnterior = _ultimaCotizacion;
                _ultimaCotizacion = cotizacionNueva;

                var texto =        
                    FormatearTextoAutomaticoCotizacion(cotizacionNueva, cotizacionAnterior) +
                    "\n\nUsa los botones para activar o cancelar el mensaje automático.  /start";

                var suscripcionesActivas = await _suscripciones.ObtenerSuscripcionesActivasAsync();

                foreach (var suscripcion in suscripcionesActivas)
                {
                    long chatId = suscripcion.ChatId;
                    try
                    {
                        await _bot.SendMessage(chatId, texto, ParseMode.Markdown);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error enviando a {chatId}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error chequeando cotización: {ex.Message}");
        }
    }


    public async Task<CotizacionUltima?> ObtenerCotizacionAsync()
    {
        try
        {
            using var http = new HttpClient();
            var response = await http.GetStringAsync("https://api.bluelytics.com.ar/v2/latest");

            var json = JsonDocument.Parse(response);
            var oficial = json.RootElement.GetProperty("oficial");
            var blue = json.RootElement.GetProperty("blue");

            return new CotizacionUltima
            {
                OficialCompra = oficial.GetProperty("value_buy").GetDecimal(),
                OficialVenta = oficial.GetProperty("value_sell").GetDecimal(),
                BlueCompra = blue.GetProperty("value_buy").GetDecimal(),
                BlueVenta = blue.GetProperty("value_sell").GetDecimal()
            };
        }
        catch
        {
            return null;
        }
    }

    public static string FormatearTextoCotizacion(CotizacionUltima cot, CotizacionUltima? anterior = null)
    {
        var ahoraAR = ObtenerFechaHoraArgentina();

        return
            $"💵 *Dólar en Argentina - Cotización Actual*\n\n" +

            $"📊 *Blue*\n" +
            $"   → Compra: `${cot.BlueCompra}` {Variacion(cot.BlueCompra, anterior?.BlueCompra)}\n" +
            $"   → Venta: `${cot.BlueVenta}` {Variacion(cot.BlueVenta, anterior?.BlueVenta)}\n\n" +

            $"🏦 *Oficial*\n" +
            $"   → Compra: `${cot.OficialCompra}` {Variacion(cot.OficialCompra, anterior?.OficialCompra)}\n" +
            $"   → Venta: `${cot.OficialVenta}` {Variacion(cot.OficialVenta, anterior?.OficialVenta)}\n\n" +

            $"🕒 _Actualizado: {ahoraAR:dd/MM/yyyy}_";
    }
    //corregir mensaje automatico/
    public static string FormatearTextoAutomaticoCotizacion(CotizacionUltima cot, CotizacionUltima? anterior = null)
    {
        var ahoraAR = ObtenerFechaHoraArgentina();

        return
            $"🔔 *Dólar Blue y Oficial (Argentina) - Cotización Automática*\n\n" +

            $"📊 *Blue*\n" +
            $"   → Compra: `${cot.BlueCompra}` {Variacion(cot.BlueCompra, anterior?.BlueCompra)}\n" +
            $"   → Venta: `${cot.BlueVenta}` {Variacion(cot.BlueVenta, anterior?.BlueVenta)}\n\n" +

            $"🏦 *Oficial*\n" +
            $"   → Compra: `${cot.OficialCompra}` {Variacion(cot.OficialCompra, anterior?.OficialCompra)}\n" +
            $"   → Venta: `${cot.OficialVenta}` {Variacion(cot.OficialVenta, anterior?.OficialVenta)}\n\n" +

            $"⏰ *Actualizado:* {ahoraAR:dd/MM/yyyy HH:mm}";
    }
    private static string Variacion(decimal actual, decimal? previo)
    {
        if (previo == null || previo == 0) return "";
        var variacion = ((actual - previo.Value) / previo.Value) * 100;
        string simbolo = variacion > 0 ? "🔼" : variacion < 0 ? "🔽" : "➖";
        return $"{simbolo} ({variacion:+0.0;-0.0;0.0}%)";
    }


    public static string Flecha(decimal actual, decimal? previo) =>
           previo == null ? "" :
           actual > previo ? " 🔺" :
           actual < previo ? " 🔻" : "➖";


    public static DateTime ObtenerFechaHoraArgentina()
    {
        string zonaId = OperatingSystem.IsWindows()
            ? "Argentina Standard Time"
            : "America/Argentina/Buenos_Aires";

        var zonaAR = TimeZoneInfo.FindSystemTimeZoneById(zonaId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaAR);
    }
}

public class CotizacionUltima
{
    public decimal OficialCompra { get; set; }
    public decimal OficialVenta { get; set; }
    public decimal BlueCompra { get; set; }
    public decimal BlueVenta { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not CotizacionUltima other) return false;
        return OficialCompra == other.OficialCompra
            && OficialVenta == other.OficialVenta
            && BlueCompra == other.BlueCompra
            && BlueVenta == other.BlueVenta;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OficialCompra, OficialVenta, BlueCompra, BlueVenta);
    }
    

}

