# Spec Delta

## REMOVED Requirements

### Requirement: RF-02 Estado ESPERANDO_CERTIFICADO
**Reason**: El estado deja de ser admitido; las solicitudes existentes se convierten en `PENDIENTE`.
**Migration**: Migrar las solicitudes persistidas con ese estado conforme a RF-08 antes de rechazar el valor retirado en nuevas escrituras.

## MODIFIED Requirements

### Requirement: RF-06 Valores de estado admitidos
CUANDO se persista el estado de una solicitud, el sistema DEBERÁ (SHALL) admitir únicamente `PENDIENTE`, `FIRMADA`, `RECHAZADA` o `EXPIRADA`, y deberá rechazar cualquier otro valor.

#### Scenario: Persistir un estado reconocido
- **GIVEN** el estado de una solicitud es uno de los cuatro valores admitidos
- **WHEN** se persiste la solicitud
- **THEN** el sistema acepta el valor

#### Scenario: Persistir un estado no reconocido
- **GIVEN** se intenta persistir `ESPERANDO_CERTIFICADO` o cualquier valor distinto de los cuatro valores admitidos
- **WHEN** se guarda el estado de la solicitud
- **THEN** el sistema rechaza el valor

### Requirement: RF-07 Transiciones entre estados
CUANDO se cambie el estado de una solicitud, el sistema DEBERÁ (SHALL) permitir el cambio desde cualquiera de los cuatro estados reconocidos hacia cualquiera de esos estados, sin validar reglas de transición entre ellos.

#### Scenario: Cambio desde un estado final a pendiente
- **GIVEN** una solicitud está en estado `Firmada`, `Rechazada` o `Expirada`
- **WHEN** se cambia su estado a `Pendiente`
- **THEN** el sistema persiste el nuevo estado sin rechazar el cambio por la transición

#### Scenario: Cambio entre estados no finales
- **GIVEN** una solicitud está en estado `Pendiente`
- **WHEN** se cambia a cualquier estado reconocido
- **THEN** el sistema persiste el nuevo estado sin validar la combinación del estado anterior y el nuevo

#### Scenario: Cambio entre estados finales
- **GIVEN** una solicitud está en estado `Firmada`, `Rechazada` o `Expirada`
- **WHEN** se cambia a otro de esos estados
- **THEN** el sistema persiste el nuevo estado sin rechazar el cambio por la transición

## ADDED Requirements

### Requirement: RF-08 Migración de solicitudes que esperan certificado
CUANDO se ejecute la migración, el sistema DEBERÁ (SHALL) mapear cada solicitud en `ESPERANDO_CERTIFICADO` a `PENDIENTE` y conservar una marca de auditoría en `migrado_desde`.

#### Scenario: Migrar una solicitud que espera certificado
- **GIVEN** una solicitud persistida tiene el estado `ESPERANDO_CERTIFICADO`
- **WHEN** se ejecuta la migración
- **THEN** su estado pasa a `PENDIENTE`, `migrado_desde` queda establecido en `ESPERANDO_CERTIFICADO` y los demás datos de la solicitud se conservan
