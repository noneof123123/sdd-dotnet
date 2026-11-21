# Modelo de datos: Detalle de propiedad en la experiencia web

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Alcance

Esta feature no introduce entidades de dominio, tablas, columnas, migraciones ni escritura. El modelo se compone del contexto de solicitud, la respuesta de solo lectura que ya expone la API, el view model que consume Razor y los estados de presentación. Los identificadores de campo de este documento son referencias estables para `tasks.md`; no implican crear nuevos tipos salvo que una tarea lo justifique.

### Convención de citas para `tasks.md`

Las tareas citan cada campo con `data-model.md: Entidad.campo`. La correspondencia canónica es la siguiente:

| Prefijo | Entidad canónica | Ejemplo de cita |
|---|---|---|
| SOL | `RequestContext` | `data-model.md: RequestContext.SafePage` |
| API | `PropertyApiResponse` | `data-model.md: PropertyApiResponse.price` |
| DVM | `PropertyDetailsViewModel` | `data-model.md: PropertyDetailsViewModel.BackUrl` |
| EST | `PresentationState` | `data-model.md: PresentationState.NoEncontrado` |
| LAB | `StatusLabel` | `data-model.md: StatusLabel.Available` |
| VIS | `DetailVisual` | `data-model.md: DetailVisual.Focus` |
| VAL | `ValidationRule` | `data-model.md: ValidationRule.VAL-004` |

| Identificador | Cita canónica |
|---|---|
| SOL-001 | `RequestContext.Id` |
| SOL-002 | `RequestContext.RequestedPage` |
| SOL-003 | `RequestContext.SafePage` |
| SOL-004 | `RequestContext.DetailsUrl` |
| SOL-005 | `RequestContext.BackUrl` |
| API-001 | `PropertyApiResponse.id` |
| API-002 | `PropertyApiResponse.title` |
| API-003 | `PropertyApiResponse.description` |
| API-004 | `PropertyApiResponse.address` |
| API-005 | `PropertyApiResponse.price` |
| API-006 | `PropertyApiResponse.status` |
| API-007 | `PropertyApiResponse.bedroomCount` |
| API-008 | `PropertyApiResponse.bathroomCount` |
| API-009 | `PropertyApiResponse.areaSquareMeters` |
| API-010 | `PropertyApiResponse.imageUrl` |
| API-011 | `PropertyApiResponse.createdAt` |
| API-012 | `PropertyApiResponse.updatedAt` |
| DVM-001 | `PropertyDetailsViewModel.Id` |
| DVM-002 | `PropertyDetailsViewModel.Title` |
| DVM-003 | `PropertyDetailsViewModel.Description` |
| DVM-004 | `PropertyDetailsViewModel.Address` |
| DVM-005 | `PropertyDetailsViewModel.Price` |
| DVM-006 | `PropertyDetailsViewModel.StatusLabel` |
| DVM-007 | `PropertyDetailsViewModel.BedroomCount` |
| DVM-008 | `PropertyDetailsViewModel.BathroomCount` |
| DVM-009 | `PropertyDetailsViewModel.AreaSquareMeters` |
| DVM-010 | `PropertyDetailsViewModel.ImageUrl` |
| DVM-011 | `PropertyDetailsViewModel.ImageAlt` |
| DVM-012 | `PropertyDetailsViewModel.CreatedAt` |
| DVM-013 | `PropertyDetailsViewModel.UpdatedAt` |
| DVM-014 | `PropertyDetailsViewModel.BackUrl` |
| DVM-015 | `PropertyDetailsViewModel.State` |
| DVM-016 | `PropertyDetailsViewModel.Message` |
| DVM-017 | `PropertyDetailsViewModel.RetryUrl` |
| EST-001 | `PresentationState.Cargando` |
| EST-002 | `PresentationState.Contenido` |
| EST-003 | `PresentationState.NoEncontrado` |
| EST-004 | `PresentationState.Error` |
| LAB-001 | `StatusLabel.Available` |
| LAB-002 | `StatusLabel.Rented` |
| LAB-003 | `StatusLabel.Maintenance` |
| LAB-004 | `StatusLabel.Unknown` |
| VIS-001 | `DetailVisual.DetailsSection` |
| VIS-002 | `DetailVisual.Heading` |
| VIS-003 | `DetailVisual.BackLink` |
| VIS-004 | `DetailVisual.StatePanel` |
| VIS-005 | `DetailVisual.Icons` |
| VIS-006 | `DetailVisual.Image` |
| VIS-007 | `DetailVisual.Focus` |
| VIS-008 | `DetailVisual.Breakpoints` |
| VAL-001 | `ValidationRule.VAL-001` |
| VAL-002 | `ValidationRule.VAL-002` |
| VAL-003 | `ValidationRule.VAL-003` |
| VAL-004 | `ValidationRule.VAL-004` |
| VAL-005 | `ValidationRule.VAL-005` |
| VAL-006 | `ValidationRule.VAL-006` |
| VAL-007 | `ValidationRule.VAL-007` |
| VAL-008 | `ValidationRule.VAL-008` |
| VAL-009 | `ValidationRule.VAL-009` |

La tabla de cada campo conserva su identificador original para trazabilidad con la spec; el nombre canónico es el que debe aparecer en las tareas.

## Contexto de solicitud

No se crea un tipo nuevo: estos valores se derivan de la ruta y la cadena de consulta.

| Id | Campo | Tipo | Origen | Regla |
|---|---|---|---|---|
| SOL-001 | Identificador de propiedad | `Guid` | segmento `{id:guid}` de `/properties/{id}` | Debe ser un GUID válido. Si no lo es o falta, la ruta no coincide y se atiende con `NotFound.razor`. |
| SOL-002 | Página solicitada | `string?` | parámetro `page` de la consulta | Valores ausentes, no numéricos, negativos o cero se tratan como 1. |
| SOL-003 | Página saneada | `int` | saneamiento de SOL-002 | `page < 1` se convierte en 1. Un valor superior a la última página no se recorta aquí: el catálogo de retorno aplica la página efectiva aprobada en la spec 011. |
| SOL-004 | Dirección de detalle | `string` | SOL-001 y SOL-003 | `/properties/{id}` si SOL-003 es 1; `/properties/{id}?page={SOL-003}` en caso contrario. Se usa como destino de «Reintentar». |
| SOL-005 | Dirección de retorno | `string` | SOL-003 | `/?page={SOL-003}` con SOL-003 saneado. |

## Respuesta de la API consumida

Tipo existente: `RealtorWeb.Features.Properties.GetPropertyByIdResponse`, obtenido por `IPropertiesApi.GetByIdAsync`. No se modifica.

| Id | Campo | Tipo | Regla de consumo |
|---|---|---|---|
| API-001 | `id` | `Guid` | Se copia al view model. No se muestra como texto visible. |
| API-002 | `title` | `string` | Se usa como encabezado `h1` y, con imagen, para construir el texto alternativo. |
| API-003 | `description` | `string` | Se muestra completa; no se trunca en detalle. |
| API-004 | `address` | `string` | Se muestra en el bloque de identificación. |
| API-005 | `price` | `decimal` | Se formatea con dos decimales y cultura `es-ES`. Ejemplo: `350.000,00`. |
| API-006 | `status` | `string` | Se traduce con `PropertyStatusLabels.Resolve`. Valores esperados: `Available`, `Rented`, `Maintenance`. |
| API-007 | `bedroomCount` | `int` | Se muestra con la etiqueta «Dormitorios». |
| API-008 | `bathroomCount` | `int` | Se muestra con la etiqueta «Baños». |
| API-009 | `areaSquareMeters` | `decimal` | Se muestra como número entero con la etiqueta «Superficie» y sufijo `m²`. |
| API-010 | `imageUrl` | `string?` | Nulo, vacío o solo espacios se normaliza a nulo y produce placeholder con icono. |
| API-011 | `createdAt` | `DateTimeOffset` | Se formatea como `dd/MM/yyyy HH:mm` con cultura `es-ES`. |
| API-012 | `updatedAt` | `DateTimeOffset?` | Si existe se formatea igual que API-011; si no, se muestra «No disponible». |

## View model de detalle

Tipo existente: `RealtorWeb.Features.Properties.PropertyDetailsViewModel`. No se añaden campos nuevos; su forma actual es suficiente para la spec 012.

| Id | Campo | Tipo | Origen | Regla |
|---|---|---|---|---|
| DVM-001 | `Id` | `Guid` | API-001 o SOL-001 en placeholders | Identifica la solicitud; nunca se expone como texto. |
| DVM-002 | `Title` | `string` | API-002 | Texto del `h1` cuando hay contenido. En estados no satisfactorios es vacío. |
| DVM-003 | `Description` | `string` | API-003 | Se renderiza solo en `Contenido`. |
| DVM-004 | `Address` | `string` | API-004 | Se renderiza solo en `Contenido`. |
| DVM-005 | `Price` | `string` | API-005 | Ya localizado; la vista no vuelve a formatear. |
| DVM-006 | `StatusLabel` | `string` | API-006 | Uno de «Disponible», «Alquilada», «Mantenimiento» o «Estado no disponible». |
| DVM-007 | `BedroomCount` | `int` | API-007 | Se muestra solo en `Contenido`. |
| DVM-008 | `BathroomCount` | `int` | API-008 | Se muestra solo en `Contenido`. |
| DVM-009 | `AreaSquareMeters` | `int` | API-009 | Convertido desde `decimal` en el mapeo existente. |
| DVM-010 | `ImageUrl` | `string?` | API-010 | Si es nulo se dibuja placeholder; si no, se usa como `src`. |
| DVM-011 | `ImageAlt` | `string` | API-002 y API-010 | Con imagen: «Imagen de {título}». Sin imagen: «Imagen no disponible para {título}». |
| DVM-012 | `CreatedAt` | `string` | API-011 | Texto listo para mostrar con etiqueta «Creación». |
| DVM-013 | `UpdatedAt` | `string` | API-012 | Texto listo para mostrar con etiqueta «Última actualización». |
| DVM-014 | `BackUrl` | `string` | SOL-005 | Destino de «Volver al catálogo» en todos los estados. |
| DVM-015 | `State` | `PresentationState` | selección de estado | Determina qué rama renderiza `PropertyDetails.razor`. |
| DVM-016 | `Message` | `string` | tabla de estados | Texto mostrado por `PresentationState` en estados no satisfactorios. |
| DVM-017 | `RetryUrl` | `string?` | SOL-004 o SOL-005 | No encontrado: dirección de retorno. Error: dirección de detalle con contexto. Contenido y carga: nulo. |

## Estados de presentación

Tipo existente: `RealtorWeb.Features.Properties.PresentationState`. No se redefinen sus valores.

| Id | Estado | Cuándo se selecciona | Mensaje | Acción |
|---|---|---|---|---|
| EST-001 | `Cargando` | valor inicial del componente antes de obtener la respuesta | «Cargando propiedad…» | Ninguna |
| EST-002 | `Contenido` | la API responde 200 con contenido | no se usa | «Volver al catálogo» |
| EST-003 | `NoEncontrado` | la API responde 404 | «No encontramos esta propiedad.» | «Volver al catálogo» |
| EST-004 | `Error` | la API responde distinto de 200/404 o falla la obtención | «No se pudo cargar la información. Inténtalo de nuevo.» | «Reintentar» |

Reglas del estado:

- Ningún estado distinto de `Contenido` debe renderizar título, descripción, precio ni datos parciales.
- Todo estado debe ofrecer un encabezado `h1` accesible y un destino de foco válido.
- `Cargando` no ofrece acción porque la navegación ya está en curso.
- La cancelación del token con el token cancelado se propaga; no se convierte en `Error`.

## Etiquetas de estado

Tipo existente: `RealtorWeb.Features.Properties.PropertyStatusLabels`.

| Id | Valor de API | Etiqueta visible |
|---|---|---|
|LAB-001 | `Available` | Disponible |
|LAB-002 | `Rented` | Alquilada |
|LAB-003 | `Maintenance` | Mantenimiento |
|LAB-004 | cualquier otro valor | Estado no disponible |

## Superficie visible y estilos

No se crean clases nuevas salvo que una tarea lo justifique; se reutilizan las clases existentes de `app.css`.

| Id | Elemento | Clase o contrato | Regla |
|---|---|---|---|
| VIS-001 | sección de detalle | `details-section` | Envuelve el estado renderizado; debe tener nombre accesible estable aunque no haya contenido. |
| VIS-002 | encabezado de detalle | `h1` con identificador estable | Existe en contenido y en estados no satisfactorios; destino de `FocusOnNavigate`. |
| VIS-003 | enlace de retorno | `back-link` | Siempre visible; usa DVM-014. |
| VIS-004 | panel de estado | `state-panel` | Usado por `PresentationState` para carga, error y no encontrado. |
| VIS-005 | iconos | LucideBlazor | Todos los iconos decorativos llevan `aria-hidden="true"`. |
| VIS-006 | imagen | `details-image` o placeholder existente | Usa DVM-010 y DVM-011; sin estilos inline. |
| VIS-007 | foco | regla global de foco visible | Debe permanecer observable con teclado. |
| VIS-008 | breakpoints | 48 rem y 80 rem en `app.css` | Una columna en móvil; dos columnas para el cuerpo del detalle en pantalla mediana. |

## Reglas de validación y consistencia

| Id | Regla | Resultado |
|---|---|---|
| VAL-001 | SOL-001 no es GUID válido o no existe la ruta | `NotFound.razor`, sin exponer el valor técnico |
| VAL-002 | GUID válido sin propiedad en la respuesta pública | EST-003 |
| VAL-003 | SOL-002 ausente, no numérico, cero o negativo | SOL-003 = 1 |
| VAL-004 | SOL-003 mayor que la última página | el detalle conserva el valor; el catálogo de retorno muestra la última página efectiva |
| VAL-005 | API-010 nulo, vacío o con espacios | DVM-010 = nulo y placeholder con DVM-011 de ausencia |
| VAL-006 | API-012 nulo | DVM-013 = «No disponible» |
| VAL-007 | API-006 desconocido | DVM-006 = «Estado no disponible» |
| VAL-008 | fallo de red, serialización o API distinto de 404 | EST-004 con DVM-017 de reintento |
| VAL-009 | contenido válido | EST-002 con los 17 campos del view model poblados según las reglas anteriores |

## Persistencia

No aplica. No hay entidades persistidas, claves foráneas, índices, migraciones ni datos de sesión. El único efecto observado es la navegación a otra dirección y una nueva solicitud HTTP de solo lectura.
