# Investigación: 009-update-status

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`
Objetivo: resolver las incógnitas técnicas necesarias para diseñar una operación pública de cambio de estado sin ambigüedades pendientes.

## R001 — Identidad de la operación

Decisión: usar `PATCH /api/properties/{id}/status` como operación pública, para un solo recurso y sin autenticación.

Justificación: la spec exige una operación dedicada que modifique solo `status`. `PATCH` expresa una modificación parcial y el sufijo `/status` delimita el campo permitido. El prompt de entrada sugiere esta forma y no contradice la spec; la decisión se registra aquí como detalle técnico, sin tratar el prompt como fuente de verdad.

Alternativas consideradas:

- `PUT /api/properties/{id}`: rechazada porque implica reemplazo completo y permitiría modificar otros campos.
- `POST /api/properties/{id}/status`: rechazada porque no comunica idempotencia y añadiría una acción innecesaria.
- `PATCH /api/properties/{id}`: rechazada porque no restringe explícitamente el campo a `status`.

## R002 — Forma del cuerpo de solicitud

Decisión: cuerpo JSON con una única propiedad `status` de tipo cadena anulable, enlazada mediante el registro anidado anulable `ChangePropertyStatusBody?`. Valores admitidos, con distinción de mayúsculas y minúsculas: `Available`, `Rented` y `Maintenance`.

Justificación: una cadena anulable permite distinguir ausencia, vacío y valor inválido dentro del validador de FluentValidation. El filtro global produce `HttpValidationProblemDetails`, cumpliendo el requisito de `400` estructurado. Un enum enlazado directamente puede fallar durante la deserialización antes del filtro y devolver un `400` sin el cuerpo de `ProblemDetails` exigido.

Alternativas consideradas:

- `PropertyStatus?`: rechazada por el riesgo de error de enlace no estructurado para valores inválidos.
- Cadena libre sin validación: rechazada porque permitiría estados desconocidos.
- Cuerpo con id incluido: rechazado porque el identificador ya está en la ruta y duplicarlo crea ambigüedad.

## R003 — Validación

Decisión: crear `ChangePropertyStatusValidator` para `ChangePropertyStatusRequest`, con reglas para `Id` como texto convertible a `Guid`, presencia de `Body` y `Body.Status`, y dejar que el filtro global convierta los fallos en `400`. La regla de estado usa `OverridePropertyName("Status")` para exponer la clave esperada en el contrato. El identificador se enlaza como texto para que un formato inválido sea validado por FluentValidation y no produzca un `400` sin cuerpo estructurado.

Justificación: reproduce el patrón existente de las slices y centraliza el formato de error. La regla de estado usa una comparación exacta contra los tres nombres del enum de dominio.

Alternativas consideradas:

- Validar dentro del handler: rechazada porque mezcla validación con persistencia y cambia el patrón del proyecto.
- Validar solo en el cliente: rechazada porque el servidor debe garantizar el contrato.
- Aceptar cualquier capitalización: rechazada porque la spec define valores exactos y la tolerancia añadiría comportamiento no especificado.

## R004 — Comportamiento de actualización

Decisión: cargar la entidad con seguimiento, comparar el estado solicitado con el persistido, guardar solo cuando haya cambio y establecer `UpdatedAt` únicamente en ese caso.

Justificación: satisface idempotencia, evita sobrescribir campos no incluidos en la solicitud y conserva `imageUrl`. El enfoque rastreado permite a Entity Framework actualizar solo las columnas modificadas.

Alternativas consideradas:

- Adjuntar una entidad nueva con `Update`: rechazada porque podría sobrescribir datos no enviados.
- Guardar siempre: rechazada porque cambiaría `updatedAt` en una repetición y rompería el criterio de idempotencia.
- Ejecutar SQL directo: rechazada porque rompe el patrón de persistencia del proyecto.

## R005 — Representación de respuesta

Decisión: crear `ChangePropertyStatusResponse` y `ChangePropertyStatusMapping` locales, con los mismos doce campos públicos y las mismas reglas de imagen y UTC que la respuesta de consulta por id.

Justificación: la spec exige devolver la propiedad actualizada con todos sus datos públicos. Duplicar la forma y el mapeo dentro de la slice evita acoplarla a `GetPropertyById` y mantiene la independencia de Vertical Slice Architecture.

Alternativas consideradas:

- Reutilizar `GetPropertyByIdResponse` y su mapeo: rechazada por acoplamiento entre slices.
- Devolver solo `id` y `status`: rechazada porque la spec pide la propiedad completa.
- Devolver la entidad de dominio: rechazada porque expone tipos internos y pierde normalización de URL y fechas.

## R006 — Errores y registro

Decisión: usar `ProblemDetailsMapper` para `404` e interno, el filtro global para `400`, y añadir un evento estructurado `PropertyStatusChanged` con id `1004` y payload de identificador más estado destino.

Justificación: mantiene consistencia con los endpoints existentes y facilita trazabilidad sin registrar datos sensibles ni detalles técnicos en respuestas.

Alternativas consideradas:

- Lanzar excepciones y dejar middleware genérico: rechazada porque rompe el formato de errores actual.
- Registro con cadenas interpoladas: rechazada porque el proyecto usa `LoggerMessage` source-generated.
- Devolver `409` en cambios repetidos: rechazada porque la spec los declara idempotentes.

## R007 — Concurrencia y cancelación

Decisión: última escritura gana, sin token de concurrencia ni respuesta `409`. Propagar `OperationCanceledException` cuando el token de la petición esté cancelado.

Justificación: la spec no define versiones ni detección de conflictos. La semántica de lectura, comparación y escritura sobre la entidad rastreada es suficiente y no amplía el alcance.

Alternativas consideradas:

- Concurrencia optimista con `409`: rechazada porque añadiría un campo de versión y una respuesta fuera de la spec.
- Ignorar cancelación: rechazada porque rompe el patrón de handlers existente.
- Bloqueo de base de datos: rechazada porque no está justificado para este alcance.

## R008 — Contrato OpenAPI

Decisión: exponer la nueva operación con `operationId` `ChangePropertyStatus`, etiqueta `Propiedades`, esquema de solicitud, respuesta `200` y respuestas `400`/`404`. El documento se genera desde código y se valida con el script canónico, Redocly y NSwag.

Justificación: la gobernanza de 008 exige que `v1.json` sea la fuente pública del contrato. Tras el cambio habrá 4 rutas y 6 operaciones. Los documentos de inventario de 008 deben actualizarse para no contradecir el artefacto generado, sin modificar la semántica de operaciones previas.

Alternativas consideradas:

- Editar `v1.json` a mano: rechazada porque es artefacto generado y fallaría el control de deriva.
- No publicar la operación: rechazada porque la spec exige contrato OpenAPI.
- Generar documentación dinámica separada: rechazada porque 008 fijó un único documento versionado.

## R009 — Estrategia de pruebas

Decisión: cubrir validador, handler, mapeo y slice con pruebas unitarias xUnit, y actualizar las pruebas OpenAPI existentes de documento, deriva, tooling y CLI.

Justificación: verifica los criterios de aceptación sin introducir nueva categoría de pruebas. El arranque de `WebApplication` en proceso y el generador en memoria son patrones ya aprobados en 008.

Alternativas consideradas:

- Pruebas de integración con PostgreSQL: rechazadas por depender de infraestructura y ser una categoría nueva.
- Pruebas solo de handler: rechazadas porque no verificarían ruta, método ni contrato HTTP.
- Pruebas manuales únicamente: rechazadas porque no son automatizables en integración continua.

## R010 — Persistencia y migraciones

Decisión: no crear migraciones, campos ni tablas; reutilizar `Status` y `UpdatedAt` de `Property`.

Justificación: el modelo actual ya contiene todo lo necesario y la spec prohíbe ampliar el alcance a otros campos.

Alternativas consideradas:

- Añadir tabla de historial de estados: rechazada porque no está en la spec.
- Añadir columna de versión: rechazada por la decisión de última escritura gana.
- Cambiar el tipo de `Status`: rechazado porque ya es `PropertyStatus` y coincide con los valores permitidos.

## Resultado

Todas las incógnitas técnicas quedan resueltas. No permanecen incógnitas pendientes. Estas decisiones alimentan directamente `plan.md`, `data-model.md`, los contratos y `quickstart.md`.
