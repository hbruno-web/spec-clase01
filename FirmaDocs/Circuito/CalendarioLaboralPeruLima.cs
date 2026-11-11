namespace FirmaDocs;

internal static class CalendarioLaboralPeruLima
{
    private static readonly TimeZoneInfo ZonaLima = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

    public static DateTimeOffset CalcularVencimiento(DateTimeOffset recibidoEn)
    {
        var recibidoLocal = TimeZoneInfo.ConvertTime(recibidoEn, ZonaLima);
        var fechaVencimiento = DateOnly.FromDateTime(recibidoLocal.DateTime);
        var diasHabiles = 0;

        while (diasHabiles < 2)
        {
            fechaVencimiento = fechaVencimiento.AddDays(1);
            if (EsDiaHabil(fechaVencimiento))
                diasHabiles++;
        }

        var vencimientoLocal = fechaVencimiento.ToDateTime(
            TimeOnly.FromTimeSpan(recibidoLocal.TimeOfDay),
            DateTimeKind.Unspecified);
        var vencimientoUtc = TimeZoneInfo.ConvertTimeToUtc(vencimientoLocal, ZonaLima);

        return new DateTimeOffset(vencimientoUtc);
    }

    private static bool EsDiaHabil(DateOnly fecha) =>
        fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday
        && !FeriadosOficiales(fecha.Year).Contains(fecha);

    private static HashSet<DateOnly> FeriadosOficiales(int year)
    {
        var domingoPascua = CalcularDomingoPascua(year);
        return new HashSet<DateOnly>
        {
            new DateOnly(year, 1, 1),
            domingoPascua.AddDays(-3),
            domingoPascua.AddDays(-2),
            new DateOnly(year, 5, 1),
            new DateOnly(year, 6, 7),
            new DateOnly(year, 6, 29),
            new DateOnly(year, 7, 23),
            new DateOnly(year, 7, 28),
            new DateOnly(year, 7, 29),
            new DateOnly(year, 8, 6),
            new DateOnly(year, 8, 30),
            new DateOnly(year, 10, 8),
            new DateOnly(year, 11, 1),
            new DateOnly(year, 12, 8),
            new DateOnly(year, 12, 9),
            new DateOnly(year, 12, 25)
        };
    }

    private static DateOnly CalcularDomingoPascua(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var mes = (h + l - 7 * m + 114) / 31;
        var dia = (h + l - 7 * m + 114) % 31 + 1;

        return new DateOnly(year, mes, dia);
    }
}
