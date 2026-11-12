# solicitudes-firma

## Purpose

Describir los estados vigentes de una solicitud de firma y el comportamiento actual al cambiar de estado.

## Requirements

### Requirement: RF-01 Estado PENDIENTE
CUANDO una solicitud esté en estado pendiente, el sistema DEBERÁ (SHALL) representarlo y persistirlo con el valor `PENDIENTE`; al cambiar a este estado no deberá modificar `resuelta_en`.

#### Scenario: Leer y persistir una solicitud pendiente
- **GIVEN** una solicitud tiene el valor persistido `PENDIENTE`
- **WHEN** el sistema lee la solicitud
- **THEN** la representa como `Pendiente`

#### Scenario: Cambiar a pendiente
- **GIVEN** una solicitud tiene un estado reconocido y un valor de `resuelta_en`
- **WHEN** se cambia su estado a `Pendiente`
- **THEN** el sistema persiste `PENDIENTE` y conserva `resuelta_en` sin cambios

### Requirement: RF-02 Estado ESPERANDO_CERTIFICADO
CUANDO una solicitud esté esperando un certificado, el sistema DEBERÁ (SHALL) representarlo y persistirlo con el valor `ESPERANDO_CERTIFICADO`; al cambiar a este estado no deberá modificar `resuelta_en`.

#### Scenario: Leer y persistir una solicitud que espera certificado
- **GIVEN** una solicitud tiene el valor persistido `ESPERANDO_CERTIFICADO`
- **WHEN** el sistema lee la solicitud
- **THEN** la representa como `EsperandoCertificado`

#### Scenario: Cambiar a esperando certificado
- **GIVEN** una solicitud tiene un estado reconocido y un valor de `resuelta_en`
- **WHEN** se cambia su estado a `EsperandoCertificado`
- **THEN** el sistema persiste `ESPERANDO_CERTIFICADO` y conserva `resuelta_en` sin cambios

### Requirement: RF-03 Estado FIRMADA
CUANDO una solicitud se cambie al estado firmada, el sistema DEBERÁ (SHALL) representarlo y persistirlo con el valor `FIRMADA`, y deberá guardar en `resuelta_en` el instante del cambio.

#### Scenario: Cambiar a firmada
- **GIVEN** una solicitud tiene un estado reconocido
- **WHEN** se cambia su estado a `Firmada` en un instante determinado
- **THEN** el sistema persiste `FIRMADA` y guarda ese instante en `resuelta_en`

### Requirement: RF-04 Estado RECHAZADA
CUANDO una solicitud se cambie al estado rechazada, el sistema DEBERÁ (SHALL) representarlo y persistirlo con el valor `RECHAZADA`, y deberá guardar en `resuelta_en` el instante del cambio.

#### Scenario: Cambiar a rechazada
- **GIVEN** una solicitud tiene un estado reconocido
- **WHEN** se cambia su estado a `Rechazada` en un instante determinado
- **THEN** el sistema persiste `RECHAZADA` y guarda ese instante en `resuelta_en`

### Requirement: RF-05 Estado EXPIRADA
CUANDO una solicitud se cambie al estado expirada, el sistema DEBERÁ (SHALL) representarlo y persistirlo con el valor `EXPIRADA`, y deberá guardar en `resuelta_en` el instante del cambio.

#### Scenario: Cambiar a expirada
- **GIVEN** una solicitud tiene un estado reconocido
- **WHEN** se cambia su estado a `Expirada` en un instante determinado
- **THEN** el sistema persiste `EXPIRADA` y guarda ese instante en `resuelta_en`

### Requirement: RF-06 Valores de estado admitidos
CUANDO se persista el estado de una solicitud, el sistema DEBERÁ (SHALL) admitir únicamente `PENDIENTE`, `ESPERANDO_CERTIFICADO`, `FIRMADA`, `RECHAZADA` o `EXPIRADA`, y deberá rechazar cualquier otro valor.

#### Scenario: Persistir un estado reconocido
- **GIVEN** el estado de una solicitud es uno de los cinco valores admitidos
- **WHEN** se persiste la solicitud
- **THEN** el sistema acepta el valor

#### Scenario: Persistir un estado no reconocido
- **GIVEN** se intenta persistir un valor distinto de los cinco valores admitidos
- **WHEN** se guarda el estado de la solicitud
- **THEN** el sistema rechaza el valor

### Requirement: RF-07 Transiciones entre estados
CUANDO se cambie el estado de una solicitud, el sistema DEBERÁ (SHALL) permitir el cambio desde cualquiera de los cinco estados reconocidos hacia cualquiera de esos estados, sin validar reglas de transición entre ellos.

#### Scenario: Cambio desde un estado final a pendiente
- **GIVEN** una solicitud está en estado `Firmada`, `Rechazada` o `Expirada`
- **WHEN** se cambia su estado a `Pendiente`
- **THEN** el sistema persiste el nuevo estado sin rechazar el cambio por la transición

#### Scenario: Cambio entre estados no finales
- **GIVEN** una solicitud está en estado `Pendiente` o `EsperandoCertificado`
- **WHEN** se cambia a cualquier estado reconocido
- **THEN** el sistema persiste el nuevo estado sin validar la combinación del estado anterior y el nuevo
