namespace FirmaDocs.Legado;

public record Bandeja(
    List<SolicitudFirma> PorFirmar,
    List<SolicitudFirma> EsperandoCertificado,
    List<SolicitudFirma> Historial);

// Arma la bandeja que ve un firmante al entrar a FirmaSimple.
public class BandejaFirmante
{
    private readonly SolicitudFirmaRepository _repo;

    public BandejaFirmante(SolicitudFirmaRepository repo)
    {
        _repo = repo;
    }

    public Bandeja Armar(string email)
    {
        var porFirmar = new List<SolicitudFirma>();
        var esperando = new List<SolicitudFirma>();
        var historial = new List<SolicitudFirma>();

        foreach (var s in _repo.ListarPorFirmante(email))
        {
            if (s.Estado == EstadoSolicitud.Pendiente)
                porFirmar.Add(s);
            else
                historial.Add(s);
        }

        return new Bandeja(porFirmar, esperando, historial);
    }
}
