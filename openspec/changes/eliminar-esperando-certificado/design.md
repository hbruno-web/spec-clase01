# Design

## Context

Ver `proposal.md` para la motivación y `specs/solicitudes-firma/spec.md` para los requisitos RF-06 a RF-08.

La tabla `solicitudes_firma` usa SQLite y su `CHECK` incluye actualmente `ESPERANDO_CERTIFICADO`. El esquema vigente replica las migraciones 001 y 002; la tabla tiene un índice por estado y `migrado_desde` ya existe. SQLite no permite modificar directamente la restricción `CHECK`, por lo que eliminar el estado requiere reconstruir la tabla.

El estado se convierte entre texto y enum en `EstadoSolicitudTexto` y es consumido por `SolicitudFirmaRepository`, `BandejaFirmante`, `RecordatorioService` y `ReporteSla`. La bandeja expone `Bandeja.EsperandoCertificado`, un miembro público cuya forma se conserva según la regla del proyecto de no cambiar contratos públicos existentes.

## Goals / Non-Goals

**Goals:**

- Retirar el estado del enum, su conversión textual y las nuevas restricciones de persistencia.
- Migrar los datos existentes de forma atómica, conservando las columnas y fechas originales, y registrando su procedencia.
- Mantener el recordatorio vigente de `PENDIENTE`, incluir esos registros en el SLA y clasificarlos como pendientes.
- Preservar el miembro público `Bandeja.EsperandoCertificado`; quedará vacío para los datos válidos después de la migración.
- Alinear el esquema vigente y el seed con las migraciones.

**Non-Goals:**

- Cambiar la firma pública de `Bandeja` ni las reglas de recordatorio de `PENDIENTE`.
- Cambiar el cálculo del SLA para estados distintos de los migrados.
- Cambiar la interfaz de usuario, `Program.cs` o el circuito de firma.
- Recalcular reportes históricos ya emitidos.

## Decisions

1. **Reconstruir la tabla con una nueva migración SQLite.** Agregar una migración posterior a `002_indice_estado.sql` que ejecute la reconstrucción en una transacción. La migración será idempotente: en cada ejecución elimina cualquier tabla temporal residual con `DROP TABLE IF EXISTS`, la crea desde cero y copia desde la tabla vigente; al terminar, no queda una tabla temporal. La copia mapea `ESPERANDO_CERTIFICADO` a `PENDIENTE` y asigna `migrado_desde = 'ESPERANDO_CERTIFICADO'` solo a esas filas; preserva todas las demás columnas y valores. Se descarta modificar solo el `CHECK`, porque SQLite no admite esa operación directa, y se descarta actualizar filas sin cambiar la restricción, porque el esquema seguiría admitiendo un estado retirado.

2. **Mantener alineados el esquema vigente y el seed con el resultado de la migración.** `db/schema.sql` tendrá el mismo `CHECK` que la tabla posterior a la migración. Las filas del seed que hoy representan el estado retirado se expresarán como `PENDIENTE` y con el marcador `ESPERANDO_CERTIFICADO`, tal como confirmó el usuario. Así el seed no vuelve a introducir el estado y conserva la procedencia de esos registros. Se descarta dejar el estado antiguo en el seed, porque sería incompatible con el nuevo esquema.

3. **Eliminar el estado de los consumidores, preservando la forma pública de la bandeja.** `EstadoSolicitud` y `EstadoSolicitudTexto` dejarán de aceptar o producir el valor retirado. `RecordatorioService` procesará solo pendientes, `ReporteSla` contará los estados admitidos con su regla vigente y `BandejaFirmante` colocará los pendientes en `PorFirmar`; `Bandeja.EsperandoCertificado` se conservará como lista vacía. Se descarta eliminar esa propiedad porque cambiaría un contrato público existente.

4. **Verificar migración y comportamiento mediante pruebas enfocadas a los RF.** Las pruebas comprobarán RF-06 a RF-08: estados admitidos y rechazados, transiciones sin validación y migración con conservación de datos. Además verificarán los efectos en consumidores descritos en la propuesta: clasificación como pendiente, ausencia del aviso de certificado e inclusión en el SLA del periodo original. Se mantendrán las pruebas de los otros estados y del recordatorio normal de pendientes.

## Risks / Trade-offs

- [La reconstrucción podría perder filas, datos o el índice si falla a mitad] → Ejecutar todas las operaciones de creación, copia, sustitución y recreación del índice en una sola transacción; probar con filas de ambos estados y verificar columnas, cantidad e índice después de aplicar la migración.
- [Un rollback podría restaurar el estado legado en filas cuyo estado cambió después de la migración] → Limitar el script inverso a restauración operativa sobre una copia o durante una reversión controlada; reconstruir el estado desde `migrado_desde` y verificar el alcance antes de ejecutarlo.
- [Clientes que referencian `EstadoSolicitud.EsperandoCertificado` dejarán de compilar] → La eliminación del miembro del enum es parte explícita del cambio; conservar el miembro público de `Bandeja` reduce el cambio de compatibilidad fuera de ese enum.
- [El seed ya no reproduce el estado legado como valor de estado] → Conservar `migrado_desde` en los registros de seed para representar su procedencia sin volver a insertar el estado retirado.

## Migración de datos

La migración hacia delante se ejecuta en una única transacción y reconstruye `solicitudes_firma` conservando sus columnas (`id`, `documento_id`, `firmante_email`, `vuelta`, `creada_en`, `resuelta_en`, `estado`, `migrado_desde`):

1. Crear una tabla temporal nueva con las mismas columnas y restricciones, pero con un `CHECK` de `estado` que admita únicamente `PENDIENTE`, `FIRMADA`, `RECHAZADA` y `EXPIRADA`; no incluir `ESPERANDO_CERTIFICADO`.
2. Copiar todas las filas. Para cada fila cuyo estado sea `ESPERANDO_CERTIFICADO`, escribir `PENDIENTE` y `migrado_desde = 'ESPERANDO_CERTIFICADO'`; para las demás, preservar `estado` y `migrado_desde`. Copiar sin cambios los otros campos.
3. Eliminar `solicitudes_firma`, renombrar la tabla temporal a `solicitudes_firma` y recrear `ix_solicitudes_estado` sobre `estado`.

El script es idempotente: una segunda ejecución reconstruye la tabla ya migrada sin volver a alterar estados ni marcadores. Las operaciones de creación, copia, reemplazo y creación del índice deben quedar dentro de la transacción para que un error deje intacta la tabla original.

El script de rollback también reconstruye la tabla dentro de una transacción, usando el `CHECK` original que incluye `ESPERANDO_CERTIFICADO`. Al copiar, debe restaurar `estado = 'ESPERANDO_CERTIFICADO'` cuando `migrado_desde` tenga ese valor y limpiar ese marcador para esas filas, reproduciendo la situación anterior a la migración; para las demás filas conserva estado, marcador y demás columnas. Finalmente elimina la tabla vigente, renombra la temporal y recrea `ix_solicitudes_estado`. El rollback no se ejecuta sobre la base de trabajo compartida ni sustituye la copia de seguridad operativa.

**Prueba local del rollback sobre una copia:**

1. Con la base local cerrada para escrituras, crear una copia de `firmasimple.db` (por ejemplo, con `.backup` de SQLite) y ejecutar todos los pasos siguientes únicamente sobre esa copia.
2. Capturar en la copia la cantidad total y los conteos por estado; ejecutar la migración hacia delante dos veces. Verificar que la segunda ejecución no cambia los datos, que las filas de origen aparecen como `PENDIENTE` con el marcador correcto, que se conservan las demás columnas y que existe `ix_solicitudes_estado`.
3. Ejecutar el script de rollback sobre la copia. Comparar los conteos por estado con los capturados antes de migrar; verificar que las filas marcadas vuelven a `ESPERANDO_CERTIFICADO`, que su `migrado_desde` vuelve a `NULL`, que el resto de datos no cambia, que el índice existe y que el `CHECK` original acepta el valor legado. Repetir el rollback y comprobar que el resultado permanece igual.
4. No aplicar el rollback a la base original durante esta prueba. Si alguna verificación falla, conservar la copia para diagnóstico y descartar/recrear solo la copia de prueba antes de repetir.

El esquema vigente y el seed deben reflejar el resultado posterior a la migración, sin insertar el estado retirado.

## Consumidores

Cada consumidor se actualiza y se verifica de forma independiente, manteniendo el cambio acotado a la semántica definida en RF-06 y RF-08:

- **BandejaFirmante:** clasificar las solicitudes migradas como `PENDIENTE` en `PorFirmar`, sin retirar el miembro público `Bandeja.EsperandoCertificado`; verificar la clasificación y que esa lista quede vacía para los datos migrados.
- **RecordatorioService:** retirar el mensaje específico para instalar el certificado y tratar las solicitudes migradas según la regla existente de `PENDIENTE`; verificar que no se envía el mensaje retirado y que se mantiene el recordatorio normal de pendientes.
- **ReporteSla:** incluir las solicitudes migradas con su `creada_en` original usando la regla vigente para `PENDIENTE`; verificar el periodo y el cálculo sin alterar el tratamiento de los demás estados.

La conversión texto/enum y la persistencia compartida se actualizan para admitir únicamente los estados vigentes; se verifica que el estado retirado no pueda leerse ni escribirse después de la migración.
