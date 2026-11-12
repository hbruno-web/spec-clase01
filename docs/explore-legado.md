# Mapa del estado `ESPERANDO_CERTIFICADO`

Exploración del módulo legado y de la base local `firmasimple.db`. La base se consultó en modo de solo lectura.

## 1. Contratos públicos y consumidores

Los contratos públicos de `FirmaDocs.Legado` son:

- `EstadoSolicitud`, que incluye `EsperandoCertificado`, y `EstadoSolicitudTexto.ATexto` / `DesdeTexto`, que convierten entre el enum y los valores persistidos ([EstadoSolicitud.cs:5](../FirmaDocs/Legado/EstadoSolicitud.cs#L5), [EstadoSolicitud.cs:16](../FirmaDocs/Legado/EstadoSolicitud.cs#L16), [EstadoSolicitud.cs:29](../FirmaDocs/Legado/EstadoSolicitud.cs#L29)).
- `SolicitudFirma`, record de una fila con id, documento, firmante, vuelta, fechas, estado y origen ([SolicitudFirma.cs:4](../FirmaDocs/Legado/SolicitudFirma.cs#L4)).
- `SolicitudFirmaRepository`, con `Listar`, `ListarPorFirmante`, `CambiarEstado` y `ContarPorEstado` ([SolicitudFirmaRepository.cs:8](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L8), [SolicitudFirmaRepository.cs:20](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L20), [SolicitudFirmaRepository.cs:37](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L37), [SolicitudFirmaRepository.cs:47](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L47), [SolicitudFirmaRepository.cs:67](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L67)).
- `Bandeja`, record con listas `PorFirmar`, `EsperandoCertificado` e `Historial`, y `BandejaFirmante.Armar(email)` ([BandejaFirmante.cs:3](../FirmaDocs/Legado/BandejaFirmante.cs#L3), [BandejaFirmante.cs:18](../FirmaDocs/Legado/BandejaFirmante.cs#L18)).
- `ICanalRecordatorio.Enviar`, usado por `RecordatorioService.EnviarRecordatorios(ahora)` ([ICanalRecordatorio.cs:4](../FirmaDocs/Legado/ICanalRecordatorio.cs#L4), [ICanalRecordatorio.cs:6](../FirmaDocs/Legado/ICanalRecordatorio.cs#L6), [RecordatorioService.cs:16](../FirmaDocs/Legado/RecordatorioService.cs#L16)).
- `ReporteSla.HorasPromedioPorMes()` ([ReporteSla.cs:17](../FirmaDocs/Legado/ReporteSla.cs#L17)).

Dentro del módulo, `BandejaFirmante` consume el repositorio y construye `Bandeja`; `RecordatorioService` consume el repositorio y el canal; `ReporteSla` consume el repositorio. No se encontraron consumidores fuera de `Legado` en el código C# ni en las pruebas. [Program.cs:2](../FirmaDocs/Program.cs#L2) solo imprime “Hello, World!”. Esto no descarta consumidores externos, dado que los tipos son públicos.

## 2. Restricciones de base de datos

- `estado` es `TEXT NOT NULL` y tiene un `CHECK` que permite `PENDIENTE`, `ESPERANDO_CERTIFICADO`, `FIRMADA`, `RECHAZADA` y `EXPIRADA`: [schema.sql:9](../db/schema.sql#L9), [001_inicial.sql:9](../db/migrations/001_inicial.sql#L9).
- Existe un índice no único sobre `estado`: [schema.sql:12](../db/schema.sql#L12), [002_indice_estado.sql:2](../db/migrations/002_indice_estado.sql#L2).
- La base local confirma el CHECK y el índice; no se encontraron triggers asociados a `solicitudes_firma`.

## 3. Filas actuales y comportamiento

En [`firmasimple.db`](../firmasimple.db) hay **250 de 1.000 filas** en `ESPERANDO_CERTIFICADO`. Las 250 tienen `resuelta_en` y `migrado_desde` nulos. Sus fechas de creación van del 6 de mayo de 2025 al 1 de noviembre de 2026. Corresponden a 182 documentos y 8 firmantes; 56 documentos tienen más de una fila en ese estado.

Actualmente, esas solicitudes:

- aparecen en `Bandeja.EsperandoCertificado` ([BandejaFirmante.cs:28](../FirmaDocs/Legado/BandejaFirmante.cs#L28));
- reciben un recordatorio para instalar el certificado en cada invocación, sin condición de antigüedad ([RecordatorioService.cs:30](../FirmaDocs/Legado/RecordatorioService.cs#L30));
- quedan excluidas del SLA: el arreglo de estados que cuentan omite ese estado ([ReporteSla.cs:7](../FirmaDocs/Legado/ReporteSla.cs#L7), [ReporteSla.cs:8](../FirmaDocs/Legado/ReporteSla.cs#L8), [ReporteSla.cs:25](../FirmaDocs/Legado/ReporteSla.cs#L25)).

No hay una transición automática visible: el repositorio permite cambiar directamente a cualquier estado ([SolicitudFirmaRepository.cs:7](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L7)).

## 4. Lecturas, escrituras y duplicaciones

Referencias explícitas al estado en las fuentes:

- Enum y conversiones a/desde texto: [EstadoSolicitud.cs:8](../FirmaDocs/Legado/EstadoSolicitud.cs#L8), [EstadoSolicitud.cs:21](../FirmaDocs/Legado/EstadoSolicitud.cs#L21), [EstadoSolicitud.cs:34](../FirmaDocs/Legado/EstadoSolicitud.cs#L34).
- Contrato y clasificación de bandeja: [BandejaFirmante.cs:5](../FirmaDocs/Legado/BandejaFirmante.cs#L5), [BandejaFirmante.cs:28](../FirmaDocs/Legado/BandejaFirmante.cs#L28).
- Recordatorios: [RecordatorioService.cs:30](../FirmaDocs/Legado/RecordatorioService.cs#L30).
- SLA: el comentario nombra el estado y la lista de strings lo excluye ([ReporteSla.cs:7](../FirmaDocs/Legado/ReporteSla.cs#L7), [ReporteSla.cs:8](../FirmaDocs/Legado/ReporteSla.cs#L8)).
- Persistencia genérica: `Listar` filtra por estado; `ListarPorFirmante` obtiene filas; el lector deserializa el estado; y `CambiarEstado` lo escribe mediante el conversor ([SolicitudFirmaRepository.cs:20](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L20), [SolicitudFirmaRepository.cs:37](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L37), [SolicitudFirmaRepository.cs:47](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L47), [SolicitudFirmaRepository.cs:62](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L62), [SolicitudFirmaRepository.cs:95](../FirmaDocs/Legado/SolicitudFirmaRepository.cs#L95)).
- Esquema y datos de ejemplo: [schema.sql:9](../db/schema.sql#L9), [001_inicial.sql:9](../db/migrations/001_inicial.sql#L9), [seed.sql:2](../db/seed.sql#L2) y 250 filas insertadas distribuidas entre las líneas 18 y 1011 de [`seed.sql`](../db/seed.sql).

## 5. Riesgos de migración

- Las 250 filas necesitan una decisión de destino antes de retirar el valor del CHECK. Conservarlas con el mismo estado ya no sería válido; borrarlas perdería solicitudes, y convertirlas a un estado final no tiene fecha de resolución ni semántica respaldada por los datos.
- Convertirlas a `PENDIENTE` cambiaría su comportamiento: pasarían a la bandeja de firma y a recordatorios condicionados por antigüedad. También alterarían el SLA: las pendientes entran al denominador aunque `resuelta_en` sea nulo —sin sumar horas para ellas— ([ReporteSla.cs:35](../FirmaDocs/Legado/ReporteSla.cs#L35), [ReporteSla.cs:37](../FirmaDocs/Legado/ReporteSla.cs#L37)).
- El cambio del CHECK requiere coordinar esquema y datos; al reconstruir la tabla en SQLite debe conservarse el índice.
- Si se elimina primero el miembro del enum o su conversión, `DesdeTexto` rechazará el valor persistido y las lecturas que pasan por ese conversor fallarán.
- Los contratos son públicos: retirar el miembro del enum o `Bandeja.EsperandoCertificado` puede romper consumidores externos.
- El seed contiene 250 inserciones de ese estado y también debe tratarse para no reintroducirlo.

## Alcance de la evidencia

Al consultar el worktree, `FirmaDocs/Legado/`, `db/` y `firmasimple.db` aparecían como no versionados. Este mapa describe el contenido local inspeccionado y no necesariamente el contenido de una rama confirmada. La exploración fue de solo lectura; no se ejecutaron pruebas.
