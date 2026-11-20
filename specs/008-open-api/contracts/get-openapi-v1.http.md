# Contrato HTTP: `GET /openapi/v1.json`

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Objetivo

Definir la interfaz pública de consulta del documento OpenAPI v1 generado. La ruta sirve exclusivamente el archivo estático versionado; no ofrece UI, no negocia formatos alternativos y no expone un endpoint dinámico en `EndpointDataSource`.

## Solicitud

```http
GET /openapi/v1.json HTTP/1.1
Host: <host-de-la-api>
Accept: application/json
```

- Método: `GET`.
- Ruta: `/openapi/v1.json`.
- Parámetros de ruta, query o cabecera requeridos: ninguno.
- Cuerpo: ninguno.
- Autenticación: ninguna; la API no define esquemas de seguridad.

## Respuesta correcta

```http
HTTP/1.1 200 OK
Content-Type: application/json
```

El cuerpo es el contenido íntegro de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.

**Garantías del cuerpo:**
- Es un documento OpenAPI 3.0 válido.
- `info.version` es `v1`.
- `paths` contiene exactamente las cinco operaciones funcionales documentadas.
- `paths` no contiene `/openapi/v1.json`.
- Las plantillas de ruta usan `{id}`, no `{id:guid}`.
- `components.schemas` contiene `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` referenciados mediante `$ref`.
- No contiene definiciones inline duplicadas de esos esquemas compartidos.
- No contiene configuración de Swagger UI ni ReDoc UI.

## Respuestas de error

| Código | Condición | Cuerpo |
|---|---|---|
| `404 Not Found` | El artefacto generado no existe o no ha sido publicado todavía. | Respuesta estándar del servidor web; no debe interpretarse como un error de negocio. |
| `405 Method Not Allowed` | Se usa un método distinto de `GET`. | Respuesta estándar de ASP.NET Core/static files. |

No se define `ProblemDetails` para esta ruta porque no pertenece a la superficie de la API de negocio y está excluida del documento por aclaración A.

## Consideraciones de caché

La spec no impone directivas de caché. Si se añaden en el futuro, deben permitir obtener la versión generada más reciente después de una regeneración y no alterar el contenido del artefacto.

## Verificación

```bash
curl -fsS -H 'Accept: application/json' http://localhost:5023/openapi/v1.json | jq -r '.openapi, .info.version, (.paths | keys | length)'
```

Salida esperada: versión `3.0.x`, versión de información `v1` y `5` paths.

## Exclusión

Esta operación queda fuera de:
- `paths` del documento generado.
- La comparación de deriva entre documento y `EndpointDataSource`.
- El conjunto de cinco endpoints cubiertos por FR-006.

Motivo: aclaración A aceptada durante `/speckit.clarify`.
