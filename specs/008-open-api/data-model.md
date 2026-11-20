# Modelo de datos: 008-open-api

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Alcance

Esta característica no introduce entidades persistentes, tablas, migraciones ni cambios en el modelo de dominio de propiedades. El "modelo de datos" son las estructuras de contrato y los artefactos de herramientas que se generan, validan y publican.

No hay transiciones de estado de negocio. El ciclo relevante es: código fuente → documento generado → validación → publicación estática → detección de deriva.

## Entidades de contrato

### `OpenApiDocumentV1`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `DocumentName` | `string` | Nombre lógico del documento. | Debe ser `v1`. |
| `SpecVersion` | `string` | Versión del formato OpenAPI. | Debe ser `3.0.x`. |
| `InfoTitle` | `string` | Título del documento. | No vacío; identifica RealtorApi. |
| `InfoVersion` | `string` | Versión de la API documentada. | Debe ser `v1`. |
| `OutputPath` | `string` | Ruta de salida generada. | `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`. |
| `PublicRoute` | `string` | Ruta HTTP pública. | `GET /openapi/v1.json`. |
| `Paths` | `IReadOnlyCollection<DocumentedEndpoint>` | Operaciones documentadas. | Exactamente cinco rutas funcionales; no incluye la ruta del documento. |
| `Schemas` | `IReadOnlyCollection<SharedSchema>` | Esquemas compartidos. | Debe contener `PropertyStatus`, `ProblemDetails`, `HttpValidationProblemDetails`. |

**Invariantes:**
- El archivo es generado; ninguna tarea debe editarlo manualmente.
- La salida es determinista para un mismo código y configuración.
- Todas las operaciones declaradas deben corresponder a endpoints reales de `EndpointDataSource`.
- La ruta de documentación no debe aparecer en `paths` ni en la comparación de deriva.

### `DocumentedEndpoint`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `Method` | `string` | Verbo HTTP. | Uno de `GET`, `POST`, `PUT`; coincide con el endpoint real. |
| `RouteTemplate` | `string` | Plantilla normalizada. | Usa `{id}`; nunca `{id:guid}` en el documento. |
| `QueryParameters` | `IReadOnlyCollection<string>` | Parámetros de consulta documentados. | `page` y `pageSize` en `GetProperties`; ausentes en las demás operaciones. |
| `OperationId` | `string` | Identificador único de operación. | `GetHealth`, `GetProperties`, `GetPropertyById`, `CreateProperty`, `UpdateProperty`. |
| `Tag` | `string` | Agrupación de la operación. | `Salud` o `Propiedades`. |
| `RequestSchema` | `string?` | Esquema del cuerpo de entrada. | Obligatorio en `POST` y `PUT`; ausente en `GET`. |
| `SuccessResponse` | `string` | Esquema de respuesta correcta. | Presente en todas las operaciones. |
| `ErrorResponses` | `IReadOnlyCollection<ErrorResponse>` | Respuestas de error documentadas. | Deben usar `ProblemDetails` o `HttpValidationProblemDetails` por `$ref`. |

**Endpoints cubiertos:**

| Operación | Método y ruta | Respuesta correcta | Errores principales |
|---|---|---|---|
| `GetHealth` | `GET /health` | estado de salud | 500 |
| `GetProperties` | `GET /api/properties` | colección de propiedades | 500 |
| `GetPropertyById` | `GET /api/properties/{id}` | propiedad | 404 |
| `CreateProperty` | `POST /api/properties` | propiedad creada | 400, 409 |
| `UpdateProperty` | `PUT /api/properties/{id}` | propiedad actualizada | 400, 404, 409 |

### `SharedSchema`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `Name` | `string` | Nombre del esquema en `components.schemas`. | Uno de los tres nombres obligatorios. |
| `Shape` | `JsonNode` | Definición del esquema. | Válida para OpenAPI 3.0. |
| `References` | `IReadOnlyCollection<string>` | Rutas `$ref` que lo consumen. | Al menos una referencia; sin definiciones inline equivalentes. |

**Catálogo obligatorio:**
- `PropertyStatus`: estados admitidos de una propiedad; se referencia desde el campo `status` de respuestas de propiedad.
- `ProblemDetails`: errores generales de la API.
- `HttpValidationProblemDetails`: errores de validación de cuerpo.

### `RouteNormalizationRule`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `Source` | `string` | Plantilla original de ASP.NET Core. | Ejemplo: `/api/properties/{id:guid}`. |
| `Normalized` | `string` | Plantilla publicada. | Ejemplo: `/api/properties/{id}`. |
| `AppliesTo` | `string` | Documento y pruebas de deriva. | Debe aplicarse en ambos para evitar falsas diferencias. |

### `RedoclyConfiguration`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `Extends` | `string[]` | Reglas base. | Debe incluir `recommended`. |
| `SecurityDefined` | `string` | Regla `security-defined`. | Debe ser `off`. |
| `Operation4xxResponse` | `string` | Regla `operation-4xx-response`. | Debe ser `warn`. |

### `NswagConfiguration`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `ToolVersion` | `string` | Versión fijada de `NSwag.ConsoleCore`. | `14.8.0`. |
| `Runtime` | `string` | Runtime de ejecución de NSwag. | `Net110`. |
| `InputDocument` | `string` | Documento de entrada. | `../../app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, relativo al archivo de configuración. |
| `OutputDirectory` | `string` | Destino del cliente de humo. | `../../artifacts/openapi-client-smoke/`, relativa al archivo de configuración e ignorada por Git. |
| `Command` | `string` | Generador configurado. | `codeGenerators.openApiToCSharpClient`. |

### `RegenerationScript`

| Campo | Tipo | Descripción | Validación |
|---|---|---|---|
| `Path` | `string` | Script de regeneración. | `support/scripts/generate-openapi-v1.sh`. |
| `WorkingDirectory` | `string` | Directorio de ejecución esperado. | Raíz del repositorio. |
| `Phases` | `string[]` | build → exists → Redocly → tool restore → NSwag → report. | Fail-fast; cualquier fallo detiene el script. |
| `ExitCode` | `int` | Resultado. | `0` solo si todas las fases pasan. |
| `UiArtifacts` | `string[]` | Artefactos visuales. | Deben estar vacíos. |

## Relaciones

- `OpenApiDocumentV1` contiene muchos `DocumentedEndpoint` y muchos `SharedSchema`.
- Cada `DocumentedEndpoint` referencia cero o más `SharedSchema` mediante `$ref`.
- `RouteNormalizationRule` se aplica a `DocumentedEndpoint.RouteTemplate` y a la comparación de deriva.
- `RegenerationScript` consume `OpenApiDocumentV1`, `RedoclyConfiguration` y `NswagConfiguration`.
- `NswagConfiguration` produce artefactos en `artifacts/openapi-client-smoke/`, que no forman parte de la solución ni del documento publicado.

## Persistencia

No aplica. No se crean tablas, migraciones EF Core, repositorios ni contextos de base de datos adicionales. El único archivo versionado generado es `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`; el resto de artefactos de validación son temporales o de configuración.
