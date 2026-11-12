namespace FirmaDocs.Legado;

// Envía recordatorios a los firmantes con solicitudes abiertas.
public class RecordatorioService
{
    private readonly SolicitudFirmaRepository _repo;
    private readonly ICanalRecordatorio _canal;

    public RecordatorioService(SolicitudFirmaRepository repo, ICanalRecordatorio canal)
    {
        _repo = repo;
        _canal = canal;
    }

    // Devuelve cuántos recordatorios se enviaron.
    public int EnviarRecordatorios(DateTimeOffset ahora)
    {
        var enviados = 0;

        foreach (var s in _repo.Listar(EstadoSolicitud.Pendiente))
        {
            if ((ahora - s.CreadaEn).TotalDays > 2)
            {
                // TODO: plantilla configurable
                _canal.Enviar(s.FirmanteEmail, "Tienes un documento por firmar: " + s.DocumentoId);
                enviados++;
            }
        }

        foreach (var s in _repo.Listar(EstadoSolicitud.EsperandoCertificado))
        {
            // a estos se les avisa siempre, sin importar la antigüedad
            _canal.Enviar(s.FirmanteEmail, "Instala tu certificado digital para firmar: " + s.DocumentoId);
            enviados++;
        }

        return enviados;
    }
}
