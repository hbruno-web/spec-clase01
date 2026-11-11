# Proposal

## Why

Los circuitos no tienen un plazo explícito para que cada firmante responda, por lo que no se puede saber si un turno sigue dentro del tiempo esperado. Este cambio establece un plazo de dos días calendario y permite consultar si un turno ya venció en un instante dado.

## What Changes

- Registrar el instante en que cada firmante recibe su turno, tanto en etapas secuenciales como paralelas.
- Calcular el vencimiento como el instante de recepción más dos días calendario, incluyendo fines de semana y feriados.
- Exponer una consulta que indique si el turno está vencido en un instante proporcionado; se considera vencido desde el instante exacto del límite.
- Conservar la aprobación y el rechazo como decisiones explícitas del firmante; el vencimiento solo informa el estado del turno.
- Mantener las firmas públicas existentes y añadir operaciones con `DateTimeOffset` explícito para iniciar o reiniciar turnos con seguimiento de vencimiento. Los turnos iniciados por las operaciones antiguas, que no reciben instante, no podrán consultarse como vencidos y deberán reportar explícitamente la falta de ese dato.

### Fuera de alcance

- Aprobación o rechazo automáticos, bloqueo de respuestas tardías, notificaciones, escalaciones o cambios de turno por vencimiento.
- Calendarios laborales, exclusión de feriados o políticas de plazo configurables.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `etapas-de-firma`: añadir el plazo por firmante y la consulta de vencimiento para un instante dado.

## Impact

- Dominio de `FirmaDocs/Circuito/`: se requiere modelar la recepción de cada turno y calcular su vencimiento para el firmante activo.
- Pruebas de `FirmaDocs.Tests/Circuito/`: cubrir cálculo del plazo, consulta antes/en/después del límite, etapas paralelas y cambios de turno.
- Se añadirán sobrecargas compatibles con `Enviar()` y `Reenviar(string)`; las operaciones actuales se conservan sin inferir la hora de recepción.
- No se requieren dependencias nuevas ni cambios en persistencia (actualmente no existe).

## Preguntas abiertas

Ninguna.
