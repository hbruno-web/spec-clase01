# Tasks

## 1. Configuración de etapas

- [x] 1.1 Añadir pruebas xUnit para RF-12 y RF-13 que cubran límites inclusivos/exclusivos de etapas y firmantes, modos permitidos/no permitidos y firmantes repetidos; comprobar que los nombres de prueba terminan en `_RF12` o `_RF13` y ejecutar `dotnet test`.
- [x] 1.2 Crear `EtapaFirma` y `ModoEtapaFirma`, y añadir `CircuitoFirma.CrearConEtapas` con validación de cantidad de etapas, límites por etapa y unicidad de firmantes; documentar los miembros públicos en español y comprobar que las pruebas RF-12/RF-13 y las pruebas secuenciales existentes pasan con `dotnet test`.

## 2. Flujo secuencial entre etapas

- [x] 2.1 Añadir pruebas xUnit para RF-14 y RF-16 que verifiquen el orden de aprobación, el rechazo de aprobaciones fuera de turno y la habilitación de la etapa siguiente solo al completar la actual; comprobar que los nombres de prueba terminan en `_RF14` o `_RF16` y ejecutar `dotnet test`.
- [x] 2.2 Adaptar `CircuitoFirma` para avanzar por firmantes y etapas secuenciales usando únicamente las firmas de la versión vigente; conservar `FirmanteEnTurno`, añadir `FirmantesPendientes` y documentar en español los miembros públicos nuevos; comprobar las pruebas RF-14/RF-16 y la suite existente con `dotnet test`.

## 3. Flujo paralelo

- [x] 3.1 Añadir pruebas xUnit para RF-15 y RF-16 que verifiquen aprobaciones en cualquier orden, que la etapa no se complete mientras falte una aprobación y que una etapa futura no acepte aprobaciones; comprobar que los nombres de prueba terminan en `_RF15` o `_RF16` y ejecutar `dotnet test`.
- [x] 3.2 Implementar la elegibilidad de todos los firmantes pendientes de una etapa paralela y avanzar solo cuando todos hayan aprobado; verificar que `FirmantesPendientes` los expone y que las pruebas RF-15/RF-16 pasan con `dotnet test`.

## 4. Integración con rechazo y nueva vuelta

- [x] 4.1 Después de completar los prerrequisitos RF-08 y RF-11 de `specs/001_rechazo_y_vueltas/tasks.md`, añadir una prueba de integración para RF-17 que rechace en una etapa posterior y reenvíe; verificar que el nombre termina en `_RF17`, que la nueva vuelta comienza en la primera etapa y que las firmas anteriores se conservan sin contar para su avance.
- [x] 4.2 Ajustar la integración de `Reenviar` con el avance por etapas para que una versión nueva empiece en la primera etapa sin borrar historiales; documentar cualquier miembro público nuevo o modificado en español y comprobar que la prueba RF-17 pasa con `dotnet test`.

## 5. Verificación integral

- [x] 5.1 Ejecutar `dotnet build`, `dotnet test` y `openspec validate etapas-paralelas --type change --strict`; confirmar que pasan las pruebas existentes sin modificarlas y que las pruebas nuevas cubren RF-12 a RF-17.
