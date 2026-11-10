# Rechazo y nueva vuelta

## 1. Contexto

Hoy el circuito de firma solo permite aprobar en orden. El negocio necesita que un firmante pueda rechazar el documento para que el autor lo corrija y reenvíe, conservando el historial de firmas y rechazos.

La versión inicial corresponde a la vuelta 1. Cada reenvío incrementa la versión y comienza una nueva vuelta; hay un máximo fijo de 3 vueltas. El contenido del documento no está modelado actualmente.

## 2. Historias de usuario

- Como firmante en turno quiero rechazar un documento indicando un motivo para dejar constancia de lo que debe corregirse.
- Como autor quiero reenviar un documento devuelto para iniciar una nueva vuelta de revisión.
- Como participante del circuito quiero consultar el historial de rechazos para conocer los motivos, firmantes, versiones e instantes de cada rechazo.
- Como participante del circuito quiero conservar las firmas anteriores para mantener evidencia de las aprobaciones por versión.

## 3. Requisitos funcionales (EARS)

- RF-01 · CUANDO el firmante en turno rechace el documento con un motivo válido, el sistema DEBERÁ registrar el rechazo con firmante, motivo, versión vigente e instante, y pasar el documento a `Devuelto`, excepto cuando aplique RF-07.
- RF-02 · CUANDO un firmante que no está en turno intente rechazar, el sistema DEBERÁ lanzar `CircuitoFirmaException` con el mismo error que al intentar aprobar fuera de turno.
- RF-03 · MIENTRAS el documento no esté `EnCircuito`, el sistema NO DEBERÁ admitir un rechazo y DEBERÁ lanzar `CircuitoFirmaException` con el mismo error de estado que al intentar aprobar.
- RF-04 · SI el motivo está vacío o contiene solo espacios, ENTONCES el sistema DEBERÁ lanzar `CircuitoFirmaException` con el mensaje `"El motivo de rechazo es obligatorio."`.
- RF-05 · CUANDO se registre un motivo válido, el sistema DEBERÁ conservar exactamente el texto ingresado, sin recortar espacios ni aplicar un límite de longitud adicional.
- RF-06 · El sistema DEBERÁ exponer el historial de rechazos separado del historial de firmas, en orden cronológico, conservando para cada rechazo firmante, motivo, versión e instante.
- RF-07 · CUANDO se rechace el documento en la vuelta 3, el sistema DEBERÁ registrar el rechazo y pasar el documento a `Rechazado`; NO DEBERÁ admitir reenvíos posteriores.
- RF-08 · CUANDO `AutorId` reenvíe un documento `Devuelto`, el sistema DEBERÁ incrementar la versión en 1, cambiar el estado a `EnCircuito` y asignar el turno al primer firmante.
- RF-09 · SI una persona distinta de `AutorId` intenta reenviar el documento, ENTONCES el sistema DEBERÁ lanzar `CircuitoFirmaException` con el mensaje `"Solo el autor puede reenviar el documento."`.
- RF-10 · SI se intenta reenviar el documento en un estado distinto de `Devuelto`, ENTONCES el sistema DEBERÁ lanzar `CircuitoFirmaException`.
- RF-11 · MIENTRAS se procesa una versión vigente, el sistema DEBERÁ contar únicamente las aprobaciones de esa versión para determinar el turno y su aprobación; NO DEBERÁ borrar ni modificar firmas de versiones anteriores.

## 4. Casos límite y errores

- Motivo vacío o compuesto solo por espacios → lanzar `CircuitoFirmaException` con `"El motivo de rechazo es obligatorio."`; no registrar rechazo ni cambiar el estado.
- Motivo válido con espacios al inicio o al final → conservarlo exactamente como fue ingresado.
- Rechazo fuera de turno → lanzar `CircuitoFirmaException` con el mismo error que al aprobar fuera de turno; no registrar rechazo ni cambiar el estado.
- Rechazo cuando el documento no está `EnCircuito` → lanzar `CircuitoFirmaException` con el mismo error de estado que al aprobar; no registrar rechazo ni cambiar el estado.
- Rechazo en las vueltas 1 o 2 → registrar el rechazo de la versión vigente y dejar el documento `Devuelto`.
- Rechazo en la vuelta 3 → registrar el rechazo de la versión vigente y dejar el documento `Rechazado`.
- Reenvío por una persona distinta de `AutorId` → lanzar `CircuitoFirmaException` con `"Solo el autor puede reenviar el documento."`; no incrementar la versión ni cambiar el estado.
- Reenvío en un estado distinto de `Devuelto`, incluido `Rechazado` → lanzar `CircuitoFirmaException`; no incrementar la versión ni cambiar el estado.
- Reenvío válido → incrementar la versión, iniciar el turno en el primer firmante y no contar aprobaciones de versiones anteriores para el turno ni para completar la versión nueva.
- Firmas de versiones anteriores → mantenerlas sin borrar ni modificar; sus registros no cuentan para aprobar la versión vigente.
- Acciones concurrentes sobre el mismo documento → el comportamiento no está definido; ver Preguntas abiertas.

## 5. Criterios de finalización

- CF-01 · `dotnet test` termina correctamente.
- CF-02 · Cada RF-01 a RF-11 tiene al menos un test xUnit que cita su identificador en el nombre.
- CF-03 · Las firmas públicas existentes del constructor, `Enviar` y `Aprobar` permanecen intactas.
- CF-04 · Los 5 tests existentes pasan sin modificarse.
- CF-05 · Los miembros públicos nuevos tienen XML docs en español.
- CF-06 · Los tests verifican la conservación cronológica de los rechazos con su firmante, motivo, versión e instante, así como la conservación de las firmas anteriores.
- CF-07 · Los tests verifican el estado, la versión y el firmante en turno después de cada operación válida, y que las operaciones inválidas no produzcan cambios parciales.

## Preguntas abiertas

- ¿Se puede reenviar sin cambiar el contenido? El contenido no está modelado actualmente.
- ¿Qué mensaje exacto corresponde al reenvío desde un estado distinto de `Devuelto`?
- ¿Qué regla de atomicidad aplica si coinciden acciones concurrentes sobre el mismo documento?
- ¿Qué precedencia tienen las validaciones si una acción incumple varias reglas a la vez?
