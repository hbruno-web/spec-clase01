namespace FirmaDocs;

/// <summary>Persona que debe firmar un documento.</summary>
/// <param name="Id">Identificador único del firmante.</param>
/// <param name="Nombre">Nombre visible en mensajes y reportes.</param>
public record Firmante(string Id, string Nombre);