namespace FirmaDocs;

/// <summary>
/// Circuito de firma compuesto por etapas secuenciales o paralelas.
/// </summary>
public sealed class CircuitoFirma
{
    private const int MaximoVueltas = 3;
    private readonly IReadOnlyList<EtapaFirma> _etapas;
    private readonly List<Firmante> _firmantes;
    private readonly List<Firma> _firmas = [];
    private readonly List<Rechazo> _rechazos = [];
    private readonly Dictionary<string, DateTimeOffset> _turnosActivos = new(StringComparer.Ordinal);

    /// <summary>Crea el circuito de un documento en estado <see cref="EstadoDocumento.Borrador"/>.</summary>
    /// <param name="documentoId">Identificador del documento.</param>
    /// <param name="autorId">Identificador del autor del documento.</param>
    /// <param name="firmantes">Firmantes en orden de firma; al menos uno y sin repetidos.</param>
    /// <exception cref="CircuitoFirmaException">No hay firmantes o hay repetidos.</exception>
    public CircuitoFirma(string documentoId, string autorId, IReadOnlyList<Firmante> firmantes)
        : this(documentoId, autorId, [new EtapaFirma(firmantes, ModoEtapaFirma.SECUENCIAL)])
    {
    }

    private CircuitoFirma(string documentoId, string autorId, EtapaFirma[] etapas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(autorId);
        ArgumentNullException.ThrowIfNull(etapas);

        if (etapas.Length is < 1 or > 5)
            throw new CircuitoFirmaException("El circuito debe tener entre 1 y 5 etapas.");
        if (etapas.Any(etapa => etapa is null))
            throw new CircuitoFirmaException("El circuito no puede contener etapas nulas.");

        var firmantes = etapas.SelectMany(etapa => etapa.Firmantes).ToArray();
        if (firmantes.Select(f => f.Id).Distinct().Count() != firmantes.Length)
            throw new CircuitoFirmaException("Un firmante no puede repetirse en el circuito.");

        DocumentoId = documentoId;
        AutorId = autorId;
        _etapas = Array.AsReadOnly(etapas.ToArray());
        _firmantes = [.. firmantes];
    }

    /// <summary>Crea un circuito configurado con entre 1 y 5 etapas ordenadas.</summary>
    /// <param name="documentoId">Identificador del documento.</param>
    /// <param name="autorId">Identificador del autor del documento.</param>
    /// <param name="etapas">Etapas ordenadas del circuito.</param>
    /// <returns>El circuito configurado.</returns>
    /// <exception cref="CircuitoFirmaException">La cantidad de etapas está fuera de límites o un firmante está repetido.</exception>
    public static CircuitoFirma CrearConEtapas(
        string documentoId,
        string autorId,
        IReadOnlyList<EtapaFirma> etapas)
    {
        ArgumentNullException.ThrowIfNull(etapas);
        return new CircuitoFirma(documentoId, autorId, etapas.ToArray());
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

    /// <summary>Etapas del circuito en el orden configurado.</summary>
    public IReadOnlyList<EtapaFirma> Etapas => _etapas;

    /// <summary>Historial de aprobaciones, en orden cronológico.</summary>
    public IReadOnlyList<Firma> Firmas => _firmas;

    /// <summary>Historial de rechazos, separado de las aprobaciones y en orden cronológico.</summary>
    public IReadOnlyList<Rechazo> Rechazos => _rechazos;

    /// <summary>Firmante que debe aprobar ahora en una etapa secuencial; <c>null</c> si no hay turno único.</summary>
    public Firmante? FirmanteEnTurno =>
        Estado == EstadoDocumento.EnCircuito && EtapaActiva?.Modo == ModoEtapaFirma.SECUENCIAL
            ? FirmantesPendientes[0]
            : null;

    /// <summary>Firmantes habilitados para aprobar en la etapa activa de la versión vigente.</summary>
    public IReadOnlyList<Firmante> FirmantesPendientes
    {
        get
        {
            if (Estado != EstadoDocumento.EnCircuito || EtapaActiva is not { } etapaActiva)
                return Array.Empty<Firmante>();

            var aprobados = FirmantesAprobadosEnVersionVigente();
            var pendientes = etapaActiva.Firmantes
                .Where(firmante => !aprobados.Contains(firmante.Id))
                .ToArray();

            return etapaActiva.Modo == ModoEtapaFirma.SECUENCIAL
                ? pendientes.Take(1).ToArray()
                : pendientes;
        }
    }

    /// <summary>Envía el documento a firma.</summary>
    /// <exception cref="CircuitoFirmaException">El documento no está en borrador.</exception>
    public void Enviar()
    {
        if (Estado != EstadoDocumento.Borrador)
            throw new CircuitoFirmaException("Solo se puede enviar a firma un documento en borrador.");

        Estado = EstadoDocumento.EnCircuito;
    }

    /// <summary>Envía el documento a firma y registra la recepción del primer turno.</summary>
    /// <param name="instante">Momento en que los firmantes de la primera etapa reciben el turno.</param>
    /// <exception cref="CircuitoFirmaException">El documento no está en borrador.</exception>
    public void Enviar(DateTimeOffset instante)
    {
        Enviar();
        RegistrarRecepcionTurnos(FirmantesPendientes, instante);
    }

    /// <summary>Indica si el turno activo del firmante venció en el instante consultado.</summary>
    /// <param name="firmante">Firmante cuyo turno activo se consulta.</param>
    /// <param name="instante">Instante en el que se evalúa el vencimiento.</param>
    /// <returns><see langword="true"/> si han transcurrido dos días hábiles desde la recepción.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="firmante"/> es <see langword="null"/>.</exception>
    /// <exception cref="CircuitoFirmaException">El firmante no tiene turno activo o no se registró su recepción.</exception>
    public bool EstaVencido(Firmante firmante, DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(firmante);

        if (!FirmantesPendientes.Any(pendiente => pendiente.Id == firmante.Id))
            throw new CircuitoFirmaException($"No hay un turno activo para {firmante.Nombre}.");
        if (!_turnosActivos.TryGetValue(firmante.Id, out var recibidoEn))
            throw new CircuitoFirmaException(
                $"No hay un instante de recepción registrado para el turno de {firmante.Nombre}.");

        return instante >= CalendarioLaboralPeruLima.CalcularVencimiento(recibidoEn);
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
        if (!FirmantesPendientes.Any(pendiente => pendiente.Id == firmante.Id))
            throw new CircuitoFirmaException($"No es el turno de {firmante.Nombre}.");

        var pendientesAntes = FirmantesPendientes
            .Select(pendiente => pendiente.Id)
            .ToHashSet(StringComparer.Ordinal);
        _firmas.Add(new Firma(firmante, Version, instante));
        _turnosActivos.Remove(firmante.Id);

        if (EtapaActiva is null)
        {
            Estado = EstadoDocumento.Aprobado;
            _turnosActivos.Clear();
            return;
        }

        var pendientesDespues = FirmantesPendientes;
        var idsPendientesDespues = pendientesDespues
            .Select(pendiente => pendiente.Id)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var id in _turnosActivos.Keys.Where(id => !idsPendientesDespues.Contains(id)).ToArray())
            _turnosActivos.Remove(id);

        foreach (var pendiente in pendientesDespues)
        {
            if (!pendientesAntes.Contains(pendiente.Id))
                _turnosActivos[pendiente.Id] = instante;
        }
    }

    /// <summary>Registra el rechazo del firmante habilitado en la etapa activa.</summary>
    /// <param name="firmante">Firmante que rechaza.</param>
    /// <param name="motivo">Motivo obligatorio del rechazo, conservado literalmente.</param>
    /// <param name="instante">Momento del rechazo.</param>
    /// <exception cref="CircuitoFirmaException">El documento no está en circuito, el firmante no está habilitado o el motivo está vacío.</exception>
    public void Rechazar(Firmante firmante, string motivo, DateTimeOffset instante)
    {
        ArgumentNullException.ThrowIfNull(firmante);

        if (Estado != EstadoDocumento.EnCircuito)
            throw new CircuitoFirmaException("El documento no está en circuito de firma.");
        if (!FirmantesPendientes.Any(pendiente => pendiente.Id == firmante.Id))
            throw new CircuitoFirmaException($"No es el turno de {firmante.Nombre}.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new CircuitoFirmaException("El motivo de rechazo es obligatorio.");

        _rechazos.Add(new Rechazo(firmante, motivo, Version, instante));
        Estado = Version == MaximoVueltas ? EstadoDocumento.Rechazado : EstadoDocumento.Devuelto;
        _turnosActivos.Clear();
    }

    /// <summary>Reenvía una versión devuelta e inicia una nueva vuelta desde la primera etapa.</summary>
    /// <param name="solicitanteId">Identificador de la persona que solicita el reenvío.</param>
    /// <exception cref="CircuitoFirmaException">La persona no es el autor o el documento no está devuelto.</exception>
    public void Reenviar(string solicitanteId)
    {
        if (solicitanteId != AutorId)
            throw new CircuitoFirmaException("Solo el autor puede reenviar el documento.");
        if (Estado != EstadoDocumento.Devuelto)
            throw new CircuitoFirmaException("Solo se puede reenviar un documento devuelto.");

        Version++;
        Estado = EstadoDocumento.EnCircuito;
        _turnosActivos.Clear();
    }

    /// <summary>Reenvía la versión devuelta y registra la recepción del primer turno de la nueva vuelta.</summary>
    /// <param name="solicitanteId">Identificador de la persona que solicita el reenvío.</param>
    /// <param name="instante">Momento en que los firmantes de la primera etapa reciben el turno.</param>
    /// <exception cref="CircuitoFirmaException">La persona no es el autor o el documento no está devuelto.</exception>
    public void Reenviar(string solicitanteId, DateTimeOffset instante)
    {
        Reenviar(solicitanteId);
        RegistrarRecepcionTurnos(FirmantesPendientes, instante);
    }

    private EtapaFirma? EtapaActiva
    {
        get
        {
            var aprobados = FirmantesAprobadosEnVersionVigente();
            return _etapas.FirstOrDefault(etapa =>
                etapa.Firmantes.Any(firmante => !aprobados.Contains(firmante.Id)));
        }
    }

    private HashSet<string> FirmantesAprobadosEnVersionVigente() =>
        _firmas
            .Where(firma => firma.Version == Version)
            .Select(firma => firma.Firmante.Id)
            .ToHashSet(StringComparer.Ordinal);

    private void RegistrarRecepcionTurnos(IEnumerable<Firmante> firmantes, DateTimeOffset instante)
    {
        foreach (var firmante in firmantes)
            _turnosActivos[firmante.Id] = instante;
    }
}