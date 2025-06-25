namespace BotCotizacionWeb.Data;

public class Suscripcion
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaSuscripcion { get; set; } = DateTime.UtcNow;
}
