using Microsoft.Data.Sqlite;

namespace FirmaDocs.Tests.Legado;

internal sealed class BaseDatosLegadoTemporal : IDisposable
{
    private readonly string _ruta;

    public BaseDatosLegadoTemporal()
    {
        _ruta = Path.Combine(Path.GetTempPath(), $"firmadocs-legado-{Guid.NewGuid():N}.sqlite");
        ConnectionString = new SqliteConnectionStringBuilder { DataSource = _ruta, Pooling = false }.ToString();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        Ejecutar(connection, File.ReadAllText(EncontrarSchema()));
        Ejecutar(connection, FilasControladas);
    }

    public string ConnectionString { get; }

    public void Dispose() => File.Delete(_ruta);

    private static string EncontrarSchema()
    {
        DirectoryInfo? directorio = new(AppContext.BaseDirectory);
        while (directorio is not null)
        {
            var schema = Path.Combine(directorio.FullName, "db", "schema.sql");
            if (File.Exists(schema))
                return schema;

            directorio = directorio.Parent;
        }

        throw new FileNotFoundException("No se encontró db/schema.sql desde el directorio de tests.");
    }

    private static void Ejecutar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private const string FilasControladas = @"
INSERT INTO solicitudes_firma
    (id, documento_id, firmante_email, vuelta, creada_en, resuelta_en, estado, migrado_desde)
VALUES
    (1, 'DOC-001', 'ana@empresa.pe', 1, '2026-01-01T09:00:00-05:00', NULL, 'PENDIENTE', NULL),
    (2, 'DOC-002', 'ana@empresa.pe', 1, '2026-03-09T09:00:00-05:00', NULL, 'PENDIENTE', NULL),
    (3, 'DOC-003', 'ana@empresa.pe', 1, '2026-01-05T09:00:00-05:00', NULL, 'PENDIENTE', 'ESPERANDO_CERTIFICADO'),
    (4, 'DOC-004', 'ana@empresa.pe', 1, '2026-02-01T09:00:00-05:00', NULL, 'PENDIENTE', 'ESPERANDO_CERTIFICADO'),
    (5, 'DOC-005', 'ana@empresa.pe', 1, '2026-01-02T09:00:00-05:00', '2026-01-03T09:00:00-05:00', 'FIRMADA', NULL),
    (6, 'DOC-006', 'ana@empresa.pe', 1, '2026-01-04T09:00:00-05:00', '2026-01-06T09:00:00-05:00', 'RECHAZADA', NULL),
    (7, 'DOC-007', 'ana@empresa.pe', 1, '2026-03-01T09:00:00-05:00', '2026-03-02T09:00:00-05:00', 'EXPIRADA', NULL),
    (8, 'DOC-008', 'bob@empresa.pe', 1, '2026-02-04T09:00:00-05:00', NULL, 'PENDIENTE', NULL);";
}
