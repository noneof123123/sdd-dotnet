# Contrato estructural: documento `openapi/v1.json`

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Objetivo

Definir la forma observable del documento generado. El documento es un derivado de la implementación; este contrato describe las invariantes que deben cumplirse después de cada regeneración.

## Identidad del documento

| Propiedad | Valor esperado |
|---|---|
| `openapi` | `3.0.x` |
| `info.title` | Identificador legible de RealtorApi |
| `info.version` | `v1` |
| `servers` | Opcional; si existe, no debe apuntar a Swagger UI ni ReDoc UI |
| Archivo | `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` |
| Ruta pública | `GET /openapi/v1.json` |

## Operaciones obligatorias

| `operationId` | Método | Plantilla | Tag | Query | Body | Éxito | Errores documentados |
|---|---|---|---|---|---|---|---|
| `GetHealth` | `GET` | `/health` | `Salud` | — | — | 200 | 500 |
| `GetProperties` | `GET` | `/api/properties` | `Propiedades` | `page`, `pageSize` | — | 200 | 500 |
| `GetPropertyById` | `GET` | `/api/properties/{id}` | `Propiedades` | — | — | 200 | 404 |
| `CreateProperty` | `POST` | `/api/properties` | `Propiedades` | — | requerido | 201 | 400, 409 |
| `UpdateProperty` | `PUT` | `/api/properties/{id}` | `Propiedades` | — | requerido | 200 | 400, 404, 409 |
| `ChangePropertyStatus` | `PATCH` | `/api/properties/{id}/status` | `Propiedades` | — | requerido (`application/json`) | 200 | 400, 404, 500 |

**Reglas:**

- El inventario actual contiene seis operaciones en cuatro rutas.
- El orden de paths y operaciones no es semántico.
- No se permite ninguna operación adicional en `paths`.
- La ruta `/openapi/v1.json` no debe aparecer en `paths`.
- La ruta `/assets/properties/{fileName}` no debe aparecer porque es contenido estático, no endpoint de API.
- Ninguna plantilla debe contener restricciones de ASP.NET Core; `{id:guid}` se publica como `{id}`.

## Esquemas compartidos

`components.schemas` debe contener al menos:

| Esquema | Uso obligatorio |
|---|---|
| `PropertyStatus` | Referenciado desde el campo `status` de respuestas de propiedad. |
| `ProblemDetails` | Referenciado desde errores generales 404/409. |
| `HttpValidationProblemDetails` | Referenciado desde errores 400 de validación. |

**Reglas de referencia:**
- Las respuestas de error deben usar `schema.$ref`, no definiciones inline equivalentes.
- El campo `status` de respuestas de propiedad debe usar `$ref` a `PropertyStatus`.
- No debe existir más de una definición equivalente inline para `title`, `status`, `detail`, `errors` u otras propiedades de `ProblemDetails`.
- Los tipos `ValidationProblemDetails` y `ProblemDetails` de ASP.NET Core deben conservar su semántica HTTP.

## Representación de errores

- Errores generales: `application/problem+json`, esquema `ProblemDetails`.
- Errores de validación: `application/problem+json`, esquema `HttpValidationProblemDetails`.
- El generador/de transformador debe conservar el `Content-Type` real devuelto por la API.

## Entradas

- `POST /api/properties` y `PUT /api/properties/{id}` declaran `requestBody` con `multipart/form-data`, coherente con los `[FromForm]` y `IFormFile` reales de los slices.
- `PATCH /api/properties/{id}/status` declara `requestBody` con `application/json` y el esquema `ChangePropertyStatusBody`; el parámetro `id` se declara como parámetro de ruta.
- El esquema de entrada debe corresponder al tipo real aceptado por cada slice.
- `required` debe reflejar las reglas de validación aplicadas por FluentValidation; no debe endurecerse ni relajarse respecto a runtime.

## Deriva

El documento se considera alineado cuando:

1. El conjunto normalizado de `(método, ruta)` del documento es igual al conjunto de endpoints reales de la asamblea de API.
2. Se excluyen solo `GET /openapi/v1.json` y endpoints de prueba.
3. `property status` y esquemas de error cumplen las reglas de `$ref` anteriores.
4. La comparación no depende del orden de rutas ni de operaciones.

## Prohibiciones

- Edición manual del JSON generado.
- Múltiples documentos versionados para sortear incompatibilidades de herramientas.
- Swagger UI, ReDoc UI o cualquier configuración visual.
- Generación de cliente en el árbol de código fuente; el cliente de humo pertenece a `artifacts/openapi-client-smoke/`.
