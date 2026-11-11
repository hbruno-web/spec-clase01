namespace FirmaDocs;

/// <summary>Configuración inmutable de una etapa y de sus firmantes ordenados.</summary>
public sealed record EtapaFirma
{
    /// <summary>Crea una etapa de firma con sus firmantes y modo de aprobación.</summary>
    /// <param name="firmantes">Firmantes configurados para la etapa, en su orden.</param>
    /// <param name="modo">Modo secuencial o paralelo de la etapa.</param>
    /// <exception cref="CircuitoFirmaException">La cantidad de firmantes está fuera de límites o el modo no es válido.</exception>
    public EtapaFirma(IReadOnlyList<Firmante> firmantes, ModoEtapaFirma modo)
    {
        ArgumentNullException.ThrowIfNull(firmantes);

        if (firmantes.Count is < 1 or > 5)
            throw new CircuitoFirmaException("Cada etapa debe tener entre 1 y 5 firmantes.");
        if (modo is not ModoEtapaFirma.SECUENCIAL and not ModoEtapaFirma.PARALELA)
            throw new CircuitoFirmaException("El modo de la etapa debe ser SECUENCIAL o PARALELA.");

        Firmantes = Array.AsReadOnly(firmantes.ToArray());
        Modo = modo;
    }

    /// <summary>Firmantes de la etapa en el orden configurado.</summary>
    public IReadOnlyList<Firmante> Firmantes { get; }

    /// <summary>Modo de aprobación de la etapa.</summary>
    public ModoEtapaFirma Modo { get; }
}
