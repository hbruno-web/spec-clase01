namespace FirmaDocs;

/// <summary>Error de dominio del circuito de firma. El mensaje está en español y se muestra al usuario.</summary>
/// <param name="mensaje">Descripción del error.</param>
public class CircuitoFirmaException(string mensaje) : Exception(mensaje);