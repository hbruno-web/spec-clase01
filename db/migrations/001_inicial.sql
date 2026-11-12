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
