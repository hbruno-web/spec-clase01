# Tasks

## 1. Consulta del plazo (RF-19)

- [x] 1.1 Actualizar las pruebas en `PlazoTurnoFirmaTests` para los límites antes/en/después, el vencimiento del viernes 13 y sábado 14 de noviembre el martes 17, y los feriados oficiales del 8 y 9 de diciembre; verificar que las pruebas de días hábiles fallen antes de la implementación (RF-19).
- [x] 1.2 Implementar `CalendarioLaboralPeruLima` para contar el segundo día hábil posterior a la recepción, saltar fines de semana y feriados oficiales aplicables en Lima, y conservar la hora local; mantener las APIs públicas y verificar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-19).

## 2. Recepción de turnos secuenciales y paralelos (RF-18)

- [x] 2.1 Agregar pruebas que verifiquen la recepción del siguiente turno secuencial en el instante de aprobación y que las aprobaciones parciales paralelas no reinicien el plazo de los firmantes pendientes; confirmar que fallen antes de implementar el seguimiento (RF-18).
- [x] 2.2 Registrar los instantes de los turnos activos en `CircuitoFirma`, asignar el instante de activación a los nuevos firmantes habilitados y conservar el de quienes siguen pendientes; comprobar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-18).

## 3. Reenvío y compatibilidad (RF-18, RF-19)

- [x] 3.1 Agregar pruebas para `Reenviar(string, DateTimeOffset)`, verificando que la nueva vuelta comienza con la recepción del primer turno en el instante de reenvío y no reutiliza la recepción anterior; comprobar el fallo esperado antes de implementar la sobrecarga (RF-18).
- [x] 3.2 Implementar la sobrecarga de reenvío, reiniciar los turnos activos de la versión y verificar que los métodos antiguos conservan su comportamiento y que consultar un turno sin recepción produce una excepción explícita; comprobar con `dotnet test --filter "FullyQualifiedName~PlazoTurnoFirmaTests"` (RF-18, RF-19).

## 4. Verificación integrada

- [x] 4.1 Ejecutar `dotnet build`, `dotnet test` y `openspec validate "plazo-turno-firma" --type change --strict`; resolver fallos vinculados al registro o cálculo de vencimientos y confirmar que las pruebas cubren ambos RF (RF-18, RF-19).
