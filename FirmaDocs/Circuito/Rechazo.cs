namespace FirmaDocs;

/// <summary>Rechazo registrado sobre una versión del documento.</summary>
/// <param name="Firmante">Quién rechazó el documento.</param>
/// <param name="Motivo">Motivo conservado exactamente como fue ingresado.</param>
/// <param name="Version">Versión del documento que fue rechazada.</param>
/// <param name="Instante">Momento del rechazo, proporcionado por quien invoca la operación.</param>
public record Rechazo(Firmante Firmante, string Motivo, int Version, DateTimeOffset Instante);
