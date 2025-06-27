namespace BotCotizacionWeb.Utils
{
    public static class HoraUtils
    {
        public static DateTime ObtenerFechaHoraArgentina()
        {
            string zonaId = OperatingSystem.IsWindows()
            ? "Argentina Standard Time"
            : "America/Argentina/Buenos_Aires";

            var zonaAR = TimeZoneInfo.FindSystemTimeZoneById(zonaId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaAR);
        }
    }

}
