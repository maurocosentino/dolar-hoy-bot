using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BotCotizacionWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Suscripcion> Suscripciones => Set<Suscripcion>();
}
