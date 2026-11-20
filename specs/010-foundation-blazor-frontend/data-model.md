# Modelo de datos: Fundación del frontend web

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Entrada: `spec.md`, `plan.md`, `research.md` y `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`

## Alcance

No existen cambios de persistencia. Este documento describe los modelos de vista que se construyen en el frontend a partir de las respuestas de la API, las reglas de presentación y los estados de la interfaz. Los DTO de transporte siguen perteneciendo al contrato HTTP de la capacidad 008; los view models son propios de la capacidad 010.

## Modelos de transporte consumidos

### `ListPropertiesResponse`

| Campo | Tipo | Uso en la vista |
|---|---|---|
| `items` | `ListPropertiesItemResponse[]` | Origen de las tarjetas del catálogo. |
| `page` | `int` | Página efectiva devuelta por la API; sustituye al valor de la URL. |
| `pageSize` | `int` | Tamaño efectivo devuelto por la API. |
| `totalItems` | `int` | Base del texto resumen y del cálculo de páginas. |
| `totalPages` | `int` | Página final para el resumen y la paginación. |
| `hasNext` | `bool` | Habilita el enlace Siguiente. |
| `hasPrevious` | `bool` | Habilita el enlace Anterior. |

### `ListPropertiesItemResponse`

| Campo | Tipo | Uso en la vista |
|---|---|---|
| `id` | `Guid` | Identificador para construir la URL de detalle. |
| `title` | `string` | Título principal de la tarjeta. |
| `description` | `string` | Resumen de la tarjeta; se recorta de forma determinista. |
| `address` | `string` | Dirección visible en tarjeta y detalle. |
| `price` | `decimal` | Precio con formato de moneda español. |
| `status` | `PropertyStatus` | Se traduce a una etiqueta visible. |
| `bedroomCount` | `int` | Dormitorios. |
| `bathroomCount` | `int` | Baños. |
| `areaSquareMeters` | `int` | Superficie en metros cuadrados. |
| `imageUrl` | `string?` | Imagen opcional; valor vacío activa el marcador de posición. |

### `GetPropertyByIdResponse`

Contiene todos los campos anteriores más `createdAt` y `updatedAt`, ambos `DateTimeOffset`. El detalle muestra fechas con formato local legible en español.

### `PropertyStatus`

| Valor de API | Etiqueta visible |
|---|---|
| `Available` | Disponible |
| `Rented` | Alquilada |
| `Maintenance` | Mantenimiento |
| Cualquier otro valor | Estado no disponible |

El valor técnico no se muestra en la interfaz. La traducción se calcula en `PropertyStatusLabels`, no en los componentes.

## View models

### `PropertyCardViewModel`

| Propiedad | Tipo | Regla de origen |
|---|---|---|
| `Id` | `Guid` | Copia de `item.id`. |
| `Title` | `string` | Copia de `item.title`. |
| `Summary` | `string` | `description` recortado a 180 caracteres con elipsis cuando excede el límite. |
| `Address` | `string` | Copia de `item.address`. |
| `Price` | `string` | `price` formateado con `es-ES`; sin símbolo de moneda configurable. |
| `StatusLabel` | `string` | Resultado de traducir `item.status`. |
| `BedroomCount` | `int` | Copia de `item.bedroomCount`. |
| `BathroomCount` | `int` | Copia de `item.bathroomCount`. |
| `AreaSquareMeters` | `int` | Copia de `item.areaSquareMeters`. |
| `ImageUrl` | `string?` | `null`, vacío o solo espacios se normaliza a `null`. |
| `ImageAlt` | `string` | `"Imagen de {Title}"` cuando existe imagen; `"Imagen no disponible para {Title}"` cuando no. |
| `DetailsUrl` | `string` | `/properties/{Id}?page={PaginaSaneada}`. |

### `PropertyCatalogViewModel`

| Propiedad | Tipo | Regla de origen |
|---|---|---|
| `Items` | `IReadOnlyList<PropertyCardViewModel>` | Mapa de `response.items` con la página saneada actual. |
| `Page` | `int` | `response.page` normalizado a al menos `1`. |
| `PageSize` | `int` | `response.pageSize` normalizado a al menos `1`. |
| `TotalItems` | `int` | `response.totalItems` normalizado a al menos `0`. |
| `TotalPages` | `int` | `response.totalPages` normalizado a al menos `1`. |
| `HasNext` | `bool` | `response.hasNext`. |
| `HasPrevious` | `bool` | `response.hasPrevious`. |
| `PreviousUrl` | `string?` | `/?page={Page - 1}` cuando `HasPrevious`; en otro caso `null`. |
| `NextUrl` | `string?` | `/?page={Page + 1}` cuando `HasNext`; en otro caso `null`. |
| `SummaryText` | `string` | `"Página {Page} de {TotalPages}".` |
| `State` | `PresentationState` | Calculado por `PropertyPresentation`. |
| `Message` | `string` | Texto visible del estado, siempre en español. |
| `RetryUrl` | `string?` | `/?page={Page}` para estados de error o vacío tras filtros agotados. |

### `PropertyDetailsViewModel`

| Propiedad | Tipo | Regla de origen |
|---|---|---|
| `Id` | `Guid` | Copia de `response.id`. |
| `Title` | `string` | Copia de `response.title`. |
| `Description` | `string` | Copia completa de `response.description`. |
| `Address` | `string` | Copia de `response.address`. |
| `Price` | `string` | Formato monetario `es-ES`. |
| `StatusLabel` | `string` | Resultado de traducir `response.status`. |
| `BedroomCount` | `int` | Copia de `response.bedroomCount`. |
| `BathroomCount` | `int` | Copia de `response.bathroomCount`. |
| `AreaSquareMeters` | `int` | Copia de `response.areaSquareMeters`. |
| `ImageUrl` | `string?` | Normalizado como en la tarjeta. |
| `ImageAlt` | `string` | `"Imagen de {Title}"` o `"Imagen no disponible para {Title}"`. |
| `CreatedAt` | `string` | `createdAt` con formato `es-ES` legible. |
| `UpdatedAt` | `string` | `updatedAt` con formato `es-ES` legible. |
| `BackUrl` | `string` | `/?page={PaginaOrigenSaneada}`. |
| `State` | `PresentationState` | Calculado por `PropertyPresentation`. |
| `Message` | `string` | Texto visible del estado, siempre en español. |
| `RetryUrl` | `string?` | `/properties/{Id}?page={PaginaOrigenSaneada}` en error; `BackUrl` en no encontrado. |

### `PresentationState`

| Valor | Cuándo se usa | Acción visible |
|---|---|---|
| `Cargando` | La petición SSR aún no termina. | Mensaje «Cargando propiedades…» o «Cargando propiedad…»; sin enlaces de paginación. |
| `Contenido` | La respuesta contiene datos. | Tarjetas o detalle y navegación válida. |
| `Vacio` | El catálogo devuelve `items` vacío. | Mensaje «No hay propiedades disponibles en esta página.» y enlace a la página válida menor o a la primera. |
| `Error` | Fallo de transporte, tiempo de espera o código distinto de 200/404. | Mensaje «No se pudo cargar la información. Inténtalo de nuevo.» y reintento a la misma URL saneada. |
| `NoEncontrado` | `GET /api/properties/{id}` devuelve 404. | Mensaje «No encontramos esta propiedad.» y enlace al catálogo. |

## Reglas de saneamiento y normalización

1. `page` de la query:
   - ausente, vacío, no numérico, negativo o cero → `1`;
   - entero positivo → ese valor;
   - desbordamiento de `int` → `1`.
2. El `id` de detalle debe ser un `Guid` válido según la restricción de ruta. Si no lo es, la ruta no coincide y se sirve la vista de ruta inexistente.
3. El servicio usa el `page` saneado para consultar, pero muestra los valores devueltos por la API cuando son válidos.
4. `pageSize` no viaja en la URL; se fija en `12` para la fundación y se mantiene estable en reintentos.
5. Un `totalPages` menor que `1` se muestra como `1`.
6. Un `imageUrl` sin esquema absoluto o relativo usable se trata como ausente.
7. Toda cadena de estado se calcula en el servidor; ningún componente recibe el enum sin traducir.

## Formato visible

- Precio: `string.Format(CultureInfo.GetCultureInfo("es-ES"), "{0:N2}", value)`.
- Fechas: patrón corto de `es-ES`, por ejemplo `dd/MM/yyyy HH:mm`.
- Números de dormitorios, baños y superficie: enteros con separador de millares `es-ES` cuando corresponda.
- Resumen de tarjeta: máximo `180` caracteres, con `…` al final si se recorta.
- Texto de paginación: «Página {Page} de {TotalPages}».
- Etiquetas de control sin texto: `aria-label` en español, por ejemplo «Página anterior» y «Página siguiente».

## Relaciones

```text
ListPropertiesResponse + página saneada
    -> PropertyCatalogViewModel
        -> IReadOnlyList<PropertyCardViewModel>
            -> DetailsUrl con página saneada

GetPropertyByIdResponse + página saneada
    -> PropertyDetailsViewModel
        -> BackUrl con página saneada

PropertyStatus
    -> PropertyStatusLabels
        -> StatusLabel Visible
```

## Persistencia

Ninguna. No hay entidades nuevas, tablas, columnas, migraciones ni escritura. El único estado es la URL de la petición y los view models efímeros construidos por solicitud.

## Derivación de requisitos

- FR-001 a FR-008: catálogo, detalle y datos visibles se cubren con los view models anteriores.
- FR-009: `Cargando` se deriva de `@attribute [StreamRendering]`.
- FR-010 a FR-012: `Vacio`, `Error` y `NoEncontrado` cubren los escenarios sin datos.
- FR-013 y FR-023: `PropertyStatusLabels` centraliza las etiquetas.
- FR-014 a FR-016: los anchos, el contraste, el foco y las alternativas se modelan como requisitos de render, no como datos.
- FR-017 a FR-022: el registro de servicios, el saneamiento y la no modificación del backend se reflejan en `plan.md` y `research.md`.
- SC-001 a SC-014: cobertura de navegación, presentación, accesibilidad y regresión.
