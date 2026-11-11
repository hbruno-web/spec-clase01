# Spec Delta

## ADDED Requirements

### Requirement: RF-18 Registro de recepción del turno
CUANDO se activa un turno de firma con un instante de recepción conocido, el sistema DEBERÁ (SHALL) registrar ese instante para cada firmante habilitado, sin reiniciarlo mientras el mismo turno siga activo.

#### Scenario: Activación de turno secuencial
- **GIVEN** un firmante secuencial aprueba en el instante registrado y queda habilitado el siguiente firmante
- **WHEN** se activa el turno del siguiente firmante
- **THEN** el sistema registra como recepción el instante de esa aprobación

#### Scenario: Activación de turno paralelo
- **GIVEN** una etapa paralela queda activa en un instante conocido
- **WHEN** se habilita a sus firmantes pendientes
- **THEN** el sistema registra ese instante como recepción para cada firmante habilitado

#### Scenario: Aprobación parcial en etapa paralela
- **GIVEN** un firmante aprobó en una etapa paralela y quedan otros firmantes en el mismo turno
- **WHEN** se consulta la recepción del turno de un firmante pendiente
- **THEN** el sistema conserva el instante original de activación de la etapa

### Requirement: RF-19 Consulta de vencimiento del turno
CUANDO se consulte un turno con instante de recepción conocido, el sistema DEBERÁ (SHALL) indicar que está vencido desde el mismo horario local del segundo día hábil posterior a la recepción, contando de lunes a viernes y excluyendo sábados, domingos y feriados oficiales aplicables en Lima, Perú.

#### Scenario: Consulta antes del vencimiento
- **GIVEN** el turno se recibió el viernes 13 de noviembre de 2026 a las 10:00, hora local de Lima
- **WHEN** se consulta el vencimiento el lunes 16 de noviembre de 2026 a las 10:00
- **THEN** el sistema indica que el turno no está vencido

#### Scenario: Consulta exactamente al vencimiento
- **GIVEN** el turno se recibió el viernes 13 de noviembre de 2026 a las 10:00, hora local de Lima
- **WHEN** se consulta el martes 17 de noviembre de 2026 a las 10:00
- **THEN** el sistema indica que el turno está vencido

#### Scenario: Consulta después del vencimiento
- **GIVEN** el turno se recibió el viernes 13 de noviembre de 2026 a las 10:00, hora local de Lima
- **WHEN** se consulta después del martes 17 de noviembre de 2026 a las 10:00
- **THEN** el sistema indica que el turno está vencido

#### Scenario: Turno recibido en sábado
- **GIVEN** el turno se recibió el sábado 14 de noviembre de 2026 a las 10:00, hora local de Lima
- **WHEN** se consulta el martes 17 de noviembre de 2026 a las 10:00
- **THEN** el sistema indica que el turno está vencido

#### Scenario: Feriados oficiales no cuentan como días hábiles
- **GIVEN** el turno se recibió el lunes 7 de diciembre de 2026 a las 10:00, hora local de Lima, y el martes 8 y miércoles 9 son feriados oficiales aplicables
- **WHEN** se consulta el viernes 11 de diciembre de 2026 a las 10:00
- **THEN** el sistema indica que el turno está vencido

#### Scenario: Turno sin instante de recepción
- **GIVEN** un turno fue iniciado por una operación que no registró su instante de recepción
- **WHEN** se consulta si está vencido
- **THEN** el sistema informa explícitamente que no hay instante de recepción disponible y no devuelve un estado de vencimiento
