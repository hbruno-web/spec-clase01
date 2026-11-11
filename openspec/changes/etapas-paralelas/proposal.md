# Proposal

## Why

El circuito actual solo permite aprobar en secuencia; no puede representar una etapa en la que varios firmantes revisen el documento en paralelo. Se necesita definir etapas secuenciales o paralelas con límites claros y conservar el comportamiento existente de rechazo y nuevas vueltas.

## What Changes

- Permitir de 1 a 5 etapas ordenadas por circuito y de 1 a 5 firmantes por etapa.
- Permitir que cada etapa opere en modo `SECUENCIAL` o `PARALELA`.
- En una etapa secuencial, requerir las aprobaciones en el orden configurado de sus firmantes.
- En una etapa paralela, exigir la aprobación de todos sus firmantes, sin importar el orden en que aprueben.
- Habilitar la etapa siguiente solo cuando termine la actual.
- Mantener el comportamiento vigente ante un rechazo: devolver el documento según las reglas actuales y, al reenviarlo, iniciar una vuelta nueva desde la primera etapa.

## Capabilities

### New Capabilities

- `etapas-de-firma`: etapas ordenadas de firma con modos secuencial y paralelo.

### Modified Capabilities

No hay capacidades registradas en `openspec/specs`; el catálogo está vacío. El comportamiento de rechazo y vueltas documentado en `specs/001_rechazo_y_vueltas/spec.md` se conserva, no se redefine.

## Impact

- Dominio del circuito de firma y sus pruebas, para representar etapas, firmantes y avance entre etapas.
- La representación del circuito deberá expresar entre 1 y 5 etapas, sus firmantes y el modo de cada etapa; la superficie para configurar esos datos queda fuera de alcance.
- No se proponen dependencias nuevas ni cambios a la política existente de rechazo, reenvío e historial.

## Fuera de alcance

- Quórum parcial, como “2 de 3”; una etapa paralela requiere la aprobación de todos sus firmantes.
- Notificaciones.
- Persistencia.
- Plazos.
- Una superficie de configuración para las etapas, los firmantes y sus modos.

## Preguntas abiertas

Ninguna pendiente por ahora.
