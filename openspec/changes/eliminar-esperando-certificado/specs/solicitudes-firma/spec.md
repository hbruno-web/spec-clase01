# Spec Delta

## REMOVED Requirements

### Requirement: RF-02 Estado ESPERANDO_CERTIFICADO
**Reason**: El estado se elimina; las solicitudes que lo usaban pasan a `PENDIENTE`.
**Migration**: Cambiar las filas existentes a `PENDIENTE`, conservar sus demás datos y establecer `migrado_desde` en `ESPERANDO_CERTIFICADO`.

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

#### Scenario: Cambio entre estados reconocidos
- **GIVEN** una solicitud está en estado `Pendiente`
- **WHEN** se cambia a cualquier estado reconocido
- **THEN** el sistema persiste el nuevo estado sin validar la combinación del estado anterior y el nuevo

## ADDED Requirements

### Requirement: RF-08 Migración de solicitudes que esperan certificado
CUANDO se migren solicitudes persistidas con el estado retirado, el sistema DEBERÁ (SHALL) cambiarlas a `PENDIENTE`, conservar los demás datos y registrar su procedencia en `migrado_desde`.

#### Scenario: Migrar una solicitud existente
- **GIVEN** una solicitud persistida tiene estado `ESPERANDO_CERTIFICADO`
- **WHEN** se aplica la migración
- **THEN** su estado pasa a `PENDIENTE`, sus demás datos se conservan y `migrado_desde` queda establecido en `ESPERANDO_CERTIFICADO`

### Requirement: RF-09 Aviso de instalación de certificado
CUANDO se procesen solicitudes para enviar recordatorios, el sistema DEBERÁ (SHALL) dejar de enviar el aviso «Instala tu certificado digital» asociado al estado retirado.

#### Scenario: Solicitud migrada procesada por recordatorios
- **GIVEN** una solicitud que estaba en `ESPERANDO_CERTIFICADO` fue migrada a `PENDIENTE`
- **WHEN** se procesan sus recordatorios
- **THEN** el sistema no envía el aviso «Instala tu certificado digital»

### Requirement: RF-10 Inclusión de solicitudes migradas en el SLA
CUANDO se calcule el SLA de solicitudes, el sistema DEBERÁ (SHALL) incluir las solicitudes migradas en el periodo correspondiente a su `creada_en` original, aplicando el tratamiento vigente para `PENDIENTE`.

#### Scenario: Solicitud migrada incluida en su periodo original
- **GIVEN** una solicitud migrada tiene estado `PENDIENTE` y una fecha `creada_en` original
- **WHEN** se calcula el SLA del periodo de creación de esa solicitud
- **THEN** la solicitud se incluye en el cálculo de ese periodo

### Requirement: RF-11 Clasificación de solicitudes migradas
CUANDO se clasifiquen las solicitudes de un firmante, el sistema DEBERÁ (SHALL) clasificar como pendientes las solicitudes migradas a `PENDIENTE` y no como solicitudes que esperan certificado.

#### Scenario: Solicitud migrada clasificada como pendiente
- **GIVEN** una solicitud que estaba en `ESPERANDO_CERTIFICADO` fue migrada a `PENDIENTE`
- **WHEN** se clasifican las solicitudes del firmante
- **THEN** la solicitud se incluye entre las solicitudes por firmar y no entre las que esperan certificado
