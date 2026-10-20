using System.Text.Json.Serialization;

namespace DiasHabilesApi;

public class DiasHabilesRequest
{
    [JsonPropertyName("fechaInicio")]
    public string? FechaInicio { get; set; }

    [JsonPropertyName("fechaFin")]
    public string? FechaFin { get; set; }

    [JsonPropertyName("festivos")]
    public List<string>? Festivos { get; set; }
}

public record DiasHabilesResponse(
    [property: JsonPropertyName("fechaInicio")] string FechaInicio,
    [property: JsonPropertyName("fechaFin")] string FechaFin,
    [property: JsonPropertyName("diasHabiles")] int DiasHabiles);