# Design

## Context

Ver [proposal.md](./proposal.md) para la motivación y [specs/etapas-de-firma/spec.md](./specs/etapas-de-firma/spec.md) para los requisitos. `CircuitoFirma` calcula los firmantes activos a partir de las aprobaciones de la versión vigente. `Enviar()` y `Reenviar(string)` no reciben un instante; `Aprobar` y `Rechazar` sí reciben `DateTimeOffset`. No hay persistencia ni reloj inyectado en el dominio.

## Goals / Non-Goals

**Goals:**

- Registrar el instante de activación de cada firmante habilitado y mantenerlo estable mientras siga en el mismo turno.
- Consultar explícitamente el vencimiento del turno activo sin leer el reloj del sistema.
- Calcular el límite con el calendario hábil de Lima, incluyendo los feriados oficiales aplicables.
- Mantener las firmas públicas existentes y documentar con claridad los flujos que carecen de instante de recepción.

**Non-Goals:**

- Conservar un historial persistente de todos los turnos o introducir persistencia.
- Cambiar la autorización o el resultado de aprobar/rechazar cuando vence el plazo.

## Decisions

1. **Capturar el instante inicial mediante sobrecargas.** Añadir `Enviar(DateTimeOffset instante)` y `Reenviar(string solicitanteId, DateTimeOffset instante)`, conservando los métodos existentes. Las operaciones sin instante continúan funcionando como antes, pero no permiten consultar el vencimiento del turno que activan; la consulta informa ese caso con `CircuitoFirmaException`, nunca con una fecha inventada ni un estado predeterminado. Se descarta usar `DateTimeOffset.Now` o un reloj global porque el dominio requiere instantes explícitos e inyectados.
2. **Guardar solo los instantes de los turnos activos en `CircuitoFirma`.** Un mapa privado por identificador de firmante evita introducir un tipo público o duplicar la configuración de `EtapaFirma`. Al enviar o reenviar con instante, se registra el turno habilitado. Después de cada aprobación, se elimina el turno completado y se registra con el instante de esa aprobación cada firmante que acaba de quedar habilitado. Los firmantes paralelos que siguen pendientes conservan su instante original. No se modifica la firma existente de `Aprobar`.
3. **Calcular el límite en el calendario local de Lima.** Convertir la recepción a la zona `America/Lima`, avanzar por las fechas locales posteriores y contar solo lunes a viernes que no sean feriados oficiales aplicables en Lima. La fecha límite conserva la hora local de recepción; una recepción en sábado, domingo o feriado empieza a contar desde el siguiente día hábil. Comparar instantes y declarar el turno vencido desde la igualdad del límite. Se descarta `AddDays(2)`, que cuenta fines de semana, y el cálculo solo por días de semana, que contaría feriados.
4. **Aislar el calendario oficial.** Añadir `CalendarioLaboralPeruLima` en `FirmaDocs/Circuito/` para determinar si una fecha es hábil y obtener el segundo día hábil. El calendario usará feriados oficiales aplicables a Lima y no incluirá días no laborables institucionales ni calendarios configurables. No se requiere paquete externo.
5. **Limitar la consulta al turno activo.** Mantener en `CircuitoFirma` la consulta de vencimiento para un firmante habilitado. Si no tiene turno activo o su recepción no fue registrada, lanzar una excepción de dominio con mensaje en español. La consulta no altera el circuito ni bloquea aprobaciones o rechazos tardíos.

### Alternativas descartadas

- Cambiar las firmas existentes de `Enviar` y `Reenviar`: incumple la restricción de compatibilidad pública.
- Usar el reloj del sistema para completar los instantes ausentes: no es determinista y contradice la inyección explícita del tiempo.
- Reiniciar el plazo de los firmantes paralelos restantes después de cada aprobación: extendería el plazo aunque el turno no haya cambiado.
- Contar solo lunes a viernes sin consultar feriados: no cumple la decisión de excluir los feriados oficiales aplicables a Lima.
- Añadir soporte para calendarios de otras regiones o días no laborables institucionales: no está solicitado.

### Tipos y módulos afectados

- `FirmaDocs/Circuito/CircuitoFirma.cs`: sobrecargas con instante, seguimiento de turnos activos y consulta de vencimiento.
- `FirmaDocs/Circuito/CalendarioLaboralPeruLima.cs`: cálculo de días hábiles y consulta del calendario de feriados oficiales.
- `FirmaDocs.Tests/Circuito/`: pruebas del límite temporal, activación secuencial/paralela y ausencia de instante.
- No se requieren dependencias nuevas.

## Risks / Trade-offs

- [Los métodos existentes de envío y reenvío no aportan hora] → Se mantienen por compatibilidad y su turno no es consultable como vencido; documentar y probar el error explícito, y usar las sobrecargas con instante para flujos que requieran el plazo.
- [La lista de feriados oficiales puede cambiar] → Aislar el calendario de Lima y mantener sus fechas a partir de las disposiciones oficiales; cubrir los cambios con pruebas de calendario.
- [`DateTimeOffset` no identifica una zona horaria] → Convertir la recepción a `America/Lima` para contar días y conservar la hora local; comparar el resultado como instante.
- [No hay persistencia] → El registro vive en el circuito en memoria, igual que el estado actual del dominio; no prometer continuidad tras reinicios.

## Migration Plan

1. Reemplazar el cálculo de días calendario por el calendario hábil de Lima y mantener las pruebas de turnos y recepción ya implementadas.
2. No se requiere migración de datos ni nuevas dependencias. El rollback consiste en restaurar el cálculo anterior de dos días calendario sin retirar las operaciones de recepción ni la consulta.
