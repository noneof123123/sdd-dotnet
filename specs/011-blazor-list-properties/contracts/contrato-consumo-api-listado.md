# Contrato de consumo del listado paginado

Rama: `011-blazor-list-properties`  
Fecha: 2026-11-21  
Estado: contrato de consumo derivado de `spec.md` y del OpenAPI `v1`

## Endpoint consumido

`GET /api/properties`

El endpoint pertenece a la capacidad aprobada 004. Esta feature no lo modifica y no añade operaciones.

## Parámetros enviados por el frontend

| Nombre | Origen | Regla |
|---|---|---|
| `Page` | Página saneada de `?page=N` o `1` | Entero positivo; se envía siempre. |
| `PageSize` | `CatalogNavigation.PageSize` | Valor fijo `12`; se envía siempre. |
| `CancellationToken` | `HttpContext.RequestAborted` | Se propaga desde la solicitud SSR. |

El cliente es la interfaz tipada `IPropertiesApi`, registrada con Refit mediante `IHttpClientFactory` bajo el nombre `RealtorApi` y con `BaseAddress` tomada de `ApiClients:RealtorApi:BaseAddress`. Está prohibido usar `HttpClient` directo, `new HttpClient()` o `RestService.For` fuera del registro.

## Respuesta 200

| Campo | Uso |
|---|---|
| `items` | Genera `PropertyCardViewModel`. |
| `page` | Define `PropertyCatalogViewModel.Page` como página efectiva. |
| `pageSize` | Define `PropertyCatalogViewModel.PageSize`. |
| `totalItems` | Define `PropertyCatalogViewModel.TotalItems` y `TotalItemsText`. |
| `totalPages` | Define `PropertyCatalogViewModel.TotalPages` y `SummaryText`. |
| `hasNext` | Define si el control «Siguiente» es enlace o deshabilitado. |
| `hasPrevious` | Define si el control «Anterior» es enlace o deshabilitado. |

La observación de `ApiResponse<T>` permite distinguir el código de estado sin convertir respuestas fallidas en excepciones opacas.

## Respuesta 400

Una respuesta de validación se traduce al estado de presentación `Error`. El visitante ve «No se pudo cargar la información. Inténtalo de nuevo.» y una acción de reintento hacia `/?page={Page}`. El detalle técnico de `ValidationProblemDetails` no se muestra.

## Fallos de red o cancelación

- `HttpRequestException`, `ApiException` y errores de deserialización esperados producen el estado `Error`.
- La cancelación solicitada por el cliente se propaga; no se genera un mensaje de error engañoso.
- Ningún fallo deja una pantalla vacía ni expone stack traces.

## Reglas de presentación derivadas

- `Page` mostrada es la devuelta por la API, no la solicitada. Por la aclaración A, una solicitud superior a la última página produce la última página disponible.
- `TotalItems` se normaliza a `>= 0` y `TotalPages` a `>= 1` antes de construir textos.
- El estado traducido de cada propiedad se calcula en `PropertyStatusLabels`; la vista nunca recibe el valor técnico sin traducir.

## Criterios verificables

1. La página principal envía `Page` y `PageSize` en cada consulta.
2. El cliente se obtiene por inyección de dependencias desde el registro Refit.
3. Una respuesta 200 con datos produce tarjetas, resumen, total y controles coherentes.
4. Una respuesta 400 o un fallo de red produce mensaje y reintento, sin paginación.
5. El OpenAPI de la API permanece sin cambios.

## Límites

No se consumen otros endpoints, no se escriben propiedades y no se modifica el contrato HTTP. Cualquier ampliación requiere una spec aprobada nueva.
