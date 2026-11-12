## Stack

- .NET 10 · C# 14 · xUnit · NSubstitute · consola. Sin paquetes nuevos sin aprobación explícita.

## Comandos

- Compilar: `dotnet build` · Probar: `dotnet test` (siempre antes de dar una tarea por terminada).

## Estilo

- Namespace `FirmaDocs` (código) y `FirmaDocs.Tests.<Carpeta>` (tests); un tipo por archivo; una carpeta por capacidad (`FirmaDocs/Circuito/`).
- Nullable habilitado; records para valores; `DateTimeOffset` para instantes, siempre inyectado.
- XML docs en español en todo miembro público. Tests: `Metodo_Escenario_Resultado` con el id del RF al final (`..._RF03`).
- Excepción: tests de caracterización de código legado → `Caracterizacion_<Clase>_<Escenario>` en `FirmaDocs.Tests/Legado/`, sin id de RF. Fijan el comportamiento actual.

## Límites

- No cambiar firmas públicas existentes. No tocar `Program.cs` salvo que la tarea lo pida.
- No modificar tests existentes para que pasen: si un test falla, corrige el código o pregunta.
- Las specs viven en `specs/<nnn>_<feature>/`; toda tarea cita el RF que cubre.
