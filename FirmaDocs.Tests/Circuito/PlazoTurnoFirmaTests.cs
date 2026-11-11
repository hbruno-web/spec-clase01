using FirmaDocs;

namespace FirmaDocs.Tests.Circuito;

public class PlazoTurnoFirmaTests
{
    private static readonly Firmante Ana = new("ana", "Ana");
    private static readonly Firmante Luis = new("luis", "Luis");
    private static readonly TimeSpan DesplazamientoLima = TimeSpan.FromHours(-5);
    private static readonly DateTimeOffset Recepcion = new(2026, 11, 13, 10, 0, 0, DesplazamientoLima);
    private static readonly DateTimeOffset Vencimiento = new(2026, 11, 17, 10, 0, 0, DesplazamientoLima);

    [Fact]
    public void EstaVencido_ConsultaAntesDelLimite_NoEstaVencido_RF19()
    {
        var circuito = CrearCircuitoEnviadoConRecepcion(Recepcion);

        Assert.False(circuito.EstaVencido(Ana, Vencimiento.AddTicks(-1)));
    }

    [Fact]
    public void EstaVencido_ConsultaEnElLimite_EstaVencido_RF19()
    {
        var circuito = CrearCircuitoEnviadoConRecepcion(Recepcion);

        Assert.True(circuito.EstaVencido(Ana, Vencimiento));
    }

    [Fact]
    public void EstaVencido_ConsultaDespuesDelLimite_EstaVencido_RF19()
    {
        var circuito = CrearCircuitoEnviadoConRecepcion(Recepcion);

        Assert.True(circuito.EstaVencido(Ana, Vencimiento.AddTicks(1)));
    }

    [Fact]
    public void EstaVencido_EnvioSinInstanteDeRecepcion_LanzaExcepcion_RF19()
    {
        var circuito = new CircuitoFirma("DOC-001", "autor", [Ana]);
        circuito.Enviar();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.EstaVencido(Ana, Vencimiento));

        Assert.Equal("No hay un instante de recepción registrado para el turno de Ana.", ex.Message);
    }

    [Fact]
    public void Aprobar_TurnoSecuencialSiguiente_IniciaPlazoAlAprobar_RF18()
    {
        var circuito = new CircuitoFirma("DOC-002", "autor", [Ana, Luis]);
        circuito.Enviar(Recepcion);
        var instanteAprobacion = Recepcion.AddHours(12);

        circuito.Aprobar(Ana, instanteAprobacion);

        var vencimiento = new DateTimeOffset(2026, 11, 17, 22, 0, 0, DesplazamientoLima);
        Assert.True(circuito.EstaVencido(Luis, vencimiento));
    }

    [Fact]
    public void Aprobar_TurnoParaleloPendiente_ConservaRecepcionOriginal_RF18()
    {
        var etapa = new EtapaFirma([Ana, Luis], ModoEtapaFirma.PARALELA);
        var circuito = CircuitoFirma.CrearConEtapas("DOC-003", "autor", [etapa]);
        var recepcionEtapa = new DateTimeOffset(2026, 11, 10, 10, 0, 0, DesplazamientoLima);
        circuito.Enviar(recepcionEtapa);

        circuito.Aprobar(Ana, recepcionEtapa.AddDays(1));

        Assert.True(circuito.EstaVencido(
            Luis,
            new DateTimeOffset(2026, 11, 12, 10, 0, 0, DesplazamientoLima)));
    }

    [Fact]
    public void EstaVencido_TurnoRecibidoEnSabado_CuentaDesdeLunes_RF19()
    {
        var sabado = new DateTimeOffset(2026, 11, 14, 10, 0, 0, DesplazamientoLima);
        var lunes = new DateTimeOffset(2026, 11, 16, 10, 0, 0, DesplazamientoLima);
        var circuito = CrearCircuitoEnviadoConRecepcion(sabado);

        Assert.False(circuito.EstaVencido(Ana, lunes));
        Assert.True(circuito.EstaVencido(Ana, Vencimiento));
    }

    [Fact]
    public void EstaVencido_FeriadosOficialesDeDiciembre_NoLosCuentaComoDiasHabiles_RF19()
    {
        var recepcion = new DateTimeOffset(2026, 12, 7, 10, 0, 0, DesplazamientoLima);
        var jueves = new DateTimeOffset(2026, 12, 10, 10, 0, 0, DesplazamientoLima);
        var viernes = new DateTimeOffset(2026, 12, 11, 10, 0, 0, DesplazamientoLima);
        var circuito = CrearCircuitoEnviadoConRecepcion(recepcion);

        Assert.False(circuito.EstaVencido(Ana, jueves));
        Assert.True(circuito.EstaVencido(Ana, viernes));
    }

    [Fact]
    public void EstaVencido_FeriadosMovilesDeSemanaSanta_NoLosCuentaComoDiasHabiles_RF19()
    {
        var recepcion = new DateTimeOffset(2026, 4, 1, 10, 0, 0, DesplazamientoLima);
        var lunes = new DateTimeOffset(2026, 4, 6, 10, 0, 0, DesplazamientoLima);
        var martes = new DateTimeOffset(2026, 4, 7, 10, 0, 0, DesplazamientoLima);
        var circuito = CrearCircuitoEnviadoConRecepcion(recepcion);

        Assert.False(circuito.EstaVencido(Ana, lunes));
        Assert.True(circuito.EstaVencido(Ana, martes));
    }

    [Fact]
    public void Reenviar_NuevaVuelta_IniciaPlazoEnInstanteDeReenvio_RF18()
    {
        var circuito = new CircuitoFirma("DOC-004", "autor", [Ana]);
        circuito.Enviar(Recepcion);
        circuito.Rechazar(Ana, "Corregir documento", Recepcion.AddHours(1));
        var instanteReenvio = Recepcion.AddDays(10);

        circuito.Reenviar("autor", instanteReenvio);

        Assert.False(circuito.EstaVencido(Ana, instanteReenvio.AddDays(2).AddTicks(-1)));
        Assert.True(circuito.EstaVencido(Ana, instanteReenvio.AddDays(2)));
    }

    [Fact]
    public void Reenviar_SinInstanteDeRecepcion_NoPermiteConsultarVencimiento_RF19()
    {
        var circuito = new CircuitoFirma("DOC-005", "autor", [Ana]);
        circuito.Enviar(Recepcion);
        circuito.Rechazar(Ana, "Corregir documento", Recepcion.AddHours(1));

        circuito.Reenviar("autor");

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.EstaVencido(Ana, Vencimiento));

        Assert.Equal("No hay un instante de recepción registrado para el turno de Ana.", ex.Message);
    }

    private static CircuitoFirma CrearCircuitoEnviadoConRecepcion(DateTimeOffset instante)
    {
        var circuito = new CircuitoFirma("DOC-001", "autor", [Ana]);
        circuito.Enviar(instante);
        return circuito;
    }
}
