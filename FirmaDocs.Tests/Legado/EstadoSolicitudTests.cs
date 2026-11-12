using FirmaDocs.Legado;

namespace FirmaDocs.Tests.Legado;

public class EstadoSolicitudTests
{
    [Fact]
    public void EstadoSolicitudTexto_ConvierteLosCuatroEstadosAdmitidos_RF06()
    {
        Assert.Equal("PENDIENTE", EstadoSolicitudTexto.ATexto(EstadoSolicitud.Pendiente));
        Assert.Equal("FIRMADA", EstadoSolicitudTexto.ATexto(EstadoSolicitud.Firmada));
        Assert.Equal("RECHAZADA", EstadoSolicitudTexto.ATexto(EstadoSolicitud.Rechazada));
        Assert.Equal("EXPIRADA", EstadoSolicitudTexto.ATexto(EstadoSolicitud.Expirada));

        Assert.Equal(EstadoSolicitud.Pendiente, EstadoSolicitudTexto.DesdeTexto("PENDIENTE"));
        Assert.Equal(EstadoSolicitud.Firmada, EstadoSolicitudTexto.DesdeTexto("FIRMADA"));
        Assert.Equal(EstadoSolicitud.Rechazada, EstadoSolicitudTexto.DesdeTexto("RECHAZADA"));
        Assert.Equal(EstadoSolicitud.Expirada, EstadoSolicitudTexto.DesdeTexto("EXPIRADA"));
    }

    [Fact]
    public void EstadoSolicitudTexto_RechazaEsperandoCertificado_RF06()
    {
        Assert.Throws<ArgumentException>(() =>
            EstadoSolicitudTexto.DesdeTexto("ESPERANDO_CERTIFICADO"));
    }

    [Fact]
    public void SolicitudFirmaRepository_CambiaEntreEstadosReconocidosSinValidarTransiciones_RF07()
    {
        using var bd = new BaseDatosLegadoTemporal();
        var repo = new SolicitudFirmaRepository(bd.ConnectionString);
        var estados = new[]
        {
            EstadoSolicitud.Pendiente,
            EstadoSolicitud.Firmada,
            EstadoSolicitud.Rechazada,
            EstadoSolicitud.Expirada
        };
        var instante = new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        foreach (var origen in estados)
        {
            foreach (var destino in estados)
            {
                repo.CambiarEstado(1, origen, instante);
                repo.CambiarEstado(1, destino, instante);

                Assert.Equal(destino, Assert.Single(repo.Listar(destino), solicitud => solicitud.Id == 1).Estado);
            }
        }
    }
}
