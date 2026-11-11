using FirmaDocs;

namespace FirmaDocs.Tests.Circuito;

public class EtapasFirmaTests
{
    private static readonly Firmante Ana = new("ana", "Ana");
    private static readonly Firmante Luis = new("luis", "Luis");
    private static readonly Firmante Marta = new("marta", "Marta");
    private static readonly Firmante Pedro = new("pedro", "Pedro");
    private static readonly Firmante Sofia = new("sofia", "Sofia");
    private static readonly Firmante Diego = new("diego", "Diego");
    private static readonly DateTimeOffset Ahora = new(2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void CrearConEtapas_CantidadDeEtapasEnLimites_IncluyeOrdenConfigurado_RF12(int cantidad)
    {
        Firmante[] firmantes = [Ana, Luis, Marta, Pedro, Sofia];
        var etapas = Enumerable.Range(0, cantidad)
            .Select(indice => new EtapaFirma([firmantes[indice]], ModoEtapaFirma.SECUENCIAL))
            .ToArray();

        var circuito = CircuitoFirma.CrearConEtapas("DOC-001", "autor", etapas);

        Assert.Equal(etapas, circuito.Etapas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CrearConEtapas_CantidadDeEtapasFueraDeLimites_LanzaExcepcion_RF12(int cantidad)
    {
        var etapas = Enumerable.Range(0, cantidad)
            .Select(_ => new EtapaFirma([Ana], ModoEtapaFirma.SECUENCIAL))
            .ToArray();

        Assert.Throws<CircuitoFirmaException>(() =>
            CircuitoFirma.CrearConEtapas("DOC-001", "autor", etapas));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void EtapaFirma_CantidadDeFirmantesEnLimites_ConservaOrden_RF12(int cantidad)
    {
        Firmante[] firmantes = [Ana, Luis, Marta, Pedro, Sofia];

        var etapa = new EtapaFirma(firmantes.Take(cantidad).ToArray(), ModoEtapaFirma.SECUENCIAL);

        Assert.Equal(firmantes.Take(cantidad), etapa.Firmantes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void EtapaFirma_CantidadDeFirmantesFueraDeLimites_LanzaExcepcion_RF12(int cantidad)
    {
        Firmante[] firmantes = [Ana, Luis, Marta, Pedro, Sofia, Diego];

        Assert.Throws<CircuitoFirmaException>(() =>
            new EtapaFirma(firmantes.Take(cantidad).ToArray(), ModoEtapaFirma.SECUENCIAL));
    }

    [Theory]
    [InlineData(ModoEtapaFirma.SECUENCIAL)]
    [InlineData(ModoEtapaFirma.PARALELA)]
    public void EtapaFirma_ModoPermitido_AceptaConfiguracion_RF13(ModoEtapaFirma modo)
    {
        var etapa = new EtapaFirma([Ana], modo);

        Assert.Equal(modo, etapa.Modo);
    }

    [Fact]
    public void EtapaFirma_ModoNoPermitido_LanzaExcepcion_RF13()
    {
        Assert.Throws<CircuitoFirmaException>(() =>
            new EtapaFirma([Ana], (ModoEtapaFirma)99));
    }

    [Fact]
    public void CrearConEtapas_FirmanteRepetidoEntreEtapas_LanzaExcepcion_RF12()
    {
        EtapaFirma[] etapas =
        [
            new([Ana], ModoEtapaFirma.SECUENCIAL),
            new([Ana], ModoEtapaFirma.PARALELA)
        ];

        Assert.Throws<CircuitoFirmaException>(() =>
            CircuitoFirma.CrearConEtapas("DOC-001", "autor", etapas));
    }

    [Fact]
    public void CrearConEtapas_FirmanteRepetidoDentroDeEtapa_LanzaExcepcion_RF12()
    {
        EtapaFirma[] etapas =
        [
            new([Ana, Ana], ModoEtapaFirma.SECUENCIAL)
        ];

        Assert.Throws<CircuitoFirmaException>(() =>
            CircuitoFirma.CrearConEtapas("DOC-001", "autor", etapas));
    }

    [Fact]
    public void Aprobar_EtapaSecuencialConFirmantesOrdenados_AvanzaEnOrden_RF14()
    {
        var circuito = CrearCircuitoSecuencial();
        circuito.Enviar();

        circuito.Aprobar(Ana, Ahora);

        Assert.Equal(Luis, circuito.FirmanteEnTurno);
        Assert.Single(circuito.Firmas);
    }

    [Fact]
    public void Aprobar_FirmanteSecuencialFueraDeTurno_RechazaSinAvanzar_RF14()
    {
        var circuito = CrearCircuitoSecuencial();
        circuito.Enviar();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Aprobar(Luis, Ahora));

        Assert.Equal("No es el turno de Luis.", ex.Message);
        Assert.Equal(Ana, circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
    }

    [Fact]
    public void Aprobar_EtapaSecuencialCompleta_HabilitaEtapaSiguiente_RF16()
    {
        var circuito = CrearCircuitoSecuencial();
        circuito.Enviar();

        circuito.Aprobar(Ana, Ahora);
        Assert.Equal(Luis, circuito.FirmanteEnTurno);

        circuito.Aprobar(Luis, Ahora);
        Assert.Equal(Marta, circuito.FirmanteEnTurno);
    }

    [Fact]
    public void Aprobar_EtapaParalelaEnOrdenInverso_RegistraTodasYAvanza_RF15()
    {
        var circuito = CrearCircuitoParalelo();
        circuito.Enviar();

        Assert.Equal([Ana, Luis], circuito.FirmantesPendientes);
        Assert.Null(circuito.FirmanteEnTurno);

        circuito.Aprobar(Luis, Ahora);
        circuito.Aprobar(Ana, Ahora.AddMinutes(1));

        Assert.Equal([Luis, Ana], circuito.Firmas.Select(firma => firma.Firmante));
        Assert.Equal(Marta, circuito.FirmantesPendientes.Single());
    }

    [Fact]
    public void Aprobar_EtapaParalelaConAprobacionPendiente_NoCompletaLaEtapa_RF15()
    {
        var circuito = CrearCircuitoParalelo();
        circuito.Enviar();

        circuito.Aprobar(Luis, Ahora);

        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(Ana, circuito.FirmantesPendientes.Single());
    }

    [Fact]
    public void Aprobar_EtapaFuturaAntesDeCompletarLaParalela_LanzaExcepcion_RF16()
    {
        var circuito = CrearCircuitoParalelo();
        circuito.Enviar();
        circuito.Aprobar(Luis, Ahora);

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Aprobar(Marta, Ahora.AddMinutes(1)));

        Assert.Equal("No es el turno de Marta.", ex.Message);
        Assert.Single(circuito.Firmas);
        Assert.Equal(Ana, circuito.FirmantesPendientes.Single());
    }

    [Fact]
    public void Reenviar_RechazoEnEtapaPosterior_IniciaVueltaDesdePrimeraYConservaFirmas_RF17()
    {
        var circuito = CircuitoFirma.CrearConEtapas(
            "DOC-001",
            "autor",
            [
                new EtapaFirma([Ana, Luis], ModoEtapaFirma.SECUENCIAL),
                new EtapaFirma([Marta], ModoEtapaFirma.SECUENCIAL)
            ]);
        circuito.Enviar();
        circuito.Aprobar(Ana, Ahora);
        circuito.Aprobar(Luis, Ahora.AddMinutes(1));
        circuito.Rechazar(Marta, "Corregir documento", Ahora.AddMinutes(2));

        circuito.Reenviar("autor");

        Assert.Equal(2, circuito.Version);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(2, circuito.Firmas.Count);
        Assert.All(circuito.Firmas, firma => Assert.Equal(1, firma.Version));
        Assert.Equal(Ana, circuito.FirmanteEnTurno);

        circuito.Aprobar(Ana, Ahora.AddDays(1));

        Assert.Equal(Luis, circuito.FirmanteEnTurno);
        Assert.Equal(3, circuito.Firmas.Count);
    }

    private static CircuitoFirma CrearCircuitoSecuencial() =>
        CircuitoFirma.CrearConEtapas(
            "DOC-001",
            "autor",
            [
                new EtapaFirma([Ana, Luis], ModoEtapaFirma.SECUENCIAL),
                new EtapaFirma([Marta, Pedro], ModoEtapaFirma.SECUENCIAL)
            ]);

    private static CircuitoFirma CrearCircuitoParalelo() =>
        CircuitoFirma.CrearConEtapas(
            "DOC-001",
            "autor",
            [
                new EtapaFirma([Ana, Luis], ModoEtapaFirma.PARALELA),
                new EtapaFirma([Marta], ModoEtapaFirma.SECUENCIAL)
            ]);
}
