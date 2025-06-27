using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Timers;
using Timer = System.Timers.Timer;
using BotCotizacionWeb.Utils;

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
                    FormateoCotizacionUtils.FormatearTextoAutomaticoCotizacion(cotizacionNueva, cotizacionAnterior) +
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

