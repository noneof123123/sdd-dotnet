# Contrato de consumo de API

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Estado: contrato derivado de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`

## Principios

- El OpenAPI `v1` de la capacidad 008 es la fuente de verdad.
- Este contrato solo describe cómo el frontend consume dos operaciones existentes.
- No se modifica el backend, el OpenAPI, el orden de resultados ni las reglas de negocio.
- No se usa `HttpClient` directo desde Razor ni `RestService.For<T>()` fuera del registro en dependencias.

## Configuración

Clave: `ApiClients:RealtorApi:BaseAddress`.

Valor recomendado para desarrollo local con el perfil HTTP del backend: `http://localhost:5085/`.

El registro debe usar un cliente nombrado `RealtorApi` con la interfaz tipada `IPropertiesApi`. La base address debe configurarse desde la configuración; no se codifica en componentes ni en el servicio.

## Interfaz tipada prevista

| Método | Verbo y ruta | Parámetros | Respuesta esperada |
|---|---|---|---|
| `ListAsync` | `GET /api/properties` | `page`, `pageSize`, `cancellationToken` | `ApiResponse<ListPropertiesResponse>` |
| `GetByIdAsync` | `GET /api/properties/{id}` | `id`, `cancellationToken` | `ApiResponse<GetPropertyByIdResponse>` |

Ambos métodos devuelven `ApiResponse<T>` para distinguir correctamente código exitoso, ausencia y error sin lanzar excepciones de transporte como flujo normal.

## Solicitudes

### Catálogo

```text
GET /api/properties?page={page}&pageSize=12
```

- `page` es el valor saneado de la query de la página.
- `pageSize` es `12`, fijo para toda la fundación.
- `cancellationToken` es `HttpContext.RequestAborted`.

### Detalle

```text
GET /api/properties/{id}
```

- `id` es un `Guid` válido.
- `cancellationToken` es `HttpContext.RequestAborted`.
- No se envían parámetros de página al backend.

## Respuestas gestionadas

| Situación | Resultado para la UI |
|---|---|
| `200` con datos | View model de catálogo o detalle y estado `Contenido`. |
| `200` con `items` vacío | Estado `Vacio` en catálogo. |
| `404` en detalle | Estado `NoEncontrado`. |
| `400`, `500` u otro código | Estado `Error`. |
| Excepción de red o cancelación distinta de la solicitud | Estado `Error`. |
| Cancelación de la solicitud | No se escribe una respuesta de error artificial; ASP.NET gestiona la cancelación. |

El contenido de `ProblemDetails`, mensajes de excepción, stack traces y detalles de infraestructura nunca se muestra al visitante.

## Campos consumidos

### Lista

`items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext`, `hasPrevious`.

Cada `items[]`: `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters`, `imageUrl`.

### Detalle

Los campos anteriores de un único elemento más `createdAt` y `updatedAt`.

## Reglas de mapeo

- `status` se traduce mediante `PropertyStatusLabels`; nunca se interpola directamente.
- `price`, fechas y números se formatean con cultura `es-ES`.
- `imageUrl` ausente o inválido activa el marcador de posición.
- `description` se recorta solo en tarjetas.
- La paginación visible usa los valores devueltos por la API, no los calculados en el frontend.
- La URL de retorno se construye con la página saneada recibida por la solicitud web, no con la respuesta de la API.

## Aislamiento en pruebas

- Las pruebas usan stubs manuales de `IPropertiesApi`; no realizan llamadas HTTP.
- Los stubs pueden simular respuesta correcta, vacía, 404, error y tarea pendiente para `StreamRendering`.
- No se usa `WebApplicationFactory`, navegador ni base de datos para cumplir los criterios de esta feature.

## Regresión

Después de la implementación deben pasar las pruebas OpenAPI de la capacidad 008. Si el número de rutas u operaciones cambia, la ejecución se detiene y se reporta divergencia; este plan no autoriza actualizar el contrato del backend.
