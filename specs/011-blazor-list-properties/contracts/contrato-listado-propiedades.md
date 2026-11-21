# Contrato de interfaz web del listado de propiedades

Rama: `011-blazor-list-properties`  
Fecha: 2026-11-21  
Estado: contrato de interfaz derivado de `spec.md`

## Superficie

| Propósito | Ruta | Parámetros | Salida |
|---|---|---|---|
| Catálogo principal | `/` | `page` opcional | Página de catálogo en Static SSR. |
| Catálogo paginado | `/?page=N` | `page` entero positivo | Misma página con el conjunto de resultados de `N`. |

No se añaden rutas, segmentos ni parámetros nuevos. El tamaño de página no es configurable por el visitante.

## Parámetro `page`

- Ausente, vacío, no numérico, cero, negativo o con desbordamiento se sanea a `1`.
- Un valor superior a la última página disponible se envía al backend; la página mostrada es la página efectiva devuelta, que la capacidad 004 normaliza a la última página disponible.
- La URL de la vista debe quedar en `/?page={Page}` cuando el visitante navega entre páginas.
- Los enlaces internos se construyen sobre la página efectiva, nunca sobre el valor solicitado sin normalizar.

## Contenido exigido en estado `Contenido`

| Elemento | Regla verificable |
|---|---|
| Tarjetas | Una por propiedad devuelta, con título, precio, etiqueta de estado en español, dirección, dormitorios, baños, superficie e imagen o alternativa. |
| Resumen de páginas | Texto visible «Página {Page} de {TotalPages}». |
| Total de elementos | Texto visible «1 propiedad» o «{TotalItems:N0} propiedades» según corresponda. |
| Control «Anterior» | Siempre visible. Enlace `/?page={Page - 1}` si `HasPrevious`; si no, elemento deshabilitado con `aria-disabled="true"` y etiqueta «Página anterior no disponible». |
| Control «Siguiente» | Siempre visible. Enlace `/?page={Page + 1}` si `HasNext`; si no, elemento deshabilitado con `aria-disabled="true"` y etiqueta «Página siguiente no disponible». |
| Grupo de paginación | Elemento de navegación con etiqueta accesible «Paginación del catálogo». |

El control deshabilitado no es un enlace, no recibe foco de teclado y mantiene el mismo texto visible que el control habilitado. La convención de ocultar el control de la iniciativa 010 queda superada únicamente para esta feature.

## Estados sin contenido

| Estado | Regla verificable |
|---|---|
| `Cargando` | Indicación de carga reconocible durante la respuesta inicial; sin tarjetas ni paginación. |
| `Vacio` | Mensaje «No hay propiedades disponibles en esta página.»; sin tarjetas ficticias ni paginación. |
| `Error` | Mensaje «No se pudo cargar la información. Inténtalo de nuevo.» y reintento a `/?page={Page}`; sin paginación. |

Ningún estado expone códigos de error, rutas internas ni valores técnicos de estado.

## Interacción y accesibilidad

- Toda la navegación se resuelve con enlaces `<a href>` generados en servidor; no se usan `@onclick`, JavaScript propio, `history.back()`, sesión ni cookies.
- El foco de teclado es visible en enlaces y controles habilitados.
- Los controles deshabilitados declaran su estado con `aria-disabled` y una etiqueta en español.
- La grilla mantiene una columna por debajo de 48 rem, dos columnas desde 48 rem y tres columnas desde 80 rem.
- La comprobación visual usa 320 px, 768 px y 1280 px, sin desplazamiento horizontal.
- Todos los textos visibles están en español.

## Criterios verificables

1. Abrir `/` sin parámetros muestra la primera página y el total de elementos.
2. Seleccionar «Siguiente» desde una página con `HasNext` cambia los resultados, el resumen y la URL.
3. En la última página, «Siguiente» aparece deshabilitado y no es enlace.
4. En la primera página, «Anterior» aparece deshabilitado y no es enlace.
5. Una URL con página superior a la última muestra la última página efectiva, no una vista vacía.
6. Los estados vacío y error no muestran tarjetas ni controles de paginación.
7. El consumo HTTP se realiza únicamente mediante el cliente tipado registrado por DI; ningún componente Razor crea un cliente HTTP.

## Límites

Este contrato no modifica el OpenAPI de la iniciativa 008 ni los contratos de la 010. Solo fija los puntos observables exigidos por la spec 011.
