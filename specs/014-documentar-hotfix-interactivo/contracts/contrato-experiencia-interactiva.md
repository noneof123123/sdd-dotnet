# Contrato observable de la experiencia interactiva

**Fecha**: 2026-11-22

**Finalidad**: Describir el comportamiento ya implementado que un visitante o una prueba de interfaz puede verificar, sin prescribir cambios.

## Alcance

- Catálogo de propiedades y detalle de propiedad.
- Estados de carga, contenido, error, espera, recuperación y no disponibilidad.
- Modal de reconexión y acciones de reintento o recarga.

## Navegación y contexto

- Cambiar de página en el catálogo muestra la lista y el resumen de la página solicitada sin una recarga completa.
- Abrir una propiedad desde una página concreta muestra su detalle y conserva la página de origen.
- «Volver al catálogo» conduce a la página de origen cuando esta es válida.
- Un enlace directo al detalle conduce a una página válida del catálogo cuando no hay página de origen.
- Una página ausente, no numérica, cero, negativa o excesiva resuelve a una página válida sin dejar una vista vacía.
- Ante acciones repetidas, solo se muestra el resultado de la última intención válida.

## Estados de contenido y reintento

- Durante una consulta se muestra un mensaje de espera en español: «Cargando propiedades…» o «Cargando propiedad…».
- El estado de error muestra el mensaje aprobado y una acción «Reintentar».
- La acción de reintento repite la misma vista, la misma propiedad cuando corresponde y el mismo contexto de retorno.
- Si no hay una acción interactiva disponible, se conserva un enlace alternativo de reintento.
- El estado de no encontrado muestra el mensaje aprobado y la acción de retorno al catálogo.
- Ningún estado visible expone identificadores internos, rutas técnicas, nombres de campos ni detalles de conexión.

## Modal de reconexión

- El modal usa el identificador `components-reconnect-modal`.
- Los estados reconocibles son `components-reconnect-show`, `components-reconnect-failed` y `components-reconnect-rejected`.
- El estado de espera muestra «Reconectando…» y «Estamos intentando restablecer la conexión.».
- El estado fallido muestra «No se pudo mantener la conexión.» y «Puedes volver a intentarlo o recargar la página.».
- El estado rechazado muestra «No se pudo mantener la conexión.» y «La conexión no se pudo restablecer. Puedes recargar la página para continuar.».
- Las acciones son «Reintentar» y «Recargar página», según el estado.
- Todos los botones son de tipo `button`, tienen texto en español y no usan estilos en línea.
- Una interrupción de hasta 10 segundos debe recuperarse sin que el visitante tenga que usar estas acciones.

## Accesibilidad

- Cada vista principal expone un único encabezado de nivel uno.
- El catálogo y el detalle declaran su región con `aria-labelledby`.
- Los iconos decorativos declaran `aria-hidden="true"`.
- Los mensajes de error usan papel `alert`; los mensajes de espera usan papel `status`.
- El modal declara título y descripción accesibles y expone mensajes como diálogo de alerta.
- Todas las acciones pueden ejecutarse con teclado y conservan foco visible.
- El contenido es legible y no genera desplazamiento horizontal a 320 px, 768 px ni 1280 px.
- La preferencia de movimiento reducido elimina animaciones decorativas.

## Aislamiento

- Dos visitas independientes mantienen su propia página, propiedad, mensajes y acciones.
- Una acción en una visita no modifica la vista de otra.
- Al cerrar una visita y abrir otra, la nueva no hereda el contexto anterior.

## Verificación

| Contrato | Evidencia |
|---|---|
| Navegación y retorno | Pruebas de interacción de catálogo y detalle; protocolos VM-001 a VM-004 |
| Espera y reintento | Pruebas de estados de error; protocolos VM-003, VM-006 y VM-007 |
| Reconexión | Pruebas del modal; protocolos VM-005 y VM-006 |
| Texto visible | Pruebas de contenido en español y sin detalles técnicos; protocolo VM-013 |
| Accesibilidad | Pruebas de encabezado, iconos y enlaces; protocolos VM-010 a VM-012 |
| Aislamiento | Pruebas de contextos independientes; protocolo VM-009 y SC-006 |
