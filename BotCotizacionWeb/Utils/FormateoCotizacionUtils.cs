namespace BotCotizacionWeb.Utils
{
    public class FormateoCotizacionUtils
    {
        public static string FormatearTextoCotizacion(CotizacionUltima cot, CotizacionUltima? anterior = null)
        {
            var ahoraAR = HoraUtils.ObtenerFechaHoraArgentina();

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
            var ahoraAR = HoraUtils.ObtenerFechaHoraArgentina();

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
    }
}
