using FirmaDocs;

namespace FirmaDocs.Tests.Circuito;

public class CircuitoFirmaTests
{
    private static readonly Firmante Ana = new("ana", "Ana Legal");
    private static readonly Firmante Luis = new("luis", "Luis Finanzas");
    private static readonly DateTimeOffset Ahora = new(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-5));

    private static CircuitoFirma CircuitoEnviado()
    {
        var circuito = new CircuitoFirma("DOC-001", "marta", [Ana, Luis]);
        circuito.Enviar();
        return circuito;
    }

    [Fact]
    public void Aprobar_TodosEnOrden_DocumentoAprobado()
    {
        var circuito = CircuitoEnviado();

        circuito.Aprobar(Ana, Ahora);
        circuito.Aprobar(Luis, Ahora.AddHours(1));

        Assert.Equal(EstadoDocumento.Aprobado, circuito.Estado);
        Assert.Equal(2, circuito.Firmas.Count);
        Assert.Null(circuito.FirmanteEnTurno);
    }

    [Fact]
    public void Aprobar_PrimerFirmante_PasaElTurnoAlSiguiente()
    {
        var circuito = CircuitoEnviado();

        circuito.Aprobar(Ana, Ahora);

        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(Luis, circuito.FirmanteEnTurno);
        Assert.Equal(new Firma(Ana, 1, Ahora), circuito.Firmas[0]);
    }

    [Fact]
    public void Aprobar_FueraDeTurno_LanzaExcepcion()
    {
        var circuito = CircuitoEnviado();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Aprobar(Luis, Ahora));

        Assert.Equal("No es el turno de Luis Finanzas.", ex.Message);
    }

    [Fact]
    public void Aprobar_SinEnviar_LanzaExcepcion()
    {
        var circuito = new CircuitoFirma("DOC-001", "marta", [Ana, Luis]);

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Aprobar(Ana, Ahora));

        Assert.Equal("El documento no está en circuito de firma.", ex.Message);
    }

    [Fact]
    public void Constructor_FirmanteRepetido_LanzaExcepcion()
    {
        var ex = Assert.Throws<CircuitoFirmaException>(() => new CircuitoFirma("DOC-001", "marta", [Ana, Ana]));

        Assert.Equal("Un firmante no puede repetirse en el circuito.", ex.Message);
    }
}