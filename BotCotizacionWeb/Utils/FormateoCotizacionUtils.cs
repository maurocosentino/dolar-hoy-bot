namespace BotCotizacionWeb.Utils
{
    public class FormateoCotizacionUtils
    {
        public static string FormatearTextoCotizacion(CotizacionUltima cot, CotizacionUltima? anterior = null)
        {
            var ahoraAR = HoraUtils.ObtenerFechaHoraArgentina();

            return
                $"💵 *Dólar Blue - Cotización Actual*\n\n" +
                $"💸 → Compra: `${cot.BlueCompra}` {Variacion(cot.BlueCompra, anterior?.BlueCompra)}\n" +
                $"💰 → Venta: `${cot.BlueVenta}` {Variacion(cot.BlueVenta, anterior?.BlueVenta)}\n\n" +
                $"🔗 Fuente: [bluelytics.com.ar](https://bluelytics.com.ar)\n\n" +
                $"📅 _{ahoraAR:dd/MM/yyyy}_";

                //$"🏦 *Oficial*\n" +
                //$"   → Compra: `${cot.OficialCompra}` {Variacion(cot.OficialCompra, anterior?.OficialCompra)}\n" +
                //$"   → Venta: `${cot.OficialVenta}` {Variacion(cot.OficialVenta, anterior?.OficialVenta)}\n\n" +

        }
        public static string FormatearTextoAutomaticoCotizacion(CotizacionUltima cot, CotizacionUltima? anterior = null)
        {
            var ahoraAR = HoraUtils.ObtenerFechaHoraArgentina();

            return
                $"🔔 *Dólar Blue - Cotización Automática*\n\n" +
                $"💸 → Compra: `${cot.BlueCompra}` {Variacion(cot.BlueCompra, anterior?.BlueCompra)}\n" +
                $"💰 → Venta: `${cot.BlueVenta}` {Variacion(cot.BlueVenta, anterior?.BlueVenta)}\n\n" +
                $"🔗 Fuente: [bluelytics.com.ar](https://bluelytics.com.ar)\n\n" +
                $"📅 _{ahoraAR:dd/MM/yyyy HH:mm}_";

                //$"🏦 *Oficial*\n" +
                //$"   → Compra: `${cot.OficialCompra}` {Variacion(cot.OficialCompra, anterior?.OficialCompra)}\n" +
                //$"   → Venta: `${cot.OficialVenta}` {Variacion(cot.OficialVenta, anterior?.OficialVenta)}\n\n" +

        }
        private static string Variacion(decimal actual, decimal? previo)
        {
            if (previo == null || previo == 0) return "";
            var variacion = ((actual - previo.Value) / previo.Value) * 100;
            string simbolo = variacion > 0 ? "🔼" : variacion < 0 ? "🔽" : "➖";
            return $"{simbolo} ({variacion:+0.0;-0.0;0.0}%)";
        }
    }
}
