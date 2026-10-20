using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DiasHabilesApi;

public class CalcularDiasHabiles
{
    private const string FormatoFecha = "yyyy-MM-dd";
    private readonly ILogger<CalcularDiasHabiles> _logger;

    public CalcularDiasHabiles(ILogger<CalcularDiasHabiles> logger)
    {
        _logger = logger;
    }

    [Function("CalcularDiasHabiles")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        DiasHabilesRequest? solicitud;

        try
        {
            solicitud = await req.ReadFromJsonAsync<DiasHabilesRequest>();
        }
        catch (JsonException)
        {
            return ErrorPeticion("El cuerpo de la petición no es un JSON válido.");
        }

        if (solicitud is null)
        {
            return ErrorPeticion("El cuerpo de la petición es obligatorio.");
        }

        if (!TryLeerFecha(solicitud.FechaInicio, out var fechaInicio))
        {
            return ErrorPeticion($"fechaInicio es obligatoria y debe tener el formato {FormatoFecha}.");
        }

        if (!TryLeerFecha(solicitud.FechaFin, out var fechaFin))
        {
            return ErrorPeticion($"fechaFin es obligatoria y debe tener el formato {FormatoFecha}.");
        }

        if (fechaFin < fechaInicio)
        {
            return ErrorPeticion("fechaFin no puede ser anterior a fechaInicio.");
        }

        var festivos = new List<DateOnly>();
        foreach (var texto in solicitud.Festivos ?? new List<string>())
        {
            if (!TryLeerFecha(texto, out var festivo))
            {
                return ErrorPeticion($"El festivo '{texto}' no tiene el formato {FormatoFecha}.");
            }

            festivos.Add(festivo);
        }

        var diasHabiles = CalculadoraDiasHabiles.Calcular(fechaInicio, fechaFin, festivos);

        _logger.LogInformation("Días hábiles entre {Inicio} y {Fin}: {Dias}", fechaInicio, fechaFin, diasHabiles);

        return new OkObjectResult(new DiasHabilesResponse(
            fechaInicio.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            fechaFin.ToString(FormatoFecha, CultureInfo.InvariantCulture),
            diasHabiles));
    }

    private static bool TryLeerFecha(string? texto, out DateOnly fecha)
    {
        fecha = default;
        return !string.IsNullOrWhiteSpace(texto)
            && DateOnly.TryParseExact(texto, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
    }

    private static BadRequestObjectResult ErrorPeticion(string mensaje)
    {
        return new BadRequestObjectResult(new { error = mensaje });
    }
}