using System.Globalization;
using Microsoft.Data.Sqlite;

namespace FirmaDocs.Legado;

// Acceso a la tabla solicitudes_firma. No valida transiciones de estado:
// cualquier estado puede pasar a cualquier otro.
public class SolicitudFirmaRepository
{
    private const string Columnas =
        "id, documento_id, firmante_email, vuelta, creada_en, resuelta_en, estado, migrado_desde";

    private readonly string _connectionString;

    public SolicitudFirmaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<SolicitudFirma> Listar(EstadoSolicitud? estado = null)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        if (estado == null)
        {
            cmd.CommandText = $"SELECT {Columnas} FROM solicitudes_firma ORDER BY id";
        }
        else
        {
            cmd.CommandText = $"SELECT {Columnas} FROM solicitudes_firma WHERE estado = $estado ORDER BY id";
            cmd.Parameters.AddWithValue("$estado", EstadoSolicitudTexto.ATexto(estado.Value));
        }
        return Leer(cmd);
    }

    public List<SolicitudFirma> ListarPorFirmante(string email)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columnas} FROM solicitudes_firma WHERE firmante_email = $email ORDER BY creada_en";
        cmd.Parameters.AddWithValue("$email", email);
        return Leer(cmd);
    }

    public void CambiarEstado(int id, EstadoSolicitud nuevo, DateTimeOffset instante)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        // resuelta_en solo se rellena en los estados finales
        if (nuevo == EstadoSolicitud.Firmada || nuevo == EstadoSolicitud.Rechazada || nuevo == EstadoSolicitud.Expirada)
        {
            cmd.CommandText = "UPDATE solicitudes_firma SET estado = $estado, resuelta_en = $instante WHERE id = $id";
            cmd.Parameters.AddWithValue("$instante", instante.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }
        else
        {
            cmd.CommandText = "UPDATE solicitudes_firma SET estado = $estado WHERE id = $id";
        }
        cmd.Parameters.AddWithValue("$estado", EstadoSolicitudTexto.ATexto(nuevo));
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<string, int> ContarPorEstado()
    {
        var resultado = new Dictionary<string, int>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT estado, COUNT(*) FROM solicitudes_firma GROUP BY estado";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            resultado[reader.GetString(0)] = reader.GetInt32(1);
        }
        return resultado;
    }

    private static List<SolicitudFirma> Leer(SqliteCommand cmd)
    {
        var lista = new List<SolicitudFirma>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new SolicitudFirma(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                DateTimeOffset.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                reader.IsDBNull(5) ? null : DateTimeOffset.Parse(reader.GetString(5), CultureInfo.InvariantCulture),
                EstadoSolicitudTexto.DesdeTexto(reader.GetString(6)),
                reader.IsDBNull(7) ? null : reader.GetString(7)));
        }
        return lista;
    }
}
