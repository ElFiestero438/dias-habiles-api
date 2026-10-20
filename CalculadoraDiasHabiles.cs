namespace DiasHabilesApi;

public static class CalculadoraDiasHabiles
{
    public static int Calcular(DateOnly fechaInicio, DateOnly fechaFin, IEnumerable<DateOnly> festivos)
    {
        var festivosSet = festivos.ToHashSet();
        var diasHabiles = 0;

        for (var dia = fechaInicio; dia <= fechaFin; dia = dia.AddDays(1))
        {
            if (EsDiaHabil(dia, festivosSet))
            {
                diasHabiles++;
            }
        }

        return diasHabiles;
    }

    private static bool EsDiaHabil(DateOnly dia, HashSet<DateOnly> festivos)
    {
        return dia.DayOfWeek != DayOfWeek.Sunday && !festivos.Contains(dia);
    }
}