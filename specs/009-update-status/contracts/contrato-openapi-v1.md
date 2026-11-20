# Contrato OpenAPI v1: cambio de estado de propiedad

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`
Documento generado: `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`

## Alcance

La característica añade una operación al documento `v1`, sin crear un nuevo documento ni cambiar la versión. El documento sigue siendo generado desde código y debe regenerarse con el script canónico; no se edita manualmente.

## Inventario esperado tras el cambio

| Elemento | Valor |
|---|---|
| Documento | `v1` |
| Ruta pública del documento | `/openapi/v1.json` |
| Rutas | 4 |
| Operaciones | 6 |
| Nueva ruta | `/api/properties/{id}/status` |
| Nuevo método | `patch` |
| Nuevo `operationId` | `ChangePropertyStatus` |
| Etiqueta | `Propiedades` |

Operaciones esperadas:

| Método | Ruta | `operationId` | Estado |
|---|---|---|---|
| `get` | `/health` | `GetHealth` | Existente |
| `get` | `/api/properties` | `GetProperties` | Existente |
| `get` | `/api/properties/{id}` | `GetPropertyById` | Existente |
| `post` | `/api/properties` | `CreateProperty` | Existente |
| `put` | `/api/properties/{id}` | `UpdateProperty` | Existente |
| `patch` | `/api/properties/{id}/status` | `ChangePropertyStatus` | Nueva |

## Requisitos de la nueva operación

### Parámetros

- Un parámetro de ruta `id`, requerido, de tipo `string` con formato `uuid`.
- Un cuerpo de solicitud `application/json`, requerido en la práctica por validación, con un campo `status` de tipo `string`.
- El campo `status` admite los valores `Available`, `Rented` y `Maintenance` según el contrato HTTP; el documento debe reflejar esta restricción siempre que el generador la exponga.

### Respuestas

| Código | Contenido |
|---|---|
| `200` | Representación pública completa de la propiedad. |
| `400` | `HttpValidationProblemDetails` para errores de validación. |
| `404` | `ProblemDetails` para propiedad inexistente. |
| `500` | `ProblemDetails` genérico sin detalles técnicos. |

No se documenta `409`.

### Esquemas

- Cuerpo de entrada: esquema derivado de `ChangePropertyStatusRequest`/`ChangePropertyStatusBody` con propiedad `status`.
- Respuesta correcta: esquema derivado de `ChangePropertyStatusResponse` con doce propiedades: `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters`, `imageUrl`, `createdAt` y `updatedAt`.
- Errores: esquemas existentes `ProblemDetails` y `HttpValidationProblemDetails`.

### Metadatos

- La constante `ChangePropertyStatusOperationId` se añade a `RealtorOpenApiConstants`.
- La slice usa `WithName`, `WithTags`, `Accepts<ChangePropertyStatusRequest>("application/json")`, `Produces<ChangePropertyStatusResponse>(200)`, `ProducesValidationProblem(400)`, `ProducesProblem(404)` y `ProducesProblem(500)`.
- La documentación y los ejemplos permanecen en español, salvo identificadores y nombres de esquema.

## Regeneración y validación

La implementación debe ejecutar:

```bash
./support/scripts/generate-openapi-v1.sh
```

Posteriormente:

```bash
npx @redocly/cli@2.19.0 lint app/backend/src/RealtorApi/wwwroot/openapi/v1.json --extends=minimal
dotnet tool restore --tool-manifest .config/dotnet-tools.json
dotnet build app/backend/src/RealtorApi/RealtorApi.csproj /p:GenerateDocumentationFile=true /p:NoWarn=1591
dotnet nswag openapi2csclient /input:app/backend/src/RealtorApi/wwwroot/openapi/v1.json /output:/tmp/realtor-openapi-v1-client.cs /namespace:RealtorApi.OpenApi.Validation /clientClass:GeneratedClient /runtime:Net110
```

Criterios:

- `generate-openapi-v1.sh` termina con `OK`.
- No queda deriva: una segunda ejecución no cambia `v1.json`.
- Redocly con `extends=minimal` no reporta errores.
- La generación NSwag local termina correctamente.
- `git diff --exit-code -- app/backend/src/RealtorApi/wwwroot/openapi/v1.json` confirma que el artefacto está actualizado en el cambio.

## Actualizaciones documentales derivadas

Para evitar contradicciones con el artefacto generado, la implementación debe actualizar los inventarios de 008:

- `specs/008-open-api/contracts/get-openapi-v1.http.md`: pasar de 3 rutas/5 operaciones a 4 rutas/6 operaciones y añadir el ejemplo de `PATCH`.
- `specs/008-open-api/contracts/openapi-v1-document.md`: actualizar el inventario y el catálogo de `operationId`.

Estos cambios son documentales y no alteran la semántica de los endpoints existentes ni la spec 008.
