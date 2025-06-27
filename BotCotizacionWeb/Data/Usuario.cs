namespace BotCotizacionWeb.Data
{
    public class Usuario
    {
        public long Id { get; set; }
        public string? Nombre { get; set; } 
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }

}
