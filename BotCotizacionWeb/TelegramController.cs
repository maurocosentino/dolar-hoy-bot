using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using BotCotizacionWeb.Data;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly SuscripcionesService _suscripcionesService;
    private readonly CotizacionService _cotizacionService;
    private readonly AppDbContext _dbContext;


    public TelegramController(ITelegramBotClient botClient,
                              SuscripcionesService suscripcionesService,
                              CotizacionService cotizacionService,
                              AppDbContext dbContext)
    {
        _botClient = botClient;
        _suscripcionesService = suscripcionesService;
        _cotizacionService = cotizacionService;
        _dbContext = dbContext;
       
    }

    [HttpPost("update")]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        await UpdateHandler.HandleUpdateAsync(_botClient, update, HttpContext.RequestAborted, _suscripcionesService, _cotizacionService,_dbContext);
        return Ok();
    }
}
