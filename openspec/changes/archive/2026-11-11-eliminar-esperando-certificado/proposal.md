# Proposal

## Why

`ESPERANDO_CERTIFICADO` mantiene una ruta de recordatorio y una excepción en el cálculo del SLA, además de existir en el contrato de persistencia. Retirarlo unifica esas solicitudes con `PENDIENTE`, sin perder las filas existentes y conservando su procedencia para auditoría.

## What Changes

- **BREAKING**: retirar `ESPERANDO_CERTIFICADO` como estado admitido por `EstadoSolicitud`, su conversión texto/enum y el `CHECK` de la tabla; actualizar el esquema vigente, las migraciones aplicables y el seed para que no vuelvan a generar ese estado.
- Eliminar el recordatorio específico «Instala tu certificado digital para firmar» de `RecordatorioService`; mantener sin cambios los recordatorios de `PENDIENTE`.
- Incluir las solicitudes migradas en `ReporteSla` según su `creada_en` original, usando el cálculo vigente para `PENDIENTE`.
- Retirar la clasificación específica del estado en `BandejaFirmante`; no modificar la interfaz de usuario.
- Migrar cada fila existente de `ESPERANDO_CERTIFICADO` a `PENDIENTE`, conservando sus demás datos y asignando `migrado_desde = 'ESPERANDO_CERTIFICADO'`; no borrar filas.

## Contexto encontrado

- El `CHECK` de `solicitudes_firma.estado` admite actualmente `ESPERANDO_CERTIFICADO` ([db/schema.sql:9](../../../db/schema.sql#L9)); la migración inicial también lo declara ([db/migrations/001_inicial.sql:9](../../../db/migrations/001_inicial.sql#L9)).
- Los tres consumidores del estado son:
  - `BandejaFirmante`, que clasifica esas filas en la lista `EsperandoCertificado` ([FirmaDocs/Legado/BandejaFirmante.cs:28](../../../FirmaDocs/Legado/BandejaFirmante.cs#L28)).
  - `RecordatorioService`, que envía el aviso para instalar el certificado ([FirmaDocs/Legado/RecordatorioService.cs:30](../../../FirmaDocs/Legado/RecordatorioService.cs#L30)).
  - `ReporteSla`, que las excluye del cálculo ([FirmaDocs/Legado/ReporteSla.cs:8](../../../FirmaDocs/Legado/ReporteSla.cs#L8), [FirmaDocs/Legado/ReporteSla.cs:25](../../../FirmaDocs/Legado/ReporteSla.cs#L25)).
- La base local contiene **250 de 1.000 filas** en ese estado; esas 250 tienen `resuelta_en` y `migrado_desde` nulos ([docs/explore-legado.md:26](../../../docs/explore-legado.md#L26)).

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `solicitudes-firma`: retirar `ESPERANDO_CERTIFICADO`, definir el uso de `PENDIENTE` para las solicitudes afectadas y actualizar las reglas de persistencia y cálculo del SLA.

## Impact

- Código legado: `EstadoSolicitud`, `EstadoSolicitudTexto`, `BandejaFirmante`, `RecordatorioService` y `ReporteSla`.
- Base de datos: `db/schema.sql`, migraciones y `db/seed.sql`; el cambio debe conservar la integridad y los demás datos de las solicitudes migradas.
- Contratos públicos: el miembro del enum `EstadoSolicitud.EsperandoCertificado` deja de estar disponible. `Bandeja.EsperandoCertificado` se conserva por compatibilidad y quedará vacío tras la migración.
- Métricas: el SLA incluirá estas solicitudes en el periodo de su `creada_en` original, siguiendo el tratamiento actual de `PENDIENTE`.

## Fuera de alcance

- Cambios en `CircuitoFirma` o en cualquier otro comportamiento que no dependa de `ESPERANDO_CERTIFICADO`.
- Cambios en la interfaz de usuario.
- Recalcular o modificar reportes históricos ya emitidos.
- Cambiar la lógica de los recordatorios de `PENDIENTE` o el cálculo del SLA para estados distintos de las solicitudes migradas.

## Preguntas abiertas

No quedan preguntas abiertas. Se confirman estas decisiones:

- Migrar las filas existentes y las del seed a `PENDIENTE`, conservando `migrado_desde = 'ESPERANDO_CERTIFICADO'`.
- Conservar `Bandeja.EsperandoCertificado` por compatibilidad, aunque no reciba solicitudes.
