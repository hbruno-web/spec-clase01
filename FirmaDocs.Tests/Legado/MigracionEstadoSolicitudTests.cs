using Microsoft.Data.Sqlite;

namespace FirmaDocs.Tests.Legado;

public class MigracionEstadoSolicitudTests
{
    [Fact]
    public void MigracionSolicitudes_ConservaDatosYRollbackRestauraConteos_RF08()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"firmadocs-migracion-{Guid.NewGuid():N}.sqlite");
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            Pooling = false
        }.ToString();

        try
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var raiz = EncontrarRaiz();

            Ejecutar(connection, File.ReadAllText(Path.Combine(raiz, "db", "schema.sql")));
            Ejecutar(connection, File.ReadAllText(Path.Combine(raiz, "db", "seed.sql")));

            Assert.Equal(1000, Contar(connection, "SELECT COUNT(*) FROM solicitudes_firma;"));
            Assert.Equal(250, Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE migrado_desde = 'ESPERANDO_CERTIFICADO';"));
            Assert.Equal(0, Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE estado = 'ESPERANDO_CERTIFICADO';"));

            PrepararDatosLegados(connection);
            Ejecutar(connection, "CREATE TEMP TABLE solicitudes_firma_original AS SELECT * FROM solicitudes_firma;");

            var filasIniciales = Contar(connection, "SELECT COUNT(*) FROM solicitudes_firma_original;");
            var conteosIniciales = ContarPorEstado(connection);
            var marcadasInicialmente = Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE migrado_desde = 'ESPERANDO_CERTIFICADO';");
            var esperandoInicialmente = conteosIniciales.GetValueOrDefault("ESPERANDO_CERTIFICADO");

            var migracion = File.ReadAllText(Path.Combine(
                raiz,
                "db",
                "migrations",
                "003_eliminar_esperando_certificado.sql"));
            Ejecutar(connection, migracion);
            Ejecutar(connection, migracion);

            var filasEsperando = Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE estado = 'ESPERANDO_CERTIFICADO';");
            var filasFinales = Contar(connection, "SELECT COUNT(*) FROM solicitudes_firma;");
            var filasMarcadas = Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE migrado_desde = 'ESPERANDO_CERTIFICADO';");
            var marcadasConEstadoIncorrecto = Contar(
                connection,
                "SELECT COUNT(*) FROM solicitudes_firma WHERE migrado_desde = 'ESPERANDO_CERTIFICADO' AND estado != 'PENDIENTE';");
            var filasPerdidasOCambiadas = Contar(connection, @"
SELECT COUNT(*)
FROM solicitudes_firma_original original
LEFT JOIN solicitudes_firma actual ON actual.id = original.id
WHERE actual.id IS NULL
   OR actual.documento_id IS NOT original.documento_id
   OR actual.firmante_email IS NOT original.firmante_email
   OR actual.vuelta IS NOT original.vuelta
   OR actual.creada_en IS NOT original.creada_en
   OR actual.resuelta_en IS NOT original.resuelta_en
   OR actual.estado IS NOT (CASE WHEN original.estado = 'ESPERANDO_CERTIFICADO' THEN 'PENDIENTE' ELSE original.estado END)
   OR actual.migrado_desde IS NOT (CASE
        WHEN original.estado = 'ESPERANDO_CERTIFICADO' THEN 'ESPERANDO_CERTIFICADO'
        ELSE original.migrado_desde
      END);");
            var indiceConservado = Contar(
                connection,
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'ix_solicitudes_estado';");

            Assert.Equal(0, filasEsperando);
            Assert.Equal(filasIniciales, filasFinales);
            Assert.Equal(marcadasInicialmente + esperandoInicialmente, filasMarcadas);
            Assert.Equal(0, marcadasConEstadoIncorrecto);
            Assert.Equal(0, filasPerdidasOCambiadas);
            Assert.Equal(1, indiceConservado);
            Assert.Throws<SqliteException>(() => Ejecutar(connection, @"
INSERT INTO solicitudes_firma
    (id, documento_id, firmante_email, vuelta, creada_en, resuelta_en, estado, migrado_desde)
VALUES
    (1001, 'DOC-INVALIDO', 'ana@empresa.pe', 1, '2026-11-11T12:00:00+00:00', NULL, 'ESPERANDO_CERTIFICADO', NULL);"));

            var rollback = File.ReadAllText(Path.Combine(
                raiz,
                "db",
                "migrations",
                "003_rollback.sql"));
            Ejecutar(connection, rollback);

            var conteosRestaurados = ContarPorEstado(connection);
            var conteosEsperados = new Dictionary<string, int>(conteosIniciales);
            if (marcadasInicialmente > 0)
            {
                conteosEsperados["PENDIENTE"] -= marcadasInicialmente;
                conteosEsperados["ESPERANDO_CERTIFICADO"] =
                    conteosEsperados.GetValueOrDefault("ESPERANDO_CERTIFICADO") + marcadasInicialmente;
            }

            Assert.Equal(conteosEsperados, conteosRestaurados);
            Assert.Equal(
                marcadasInicialmente + esperandoInicialmente,
                Contar(connection, "SELECT COUNT(*) FROM solicitudes_firma WHERE estado = 'ESPERANDO_CERTIFICADO' AND migrado_desde IS NULL;"));

            Ejecutar(connection, rollback);
            Assert.Equal(conteosRestaurados, ContarPorEstado(connection));
        }
        finally
        {
            if (File.Exists(ruta))
                File.Delete(ruta);
        }
    }

    private static string EncontrarRaiz()
    {
        DirectoryInfo? directorio = new(AppContext.BaseDirectory);
        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "db", "schema.sql")))
                return directorio.FullName;

            directorio = directorio.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró la raíz del proyecto con db/schema.sql.");
    }

    private static void Ejecutar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void PrepararDatosLegados(SqliteConnection connection)
    {
        Ejecutar(connection, @"
BEGIN TRANSACTION;
CREATE TABLE solicitudes_firma_legada (
  id INTEGER PRIMARY KEY,
  documento_id TEXT NOT NULL,
  firmante_email TEXT NOT NULL,
  vuelta INTEGER NOT NULL DEFAULT 1,
  creada_en TEXT NOT NULL,
  resuelta_en TEXT NULL,
  estado TEXT NOT NULL CHECK (estado IN ('PENDIENTE','ESPERANDO_CERTIFICADO','FIRMADA','RECHAZADA','EXPIRADA')),
  migrado_desde TEXT NULL
);
INSERT INTO solicitudes_firma_legada
  (id, documento_id, firmante_email, vuelta, creada_en, resuelta_en, estado, migrado_desde)
SELECT
  id,
  documento_id,
  firmante_email,
  vuelta,
  creada_en,
  resuelta_en,
  CASE WHEN migrado_desde = 'ESPERANDO_CERTIFICADO' THEN 'ESPERANDO_CERTIFICADO' ELSE estado END,
  CASE WHEN migrado_desde = 'ESPERANDO_CERTIFICADO' THEN NULL ELSE migrado_desde END
FROM solicitudes_firma;
DROP TABLE solicitudes_firma;
ALTER TABLE solicitudes_firma_legada RENAME TO solicitudes_firma;
CREATE INDEX ix_solicitudes_estado ON solicitudes_firma(estado);
COMMIT;");
    }

    private static int Contar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static Dictionary<string, int> ContarPorEstado(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT estado, COUNT(*) FROM solicitudes_firma GROUP BY estado;";
        using var reader = command.ExecuteReader();
        var conteos = new Dictionary<string, int>();
        while (reader.Read())
            conteos[reader.GetString(0)] = reader.GetInt32(1);

        return conteos;
    }
}
