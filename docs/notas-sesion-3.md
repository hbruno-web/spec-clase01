# Notas de sesión 3

## Bug observado en el reporte SLA

`ReporteSla` agrega al denominador las solicitudes `PENDIENTE` aunque no tengan `resuelta_en`, pero no agrega horas para esas filas. Esto las trata efectivamente como resoluciones de cero horas y sesga hacia abajo el promedio de solicitudes resueltas.

La base temporal de caracterización muestra el caso en enero de 2026: dos solicitudes resueltas suman 24 y 48 horas, y una pendiente sigue sin resolverse. El reporte actual devuelve 24 horas (`72 / 3`), en vez del promedio de 36 horas de las dos solicitudes resueltas (`72 / 2`). El test `Caracterizacion_ReporteSla_PendienteSinResolverReduceElPromedio` fija el resultado actual.

La spec base `openspec/specs/solicitudes-firma/spec.md` no define el cálculo del SLA, por lo que el test no la contradice. No se modificó código de producción; la corrección del promedio queda para otro cambio con su propia spec.
