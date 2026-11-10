namespace FirmaDocs;

/// <summary>
/// Circuito de firma secuencial: los firmantes aprueban uno tras otro,
/// en el orden en que se definieron.
/// </summary>
public sealed class CircuitoFirma
{
    private readonly List<Firmante> _firmantes;
    private readonly List<Firma> _firmas = [];

    /// <summary>Crea el circuito de un documento en estado <see cref="EstadoDocumento.Borrador"/>.</summary>
    /// <param name="documentoId">Identificador del documento.</param>
    /// <param name="autorId">Identificador del autor del documento.</param>
    /// <param name="firmantes">Firmantes en orden de firma; al menos uno y sin repetidos.</param>
    /// <exception cref="CircuitoFirmaException">No hay firmantes o hay repetidos.</exception>
    public CircuitoFirma(string documentoId, string autorId, IReadOnlyList<Firmante> firmantes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentNullException.ThrowIfNull(firmantes);

        if (firmantes.Count == 0)
            throw new CircuitoFirmaException("El circuito necesita al menos un firmante.");
        if (firmantes.Select(f => f.Id).Distinct().Count() != firmantes.Count)
            throw new CircuitoFirmaException("Un firmante no puede repetirse en el circuito.");

        DocumentoId = documentoId;
        AutorId = autorId;
        _firmantes = [.. firmantes];
    }

    /// <summary>Identificador del documento.</summary>
    public string DocumentoId { get; }

    /// <summary>Identificador del autor.</summary>
    public string AutorId { get; }

    /// <summary>Versión vigente del documento. Empieza en 1.</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Estado actual del documento.</summary>
    public EstadoDocumento Estado { get; private set; } = EstadoDocumento.Borrador;

    /// <summary>Firmantes en orden de firma.</summary>
    public IReadOnlyList<Firmante> Firmantes => _firmantes;

    /// <summary>Historial de aprobaciones, en orden cronológico.</summary>
    public IReadOnlyList<Firma> Firmas => _firmas;

    /// <summary>Firmante que debe aprobar ahora; <c>null</c> si el documento no está en circuito.</summary>
    public Firmante? FirmanteEnTurno =>
        Estado == EstadoDocumento.EnCircuito ? _firmantes[_firmas.Count] : null;

    /// <summary>Envía el documento a firma.</summary>
    /// <exception cref="CircuitoFirmaException">El documento no está en borrador.</exception>
    public void Enviar()
    {
        if (Estado != EstadoDocumento.Borrador)
            throw new CircuitoFirmaException("Solo se puede enviar a firma un documento en borrador.");

        Estado = EstadoDocumento.EnCircuito;
    }

    /// <summary>Registra la aprobación del firmante en turno.</summary>
    /// <param name="firmante">Firmante que aprueba.</param>
    /// <param name="instante">Momento de la aprobación.</param>
    /// <exception cref="CircuitoFirmaException">El documento no está en circuito o no es el turno del firmante.</exception>
    public void Aprobar(Firmante firmante, DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(firmante);

        if (Estado != EstadoDocumento.EnCircuito)
            throw new CircuitoFirmaException("El documento no está en circuito de firma.");
        if (FirmanteEnTurno!.Id != firmante.Id)
            throw new CircuitoFirmaException($"No es el turno de {firmante.Nombre}.");

        _firmas.Add(new Firma(firmante, Version, instante));

        if (_firmas.Count == _firmantes.Count)
            Estado = EstadoDocumento.Aprobado;
    }
}