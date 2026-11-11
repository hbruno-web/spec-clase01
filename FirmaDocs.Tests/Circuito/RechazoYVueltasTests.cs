using FirmaDocs;

namespace FirmaDocs.Tests.Circuito;

public class RechazoYVueltasTests
{
    private const string AutorId = "marta";
    private static readonly Firmante Ana = new("ana", "Ana Legal");
    private static readonly Firmante Luis = new("luis", "Luis Finanzas");
    private static readonly DateTimeOffset Ahora = new(2026, 11, 10, 10, 0, 0, TimeSpan.FromHours(-5));

    [Fact]
    public void Rechazar_FirmanteEnTurnoConMotivoValido_RegistraYDevuelveDocumento_RF01()
    {
        var circuito = CircuitoEnviado();

        circuito.Rechazar(Ana, "Corregir anexo", Ahora);

        Assert.Equal(EstadoDocumento.Devuelto, circuito.Estado);
        Assert.Equal(new Rechazo(Ana, "Corregir anexo", 1, Ahora), Assert.Single(circuito.Rechazos));
        Assert.Empty(circuito.Firmas);
    }

    [Fact]
    public void Rechazar_FirmanteFueraDeTurno_LanzaMismoErrorQueAprobar_RF02()
    {
        var circuito = CircuitoEnviado();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Rechazar(Luis, "Corregir anexo", Ahora));

        Assert.Equal("No es el turno de Luis Finanzas.", ex.Message);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(1, circuito.Version);
        Assert.Equal(Ana, circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Rechazar_EnBorrador_LanzaMismoErrorDeEstadoQueAprobar_RF03()
    {
        var circuito = CircuitoBorrador();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Rechazar(Ana, "Corregir anexo", Ahora));

        Assert.Equal("El documento no está en circuito de firma.", ex.Message);
        Assert.Equal(EstadoDocumento.Borrador, circuito.Estado);
        Assert.Equal(1, circuito.Version);
        Assert.Null(circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Rechazar_DespuesDeAprobado_LanzaMismoErrorDeEstadoQueAprobar_RF03()
    {
        var circuito = CircuitoEnviado();
        circuito.Aprobar(Ana, Ahora);
        circuito.Aprobar(Luis, Ahora.AddMinutes(1));

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Rechazar(Ana, "Corregir anexo", Ahora.AddMinutes(2)));

        Assert.Equal("El documento no está en circuito de firma.", ex.Message);
        Assert.Equal(EstadoDocumento.Aprobado, circuito.Estado);
        Assert.Equal(1, circuito.Version);
        Assert.Null(circuito.FirmanteEnTurno);
        Assert.Equal(2, circuito.Firmas.Count);
        Assert.Empty(circuito.Rechazos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rechazar_MotivoVacioOConEspacios_LanzaExcepcionSinCambios_RF04(string motivo)
    {
        var circuito = CircuitoEnviado();

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Rechazar(Ana, motivo, Ahora));

        Assert.Equal("El motivo de rechazo es obligatorio.", ex.Message);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(1, circuito.Version);
        Assert.Equal(Ana, circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Rechazar_MotivoConEspaciosYTextoLargo_ConservaTextoExacto_RF05()
    {
        var circuito = CircuitoEnviado();
        var motivo = $"  {new string('x', 2000)}  ";

        circuito.Rechazar(Ana, motivo, Ahora);

        Assert.Equal(motivo, Assert.Single(circuito.Rechazos).Motivo);
    }

    [Fact]
    public void Rechazar_VariasVueltas_ExponeHistorialCompletoCronologicoYSeparado_RF06()
    {
        var circuito = CircuitoEnviado();
        var primerRechazo = new Rechazo(Ana, "Primera corrección", 1, Ahora);
        circuito.Rechazar(primerRechazo.Firmante, primerRechazo.Motivo, primerRechazo.Instante);
        circuito.Reenviar(AutorId);
        var segundoRechazo = new Rechazo(Ana, "  Segunda corrección  ", 2, Ahora.AddDays(1));
        circuito.Rechazar(segundoRechazo.Firmante, segundoRechazo.Motivo, segundoRechazo.Instante);

        Assert.Equal([primerRechazo, segundoRechazo], circuito.Rechazos);
        Assert.Empty(circuito.Firmas);
        Assert.Equal(EstadoDocumento.Devuelto, circuito.Estado);
    }

    [Fact]
    public void Rechazar_TerceraVuelta_RegistraRechazoTerminalYBloqueaReenvio_RF07()
    {
        var circuito = CircuitoEnviado();
        circuito.Rechazar(Ana, "Primera corrección", Ahora);
        circuito.Reenviar(AutorId);
        circuito.Rechazar(Ana, "Segunda corrección", Ahora.AddDays(1));
        circuito.Reenviar(AutorId);

        circuito.Rechazar(Ana, "Tercera corrección", Ahora.AddDays(2));

        Assert.Equal(EstadoDocumento.Rechazado, circuito.Estado);
        Assert.Equal(3, circuito.Version);
        Assert.Equal(3, circuito.Rechazos.Count);
        Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar(AutorId));
    }

    [Fact]
    public void Reenviar_AutorDesdeDevuelto_IniciaNuevaVersionEnPrimerFirmante_RF08()
    {
        var circuito = CircuitoEnviado();
        circuito.Rechazar(Ana, "Corregir anexo", Ahora);

        circuito.Reenviar(AutorId);

        Assert.Equal(2, circuito.Version);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(Ana, circuito.FirmanteEnTurno);
    }

    [Fact]
    public void Reenviar_PersonaDistintaDelAutor_LanzaExcepcionSinCambios_RF09()
    {
        var circuito = CircuitoEnviado();
        circuito.Rechazar(Ana, "Corregir anexo", Ahora);

        var ex = Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar("otra-persona"));

        Assert.Equal("Solo el autor puede reenviar el documento.", ex.Message);
        Assert.Equal(1, circuito.Version);
        Assert.Equal(EstadoDocumento.Devuelto, circuito.Estado);
        Assert.Null(circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
        Assert.Single(circuito.Rechazos);
    }

    [Fact]
    public void Reenviar_DesdeBorrador_LanzaExcepcionSinCambios_RF10()
    {
        var circuito = CircuitoBorrador();

        Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar(AutorId));

        Assert.Equal(1, circuito.Version);
        Assert.Equal(EstadoDocumento.Borrador, circuito.Estado);
        Assert.Null(circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Reenviar_DesdeEnCircuito_LanzaExcepcionSinCambios_RF10()
    {
        var circuito = CircuitoEnviado();

        Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar(AutorId));

        Assert.Equal(1, circuito.Version);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(Ana, circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Firmas);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Reenviar_DesdeAprobado_LanzaExcepcionSinCambios_RF10()
    {
        var circuito = CircuitoEnviado();
        circuito.Aprobar(Ana, Ahora);
        circuito.Aprobar(Luis, Ahora.AddMinutes(1));

        Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar(AutorId));

        Assert.Equal(1, circuito.Version);
        Assert.Equal(EstadoDocumento.Aprobado, circuito.Estado);
        Assert.Equal(2, circuito.Firmas.Count);
        Assert.Null(circuito.FirmanteEnTurno);
        Assert.Empty(circuito.Rechazos);
    }

    [Fact]
    public void Reenviar_DesdeRechazado_LanzaExcepcionSinCambios_RF10()
    {
        var circuito = CrearRechazado();

        Assert.Throws<CircuitoFirmaException>(() => circuito.Reenviar(AutorId));

        Assert.Equal(3, circuito.Version);
        Assert.Equal(EstadoDocumento.Rechazado, circuito.Estado);
        Assert.Equal(3, circuito.Rechazos.Count);
        Assert.Null(circuito.FirmanteEnTurno);
    }

    [Fact]
    public void Reenviar_ConFirmasDeVersionAnterior_ConservaHistorialYSoloCuentaVersionVigente_RF11()
    {
        var circuito = CircuitoEnviado();
        circuito.Aprobar(Ana, Ahora);
        circuito.Rechazar(Luis, "Corregir presupuesto", Ahora.AddMinutes(1));

        circuito.Reenviar(AutorId);
        circuito.Aprobar(Ana, Ahora.AddDays(1));

        Assert.Equal(2, circuito.Firmas.Count);
        Assert.Equal(new Firma(Ana, 1, Ahora), circuito.Firmas[0]);
        Assert.Equal(new Firma(Ana, 2, Ahora.AddDays(1)), circuito.Firmas[1]);
        Assert.Equal(EstadoDocumento.EnCircuito, circuito.Estado);
        Assert.Equal(Luis, circuito.FirmanteEnTurno);

        circuito.Aprobar(Luis, Ahora.AddDays(1).AddMinutes(1));

        Assert.Equal(EstadoDocumento.Aprobado, circuito.Estado);
        Assert.Equal(3, circuito.Firmas.Count);
        Assert.All(circuito.Firmas.Where(firma => firma.Version == 1), firma => Assert.Equal(1, firma.Version));
    }

    private static CircuitoFirma CircuitoBorrador() =>
        new("DOC-001", AutorId, [Ana, Luis]);

    private static CircuitoFirma CircuitoEnviado()
    {
        var circuito = CircuitoBorrador();
        circuito.Enviar();
        return circuito;
    }

    private static CircuitoFirma CrearRechazado()
    {
        var circuito = CircuitoEnviado();
        circuito.Rechazar(Ana, "Primera corrección", Ahora);
        circuito.Reenviar(AutorId);
        circuito.Rechazar(Ana, "Segunda corrección", Ahora.AddDays(1));
        circuito.Reenviar(AutorId);
        circuito.Rechazar(Ana, "Tercera corrección", Ahora.AddDays(2));
        return circuito;
    }
}
