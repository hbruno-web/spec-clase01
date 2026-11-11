# Design

## Context

Ver `proposal.md` para la motivación y `specs/etapas-de-firma/spec.md` para los requisitos. El dominio actual concentra el ciclo de vida en `CircuitoFirma`, que mantiene una lista plana de firmantes y firmas y deriva el turno de la cantidad total de firmas. La nueva representación debe admitir varios modos por etapa, respetar la firma secuencial existente y conservar el historial por versión. Se mantienen las restricciones de `AGENTS.md` y `docs/constitution.md`: firmas públicas existentes intactas, lógica en el dominio, errores explícitos y ningún cambio a `Program.cs`.

## Goals / Non-Goals

**Goals:**
- Representar etapas y su modo en el dominio, con validación de los límites y del firmante único en el circuito.
- Calcular elegibilidad y avance a partir de las aprobaciones de la versión vigente.
- Mantener compatibles el constructor, `Enviar`, `Aprobar` y el comportamiento secuencial existente.

**Non-Goals:**
- Agregar persistencia, configuración externa, notificaciones, plazos o garantías de concurrencia.
- Cambiar la política vigente de rechazo, reenvío, versiones o conservación del historial.

## Decisions

1. **Modelar la configuración con `EtapaFirma` y `ModoEtapaFirma`.** `EtapaFirma` será un valor inmutable con firmantes ordenados y modo `SECUENCIAL` o `PARALELA`; `ModoEtapaFirma` será un enum. La etapa validará que incluya de 1 a 5 firmantes y copie la colección recibida a una vista de solo lectura. El circuito validará de 1 a 5 etapas y que ningún identificador de firmante se repita entre etapas, conservando el invariante existente. Un tipo explícito hace visibles los límites y el modo en el modelo; una lista plana con indicadores separados no representaría claramente la configuración ni impediría combinaciones inválidas.

2. **Añadir `CircuitoFirma.CrearConEtapas` en vez de sobrecargar el constructor actual.** El constructor existente se conserva y adapta sus firmantes a una única etapa `SECUENCIAL`. Una sobrecarga con otra colección podría volver ambiguas llamadas existentes basadas en expresiones de colección de C#; una fábrica con nombre permite configurar etapas sin cambiar firmas públicas ni romper esa forma de invocación.

3. **Derivar el avance de las firmas de la versión vigente.** `CircuitoFirma` conservará una sola secuencia cronológica de `Firma`. La etapa activa será la primera cuyos firmantes no hayan aprobado en la versión vigente. En una etapa secuencial, solo el primer firmante pendiente es elegible; en una paralela, lo son todos los pendientes. La etapa queda completa cuando todos sus firmantes tienen una firma de la versión vigente. Filtrar por versión reutiliza el historial existente y evita que una vuelta anterior avance la nueva; derivar el estado evita mantener cursores y conjuntos de aprobaciones duplicados que podrían desincronizarse.

4. **Preservar `FirmanteEnTurno` y exponer `FirmantesPendientes`.** La propiedad existente seguirá devolviendo el firmante elegible en modo secuencial y `null` cuando no exista un turno único, incluido el modo paralelo. Una propiedad nueva, `FirmantesPendientes`, devolverá el firmante único de una etapa secuencial o todos los pendientes de una paralela, y una colección vacía cuando el documento no esté en circuito. Así se conserva la interpretación actual del turno sin ocultar a los participantes habilitados en paralelo.

5. **Validar cada acción contra la etapa activa antes de registrar una firma.** `Aprobar` solo aceptará firmantes elegibles de la etapa activa y mantendrá el error de dominio explícito para acciones fuera de turno o de etapa. Cuando todos los firmantes de una etapa hayan aprobado, la siguiente etapa pasará a ser la activa; tras la última, el documento pasará a `Aprobado`. Para conservar la regla vigente de rechazo, en una etapa paralela cualquiera de sus firmantes pendientes se considera habilitado para rechazar; un firmante de una etapa futura o que ya aprobó no lo está.

6. **Reiniciar el avance usando la nueva versión, no borrando firmas.** Al iniciar o reenviar una vuelta, el cálculo de etapa activa se hará con la versión vigente y comenzará en la primera etapa. Las firmas anteriores permanecerán intactas y no contarán para la vuelta nueva, manteniendo el modelo de evidencia actual.

7. **Mantener los cambios dentro del dominio y las pruebas existentes.** Se modificarán `CircuitoFirma` y sus pruebas en `FirmaDocs/Circuito/` y `FirmaDocs.Tests/Circuito/`. Se añadirán archivos para los tipos nuevos, con documentación XML pública en español, sin modificar `Program.cs` ni incorporar paquetes. Las pruebas cubrirán RF-12 a RF-17 y mantendrán las pruebas existentes de la API secuencial.

## Risks / Trade-offs

- [La derivación repetida del avance recorre las firmas de la versión y los firmantes configurados] → Ambos límites son cinco por etapa y cinco etapas, por lo que la búsqueda queda acotada; se prioriza un estado único sobre optimizar prematuramente.
- [El límite de cinco firmantes por circuito implícito en una única etapa puede rechazar llamadas antiguas con más de cinco firmantes] → El constructor conserva su firma, pero aplica RF-12; los clientes que necesiten más de cinco firmantes deberán distribuirlos en etapas secuenciales de hasta cinco.
- [Una nueva propiedad y el uso de `null` en `FirmanteEnTurno` en paralelo pueden requerir cambios en consumidores] → Documentar que solo representa un turno único y usar `FirmantesPendientes` para ambas modalidades.
- [El agregado no ofrece seguridad para operaciones concurrentes sobre la misma instancia] → Mantener este comportamiento fuera del alcance, coherente con la especificación vigente; no prometer atomicidad ni introducir sincronización en este cambio.

## Migration Plan

No hay migración de datos porque el circuito es actualmente en memoria y no se agrega persistencia. El constructor legado seguirá creando una etapa secuencial. Antes de adoptar el cambio, revisar llamadas que configuren más de cinco firmantes; si deben conservar su orden, distribuirlos en etapas secuenciales consecutivas. Para revertir, retirar el uso de la fábrica y los nuevos tipos junto con los cambios de dominio; no se requiere revertir datos almacenados.
