# Plan: rechazo y nuevas vueltas

## 1. Archivos a crear o modificar

### Modificar

- `FirmaDocs/Circuito/CircuitoFirma.cs`
  - Añadir el historial de rechazos y sus validaciones.
  - Añadir el reenvío exclusivo del autor y el control del máximo de tres vueltas.
  - Calcular turno y aprobación usando solo las firmas de la versión vigente; conservar las firmas anteriores sin alterarlas.
- `FirmaDocs/Circuito/EstadoDocumento.cs`
  - Añadir los estados `Devuelto` y `Rechazado`, con XML docs en español.

### Crear

- `FirmaDocs/Circuito/Rechazo.cs`
  - Tipo de valor para cada rechazo registrado.
- `FirmaDocs.Tests/Circuito/RechazoYVueltasTests.cs`
  - Pruebas nuevas para RF-01 a RF-11, sin modificar los cinco tests existentes de `CircuitoFirmaTests.cs`.

No se modifican el constructor, `Enviar` ni `Aprobar`, tampoco `Program.cs` ni los archivos de proyecto.

## 2. Tipos y miembros públicos nuevos

- `public record Rechazo(Firmante Firmante, string Motivo, int Version, DateTimeOffset Instante)`
  - Representa la evidencia de un rechazo, conservando el motivo exactamente como se recibió y el instante proporcionado por quien invoca la operación.
- En `EstadoDocumento`, los valores `Devuelto` y `Rechazado`.
- En `CircuitoFirma`:
  - `public IReadOnlyList<Rechazo> Rechazos { get; }`, historial cronológico separado de `Firmas`.
  - `public void Rechazar(Firmante firmante, string motivo, DateTimeOffset instante)`, registra el rechazo del firmante en turno.
  - `public void Reenviar(string solicitanteId)`, permite al autor reenviar un documento devuelto.

Los miembros públicos nuevos llevarán XML docs en español. El máximo de tres vueltas se mantendrá como regla interna fija, no como configuración pública.

## 3. Cálculo del turno y aprobación por versión

La versión inicial permanece en 1 y cada reenvío válido incrementa `Version` exactamente en uno. Para determinar el turno, obtener el número de firmas cuya `Firma.Version` coincide con la versión vigente y usar ese número como índice en la lista ordenada de firmantes. `FirmanteEnTurno` solo expondrá ese firmante cuando `Estado` sea `EnCircuito`.

La aprobación de la versión vigente se determina con ese mismo conteo filtrado: solo cuando alcanza el número total de firmantes se pasa a `Aprobado`. No se usará `_firmas.Count` sin filtrar, porque contiene también las aprobaciones de vueltas previas. En `Reenviar`, la versión nueva empieza con cero firmas contabilizables y el turno vuelve al primer firmante; no se borran, reescriben ni reordenan los registros históricos.

## 4. Alternativas de diseño descartadas

1. **Vaciar el historial de firmas al reenviar.** Se descarta porque contradice RF-11 y elimina evidencia de aprobaciones anteriores. Se conserva un único historial y se filtra por versión al calcular el progreso vigente.
2. **Guardar un contador de turno mutable, reiniciándolo en cada reenvío.** Se descarta porque duplica información derivable de las firmas, puede desincronizarse ante transiciones o fallos parciales y no demuestra qué versión aprobó cada firmante. El turno se deriva de las firmas de la versión actual.

## 5. Riesgos y mitigaciones

- **Contar firmas antiguas por error:** filtrar siempre por `Version` tanto para el turno como para completar la vuelta; probar aprobación parcial anterior, reenvío, turno inicial y aprobación completa de la nueva versión (RF-08, RF-11).
- **Cambios parciales cuando una operación falla:** validar estado, identidad, motivo y turno antes de añadir historial, incrementar versión o cambiar estado; comprobar en pruebas que cada error no modifica estado, versión ni historiales.
- **Perder o alterar evidencia:** mantener listas de firmas y rechazos separadas, agregar registros cronológicamente y comprobar los datos completos de ambos historiales (RF-06, RF-11).
- **Reenvío después de la tercera vuelta o desde un estado inválido:** al rechazar en versión 3, registrar primero el rechazo y dejar estado `Rechazado`; aceptar reenvíos únicamente desde `Devuelto`. Cubrir límites y errores con pruebas (RF-07, RF-10).
- **Validaciones concurrentes:** la especificación deja la concurrencia sin definir. Este cambio no añadirá sincronización; se documentará como limitación y se mantendrá el ámbito de las operaciones secuenciales del dominio.
- **Precedencia de errores cuando coinciden varias entradas inválidas:** la especificación la deja abierta. Definir un orden determinista acorde con las validaciones existentes y probar los errores requeridos individualmente; no asumir que los casos con múltiples errores tienen precedencia contractual sin decisión del equipo.

## 6. Orden de implementación

1. Crear pruebas xUnit para RF-01 a RF-11, con los identificadores al final de sus nombres, cubriendo también los errores sin mutaciones parciales y conservando intactos los tests existentes.
2. Crear el record `Rechazo` y añadir `Devuelto` y `Rechazado` al enum, con documentación XML en español.
3. Implementar el historial de rechazos y `Rechazar`: validar estado, turno y motivo; preservar literalmente el motivo; registrar firmante, versión e instante; seleccionar `Devuelto` o `Rechazado` según la versión vigente.
4. Implementar `Reenviar(string solicitanteId)`: validar estado y autor, incrementar versión y reanudar el circuito en el primer firmante sin modificar historiales anteriores.
5. Ajustar `FirmanteEnTurno` y la transición a `Aprobado` para contar exclusivamente firmas de la versión vigente.
6. Ejecutar `dotnet test`, verificar los criterios CF-01 a CF-07 y corregir cualquier regresión antes de considerar terminado el cambio.
