# Tasks

## 1. Retirar el estado del modelo y la persistencia (RF-06, RF-07)

- [ ] 1.1 Agregar pruebas para los cuatro estados admitidos, el rechazo de `ESPERANDO_CERTIFICADO` y los cambios entre estados reconocidos sin validar transiciones; verificar que las pruebas cubren RF-06 y RF-07.
- [ ] 1.2 Retirar `ESPERANDO_CERTIFICADO` de `EstadoSolicitud` y `EstadoSolicitudTexto` y mantener la persistencia limitada a los estados reconocidos; verificar con las pruebas de RF-06 y RF-07.

## 2. Migrar las solicitudes existentes (RF-08)

- [ ] 2.1 Crear la migración SQLite transaccional e idempotente que reconstruye `solicitudes_firma`, convierte el estado retirado en `PENDIENTE`, conserva los demás datos y el índice, y proporciona su rollback; verificar dos ejecuciones consecutivas y rollback sobre una copia de la base.
- [ ] 2.2 Alinear `db/schema.sql` y `db/seed.sql` con los estados admitidos y la marca de procedencia definida; verificar que el seed carga bajo el esquema nuevo y conserva la marca en los registros convertidos.

## 3. Actualizar los consumidores y verificar el comportamiento (RF-08)

- [ ] 3.1 Agregar pruebas y actualizar `BandejaFirmante`, `RecordatorioService` y `ReporteSla` para tratar las solicitudes convertidas como pendientes, conservar `Bandeja.EsperandoCertificado` vacía y mantener el periodo original del SLA; verificar los escenarios de consumidores descritos en el diseño.

## 4. Verificación integrada

- [ ] 4.1 Ejecutar `dotnet build` y `dotnet test`, y verificar en una base de prueba que la migración conserva filas, columnas e índice, rechaza el estado retirado y deja intactos los estados no afectados (RF-06, RF-08).
