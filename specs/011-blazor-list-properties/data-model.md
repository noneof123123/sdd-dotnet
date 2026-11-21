# Modelo de datos: Listado paginado de propiedades en la página principal

Rama: `011-blazor-list-properties`  
Fecha: 2026-11-21  
Entrada: `spec.md`, `plan.md`, `research.md` y `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`

## Alcance

No existen cambios de persistencia. Este documento describe los modelos de vista que el frontend construye por solicitud a partir del contrato ya publicado por la capacidad 004. Los DTO de transporte pertenecen al OpenAPI de la capacidad 008; los view models y las cadenas visibles son propios de esta feature.

## Modelo de transporte consumido

### `ListPropertiesResponse`

| Campo | Tipo | Restricción | Uso en esta feature |
|---|---|---|---|
| `items` | `IReadOnlyList<ListPropertiesItemResponse>` | Puede estar vacío; nunca `null` en el contrato consumido. | Origen de las tarjetas del catálogo. |
| `page` | `int` | Debe normalizarse a al menos `1`. | Página efectiva; prevalece sobre el valor solicitado en la URL. |
| `pageSize` | `int` | Debe normalizarse a al menos `1`; el frontend envía `12`. | Tamaño efectivo devuelto. |
| `totalItems` | `int` | Debe normalizarse a al menos `0`. | Base de `TotalItemsText`. |
| `totalPages` | `int` | Debe normalizarse a al menos `1`. | Base de `SummaryText`. |
| `hasNext` | `bool` | Solo `true` si existe una página posterior. | Determina si el control «Siguiente» es enlace o deshabilitado. |
| `hasPrevious` | `bool` | Solo `true` si existe una página anterior. | Determina si el control «Anterior» es enlace o deshabilitado. |

### `ListPropertiesItemResponse`

Los campos siguientes se consumen sin cambios. El mapeo a tarjeta, el recorte de descripción, el formato de precio y las etiquetas de estado ya están definidos en la iniciativa 010 y no se modifican.

| Campo | Restricción |
|---|---|
| `id` | Sin cambios respecto al contrato OpenAPI consumido. |
| `title` | Sin cambios respecto al contrato OpenAPI consumido. |
| `description` | Sin cambios respecto al contrato OpenAPI consumido. |
| `address` | Sin cambios respecto al contrato OpenAPI consumido. |
| `price` | Sin cambios respecto al contrato OpenAPI consumido. |
| `status` | Sin cambios respecto al contrato OpenAPI consumido. |
| `bedroomCount` | Sin cambios respecto al contrato OpenAPI consumido. |
| `bathroomCount` | Sin cambios respecto al contrato OpenAPI consumido. |
| `areaSquareMeters` | Sin cambios respecto al contrato OpenAPI consumido. |
| `imageUrl` | Sin cambios respecto al contrato OpenAPI consumido. |

## View model de catálogo

### `PropertyCatalogViewModel`

| Campo | Tipo | Restricción y origen |
|---|---|---|
| `Items` | `IReadOnlyList<PropertyCardViewModel>` | Mapa de `response.items`; vacío en estados `Vacio`, `Error` y `Cargando`. |
| `Page` | `int` | Debe ser `>= 1`; se toma de `response.page` y representa la página efectiva, incluso cuando la URL solicitó una página superior a la última. |
| `PageSize` | `int` | Debe ser `>= 1`; el frontend solicita exactamente `12`. |
| `TotalItems` | `int` | Debe ser `>= 0`; copia normalizada de `response.totalItems`. |
| `TotalPages` | `int` | Debe ser `>= 1`; copia normalizada de `response.totalPages`. |
| `HasNext` | `bool` | Copia de `response.hasNext`; `false` exige control deshabilitado. |
| `HasPrevious` | `bool` | Copia de `response.hasPrevious`; `false` exige control deshabilitado. |
| `PreviousUrl` | `string?` | `/?page={Page - 1}` cuando `HasPrevious` es `true`; `null` cuando es `false`. |
| `NextUrl` | `string?` | `/?page={Page + 1}` cuando `HasNext` es `true`; `null` cuando es `false`. |
| `SummaryText` | `string` | Formato exacto «Página {Page} de {TotalPages}», con números en `es-ES`. |
| `TotalItemsText` | `string` | «1 propiedad» cuando `TotalItems` es `1`; en otro caso «{TotalItems:N0} propiedades», con `es-ES`. |
| `State` | `PresentationState` | Calculado por `PropertyPresentation.CatalogState`. |
| `Message` | `string` | Texto visible del estado, siempre en español. |
| `RetryUrl` | `string?` | `/?page={Page}` en error; `null` en contenido. |

### `PropertyCardViewModel`

Sin cambios respecto a la iniciativa 010. `DetailsUrl` se construye sobre la página efectiva.

| Campo | Restricción |
|---|---|
| `Id` | Sin cambios respecto a la iniciativa 010. |
| `Title` | Sin cambios respecto a la iniciativa 010. |
| `Summary` | Sin cambios respecto a la iniciativa 010. |
| `Address` | Sin cambios respecto a la iniciativa 010. |
| `Price` | Sin cambios respecto a la iniciativa 010. |
| `StatusLabel` | Sin cambios respecto a la iniciativa 010. |
| `BedroomCount` | Sin cambios respecto a la iniciativa 010. |
| `BathroomCount` | Sin cambios respecto a la iniciativa 010. |
| `AreaSquareMeters` | Sin cambios respecto a la iniciativa 010. |
| `ImageUrl` | Sin cambios respecto a la iniciativa 010. |
| `ImageAlt` | Sin cambios respecto a la iniciativa 010. |
| `DetailsUrl` | Construido sobre la página efectiva. |

## Cadenas visibles de paginación

### `PropertyPresentation`

| Campo | Valor | Restricción |
|---|---|---|
| `PreviousControlText` | «Anterior» | Texto visible del control para retroceder. |
| `NextControlText` | «Siguiente» | Texto visible del control para avanzar. |
| `PreviousDisabledLabel` | «Página anterior no disponible» | Etiqueta accesible cuando `HasPrevious` es `false`. |
| `NextDisabledLabel` | «Página siguiente no disponible» | Etiqueta accesible cuando `HasNext` es `false`. |
| `PaginationNavLabel` | «Paginación del catálogo» | Etiqueta del grupo de navegación. |
| `LoadingCatalogMessage` | Sin cambios | Texto del estado de carga ya aprobado. |
| `EmptyMessage` | Sin cambios | Texto del estado vacío ya aprobado. |
| `ErrorMessage` | Sin cambios | Texto del estado de error ya aprobado. |

### `EstilosPaginacion`

| Campo | Valor | Restricción |
|---|---|---|
| `DisabledControlClass` | `.button-link--disabled` | Debe derivar sus colores, bordes y espaciado de tokens existentes; no puede definir colores literales nuevos ni estilos inline. |
| `SummaryClass` | `.catalog-summary` | Contenedor existente del resumen; puede ampliarse para incluir `TotalItemsText` sin crear un lenguaje visual paralelo. |

### `PresentationState`

| Valor | Cuándo se usa | Paginación visible |
|---|---|---|
| `Cargando` | La respuesta SSR inicial aún no termina. | No. |
| `Contenido` | La respuesta contiene al menos una propiedad. | Sí, con controles activos o deshabilitados según `HasPrevious` y `HasNext`. |
| `Vacio` | `items` está vacío. | No. |
| `Error` | Fallo de transporte o respuesta no exitosa. | No. |
| `NoEncontrado` | Solo aplica al detalle; no aparece en esta feature. | No. |

## Reglas de normalización

1. `page` ausente, vacío, no numérico, cero, negativo o con desbordamiento se sanea a `1` en `CatalogNavigation`.
2. Un `page` superior a la última página disponible se envía tal cual al backend; la página mostrada es `response.page`, ya normalizada por la capacidad 004 a la última página disponible.
3. El tamaño de página no viaja en la URL y se mantiene en `12`.
4. `totalPages` menor que `1` se muestra como `1`; `totalItems` negativo se muestra como `0`.
5. Los enlaces de paginación se construyen sobre `Page`, nunca sobre el valor solicitado sin normalizar.
6. Ningún componente Razor calcula formatos, etiquetas de estado ni textos de error; todo viene de la capa de presentación.

## Formato visible

- Resumen: «Página {Page} de {TotalPages}».
- Total: «1 propiedad» si `TotalItems` es `1`; si no, «{TotalItems:N0} propiedades» con `CultureInfo.GetCultureInfo("es-ES")`.
- Precio y fechas: reglas sin cambios de la iniciativa 010.
- La URL canónica de paginación es `/?page=N` y es compartible.

## Relaciones

```text
ListPropertiesResponse + página saneada
    -> PropertyCatalogViewModel
        -> IReadOnlyList<PropertyCardViewModel>
        -> SummaryText + TotalItemsText
        -> PreviousUrl / NextUrl o controles deshabilitados
        -> State + Message + RetryUrl
```

## Persistencia

Ninguna. No hay entidades nuevas, tablas, columnas, migraciones ni escritura. El único estado es la URL de la petición y los view models efímeros construidos por solicitud.

## Derivación de requisitos

- FR-001 a FR-004: catálogo en `/`, tamaño fijo, página saneada y URL compartible.
- FR-005: `Page` toma el valor efectivo devuelto por la API, conforme a la aclaración A.
- FR-006: `PropertyCardViewModel` sin cambios cubre los datos visibles.
- FR-007: `SummaryText` y `TotalItemsText` cubren página, total de páginas y total de elementos.
- FR-008 y FR-009: `PreviousUrl`, `NextUrl`, `HasPrevious` y `HasNext` determinan enlace o control deshabilitado.
- FR-010 a FR-013: navegación por enlaces y estados `Cargando`, `Vacio` y `Error` sin cambios funcionales.
- FR-014 a FR-017: `PresentationState`, textos centralizados, `app.css` y atributos accesibles.
- FR-018 a FR-020: sin CRUD, filtros, ordenación, cambios de backend ni reglas de negocio nuevas.
- SC-001 a SC-012: cobertura mediante view models, contratos y pruebas descritas en `quickstart.md`.
