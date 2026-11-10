# Rechazo y nueva vuelta · spec v0

## Contexto

Hoy CircuitoFirma solo permite aprobar en orden. El negocio necesita que un firmante pueda rechazar y que el autor corrija y reenvíe el documento, sin perder el historial de firmas.

## Requisitos funcionales (EARS)

- RF-01 · CUANDO el firmante en turno rechace con un motivo, el sistema DEBERÁ pasar el documento a Devuelto y registrar el rechazo con firmante, motivo, versión e instante.
- RF-02 · CUANDO el autor reenvíe un documento Devuelto, el sistema DEBERÁ incrementar la versión en 1, pasar a EnCircuito y dar el turno al primer firmante.
- RF-03 · El sistema DEBERÁ conservar las firmas de versiones anteriores en el historial y NO DEBERÁ contarlas para aprobar la versión vigente.
- RF-04 · SI se rechaza en la vuelta 3, ENTONCES el sistema DEBERÁ pasar el documento a Rechazado y no admitir más reenvíos.
- RF-05 · SI el motivo de rechazo está vacío o solo tiene espacios, ENTONCES el sistema DEBERÁ lanzar CircuitoFirmaException con el mensaje "El motivo de rechazo es obligatorio.".

## Criterios de finalización

- Tests xUnit en FirmaDocs.Tests, uno por RF, con el id del RF en el nombre del test.
- Firmas públicas existentes intactas (constructor, Enviar, Aprobar) y los 5 tests existentes en verde sin modificarlos.
- XML docs en español.
