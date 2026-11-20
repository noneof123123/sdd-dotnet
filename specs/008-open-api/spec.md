# Especificación de la característica: Documento OpenAPI v1 de la API pública

**Rama de característica**: `008-open-api`

**Creado**: 2026-11-20

**Estado**: Implementada

**Entrada**: Descripción del usuario: "008-open-api — documento OpenAPI v1 generado desde la implementación real, sin interfaz interactiva".

## Aclaraciones

### Decisiones iniciales adoptadas

- Q: ¿Qué entrega esta característica? → A: Un único documento OpenAPI versionado (v1) del contrato HTTP público de RealtorApi, publicado como `openapi/v1.json` y accesible mediante `GET /openapi/v1.json`.
- Q: ¿Cuál es la fuente de verdad del documento? → A: Dentro del alcance aprobado por esta spec, la implementación real del backend es la entrada de generación del documento; esta spec aprobada sigue siendo la única fuente de verdad del proyecto. El archivo NO debe editarse manualmente para corregir el contrato.
- Q: ¿Qué endpoints están en alcance? → A: El 100 % de los endpoints de negocio implementados: `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties` y `PUT /api/properties/{id}`; la ruta que publica el documento queda excluida.
- Q: ¿Cómo se tratan las constraints del enrutador? → A: Una ruta con constraint, por ejemplo `{id:guid}`, se considera equivalente a su forma canónica `{id}` tanto en el documento como en la verificación automatizada de desalineación.
- Q: ¿Se incluye alguna interfaz visual? → A: No. Se excluyen Swagger UI, ReDoc UI y cualquier endpoint de exploración interactiva o interfaz visual de documentación. Solo se publica el documento estático JSON.
- Q: ¿Redocly y NSwag son interfaces interactivas? → A: No. `@redocly/cli` y NSwag son herramientas de línea de comandos usadas solo durante desarrollo y validación; no forman parte de la superficie de la aplicación en ejecución. La exclusión de Swagger UI se destaca explícitamente porque suele confundirse con estas herramientas.
- Q: ¿Cuántas versiones del contrato se publican? → A: Solo la versión v1; el versionado múltiple queda fuera del alcance.
- Q: ¿El documento requiere autenticación? → A: No. Es público, de solo lectura y no introduce credenciales ni permisos nuevos.

### Sesión 2026-11-20

- Q: ¿Debe incluirse el propio endpoint `GET /openapi/v1.json` en el documento OpenAPI v1 y en la comparación de desalineación? → A: Opción A: excluirlo del documento y de la comparación; el contrato cubre solo los cinco endpoints de negocio.

### Términos y alcance

- **Versión de contrato v1**: valor de `info.version` igual a `v1`; representa esta única versión de la API documentada.
- **Artefacto versionado**: archivo `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` controlado por Git y regenerado desde la implementación aprobada.
- **Ruta estable**: `GET /openapi/v1.json`, servida siempre desde el artefacto estático y sin endpoints interactivos asociados.
- **Fuente de generación**: implementación real de los cinco endpoints, limitada a lo descrito en esta spec aprobada.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Obtener y consumir el contrato v1 (Prioridad: P1)

Como consumidor de RealtorApi, quiero descargar un documento OpenAPI v1 válido desde una ruta estable, para generar o validar clientes sin corregir el contrato manualmente.

**Por qué esta prioridad**: Es el valor central de la característica: publicar una descripción fiable y consumible de la API pública.

**Prueba independiente**: Puede probarse solicitando `GET /openapi/v1.json`, verificando el código de respuesta y el tipo de contenido, y generando un cliente tipado de prueba con NSwag sin correcciones manuales.

**Escenarios de aceptación**:

1. **Dado** el sistema en ejecución, **Cuando** se solicita `GET /openapi/v1.json`, **Entonces** se devuelve `200 OK` con `Content-Type application/json` y el documento OpenAPI v1.
2. **Dado** el documento publicado, **Cuando** se genera un cliente tipado con `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json`, **Entonces** el cliente se genera sin correcciones manuales del contrato.
3. **Dado** un consumidor sin credenciales, **Cuando** consulta la ruta estable del documento, **Entonces** obtiene el contrato público sin autenticación.

---

### Historia de usuario 2 - Detectar desalineación entre implementación y documento (Prioridad: P2)

Como responsable del contrato, quiero que las pruebas automatizadas fallen cuando el documento deje de coincidir con los endpoints reales, para evitar publicaciones con documentación obsoleta.

**Por qué esta prioridad**: La generación automática pierde valor si no existe una barrera que detecte cambios no reflejados en el documento versionado.

**Prueba independiente**: Puede probarse induciendo una desalineación controlada entre el inventario de endpoints y el documento, y verificando que la verificación automatizada falla; después debe restaurarse el estado y verificarse que pasa.

**Escenarios de aceptación**:

1. **Dado** el inventario de endpoints en ejecución, **Cuando** se compara con los paths documentados aplicando la normalización de constraints, **Entonces** ambos inventarios coinciden para los cinco endpoints en alcance.
2. **Dado** un endpoint nuevo, renombrado o eliminado sin reflejo en el documento, **Cuando** se ejecuta la verificación de desalineación, **Entonces** la prueba falla.
3. **Dado** un cambio de ruta que solo varía en la constraint del enrutador, **Cuando** se compara con el documento, **Entonces** se trata como equivalente y no genera un falso positivo.

---

### Historia de usuario 3 - Regenerar y validar de forma reproducible (Prioridad: P3)

Como miembro del equipo, quiero un script que regenere y valide el documento con un único comando, para repetir la misma verificación en cualquier máquina de desarrollo preparada.

**Por qué esta prioridad**: La reproducibilidad evita diferencias entre entornos y hace explícitas las herramientas necesarias antes de la implementación y la validación.

**Prueba independiente**: Puede probarse ejecutando `support/scripts/generate-openapi-v1.sh` desde una máquina que cumpla los requisitos previos y verificando que genera el documento, valida con Redocly, genera el cliente de prueba con NSwag y termina con un reporte de éxito.

**Escenarios de aceptación**:

1. **Dado** un entorno con las herramientas previas instaladas, **Cuando** se ejecuta el script, **Entonces** compila RealtorApi y genera `v1.json` automáticamente.
2. **Dado** el documento generado, **Cuando** el script ejecuta `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, **Entonces** la validación termina sin errores.
3. **Dado** un fallo en la generación del documento o en alguna validación, **Cuando** termina el script, **Entonces** falla explícitamente y reporta la causa.

---

## Casos límite

- Si el documento no se genera durante la compilación, el script debe fallar antes de intentar validaciones posteriores.
- Si `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json` detecta errores, el script y la verificación automatizada deben fallar.
- Si falta `.redocly.yaml`, la validación Redocly puede rechazar una API pública sin esquema de seguridad; el archivo es obligatorio y debe desactivar `security-defined` y degradar `operation-4xx-response` a advertencia.
- Si un endpoint de negocio implementado no aparece en `v1.json`, la verificación de desalineación debe fallar.
- Si el documento contiene un path que no corresponde a un endpoint real, la verificación de desalineación debe fallar.
- Si una ruta usa una constraint del enrutador, debe normalizarse a su forma canónica antes de compararse con el documento.
- Si el inventario de endpoints incluye la ruta estática `GET /openapi/v1.json`, debe excluirse de la comparación para evitar un falso fallo de desalineación.
- Si un tipo compartido aparece en línea en lugar de referenciarse mediante `$ref`, el contrato se considera incompleto.
- Si la generación del cliente tipado con NSwag requiere corregir el documento, la validación debe considerarse fallida.
- Si se intenta agregar Swagger UI, ReDoc UI u otra interfaz visual como parte de la aplicación, queda fuera del alcance aprobado.
- Esta iniciativa no modifica el comportamiento funcional de los endpoints existentes ni incorpora versiones distintas de v1.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El sistema DEBE generar un único documento OpenAPI de versión de contrato v1 del contrato HTTP público de RealtorApi a partir de la implementación real del backend, dentro del alcance aprobado por esta spec.
- **FR-002**: El documento generado NO DEBE ser corregido mediante edición manual; una regeneración con código, configuración y dependencias idénticos DEBE producir un contenido byte-idéntico.
- **FR-003**: El archivo versionado DEBE ubicarse en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
- **FR-004**: El documento DEBE exponerse mediante `GET /openapi/v1.json` como archivo estático en una ruta HTTP estable.
- **FR-005**: `GET /openapi/v1.json` DEBE responder `200 OK` con `Content-Type application/json`.
- **FR-006**: El documento DEBE cubrir el 100 % de los endpoints de negocio implementados: `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties` y `PUT /api/properties/{id}`. La ruta `GET /openapi/v1.json`, que publica el propio documento, queda excluida por ser infraestructura de documentación.
- **FR-007**: Cada endpoint documentado DEBE incluir método, ruta, parámetros de ruta y query declarados, cuerpo de solicitud en `POST` y `PUT`, respuesta de éxito y al menos una respuesta de error; `GET /api/properties` DEBE documentar `page` y `pageSize`, y `GET /health` DEBE documentar `500`.
- **FR-008**: El formato de error `ProblemDetails` DEBE representarse de forma consistente en todos los endpoints documentados; todos los errores documentados DEBEN referenciar `ProblemDetails` o `HttpValidationProblemDetails` mediante `$ref`.
- **FR-009**: Los tipos compartidos `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` DEBEN exponerse en `components/schemas` y referenciarse mediante `$ref`, sin duplicación en línea.
- **FR-010**: Las constraints del enrutador, como `{id:guid}`, DEBEN considerarse equivalentes a su forma canónica `{id}` en el documento y en la verificación de desalineación.
- **FR-011**: DEBE existir una verificación automatizada con xUnit que compare el inventario de endpoints en ejecución, obtenido de `EndpointDataSource`, contra los paths documentados aplicando la normalización de constraints; la ruta `GET /openapi/v1.json` debe excluirse explícitamente de la comparación.
- **FR-012**: La verificación automatizada DEBE fallar cuando exista desalineación entre los endpoints reales y el documento `openapi/v1.json`.
- **FR-013**: La verificación automatizada DEBE validar el documento con `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
- **FR-014**: La verificación automatizada DEBE generar un cliente C# tipado real con `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json` como prueba de humo de consumibilidad del contrato.
- **FR-015**: DEBE existir `.redocly.yaml` en la raíz del repositorio con `security-defined` desactivado y `operation-4xx-response` degradado a advertencia.
- **FR-016**: DEBE existir `support/scripts/generate-openapi-v1.sh` que regenere y valide el documento de forma reproducible.
- **FR-017**: El script DEBE ejecutar en secuencia `dotnet build app/Realtor.sln`, la verificación de `v1.json`, la validación con Redocly, `dotnet tool restore`, la generación del cliente tipado en `artifacts/openapi-client-smoke/` y un reporte final con la ruta del documento y la confirmación de cada fase.
- **FR-018**: El script DEBE fallar explícitamente con mensaje `[ERROR]` y código distinto de cero si el documento no se genera o si alguna validación falla.
- **FR-019**: La solución NO DEBE incluir Swagger UI, ReDoc UI, endpoints de exploración interactiva ni interfaces visuales de documentación en la aplicación en ejecución.
- **FR-020**: `@redocly/cli` y NSwag DEBEN usarse exclusivamente como herramientas de línea de comandos durante desarrollo y validación.
- **FR-021**: La iniciativa NO DEBE modificar el comportamiento funcional de los endpoints existentes.
- **FR-022**: La iniciativa DEBE limitarse a una única versión de contrato, v1, sin versionado múltiple.

### Renuncia explícita al principio V

Esta spec aprobada justifica explícitamente el uso de pruebas distintas de unit tests. La verificación de consumibilidad con Redocly CLI y NSwag CLI y la comparación de deriva contra `EndpointDataSource` requieren ejecutar herramientas externas y una aplicación en proceso; una prueba unitaria que solo lea JSON no puede detectar fallos de linting, generación de cliente ni desalineación real de endpoints. Esta justificación DEBE reflejarse en `plan.md` y `tasks.md` conforme al principio V de la constitución.

### Entidades clave *(incluir si la característica implica datos)*

- **Documento OpenAPI v1**: Archivo `openapi/v1.json` generado desde la implementación real y publicado como contrato versionado.
- **Endpoint documentado**: Operación HTTP pública incluida en el documento, con método, ruta, parámetros, cuerpo cuando corresponda y respuestas.
- **Constraint normalizada**: Regla que hace equivalentes las rutas con restricciones del enrutador y sus formas canónicas para efectos de documentación y verificación.
- **Componente compartido**: Tipo reutilizable expuesto en `components/schemas` y referenciado mediante `$ref`.
- **Inventario de endpoints en ejecución**: Conjunto de endpoints reales expuesto por la aplicación y usado como referencia para detectar desalineación.
- **Cliente de prueba**: Cliente C# tipado generado con NSwag para verificar que el contrato es consumible sin correcciones manuales.
- **Configuración de validación**: Archivo `.redocly.yaml` que ajusta las reglas de validación para una API pública sin autenticación.
- **Script de regeneración**: Archivo `support/scripts/generate-openapi-v1.sh` que reproduce generación, validación y reporte de éxito.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

Los SC que repiten un FR añaden el método de verificación observable; no introducen requisitos nuevos.

- **SC-001**: El 100 % de los endpoints de negocio implementados, incluido `GET /health`, aparece en `v1.json`; `GET /openapi/v1.json` queda excluido del inventario documentado. Se verifica comparando el documento con `EndpointDataSource`.
- **SC-002**: `GET /openapi/v1.json` responde `200 OK` con `Content-Type application/json` en el 100 % de las validaciones funcionales. Se verifica con una prueba HTTP en proceso.
- **SC-003**: `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json` valida el documento sin errores en el 100 % de las ejecuciones.
- **SC-004**: La verificación de desalineación falla en el 100 % de las pruebas de desalineación inducida.
- **SC-005**: Un consumidor genera un cliente tipado con `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json` sin correcciones manuales del contrato en el 100 % de las ejecuciones de validación.
- **SC-006**: `support/scripts/generate-openapi-v1.sh` regenera y valida el documento sin errores en el 100 % de sus ejecuciones.
- **SC-007**: `.redocly.yaml` existe, es válido y contiene `security-defined` desactivado y `operation-4xx-response` como advertencia.
- **SC-008**: El 100 % de los tipos compartidos `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` se expone en `components/schemas` y se referencia mediante `$ref`.
- **SC-009**: La aplicación publica 0 interfaces visuales de documentación y 0 endpoints de exploración interactiva.
- **SC-010**: El 100 % de las verificaciones existentes de salud, listado, creación, actualización y consulta por identificador continúa superándose tras incorporar la documentación.
- **SC-011**: El documento publicado declara `info.version` igual a `v1` en el 100 % de las validaciones del contrato.

## Supuestos

- La iniciativa depende de los contratos y convenciones aprobados en 001-solution-foundation, 002-foundation-backend, 003-properties-persistence-seeding, 004-properties-list-pagination, 005-properties-create, 006-properties-update y 007-properties-get-by-id.
- El desarrollador tiene Node.js 20+ y puede ejecutar `npx --yes @redocly/cli`.
- `.config/dotnet-tools.json` del repositorio incluye `NSwag.ConsoleCore`, restaurable con `dotnet tool restore`.
- `.redocly.yaml` existe en la raíz del repositorio como artefacto obligatorio de la característica.
- El archivo estático `wwwroot/openapi/v1.json` es servible por la configuración de archivos estáticos existente.
- La implementación real del backend, limitada al alcance aprobado por esta spec, es la entrada de generación del documento; esta spec prevalece sobre contratos de specs previas.
- La característica se limita a una única versión de contrato, v1, sin versionado múltiple.
- No se modifica el comportamiento funcional de los endpoints existentes; solo su representación contractual y su verificación de consistencia.
- La documentación es pública, de solo lectura y no requiere autenticación.

## Requisitos técnicos y herramientas externas

### Requisitos de sistema

- Node.js 20+ instalado y disponible en `PATH`, verificable con `node --version`.
- `npm` disponible en `PATH`, verificable con `npm --version`.
- `npx` disponible para ejecutar `@redocly/cli`.

### Herramientas NPM

- `@redocly/cli`, validador de especificaciones OpenAPI, ejecutable mediante `npx --yes @redocly/cli --version`.

### Herramientas .NET locales

- NSwag (`NSwag.ConsoleCore`) registrado en `.config/dotnet-tools.json` del repositorio.
- Restaurable con `dotnet tool restore`.
- Ejecutable con `dotnet tool run nswag`.
- Uso exclusivo: generar un cliente C# tipado de prueba para verificar que el documento es consumible sin correcciones manuales; NO se usa para servir Swagger ni ninguna interfaz visual.

### Archivo de configuración requerido

- `.redocly.yaml` en la raíz del repositorio.
- Redocly es estricto por defecto y rechaza APIs públicas sin esquema de seguridad.
- La configuración debe desactivar la regla `security-defined` porque la API es pública y no usa autenticación.
- La configuración debe degradar `operation-4xx-response` a advertencia.
- Sin este archivo, la validación Redocly falla con errores.

### Verificación previa de herramientas

- [ ] `node --version` devuelve 20 o superior.
- [ ] `npm --version` se ejecuta correctamente.
- [ ] `npx --yes @redocly/cli --version` se ejecuta correctamente.
- [ ] `dotnet tool restore` y `dotnet tool list --local` confirman que NSwag está disponible.
