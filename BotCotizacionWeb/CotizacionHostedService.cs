using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CotizacionHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(1);

    public CotizacionHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var cotizacionService = scope.ServiceProvider.GetRequiredService<CotizacionService>();
            await cotizacionService.ChequearYNotificarAsync();

            await Task.Delay(_intervalo, stoppingToken);
        }
    }
}
