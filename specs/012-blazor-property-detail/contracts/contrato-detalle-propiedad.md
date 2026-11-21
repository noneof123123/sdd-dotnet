# Contrato de superficie web del detalle de propiedad

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

## Propósito

Fijar el comportamiento observable de `/properties/{id}` para implementación y pruebas bUnit. Este contrato describe la superficie web; no modifica el contrato HTTP de la iniciativa 007 ni el OpenAPI de la 008.

## Ruta y parámetros

| Elemento | Contrato |
|---|---|
| Ruta | `/properties/{id}` |
| Segmento `id` | GUID válido. Si falta o no tiene formato GUID, se atiende con la página global «Página no encontrada» de `NotFound.razor`. |
| Consulta `page` | Opcional. Entero positivo. Valores ausentes, no numéricos o menores que 1 se tratan como 1. |
| Título del documento | `Detalle de la propiedad` |
| Método | Navegación Static SSR con `StreamRendering` |
| Modo interactivo | No aplica |

## Estructura observable

El orden de la página es el siguiente:

1. `<section class="details-section">` con nombre accesible estable, mediante `aria-labelledby="detalle-titulo"`.
2. Enlace `Volver al catálogo` con icono `arrow-left` decorativo y `href` igual a `BackUrl`.
3. Encabezado `h1` con `id="detalle-titulo"`. Debe existir en todos los estados:
   - En `Contenido`, su texto es el título de la propiedad.
   - En estados no satisfactorios, su texto es `Detalle de la propiedad` y se oculta visualmente con la clase `sr-only`.
4. Bloque de contenido completo cuando el estado es `Contenido`; en cualquier otro caso, el componente compartido `PresentationState`.

## Contenido cuando el estado es `Contenido`

| Bloque | Etiqueta o texto | Origen |
|---|---|---|
| Antetítulo | `Ficha de la propiedad` | texto fijo |
| Encabezado | título de la propiedad | `PropertyDetailsViewModel.Title` |
| Dirección | dirección de la propiedad | `PropertyDetailsViewModel.Address` |
| Estado | `Disponible`, `Alquilada`, `Mantenimiento` o `Estado no disponible` | `PropertyDetailsViewModel.StatusLabel` |
| Precio | importe con dos decimales y cultura `es-ES` | `PropertyDetailsViewModel.Price` |
| Imagen | `<img>` con `alt`, `loading="lazy"` | `ImageUrl` y `ImageAlt` |
| Sin imagen | placeholder con icono `image`, `role="img"` y `aria-label` | `ImageAlt` |
| Descripción | texto completo sin truncar | `Description` |
| Dato | `Dormitorios` → `{n} dormitorios` | `BedroomCount` |
| Dato | `Baños` → `{n} baños` | `BathroomCount` |
| Dato | `Superficie` → `{n} m²` | `AreaSquareMeters` |
| Fecha | `Creación` → `dd/MM/yyyy HH:mm` | `CreatedAt` |
| Fecha | `Última actualización` → `dd/MM/yyyy HH:mm` o `No disponible` | `UpdatedAt` |

No se permite mostrar valores vacíos etiquetados, precio sin formatear, fechas ISO ni el identificador técnico como texto visible.

## Estados

| Estado | Condición | Mensaje | Acción | Contenido parcial |
|---|---|---|---|---|
| `Cargando` | estado inicial antes de la respuesta | `Cargando propiedad…` | ninguna | no |
| `Contenido` | respuesta 200 con datos | no aplica | `Volver al catálogo` | no aplica |
| `NoEncontrado` | respuesta 404 | `No encontramos esta propiedad.` | `Volver al catálogo` | no |
| `Error` | respuesta distinta de 200/404 o fallo esperado | `No se pudo cargar la información. Inténtalo de nuevo.` | `Reintentar` | no |

Reglas comunes:

- El enlace `Volver al catálogo` aparece en todos los estados.
- En `NoEncontrado`, la acción del panel y el enlace superior apuntan al catálogo.
- En `Error`, «Reintentar» apunta a la misma dirección de detalle con el contexto `page` saneado; al activarlo se emite una nueva solicitud sin exigir recarga manual.
- En `Cargando` y `Contenido`, `RetryUrl` es nulo.
- Ningún estado distinto de `Contenido` renderiza título, dirección, precio, descripción, imágenes, datos ni fechas de la propiedad.

## Navegación de retorno

- Con `page` ausente o inválido, `BackUrl` es `/?page=1`.
- Con `page=3`, `BackUrl` es `/?page=3`.
- Con `page=999999` y menos páginas disponibles, el detalle conserva `?page=999999`; al volver, el catálogo resuelve y muestra la última página efectiva sin vista vacía.
- El destino de reintento usa `/properties/{id}` o `/properties/{id}?page={n}` según corresponda.

## Accesibilidad

- Debe existir exactamente un `h1` por estado renderizado y coincidir con el objetivo de `FocusOnNavigate`.
- La sección debe tener nombre accesible aunque el estado no sea `Contenido`.
- Los iconos de Lucide son decorativos y llevan `aria-hidden="true"`.
- El placeholder de imagen lleva `role="img"` y `aria-label`.
- El foco visible global debe permanecer visible al navegar con teclado por ambos enlaces.
- Todos los textos visibles están en español.
- Objetivo interno: WCAG 2.1 AA para contraste, estructura de encabezados, foco y texto alternativo.

## Adaptación visual

- Sin frameworks CSS ni estilos inline.
- Base de una columna en móvil.
- A 48 rem, el cuerpo de la ficha se organiza en dos columnas: media a la izquierda y descripción/datos/fechas a la derecha.
- A 80 rem, la cuadrícula del catálogo permanece de tres columnas; la pantalla de detalle no introduce una nueva cuadrícula de tarjetas.
- Se reutilizan tokens y clases existentes de `app.css`.

## Fuera de alcance

Formularios, edición, creación, eliminación, filtros, ordenación, favoritos, mapas, galerías, contenido multimedia adicional, rutas nuevas, cambios de backend y modos interactivos.
