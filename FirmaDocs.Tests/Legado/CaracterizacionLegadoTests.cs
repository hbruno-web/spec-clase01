using FirmaDocs.Legado;
using NSubstitute;

namespace FirmaDocs.Tests.Legado;

public class CaracterizacionLegadoTests
{
    [Fact]
    public void Caracterizacion_BandejaFirmante_ClasificaPorEstado()
    {
        using var bd = new BaseDatosLegadoTemporal();
        var servicio = new BandejaFirmante(new SolicitudFirmaRepository(bd.ConnectionString));

        var bandeja = servicio.Armar("ana@empresa.pe");

        Assert.Equal(new[] { 1, 3, 4, 2 }, bandeja.PorFirmar.Select(solicitud => solicitud.Id));
        Assert.Empty(bandeja.EsperandoCertificado);
        Assert.Equal(new[] { 5, 6, 7 }, bandeja.Historial.Select(solicitud => solicitud.Id));
    }

    [Fact]
    public void Caracterizacion_RecordatorioService_EnviaPendientesAntiguasSinAvisoDeCertificado()
    {
        using var bd = new BaseDatosLegadoTemporal();
        var canal = Substitute.For<ICanalRecordatorio>();
        var servicio = new RecordatorioService(new SolicitudFirmaRepository(bd.ConnectionString), canal);

        var enviados = servicio.EnviarRecordatorios(new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.FromHours(-5)));

        Assert.Equal(4, enviados);
        canal.Received(1).Enviar("ana@empresa.pe", "Tienes un documento por firmar: DOC-001");
        canal.Received(1).Enviar("ana@empresa.pe", "Tienes un documento por firmar: DOC-003");
        canal.Received(1).Enviar("ana@empresa.pe", "Tienes un documento por firmar: DOC-004");
        canal.Received(1).Enviar("bob@empresa.pe", "Tienes un documento por firmar: DOC-008");
        canal.DidNotReceive().Enviar("ana@empresa.pe", "Instala tu certificado digital para firmar: DOC-003");
        canal.DidNotReceive().Enviar("ana@empresa.pe", "Instala tu certificado digital para firmar: DOC-004");
        canal.DidNotReceive().Enviar("ana@empresa.pe", "Tienes un documento por firmar: DOC-002");
    }

    [Fact]
    public void Caracterizacion_ReporteSla_IncluyePendientesMigradasEnSuMesOriginal()
    {
        using var bd = new BaseDatosLegadoTemporal();
        var servicio = new ReporteSla(new SolicitudFirmaRepository(bd.ConnectionString));

        var resultado = servicio.HorasPromedioPorMes();

        Assert.Equal(3, resultado.Count);
        Assert.Equal(18, resultado["2026-01"]);
        Assert.Equal(0, resultado["2026-02"]);
        Assert.Equal(12, resultado["2026-03"]);
    }

    [Fact]
    public void Caracterizacion_ReporteSla_PendientesSinResolverReducenElPromedio()
    {
        using var bd = new BaseDatosLegadoTemporal();
        var servicio = new ReporteSla(new SolicitudFirmaRepository(bd.ConnectionString));

        var resultado = servicio.HorasPromedioPorMes();

        Assert.Equal(18, resultado["2026-01"]);
    }
}
