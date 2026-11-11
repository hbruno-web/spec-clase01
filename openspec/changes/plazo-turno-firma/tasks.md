# Tasks

## 1. Consulta del plazo (RF-19)

- [ ] 1.1 Agregar pruebas en `PlazoTurnoFirmaTests` para consultas antes, exactamente en y después de `RecibidoEn + 2 días`, incluyendo el ejemplo de viernes a domingo y el error explícito para un turno iniciado sin instante; verificar que las pruebas nuevas fallen antes de la implementación (RF-19).
- [ ] 1.2 Añadir `Enviar(DateTimeOffset)` y la consulta del vencimiento del firmante activo, conservando las firmas existentes; calcular el límite y lanzar `CircuitoFirmaException` si falta la recepción; documentar los miembros públicos en español y comprobar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-19).

## 2. Recepción de turnos secuenciales y paralelos (RF-18)

- [ ] 2.1 Agregar pruebas que verifiquen la recepción del siguiente turno secuencial en el instante de aprobación y que las aprobaciones parciales paralelas no reinicien el plazo de los firmantes pendientes; confirmar que fallen antes de implementar el seguimiento (RF-18).
- [ ] 2.2 Registrar los instantes de los turnos activos en `CircuitoFirma`, asignar el instante de activación a los nuevos firmantes habilitados y conservar el de quienes siguen pendientes; comprobar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-18).

## 3. Reenvío y compatibilidad (RF-18, RF-19)

- [ ] 3.1 Agregar pruebas para `Reenviar(string, DateTimeOffset)`, verificando que la nueva vuelta comienza con la recepción del primer turno en el instante de reenvío y no reutiliza la recepción anterior; comprobar el fallo esperado antes de implementar la sobrecarga (RF-18).
- [ ] 3.2 Implementar la sobrecarga de reenvío, reiniciar los turnos activos de la versión y verificar que los métodos antiguos conservan su comportamiento y que consultar un turno sin recepción produce una excepción explícita; comprobar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-18, RF-19).

## 4. Verificación integrada

- [ ] 4.1 Ejecutar `dotnet build`, `dotnet test` y `openspec validate --change "plazo-turno-firma" --strict`; resolver fallos vinculados al registro o cálculo de vencimientos y confirmar que las pruebas cubren ambos RF (RF-18, RF-19).
