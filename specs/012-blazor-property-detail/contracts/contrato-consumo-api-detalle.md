# Contrato de consumo de API para el detalle de propiedad

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

## Propósito

Fijar cómo la capa de presentación obtiene y transforma los datos de una propiedad. No define ni modifica el contrato HTTP público; consume el endpoint existente publicado en `GET /api/properties/{id}`.

## Endpoint consumido

| Elemento | Contrato |
|---|---|
| Método | `GET` |
| Ruta | `/api/properties/{id}` |
| Parámetro de ruta | `id` de tipo `Guid` |
| Autenticación | no aplica |
| Cuerpo de solicitud | no aplica |
| Respuesta correcta | `200 OK` con `GetPropertyByIdResponse` |
| Respuesta esperada de no encontrado | `404 Not Found` |
| Otras respuestas | se tratan como error de presentación |
| Cancelación | se propaga cuando el token de la solicitud está cancelado |

## Cliente

- Se usa la interfaz Refit tipada `IPropertiesApi.GetByIdAsync(Guid id, CancellationToken cancellationToken)`.
- El cliente se obtiene mediante `IHttpClientFactory` con el nombre `RealtorApi`.
- `BaseAddress` continúa viniendo de configuración.
- Se usa `ApiResponse<GetPropertyByIdResponse>` para inspeccionar código de estado antes de mapear contenido.
- No se usa `HttpClient` directo, `RestService.For`, JavaScript ni acceso a base de datos desde el frontend.

## Campos de la respuesta

| Campo | Tipo de API | Uso en el view model |
|---|---|---|
| `id` | `Guid` | `PropertyDetailsViewModel.Id` |
| `title` | `string` | `Title` y construcción de `ImageAlt` |
| `description` | `string` | `Description` |
| `address` | `string` | `Address` |
| `price` | `decimal` | `Price` con formato `{0:N2}` y cultura `es-ES` |
| `status` | `string` | `StatusLabel` mediante `PropertyStatusLabels.Resolve` |
| `bedroomCount` | `int` | `BedroomCount` |
| `bathroomCount` | `int` | `BathroomCount` |
| `areaSquareMeters` | `decimal` | `AreaSquareMeters` como entero |
| `imageUrl` | `string?` | `ImageUrl` normalizado y `ImageAlt` |
| `createdAt` | `DateTimeOffset` | `CreatedAt` con formato `dd/MM/yyyy HH:mm` |
| `updatedAt` | `DateTimeOffset?` | `UpdatedAt` con el mismo formato o `No disponible` |

## Reglas de mapeo

- Un `imageUrl` nulo, vacío o formado solo por espacios se normaliza a nulo.
- Con imagen, `ImageAlt` es `Imagen de {title}`.
- Sin imagen, `ImageAlt` es `Imagen no disponible para {title}`.
- `createdAt` y `updatedAt` usan siempre la cultura `es-ES`, independientemente de la configuración regional del host.
- Un estado desconocido se resuelve como `Estado no disponible`.
- El `page` saneado de la solicitud no se envía a la API; solo se usa para construir `BackUrl` y `RetryUrl`.
- El mapeo ocurre en la capa de servicio; Razor no formula ni interpreta valores de la API.

## Resultado del servicio

`IPropertyCatalogService.GetDetailsAsync(Guid id, int page, CancellationToken cancellationToken)` devuelve siempre un `PropertyDetailsViewModel` para los casos esperados, en lugar de lanzar hacia el componente.

| Situación | Estado devuelto | `RetryUrl` | Datos visibles |
|---|---|---|---|
| `200 OK` con contenido | `Contenido` | nulo | completos |
| `404 Not Found` | `NoEncontrado` | dirección de retorno al catálogo | ninguno |
| otro código de error o contenido nulo | `Error` | dirección de detalle con contexto `page` | ninguno |
| `HttpRequestException`, `ApiException` o `JsonException` | `Error` | dirección de detalle con contexto `page` | ninguno |
| cancelación con token cancelado | excepción propagada | no aplica | no aplica |

## Normalización de página

- `page < 1` se convierte en 1 antes de construir direcciones.
- Un valor mayor que la última página no se envía a la API ni se recorta en el detalle.
- El catálogo de retorno aplica la página efectiva ya aprobada en la iniciativa 011.

## Pruebas requeridas

- Éxito con todos los campos y formatos.
- Ausencia de imagen y de fecha de actualización.
- Estado desconocido.
- `404` mapeado a no encontrado sin datos parciales.
- Error de API y excepción de red mapeados a error con reintento.
- Página inválida saneada a 1 en retorno y reintento.
- Cancelación propagada.
- Llamada exacta a `GetByIdAsync` con el identificador solicitado y el token recibido.

## Compatibilidad

No se modifica el contrato HTTP de la iniciativa 007, el documento OpenAPI de la 008, los modelos de dominio ni la persistencia. Cualquier divergencia detectada entre este contrato y el OpenAPI publicado debe resolverse a favor del OpenAPI y documentarse antes de implementar.
