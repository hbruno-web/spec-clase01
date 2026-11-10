# Notas de sesión 1

- Archivos tocados: `FirmaDocs/Circuito/CircuitoFirma.cs`, `FirmaDocs/Circuito/EstadoDocumento.cs`, `FirmaDocs/Circuito/Rechazo.cs`, `FirmaDocs.Tests/Circuito/CircuitoFirmaTests.cs`, `docs/notas-sesion-1.md`.
- ¿Cambió la firma de `Aprobar` o del constructor? No. `Aprobar(Firmante, DateTimeOffset)` y el constructor conservan su firma.
- ¿Rompió o editó alguno de los 5 tests existentes? No. Los cinco siguen intactos y pasan.
- ¿Creó tests nuevos? Sí. ¿Cuántos? 5.
- Tras reenviar, ¿quién queda en turno? El primer firmante del circuito. ¿Las aprobaciones de la vuelta anterior cuentan, se borran o se conservan? Se conservan en el historial, pero no cuentan para completar la nueva versión.
- ¿Hay un máximo de vueltas? Sí. Al rechazar la versión 3, el documento pasa a `Rechazado` y no admite más reenvíos.
- ¿Es obligatorio el motivo del rechazo? Sí; si está vacío o contiene solo espacios, se lanza `CircuitoFirmaException` con el mensaje especificado.
- ¿Quién puede reenviar? Solo el autor, validado mediante su identificador.
- ¿Cuántas veces hubo que reformular? 0.
- ¿Cuántos tests llevan el id del RF en el nombre? 5 (RF-01 a RF-05).

## Evaluación del Lab 6

| Criterio | 0 | 1 | 2 | Puntaje |
|---|---|---|---|---:|
| Respetó `AGENTS.md` (nombres de tests, XML docs en español, instante inyectado) | no | parcial | sí | 2 |
| Mostró el test en rojo antes de implementar | nunca | a veces | siempre | 0 |
| Tocó solo los archivos de la tarea | no | 1 extra | sí | 2 |
| No modificó tests existentes ni borró historial de firmas | no | — | sí | 2 |
| Cada tarea cerró con `dotnet test` en verde | no | parcial | sí | 2 |

Puntaje total: **8/10**.

- RF-02 y RF-03 tienen nombres que incluyen su requisito y usan el instante inyectado; no se añadieron miembros públicos que requirieran XML docs.
- Las pruebas de RF-02 y RF-03 pasaron en su primera ejecución porque la implementación existente ya cumplía los requisitos; no se mostró rojo.
- En estas tareas solo se modificaron el archivo de pruebas y el seguimiento solicitado en `tasks.md`.
- No se alteraron pruebas existentes ni se borró historial de firmas; ambas tareas cerraron con `dotnet test` en verde.
