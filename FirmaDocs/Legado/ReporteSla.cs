namespace FirmaDocs.Legado;

// Reporte de nivel de servicio: horas promedio entre que se crea una
// solicitud y se resuelve, agrupado por mes de creación ("yyyy-MM").
public class ReporteSla
{
    // ESPERANDO_CERTIFICADO no cuenta: el reloj no corre hasta tener certificado.
    private static readonly string[] EstadosQueCuentan = { "PENDIENTE", "FIRMADA", "RECHAZADA", "EXPIRADA" };

    private readonly SolicitudFirmaRepository _repo;

    public ReporteSla(SolicitudFirmaRepository repo)
    {
        _repo = repo;
    }

    public Dictionary<string, double> HorasPromedioPorMes()
    {
        var horas = new Dictionary<string, double>();
        var cantidad = new Dictionary<string, int>();

        foreach (var s in _repo.Listar())
        {
            var texto = EstadoSolicitudTexto.ATexto(s.Estado);
            if (!EstadosQueCuentan.Contains(texto))
                continue;

            var mes = s.CreadaEn.ToString("yyyy-MM");
            if (!horas.ContainsKey(mes))
            {
                horas[mes] = 0;
                cantidad[mes] = 0;
            }

            if (s.ResueltaEn != null)
                horas[mes] += (s.ResueltaEn.Value - s.CreadaEn).TotalHours;

            cantidad[mes]++;
        }

        var promedio = new Dictionary<string, double>();
        foreach (var mes in horas.Keys)
        {
            promedio[mes] = Math.Round(horas[mes] / cantidad[mes], 2);
        }
        return promedio;
    }
}
