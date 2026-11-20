# Contrato de vistas de propiedad

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Estado: contrato de interfaz web derivado de `spec.md`

## Catálogo

### Estructura visible

1. Encabezado con título «Propiedades» y texto de resumen de página.
2. Lista de tarjetas, una por cada elemento devuelto por la API.
3. Controles de paginación: Anterior, resumen «Página X de Y» y Siguiente.
4. Bloque de estado para carga, vacío o error.

### Tarjeta de propiedad

Cada tarjeta muestra:

| Elemento | Origen | Presentación |
|---|---|---|
| Imagen | `imageUrl` | Imagen con `alt` descriptivo; si falta, marcador de posición accesible. |
| Estado | `status` traducido | Etiqueta «Disponible», «Alquilada» o «Mantenimiento». |
| Título | `title` | Texto principal y enlace al detalle. |
| Precio | `price` | Formato monetario `es-ES`. |
| Dirección | `address` | Texto plano. |
| Resumen | `description` | Máximo 180 caracteres. |
| Datos | `bedroomCount`, `bathroomCount`, `areaSquareMeters` | Texto con iconos decorativos Lucide. |
| Enlace | `DetailsUrl` | Conserva `page`. |

El valor técnico de `status` no aparece en atributos visibles ni en texto.

### Estados del catálogo

| Estado | Texto visible | Acción |
|---|---|---|
| Cargando | «Cargando propiedades…» | Ninguna. |
| Contenido | Tarjetas y paginación | Enlaces válidos. |
| Vacio | «No hay propiedades disponibles en esta página.» | Enlace a la página válida menor o a la primera. |
| Error | «No se pudo cargar la información. Inténtalo de nuevo.» | Enlace de reintento a la misma URL saneada. |

No se muestran tarjetas de ejemplo, datos simulados ni mensajes en inglés.

## Detalle de propiedad

### Estructura visible

1. Enlace «Volver al catálogo» hacia `/?page={paginaOrigen}`.
2. Título y etiqueta de estado traducida.
3. Imagen principal con alternativa textual o marcador de posición.
4. Precio y dirección.
5. Descripción completa.
6. Datos de dormitorios, baños y superficie.
7. Fechas de creación y actualización con formato `es-ES`.
8. Bloque de estado para carga, no encontrado o error.

### Estados del detalle

| Estado | Texto visible | Acción |
|---|---|---|
| Cargando | «Cargando propiedad…» | Ninguna. |
| Contenido | Todos los campos anteriores | Volver al catálogo. |
| NoEncontrado | «No encontramos esta propiedad.» | Volver al catálogo. |
| Error | «No se pudo cargar la información. Inténtalo de nuevo.» | Reintento a la misma URL de detalle. |

## Formato y accesibilidad

- Idioma del documento: `lang="es"`.
- Foco visible en enlaces con contorno y anillo contrastados.
- Contraste de texto y fondos con objetivo WCAG 2.1 AA interno.
- Iconos Lucide decorativos con `aria-hidden="true"`.
- Iconos que son el único contenido de un control requieren `aria-label` en español.
- Imágenes con `alt` descriptivo; el marcador de posición no usa texto alternativo vacío cuando representa información.
- La jerarquía usa `h1` para el título de página y `h2` para secciones; las tarjetas usan `h2` o `h3` coherente dentro de la lista.
- En 320 px la tarjeta apila contenido; en 768 px usa dos columnas; en 1280 px usa cuadrícula de catálogo legible sin desbordamiento horizontal.

## Codificación y texto

Todos los mensajes de este contrato están en español. Los identificadores de estado internos (`Cargando`, `Contenido`, `Vacio`, `Error`, `NoEncontrado`) no se muestran al visitante; solo determinan el bloque renderizado.

## Criterios de aceptación específicos

- Cada tarjeta contiene todos los campos requeridos por FR-004 y FR-005.
- La etiqueta de estado nunca muestra `Available`, `Rented` ni `Maintenance`.
- El detalle muestra `createdAt` y `updatedAt` formateados.
- Los estados sin datos no dejan contenedores vacíos sin mensaje.
- El marcado renderizado es verificable con bUnit sin iniciar servidor.
