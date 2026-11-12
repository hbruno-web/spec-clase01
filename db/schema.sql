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
