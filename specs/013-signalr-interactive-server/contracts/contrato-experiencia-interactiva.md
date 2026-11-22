# Contrato de experiencia interactiva

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](../spec.md)

Este contrato describe la superficie observable por el visitante. No define tecnología, archivos ni mecanismos internos.

## Rutas y contexto

| Vista | Dirección | Contexto obligatorio | Contexto opcional |
|---|---|---|---|
| Catálogo | `/` | Ninguno | `?page=N` |
| Detalle | `/properties/{id}` | Identificador de propiedad | `?page=N` |

- `{id}` debe ser un identificador reconocible; en otro caso se mantiene la página global «Página no encontrada».
- `page` ausente, vacío, no numérico, cero o negativo se resuelve a la página 1.
- Un valor positivo superior a la última página se conserva en la dirección de retorno y el catálogo muestra la última página efectiva.
- Las direcciones son compartibles y compatibles con avance y retroceso del navegador.

## Acciones observables

| Acción | Resultado esperado |
|---|---|
| Cambiar de página en el catálogo | Se muestran las propiedades y los metadatos de la página solicitada sin recarga completa. |
| Abrir una propiedad | Se muestra el detalle de la propiedad solicitada sin recarga completa. |
| Volver al catálogo | Se restaura la página de origen con sus propiedades y metadatos. |
| Reintentar tras error | Se repite la misma vista, con la misma propiedad cuando corresponda y el mismo contexto. |
| Recuperar conexión | Se conserva la vista y el contexto sin recarga manual. |

Los controles principales permanecen como enlaces con dirección válida y texto visible. No se introducen acciones que dependan de un estado oculto para reconstruir la vista.

## Estados visibles

| Estado | Mensaje o indicación | Acción ofrecida |
|---|---|---|
| Espera inicial o de consulta | «Cargando propiedades…» o «Cargando propiedad…» | Ninguna mientras la respuesta sigue en curso. |
| Reconectando | «Reconectando…» | El marco continúa intentando la recuperación. |
| No disponible | «No se pudo mantener la conexión.» | «Reintentar» y «Recargar página». |
| Error de contenido | «No se pudo cargar la información. Inténtalo de nuevo.» | «Reintentar» o «Volver al catálogo», según la vista. |
| No encontrado | «No encontramos esta propiedad.» | «Volver al catálogo». |

Ningún estado visible incluye identificadores internos, rutas técnicas, nombres de servicios, códigos de error ni texto en otro idioma.

## Umbrales y señales

- Una respuesta visible debe aparecer en menos de 1 segundo en al menos el 95 % de las acciones probadas en condiciones normales.
- Una interrupción de hasta 10 segundos debe recuperarse automáticamente.
- Una interrupción superior a 10 segundos debe mostrar el estado «No disponible» y una acción de reintento.
- Ante acciones rápidas repetidas, solo se conserva la última intención válida y no se duplican mensajes ni contenidos.

## Accesibilidad y adaptación

- Cada vista mantiene un único encabezado `h1` accesible.
- Todas las acciones pueden ejecutarse con teclado y muestran foco visible.
- Los iconos son decorativos y no anuncian caracteres adicionales.
- El contenido es legible y no genera desplazamiento horizontal a 320 px, 768 px ni 1280 px.
- El objetivo interno de accesibilidad es WCAG 2.1 nivel AA.

## Aislamiento

- Dos visitas independientes pueden usar el catálogo y el detalle al mismo tiempo sin compartir selección, página, mensajes ni estados.
- Cerrar una visita no afecta a las demás y no hace reaparecer el contexto anterior en una visita nueva.
- La iniciativa no recopila datos personales ni introduce autenticación.
