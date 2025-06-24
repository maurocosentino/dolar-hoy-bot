using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class SuscripcionesService
{
    private const string ArchivoSuscripciones = "suscripciones.json";

    public Dictionary<long, bool> Suscripciones { get; private set; }

    public SuscripcionesService()
    {
        Suscripciones = CargarSuscripciones();
    }

    public void Activar(long chatId)
    {
        Suscripciones[chatId] = true;
        GuardarSuscripciones();
    }

    public void Cancelar(long chatId)
    {
        Suscripciones[chatId] = false;
        GuardarSuscripciones();
    }

    public bool EstaActivo(long chatId)
    {
        return Suscripciones.TryGetValue(chatId, out bool activo) && activo;
    }

    private void GuardarSuscripciones()
    {
        var json = JsonSerializer.Serialize(Suscripciones, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ArchivoSuscripciones, json);
    }

    private Dictionary<long, bool> CargarSuscripciones()
    {
        if (!File.Exists(ArchivoSuscripciones))
            return new Dictionary<long, bool>();

        try
        {
            string contenido = File.ReadAllText(ArchivoSuscripciones);
            return JsonSerializer.Deserialize<Dictionary<long, bool>>(contenido) ?? new Dictionary<long, bool>();
        }
        catch
        {
            Console.WriteLine("⚠️ Error al leer suscripciones. Se iniciará vacío.");
            return new Dictionary<long, bool>();
        }
    }
}
