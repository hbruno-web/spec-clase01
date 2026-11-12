BEGIN TRANSACTION;

DROP TABLE IF EXISTS solicitudes_firma_nueva;

CREATE TABLE solicitudes_firma_nueva (
  id INTEGER PRIMARY KEY,
  documento_id TEXT NOT NULL,
  firmante_email TEXT NOT NULL,
  vuelta INTEGER NOT NULL DEFAULT 1,
  creada_en TEXT NOT NULL,
  resuelta_en TEXT NULL,
  estado TEXT NOT NULL CHECK (estado IN ('PENDIENTE','FIRMADA','RECHAZADA','EXPIRADA')),
  migrado_desde TEXT NULL
);

INSERT INTO solicitudes_firma_nueva
  (id, documento_id, firmante_email, vuelta, creada_en, resuelta_en, estado, migrado_desde)
SELECT
  id,
  documento_id,
  firmante_email,
  vuelta,
  creada_en,
  resuelta_en,
  CASE WHEN estado = 'ESPERANDO_CERTIFICADO' THEN 'PENDIENTE' ELSE estado END,
  CASE WHEN estado = 'ESPERANDO_CERTIFICADO' THEN 'ESPERANDO_CERTIFICADO' ELSE migrado_desde END
FROM solicitudes_firma;

DROP TABLE solicitudes_firma;
ALTER TABLE solicitudes_firma_nueva RENAME TO solicitudes_firma;
CREATE INDEX ix_solicitudes_estado ON solicitudes_firma(estado);

COMMIT;
