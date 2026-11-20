# Contrato de navegación del catálogo

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Estado: contrato de interfaz web derivado de `spec.md`

## Rutas canónicas

| Propósito | Ruta | Parámetros | Respuesta |
|---|---|---|---|
| Catálogo | `/` | `page` opcional | Página de catálogo en Static SSR. |
| Detalle | `/properties/{id:guid}` | `page` opcional | Página de detalle en Static SSR. |
| Ruta inexistente | `/not-found` | ninguno | Vista de ruta inexistente. |
| Error no controlado | `/Error` | ninguno | Vista de error genérico. |

La ruta `/properties/{id:guid}` solo coincide con GUID válidos. Un identificador inválido debe resolverse como ruta inexistente, no como error de API.

## Parámetro `page`

- El nombre es `page`, minúsculo.
- El valor es un entero positivo en formato decimal.
- Ausente, vacío, no numérico, cero, negativo o con desbordamiento de `int` se sanea a `1`.
- El valor saneado es el único que se usa para consultar y para construir enlaces internos.
- La API puede devolver una página corregida; la vista usa `response.page` para mostrar el estado efectivo, pero los enlaces se construyen sobre esa página efectiva.
- El tamaño de página no viaja en la URL; se mantiene fijo en `12`.

## Enlaces generados

| Origen | Destino | Regla |
|---|---|---|
| Tarjeta del catálogo | `/properties/{id}?page={Page}` | Conserva la página actual. |
| Enlace Anterior | `/?page={Page - 1}` | Solo si `hasPrevious` es `true`. |
| Enlace Siguiente | `/?page={Page + 1}` | Solo si `hasNext` es `true`. |
| Reintento de catálogo | `/?page={Page}` | Misma página saneada. |
| Volver del detalle | `/?page={PageOrigen}` | Página saneada recibida en la URL de detalle. |
| Reintento de detalle | `/properties/{id}?page={PageOrigen}` | Mismo identificador y contexto. |

Todos son enlaces `<a href>` generados en servidor. No se usan `history.back()`, formularios ocultos, sesión, cookies ni JavaScript.

## Navegación mejorada

- Se conserva `blazor.web.js` para que Blazor intercepte los enlaces internos.
- No se define `@rendermode` en `Routes`, páginas ni componentes.
- No se usa navegación programática desde código de componente.
- Los enlaces externos, si aparecen, deben abrirse con su comportamiento nativo y no forman parte de este contrato.

## Flujo esperado

1. El visitante abre `/` o `/?page=3`.
2. El servidor sanea la página y consulta la API.
3. Durante la consulta se envía el estado `Cargando` mediante `@attribute [StreamRendering]`.
4. Con datos, se renderizan tarjetas, resumen de página y enlaces válidos.
5. Sin datos, se muestra el estado `Vacio` con una acción de retorno.
6. Con error, se muestra el estado `Error` con reintento a la misma URL saneada.
7. Al abrir una tarjeta, el detalle recibe `page` y ofrece `Volver al catálogo` hacia esa página.
8. Si el identificador no existe, se muestra `NoEncontrado` con retorno al catálogo.

## Criterios verificables

- La URL del catálogo es compartible y reproducible.
- El enlace de detalle conserva la página de origen.
- El enlace de retorno devuelve a la misma página del catálogo sin estado adicional.
- La navegación funciona con JavaScript desactivado.
- Los enlaces deshabilitados no se renderizan como enlaces; solo aparecen los permitidos por `hasPrevious` y `hasNext`.
- A 320, 768 y 1280 px los controles de paginación no se solapan y siguen siendo alcanzables por teclado.
