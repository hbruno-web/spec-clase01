# Constitución de FirmaDocs

1.  Ningún requisito sin test; ningún test sin requisito.
2.  TDD: primero el test en rojo, luego el código mínimo, luego refactor. Un commit por ciclo.
3.  Sin lógica de negocio en `Program.cs` ni en consola: todo en clases de dominio probables.
4.  Errores de dominio explícitos (`CircuitoFirmaException` u otra propia, mensaje en español), nunca `null` silencioso.
5.  Sin `DateTime.Now` ni `DateTimeOffset.Now` en la lógica: el instante se inyecta.
6.  Una firma registrada nunca se borra ni se modifica: el historial es evidencia.
7.  Un cambio = una spec + un plan + tareas trazables. Sin spec no hay implementación.
8.  El agente propone; el equipo aprueba cada artefacto antes del siguiente.
