-- 002 · índice por estado (la bandeja y el reporte filtran por estado)
CREATE INDEX ix_solicitudes_estado ON solicitudes_firma(estado);
