# Plan de implementación: 009-update-status

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`
Estado de la especificación: `Implementada`; revisión y aprobación registradas el 2026-11-20.
Entrada: usuario y solicitud de planeación para `/speckit.plan`.

## Resumen

Añadir una operación pública y dedicada para cambiar únicamente el estado de una propiedad existente, identificada por `id`, siguiendo Minimal APIs y Vertical Slice Architecture. La operación usará `PATCH /api/properties/{id}/status`, aceptará un cuerpo JSON con `status`, rechazará estados ausentes, vacíos o desconocidos con `400` estructurado, devolverá `404` cuando la propiedad no exista y responderá `200` con la representación pública completa de la propiedad. Repetir el estado vigente no modificará `updatedAt`. No se crean tablas, campos, migraciones, proyectos, controladores ni depends externas.

## Contexto técnico

- Lenguaje y SDK: C# con .NET 11, SDK `11.0.100-rc.3.26522.148`.
- Framework: ASP.NET Core 11 con Minimal APIs; descubrimiento automático mediante `ISlice`, `RegisterSlices`, `MapSliceEndpoints` e `IHandler`.
- Persistencia: Entity Framework Core con Npgsql y PostgreSQL; `AppDbContext.Properties`, entidad `Property` y enumeración `PropertyStatus`.
- Esquema: sin cambios; `Property.Status` y `Property.UpdatedAt` ya existen y no requieren migración.
- Validación: FluentValidation con filtro global de endpoint; errores `400` como `HttpValidationProblemDetails`.
- Resultados de dominio: `Result<T>`, `Error` y `ProblemDetailsMapper` para `400`, `404`, `409` e interno.
- Contrato de solicitud: JSON con `status` como cadena anulable exacta (`Available`, `Rented`, `Maintenance`). Se elige cadena en lugar de enum para que valores inválidos lleguen al validador y produzcan `400` estructurado, en vez de fallar en enlace de modelo.
- Contrato de respuesta: récord local con los mismos doce campos públicos de `GetPropertyByIdResponse`; `status` como texto, `imageUrl` absoluta o cadena vacía y fechas normalizadas a UTC.
- Idempotencia: si el estado solicitado es igual al persistido, no se llama a `SaveChangesAsync` y no se actualiza `updatedAt`.
- Concurrencia: última escritura gana; no se añaden tokens de concurrencia ni `409`.
- Registro: `InfrastructureLog` con nuevo evento estructurado `1004` para el cambio de estado.
- OpenAPI: documento `v1` generado desde el código; nuevo `operationId` `ChangePropertyStatus`; esperado 4 rutas y 6 operaciones tras el cambio.
- Herramientas de contrato: `support/scripts/generate-openapi-v1.sh`, Redocly y NSwag local configurado en `support/scripts/openapi-v1.nswag.json` con `runtime: Net110`.
- Pruebas: xUnit, patrones existentes con EF Core InMemory y `WebApplication` en proceso; sin categorías nuevas de prueba. Se reutiliza la excepción de herramientas OpenAPI ya aprobada en 008.
- Documentación y ejemplos: español obligatorio en markdown; nombres de archivos y comandos en minúsculas/kebab; ejemplo manual en `RealtorApi.http`.
- Entorno: Linux sin `pwsh`. La resolución de plantillas usa `.specify/scripts/sh/resolve-template.sh`, cambio solicitado explícitamente por el usuario en los archivos de comando/skill. La emulación de `setup-plan` se basa en `.specify/feature.json` y no modifica scripts aguas arriba.
- Incógnitas técnicas pendientes: ninguna; resueltas en `research.md`.

## Comprobación de la constitución previa al diseño

_PUERTA: todos los principios deben cumplirse antes de continuar con Fase 1._

| Principio | Estado | Justificación |
|---|---|---|
| I. Solución única y prioridad de specs | CUMPLE | No se propone un segundo backend, carpeta paralela ni solución alternativa; la spec activa es `009-update-status`. |
| II. Desarrollo guiado por especificaciones | CUMPLE | Existe `spec.md`, checklist 16/16 y clarify sin ambigüedades críticas; este plan no es código ni reemplaza la aprobación humana posterior. |
| III. Arquitectura limpia y Vertical Slice Architecture | CUMPLE | Una sola slice con endpoint, solicitud, respuesta, validador, mapeo y handler; registro por reflexión, sin controladores, repositorios ni servicios compartidos nuevos. |
| IV. Stack y estándares obligatorios | CUMPLE | .NET 11, Minimal APIs, EF Core/Npgsql/PostgreSQL, FluentValidation, ProblemDetails y logging estructurado; sin frameworks nuevos. |
| V. Dominio, contratos y pruebas | DEVIACIÓN JUSTIFICADA | Contratos explícitos, validación antes del handler, errores estructurados y pruebas unitarias con los patrones ya aprobados. Se reutiliza de forma explícita la excepción aprobada en la spec 008 para pruebas de documento, deriva, CLI y herramientas OpenAPI, reflejada también en `tasks.md`. |
| VI. Idioma y documentación | CUMPLE | Toda documentación de planeación será markdown en español; se conservan identificadores técnicos. |
| VII. Gobernanza y trazabilidad | CUMPLE | Sin edición silenciosa de estado, sin inserción de fases, sin cambios fuera del alcance y sin modificar specs anteriores; divergencia operativa de `pwsh` documentada y resolver shell cambiado por instrucción humana. |

Resultado: **PUERTA CUMPLIDA**.

## Fase 0: Investigación

Artefacto: `specs/009-update-status/research.md`.

Decisiones registradas:

- R001: operación `PATCH /api/properties/{id}/status`, pública y de un solo recurso.
- R002: `status` de solicitud como cadena anulable en un registro anidado, validada contra nombres exactos del enum.
- R003: validación de `id`, presencia de cuerpo y `status` mediante un validador de slice y filtro global.
- R004: handler con entidad rastreada, comparación previa y guardado condicional.
- R005: respuesta local equivalente a la representación pública de consulta, sin acoplamiento entre slices.
- R006: errores mediante `ProblemDetailsMapper` y logging estructurado nuevo.
- R007: cancelación propagada y concurrencia de última escritura gana.
- R008: documento OpenAPI generado, no editado a mano; actualización del inventario documental de 008.
- R009: pruebas unitarias y actualización de pruebas OpenAPI existentes.
- R010: sin migración ni cambios de esquema.

## Fase 1: Diseño y contratos

### D001 — Superficie HTTP

`PATCH /api/properties/{id}/status`, sin autenticación, con `Content-Type: application/json`, cuerpo `{ "status": "Rented" }` y respuesta `200 OK` con la propiedad completa. `id` se enlaza desde ruta.

### D002 — Validación

`ChangePropertyStatusValidator` valida:

- `Id` no vacío y convertible a `Guid`.
- `Body` no nulo.
- `Body.Status` no nulo, no vacío ni solo espacios.
- `Body.Status` exactamente `Available`, `Rented` o `Maintenance`, exponiendo la clave `Status`.

El filtro global convierte fallos en `HttpValidationProblemDetails` con `400`. El handler recibe valores válidos o `NotFound`.

### D003 — Handler y persistencia

`ChangePropertyStatusHandler` convierte el `Id` validado a `Guid`, obtiene `Body.Status` de la solicitud validada y busca la propiedad con seguimiento de EF Core. Si no existe, devuelve `Error.NotFound`. Si el estado ya coincide, devuelve la representación actual sin guardar. Si difiere, asigna el nuevo `PropertyStatus`, actualiza `UpdatedAt`, llama a `SaveChangesAsync`, registra el evento `1004` y devuelve la representación actualizada. Propaga `OperationCanceledException` cuando corresponde y convierte otras excepciones en `Error.Internal`.

### D004 — Representación de respuesta

`ChangePropertyStatusResponse` y `ChangePropertyStatusMapping` reproducen la forma pública de `GetPropertyByIdResponse`: doce campos, `status` como cadena, `imageUrl` absoluta con prefijo `/assets/properties/{fileName}` o cadena vacía, y normalización UTC de `CreatedAt`/`UpdatedAt`. Se evita importar tipos de otra slice.

### D005 — Errores y observabilidad

- `400`: errores de validación globales con claves `Id` y `Status`, incluido cuerpo ausente.
- `404`: `ProblemDetails` con título y detalle estables.
- `500`: `ProblemDetails` genérico sin detalles técnicos.
- No se documenta `409`; no aplica por decisión de concurrencia.
- Evento `InfrastructureLog.PropertyStatusChanged(ILogger, Guid, string)` con id `1004`.

### D006 — Registro y estructura

La slice se registra automáticamente. No se modifica `Program.cs`, no se crean controladores, repositorios, mapas manuales ni proyectos.

### D007 — OpenAPI

Añadir `ChangePropertyStatusOperationId` a `RealtorOpenApiConstants`. La slice usa `WithName`, `WithTags`, `Accepts<ChangePropertyStatusRequest>("application/json")`, `Produces<ChangePropertyStatusResponse>(200)`, `ProducesValidationProblem(400)`, `ProducesProblem(404)` y `ProducesProblem(500)`. El documento `v1.json` se regenera con el script canónico. Se actualizan los contratos de 008 para reflejar 4 rutas y 6 operaciones sin alterar la semántica de operaciones existentes.

### D008 — Pruebas

- `ValidatorTests`: ausente, vacío, inválido, minúsculas rechazadas y tres valores válidos.
- `HandlerTests`: cambio correcto, preservación de campos e imagen, no encontrado, idempotencia sin guardado ni `updatedAt`, cancelación.
- `MappingTests`: imagen absoluta, imagen vacía e UTC.
- `SliceTests`: ruta, método, `operationId`, 200, 400 y 404 sobre `WebApplication` en proceso.
- OpenAPI: actualizar pruebas existentes de documento, deriva, CLI y herramientas para la nueva operación y conteos.

### D009 — Manual y ejemplos

Añadir petición exitosa, validación inválida y no encontrado a `RealtorApi.http`. El quickstart documenta la secuencia completa de build, prueba, regeneración de OpenAPI y validación Redocly/NSwag.

### D010 — Persistencia

Sin nueva migración. El único cambio de base de datos en ejecución son valores de `Status` y `UpdatedAt` en la fila existente.

## Estructura del proyecto

### Documentación creada por este plan

```text
specs/009-update-status/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── cambiar-estado-propiedad.http.md
│   └── contrato-openapi-v1.md
└── checklists/
    └── requirements.md
```

### Código y pruebas previstos para `/speckit.tasks`

```text
app/backend/src/RealtorApi/
├── Features/Properties/ChangePropertyStatus/
│   ├── ChangePropertyStatusSlice.cs
│   ├── ChangePropertyStatusRequest.cs
│   ├── ChangePropertyStatusResponse.cs
│   ├── ChangePropertyStatusValidator.cs
│   ├── ChangePropertyStatusMapping.cs
│   └── ChangePropertyStatusHandler.cs
├── Infrastructure/Logging/InfrastructureLog.cs
├── Infrastructure/OpenApi/OpenApiConstants.cs
├── RealtorApi.http
└── wwwroot/openapi/v1.json

app/backend/tests/RealtorApiTests/
├── Features/Properties/ChangePropertyStatus/
│   ├── ChangePropertyStatusValidatorTests.cs
│   ├── ChangePropertyStatusHandlerTests.cs
│   ├── ChangePropertyStatusMappingTests.cs
│   └── ChangePropertyStatusSliceTests.cs
└── OpenApi/
    ├── OpenApiDocumentTests.cs
    ├── OpenApiDriftTests.cs
    ├── OpenApiToolingTests.cs
    └── OpenApiContractCliTests.cs

support/scripts/generate-openapi-v1.sh
specs/008-open-api/contracts/get-openapi-v1.http.md
specs/008-open-api/contracts/openapi-v1-document.md
```

## Fase 2: Descomposición

`tasks.md` no se crea en esta fase. El siguiente comando debe ser `/speckit.tasks`, usando este plan y `research.md`, `data-model.md`, `contracts/` y `quickstart.md` como entradas.

Paquetes de trabajo esperados, en orden: contratos y constantes, slice y handler, pruebas de slice, regeneración OpenAPI, actualización documental de 008 y verificación final.

## Seguimiento de complejidad

No existen desviaciones constitucionales que requieran justificación. La ausencia de `pwsh` es una condición operativa del entorno, resuelta mediante el resolver shell solicitado por el usuario y la emulación de prerrequisitos; no cambia el diseño ni la arquitectura.

## Comprobación de la constitución posterior al diseño

| Principio | Estado | Justificación post-diseño |
|---|---|---|
| I. Solución única y prioridad de specs | CUMPLE | El diseño se limita a la spec activa y no propone alternativas paralelas. |
| II. Desarrollo guiado por especificaciones | CUMPLE | Todas las decisiones derivan de requisitos y clarificaciones; la revisión y aprobación de `Borrador` a `Aprobada` se registraron antes de la implementación. |
| III. Arquitectura limpia y Vertical Slice Architecture | CUMPLE | Slice autocontenida, sin acoplamiento entre features ni cambios globales además de constantes/log. |
| IV. Stack y estándares obligatorios | CUMPLE | Sin tecnologías nuevas y dentro del stack existente. |
| V. Dominio, contratos y pruebas | DEVIACIÓN JUSTIFICADA | Contrato, validación, errores y pruebas cubiertos; la excepción OpenAPI aprobada en 008 se reutiliza y declara explícitamente en `plan.md` y `tasks.md` para pruebas de documento, deriva, CLI y herramientas. |
| VI. Idioma y documentación | CUMPLE | Artefactos en español con identificadores técnicos permitidos. |
| VII. Gobernanza y trazabilidad | CUMPLE | Alcance trazado, documentos de 008 solo actualizados por deriva de contrato y sin cambios de estado silentos. |

Resultado final: **PUERTA CUMPLIDA**; implementación completada y verificada.
