using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotCotizacionWeb.Data;
using Microsoft.EntityFrameworkCore;

public class SuscripcionesService
{
    private readonly AppDbContext _context;

    public SuscripcionesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ActivarAsync(long chatId)
    {
        var suscripcion = await _context.Suscripciones.SingleOrDefaultAsync(s => s.ChatId == chatId);
        if (suscripcion == null)
        {
            suscripcion = new Suscripcion
            {
                ChatId = chatId,
                Activo = true,
                FechaSuscripcion = DateTime.UtcNow
            };
            await _context.Suscripciones.AddAsync(suscripcion);
        }
        else
        {
            suscripcion.Activo = true;
            _context.Suscripciones.Update(suscripcion);
        }
        await _context.SaveChangesAsync();
    }

    public async Task CancelarAsync(long chatId)
    {
        var suscripcion = await _context.Suscripciones.SingleOrDefaultAsync(s => s.ChatId == chatId);
        if (suscripcion != null)
        {
            suscripcion.Activo = false;
            _context.Suscripciones.Update(suscripcion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> EstaActivoAsync(long chatId)
    {
        var suscripcion = await _context.Suscripciones.SingleOrDefaultAsync(s => s.ChatId == chatId);
        return suscripcion != null && suscripcion.Activo;
    }

    public async Task<List<Suscripcion>> ObtenerSuscripcionesActivasAsync()
    {
        return await _context.Suscripciones.Where(s => s.Activo).ToListAsync();
    }
}
