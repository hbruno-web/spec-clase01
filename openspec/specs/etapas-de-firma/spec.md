# etapas-de-firma Specification

## Purpose

Define circuitos de firma compuestos por etapas configurables, para coordinar aprobaciones secuenciales o paralelas y mantener el avance entre etapas y vueltas.

## Requirements

### Requirement: RF-12 Límites de etapas y firmantes
CUANDO se configure un circuito, el sistema DEBERÁ (SHALL) admitir de 1 a 5 etapas ordenadas y de 1 a 5 firmantes por etapa, y DEBERÁ rechazar configuraciones fuera de esos límites.

#### Scenario: Configuración dentro de los límites
- **GIVEN** cada etapa tiene entre 1 y 5 firmantes y el circuito tiene entre 1 y 5 etapas
- **WHEN** se configura el circuito
- **THEN** el sistema acepta la configuración y conserva el orden de las etapas y sus firmantes

#### Scenario: Configuración fuera de los límites
- **GIVEN** el circuito tiene cero o más de 5 etapas, o una etapa tiene cero o más de 5 firmantes
- **WHEN** se configura el circuito
- **THEN** el sistema rechaza la configuración

### Requirement: RF-13 Modo de cada etapa
CUANDO se configure una etapa, el sistema DEBERÁ (SHALL) requerir que su modo sea `SECUENCIAL` o `PARALELA`.

#### Scenario: Modo permitido
- **GIVEN** una etapa está configurada con el modo `SECUENCIAL` o `PARALELA`
- **WHEN** se configura el circuito
- **THEN** el sistema acepta el modo de la etapa

#### Scenario: Modo no permitido
- **GIVEN** una etapa está configurada con un modo distinto de `SECUENCIAL` y `PARALELA`
- **WHEN** se configura el circuito
- **THEN** el sistema rechaza la configuración

### Requirement: RF-14 Aprobaciones en etapa secuencial
MIENTRAS una etapa está en modo `SECUENCIAL`, el sistema DEBERÁ (SHALL) solicitar las aprobaciones en el orden configurado y avanzar al siguiente firmante solo después de la aprobación del firmante en turno.

#### Scenario: Aprobación del firmante en turno
- **GIVEN** una etapa secuencial tiene firmantes ordenados y el primer firmante está en turno
- **WHEN** el firmante en turno aprueba
- **THEN** el sistema registra la aprobación y asigna el turno al siguiente firmante de esa etapa, si lo hay

#### Scenario: Aprobación fuera de orden
- **GIVEN** una etapa secuencial tiene un firmante en turno y otro firmante posterior en el orden configurado
- **WHEN** el firmante posterior intenta aprobar antes de su turno
- **THEN** el sistema rechaza la aprobación y conserva el turno vigente

### Requirement: RF-15 Aprobaciones en etapa paralela
MIENTRAS una etapa está en modo `PARALELA`, el sistema DEBERÁ (SHALL) aceptar las aprobaciones de sus firmantes en cualquier orden y considerar completa la etapa solo cuando todos hayan aprobado.

#### Scenario: Aprobaciones en cualquier orden
- **GIVEN** una etapa paralela tiene varios firmantes pendientes
- **WHEN** sus firmantes aprueban en un orden distinto al configurado
- **THEN** el sistema registra cada aprobación y completa la etapa al registrarse la aprobación de todos sus firmantes

#### Scenario: Aprobaciones incompletas
- **GIVEN** al menos un firmante de la etapa paralela aún no ha aprobado
- **WHEN** se procesa una aprobación de otro firmante de esa etapa
- **THEN** el sistema no considera completa la etapa

### Requirement: RF-16 Avance entre etapas
MIENTRAS el circuito tenga etapas pendientes, el sistema DEBERÁ (SHALL) habilitar las aprobaciones de una etapa únicamente cuando la etapa anterior esté completa.

#### Scenario: Habilitar la etapa siguiente
- **GIVEN** todos los firmantes de una etapa han aprobado y existe una etapa siguiente
- **WHEN** se completa la etapa actual
- **THEN** el sistema habilita la etapa siguiente y no solicita aprobaciones de etapas posteriores

#### Scenario: Etapa anterior incompleta
- **GIVEN** una etapa no está completa y existe una etapa posterior
- **WHEN** un firmante de la etapa posterior intenta aprobar
- **THEN** el sistema rechaza la aprobación y conserva el avance de la etapa actual

### Requirement: RF-17 Rechazo y nueva vuelta en etapas
CUANDO un documento devuelto se reenvíe conforme a las reglas vigentes, el sistema DEBERÁ (SHALL) iniciar la nueva vuelta desde la primera etapa sin contar aprobaciones de vueltas anteriores.

#### Scenario: Reenvío tras rechazo
- **GIVEN** un documento fue devuelto después de un rechazo en cualquier etapa
- **WHEN** su autor lo reenvía válidamente
- **THEN** el sistema inicia una vuelta nueva en la primera etapa y conserva el historial sin usar aprobaciones anteriores para completar la nueva vuelta

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
