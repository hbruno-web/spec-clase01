This file is a merged representation of a subset of the codebase, containing specifically included files and files not matching ignore patterns, combined into a single document by Repomix.

# File Summary

## Purpose
This file contains a packed representation of a subset of the repository's contents that is considered the most important context.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.

## File Format
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  a. A header with the file path (## File: path/to/file)
  b. The full contents of the file in a code block

## Usage Guidelines
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.

## Notes
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Only files matching these patterns are included: FirmaDocs/Legado/**, db/**
- Files matching these patterns are excluded: db/seed.sql
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
```
db/migrations/001_inicial.sql
db/migrations/002_indice_estado.sql
db/schema.sql
FirmaDocs/Legado/BandejaFirmante.cs
FirmaDocs/Legado/EstadoSolicitud.cs
FirmaDocs/Legado/ICanalRecordatorio.cs
FirmaDocs/Legado/RecordatorioService.cs
FirmaDocs/Legado/ReporteSla.cs
FirmaDocs/Legado/SolicitudFirma.cs
FirmaDocs/Legado/SolicitudFirmaRepository.cs
```

# Files

## File: db/migrations/001_inicial.sql
```sql
-- 001 · tabla inicial de solicitudes de firma
CREATE TABLE solicitudes_firma (
  id INTEGER PRIMARY KEY,
  documento_id TEXT NOT NULL,
  firmante_email TEXT NOT NULL,
  vuelta INTEGER NOT NULL DEFAULT 1,
  creada_en TEXT NOT NULL,
  resuelta_en TEXT NULL,
  estado TEXT NOT NULL CHECK (estado IN ('PENDIENTE','ESPERANDO_CERTIFICADO','FIRMADA','RECHAZADA','EXPIRADA')),
  migrado_desde TEXT NULL
);
```

## File: db/migrations/002_indice_estado.sql
```sql
-- 002 · índice por estado (la bandeja y el reporte filtran por estado)
CREATE INDEX ix_solicitudes_estado ON solicitudes_firma(estado);
```

## File: db/schema.sql
```sql
-- Esquema vigente de FirmaSimple (equivale a migrations/001 + 002).
CREATE TABLE solicitudes_firma (
  id INTEGER PRIMARY KEY,
  documento_id TEXT NOT NULL,
  firmante_email TEXT NOT NULL,
  vuelta INTEGER NOT NULL DEFAULT 1,
  creada_en TEXT NOT NULL,          -- ISO 8601, p. ej. 2026-05-04T09:30:00-05:00
  resuelta_en TEXT NULL,
  estado TEXT NOT NULL CHECK (estado IN ('PENDIENTE','ESPERANDO_CERTIFICADO','FIRMADA','RECHAZADA','EXPIRADA')),
  migrado_desde TEXT NULL
);
CREATE INDEX ix_solicitudes_estado ON solicitudes_firma(estado);
```

## File: FirmaDocs/Legado/BandejaFirmante.cs
```csharp
namespace FirmaDocs.Legado;

public record Bandeja(
    List<SolicitudFirma> PorFirmar,
    List<SolicitudFirma> EsperandoCertificado,
    List<SolicitudFirma> Historial);

// Arma la bandeja que ve un firmante al entrar a FirmaSimple.
public class BandejaFirmante
{
    private readonly SolicitudFirmaRepository _repo;

    public BandejaFirmante(SolicitudFirmaRepository repo)
    {
        _repo = repo;
    }

    public Bandeja Armar(string email)
    {
        var porFirmar = new List<SolicitudFirma>();
        var esperando = new List<SolicitudFirma>();
        var historial = new List<SolicitudFirma>();

        foreach (var s in _repo.ListarPorFirmante(email))
        {
            if (s.Estado == EstadoSolicitud.Pendiente)
                porFirmar.Add(s);
            else if (s.Estado == EstadoSolicitud.EsperandoCertificado)
                esperando.Add(s); // seccion "Esperando certificado"
            else
                historial.Add(s);
        }

        return new Bandeja(porFirmar, esperando, historial);
    }
}
```

## File: FirmaDocs/Legado/EstadoSolicitud.cs
```csharp
namespace FirmaDocs.Legado;

// Estados de una solicitud de firma en FirmaSimple.
// Los textos deben coincidir con el CHECK de db/schema.sql.
public enum EstadoSolicitud
{
    Pendiente,
    EsperandoCertificado,
    Firmada,
    Rechazada,
    Expirada
}

public static class EstadoSolicitudTexto
{
    public static string ATexto(EstadoSolicitud estado)
    {
        switch (estado)
        {
            case EstadoSolicitud.Pendiente: return "PENDIENTE";
            case EstadoSolicitud.EsperandoCertificado: return "ESPERANDO_CERTIFICADO";
            case EstadoSolicitud.Firmada: return "FIRMADA";
            case EstadoSolicitud.Rechazada: return "RECHAZADA";
            case EstadoSolicitud.Expirada: return "EXPIRADA";
            default: throw new ArgumentOutOfRangeException(nameof(estado));
        }
    }

    public static EstadoSolicitud DesdeTexto(string texto)
    {
        switch (texto)
        {
            case "PENDIENTE": return EstadoSolicitud.Pendiente;
            case "ESPERANDO_CERTIFICADO": return EstadoSolicitud.EsperandoCertificado;
            case "FIRMADA": return EstadoSolicitud.Firmada;
            case "RECHAZADA": return EstadoSolicitud.Rechazada;
            case "EXPIRADA": return EstadoSolicitud.Expirada;
            default: throw new ArgumentException("Estado desconocido: " + texto, nameof(texto));
        }
    }
}
```

## File: FirmaDocs/Legado/ICanalRecordatorio.cs
```csharp
namespace FirmaDocs.Legado;

// Canal por el que salen los recordatorios (en producción, correo).
public interface ICanalRecordatorio
{
    void Enviar(string email, string mensaje);
}
```

## File: FirmaDocs/Legado/RecordatorioService.cs
```csharp
namespace FirmaDocs.Legado;

// Envía recordatorios a los firmantes con solicitudes abiertas.
public class RecordatorioService
{
    private readonly SolicitudFirmaRepository _repo;
    private readonly ICanalRecordatorio _canal;

    public RecordatorioService(SolicitudFirmaRepository repo, ICanalRecordatorio canal)
    {
        _repo = repo;
        _canal = canal;
    }

    // Devuelve cuántos recordatorios se enviaron.
    public int EnviarRecordatorios(DateTimeOffset ahora)
    {
        var enviados = 0;

        foreach (var s in _repo.Listar(EstadoSolicitud.Pendiente))
        {
            if ((ahora - s.CreadaEn).TotalDays > 2)
            {
                // TODO: plantilla configurable
                _canal.Enviar(s.FirmanteEmail, "Tienes un documento por firmar: " + s.DocumentoId);
                enviados++;
            }
        }

        foreach (var s in _repo.Listar(EstadoSolicitud.EsperandoCertificado))
        {
            // a estos se les avisa siempre, sin importar la antigüedad
            _canal.Enviar(s.FirmanteEmail, "Instala tu certificado digital para firmar: " + s.DocumentoId);
            enviados++;
        }

        return enviados;
    }
}
```

## File: FirmaDocs/Legado/ReporteSla.cs
```csharp
namespace FirmaDocs.Legado;

// Reporte de nivel de servicio: horas promedio entre que se crea una
// solicitud y se resuelve, agrupado por mes de creación ("yyyy-MM").
public class ReporteSla
{
    // ESPERANDO_CERTIFICADO no cuenta: el reloj no corre hasta tener certificado.
    private static readonly string[] EstadosQueCuentan = { "PENDIENTE", "FIRMADA", "RECHAZADA", "EXPIRADA" };

    private readonly SolicitudFirmaRepository _repo;

    public ReporteSla(SolicitudFirmaRepository repo)
    {
        _repo = repo;
    }

    public Dictionary<string, double> HorasPromedioPorMes()
    {
        var horas = new Dictionary<string, double>();
        var cantidad = new Dictionary<string, int>();

        foreach (var s in _repo.Listar())
        {
            var texto = EstadoSolicitudTexto.ATexto(s.Estado);
            if (!EstadosQueCuentan.Contains(texto))
                continue;

            var mes = s.CreadaEn.ToString("yyyy-MM");
            if (!horas.ContainsKey(mes))
            {
                horas[mes] = 0;
                cantidad[mes] = 0;
            }

            if (s.ResueltaEn != null)
                horas[mes] += (s.ResueltaEn.Value - s.CreadaEn).TotalHours;

            cantidad[mes]++;
        }

        var promedio = new Dictionary<string, double>();
        foreach (var mes in horas.Keys)
        {
            promedio[mes] = Math.Round(horas[mes] / cantidad[mes], 2);
        }
        return promedio;
    }
}
```

## File: FirmaDocs/Legado/SolicitudFirma.cs
```csharp
namespace FirmaDocs.Legado;

// Una fila de la tabla solicitudes_firma.
public record SolicitudFirma(
    int Id,
    string DocumentoId,
    string FirmanteEmail,
    int Vuelta,
    DateTimeOffset CreadaEn,
    DateTimeOffset? ResueltaEn,
    EstadoSolicitud Estado,
    string? MigradoDesde);
```

## File: FirmaDocs/Legado/SolicitudFirmaRepository.cs
```csharp
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
```
