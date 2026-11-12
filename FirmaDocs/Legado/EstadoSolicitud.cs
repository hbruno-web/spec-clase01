namespace FirmaDocs.Legado;

// Estados de una solicitud de firma en FirmaSimple.
// Los textos deben coincidir con el CHECK de db/schema.sql.
public enum EstadoSolicitud
{
    Pendiente,
    Firmada,
    Rechazada,
    Expirada
}

public static class EstadoSolicitudTexto
{
    public static string ATexto(EstadoSolicitud estado)
    {
        switch (estado)
        {
            case EstadoSolicitud.Pendiente: return "PENDIENTE";
            case EstadoSolicitud.Firmada: return "FIRMADA";
            case EstadoSolicitud.Rechazada: return "RECHAZADA";
            case EstadoSolicitud.Expirada: return "EXPIRADA";
            default: throw new ArgumentOutOfRangeException(nameof(estado));
        }
    }

    public static EstadoSolicitud DesdeTexto(string texto)
    {
        switch (texto)
        {
            case "PENDIENTE": return EstadoSolicitud.Pendiente;
            case "FIRMADA": return EstadoSolicitud.Firmada;
            case "RECHAZADA": return EstadoSolicitud.Rechazada;
            case "EXPIRADA": return EstadoSolicitud.Expirada;
            default: throw new ArgumentException("Estado desconocido: " + texto, nameof(texto));
        }
    }
}
