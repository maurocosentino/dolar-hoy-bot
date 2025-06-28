using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotCotizacionWeb.Data;
using BotCotizacionWeb.Utils;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public static class MessageHandler
{
    private static readonly Dictionary<long, string> _esperandoConversion = new();

    public static async Task HandleMessageAsync(
        ITelegramBotClient botClient,
        Telegram.Bot.Types.Update update,
        CancellationToken token,
        SuscripcionesService suscripcionesService,
        CotizacionService cotizacionService,
        AppDbContext dbContext)
    {
        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text.ToLower();

        if (_esperandoConversion.TryGetValue(chatId, out var tipoConversion))
        {
            await ConversorHandler.HandleConversorAsync(botClient, chatId, text, tipoConversion, cotizacionService, token);
            _esperandoConversion.Remove(chatId);
            return;
        }

        switch (text)
        {
            case "/start":
                await MensajeUtils.EnviarMensajeInicio(botClient, chatId);
                if (update.Message.From is { } fromUser)
                {
                    if (!await dbContext.Usuarios.AnyAsync(u => u.Id == chatId, token))
                    {
                        var nuevoUsuario = new Usuario
                        {
                            Id = chatId,
                            Nombre = fromUser.Username ?? fromUser.FirstName ?? "Desconocido",
                            FechaRegistro = HoraUtils.ObtenerFechaHoraArgentina()
                        };
                        dbContext.Usuarios.Add(nuevoUsuario);
                        await dbContext.SaveChangesAsync(token);
                    }
                }
                break;

            case "/menu":
                await MensajeUtils.EnviarMensajeMenuPrincipal(botClient, chatId); // mensaje más corto, sin bienvenida
                break;

            case "/convertir":
                await MensajeUtils.EnviarMensajeConversor(botClient, chatId);
                break;

            case "/activar":
                if (await suscripcionesService.EstaActivoAsync(chatId))
                {
                    await botClient.SendMessage(chatId, "⚠️ El mensaje automático ya está ACTIVADO.", cancellationToken: token);
                }
                else
                {
                    await suscripcionesService.ActivarAsync(chatId);
                    await botClient.SendMessage(chatId, "✅ Mensaje automático diario ACTIVADO. Recibirás notificaciones cuando la cotización cambie.", cancellationToken: token);
                }
                break;

            case "/cancelar":
                await suscripcionesService.CancelarAsync(chatId);
                await botClient.SendMessage(chatId, "❌ Mensaje automático diario CANCELADO.", cancellationToken: token);
                break;

            case "/dolar":
                var cotizacion = await cotizacionService.ObtenerCotizacionAsync();
                if (cotizacion != null)
                {
                    var texto = FormateoCotizacionUtils.FormatearTextoCotizacion(cotizacion);
                    await botClient.SendMessage(chatId, texto, ParseMode.Markdown, cancellationToken: token);
                }
                else
                {
                    await botClient.SendMessage(chatId, "Error al obtener la cotización. Por favor intenta más tarde.", cancellationToken: token);
                }
                break;

            default:
                await botClient.SendMessage(chatId, "No reconozco ese comando. Usa /inicio para ver opciones.", cancellationToken: token);
                break;
        }
    }

    public static Dictionary<long, string> EsperandoConversion => _esperandoConversion;
}
