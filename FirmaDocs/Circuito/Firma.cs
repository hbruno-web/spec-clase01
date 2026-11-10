namespace FirmaDocs;

/// <summary>Aprobación registrada por un firmante sobre una versión del documento.</summary>
/// <param name="Firmante">Quién aprobó.</param>
/// <param name="Version">Versión del documento que aprobó.</param>
/// <param name="Instante">Momento de la aprobación (inyectado, nunca el reloj del sistema).</param>
public record Firma(Firmante Firmante, int Version, DateTimeOffset Instante);