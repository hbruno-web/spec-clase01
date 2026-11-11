# Design

## Context

Ver [proposal.md](./proposal.md) para la motivación y [specs/etapas-de-firma/spec.md](./specs/etapas-de-firma/spec.md) para los requisitos. `CircuitoFirma` calcula los firmantes activos a partir de las aprobaciones de la versión vigente. `Enviar()` y `Reenviar(string)` no reciben un instante; `Aprobar` y `Rechazar` sí reciben `DateTimeOffset`. No hay persistencia ni reloj inyectado en el dominio.

## Goals / Non-Goals

**Goals:**

- Registrar el instante de activación de cada firmante habilitado y mantenerlo estable mientras siga en el mismo turno.
- Consultar explícitamente el vencimiento del turno activo sin leer el reloj del sistema.
- Mantener las firmas públicas existentes y documentar con claridad los flujos que carecen de instante de recepción.

**Non-Goals:**

- Conservar un historial persistente de todos los turnos o introducir persistencia.
- Cambiar la autorización o el resultado de aprobar/rechazar cuando vence el plazo.

## Decisions

1. **Capturar el instante inicial mediante sobrecargas.** Añadir `Enviar(DateTimeOffset instante)` y `Reenviar(string solicitanteId, DateTimeOffset instante)`, conservando los métodos existentes. Las operaciones sin instante continúan funcionando como antes, pero no permiten consultar el vencimiento del turno que activan; la consulta informa ese caso con `CircuitoFirmaException`, nunca con una fecha inventada ni un estado predeterminado. Se descarta usar `DateTimeOffset.Now` o un reloj global porque el dominio requiere instantes explícitos e inyectados.
2. **Guardar solo los instantes de los turnos activos en `CircuitoFirma`.** Un mapa privado por identificador de firmante evita introducir un tipo público o duplicar la configuración de `EtapaFirma`. Al enviar o reenviar con instante, se registra el turno habilitado. Después de cada aprobación, se elimina el turno completado y se registra con el instante de esa aprobación cada firmante que acaba de quedar habilitado. Los firmantes paralelos que siguen pendientes conservan su instante original. No se modifica la firma existente de `Aprobar`.
3. **Calcular el vencimiento desde el instante registrado.** La consulta del turno compara el instante solicitado con `RecibidoEn.AddDays(2)` y devuelve vencido desde la igualdad del límite. Fines de semana y feriados no alteran el cálculo. No se necesita configuración ni dependencia nueva.
4. **Limitar la consulta al turno activo.** Añadir en `CircuitoFirma` una consulta de vencimiento para un firmante habilitado. Si no tiene turno activo o su recepción no fue registrada, lanzar una excepción de dominio con mensaje en español. La consulta no altera el circuito ni bloquea aprobaciones o rechazos tardíos.

### Alternativas descartadas

- Cambiar las firmas existentes de `Enviar` y `Reenviar`: incumple la restricción de compatibilidad pública.
- Usar el reloj del sistema para completar los instantes ausentes: no es determinista y contradice la inyección explícita del tiempo.
- Reiniciar el plazo de los firmantes paralelos restantes después de cada aprobación: extendería el plazo aunque el turno no haya cambiado.
- Introducir calendario laboral o política configurable: no está solicitado y excluiría días que el requisito cuenta.

### Tipos y módulos afectados

- `FirmaDocs/Circuito/CircuitoFirma.cs`: sobrecargas con instante, seguimiento de turnos activos y consulta de vencimiento.
- `FirmaDocs.Tests/Circuito/`: pruebas del límite temporal, activación secuencial/paralela y ausencia de instante.
- No se introduce un tipo de dominio nuevo.

## Risks / Trade-offs

- [Los métodos existentes de envío y reenvío no aportan hora] → Se mantienen por compatibilidad y su turno no es consultable como vencido; documentar y probar el error explícito, y usar las sobrecargas con instante para flujos que requieran el plazo.
- [`DateTimeOffset` no identifica una zona horaria] → Calcular con el desplazamiento del instante recibido y comparar instantes; no aplicar reglas de cambio de horario estacional de una zona no proporcionada.
- [No hay persistencia] → El registro vive en el circuito en memoria, igual que el estado actual del dominio; no prometer continuidad tras reinicios.

## Migration Plan

1. Añadir las sobrecargas con instante y la consulta, sin retirar ni cambiar las firmas actuales.
2. Migrar los puntos de entrada que requieran seguimiento de vencimiento para enviar y reenviar con un `DateTimeOffset` explícito.
3. No se requiere migración de datos ni cambio de dependencias. El rollback consiste en retirar las nuevas operaciones y el seguimiento, dejando disponibles los métodos previos.
