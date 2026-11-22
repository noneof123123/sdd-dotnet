# Modelo de datos: Experiencia interactiva en la web de propiedades

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md)

Este modelo describe los datos de la experiencia interactiva dentro del frontend. No introduce persistencia, tablas, migraciones ni información personal.

## Alcance

- Los datos de propiedades continúan siendo propiedad del backend y del contrato aprobado en iniciativas anteriores.
- El contexto de navegación se reconstruye desde la dirección visible; no se copia a almacenamiento oculto.
- El estado de interacción y el incidente registrado viven solo durante la visita.

## Entidades

### Visita interactiva

Representa el período de uso de una persona desde que abre una vista interactiva hasta que cierra la visita o se descarta el circuito.

| Campo | Tipo | Descripción |
|---|---|---|
| `VistaActual` | `Catalogo` o `Detalle` | Vista que está usando la visita en este momento. |
| `UltimoResultado` | `Correcto`, `Lento`, `NoDisponible` o `Recuperado` | Resultado observable más reciente de la interacción. |
| `Inicio` | fecha y hora | Momento en que se crea el contexto de la visita. |
| `Fin` | fecha y hora opcional | Momento en que se descarta el contexto; no se persiste. |

**Reglas**: existe una visita interactiva por circuito; su estado no se comparte con otras visitas; no se conserva después del cierre.

### Contexto de navegación

Representa la información mínima necesaria para reconstruir la vista correcta.

| Campo | Tipo | Descripción |
|---|---|---|
| `PaginaSolicitada` | texto opcional | Valor de página recibido en la dirección. |
| `PaginaSaneada` | entero positivo | Valor resultante de saneamiento; nunca menor que 1. |
| `PropiedadId` | identificador opcional | Propiedad solicitada cuando la vista es detalle. |
| `DireccionRetorno` | dirección válida | Dirección al catálogo que conserva el contexto aprobado. |

**Reglas**: la fuente canónica es la dirección visible; los valores inválidos se sanean según `ValidationRule.VAL-001`; un valor superior a la última página se conserva para que el catálogo lo resuelva.

### Estado de interacción

Representa la condición visible de la experiencia para la visita.

| Valor | Descripción |
|---|---|
| `Activa` | La vista responde a las acciones del visitante. |
| `Esperando` | Hay una acción en curso y se muestra una indicación de progreso. |
| `Recuperando` | La conexión se interrumpió y se está intentando restablecer. |
| `NoDisponible` | La espera o la interrupción superó el umbral y se ofrece una acción de reintento. |

**Reglas**: solo un estado es visible a la vez; la transición a `NoDisponible` requiere superar 10 segundos; el estado no expone motivos técnicos.

### Incidente registrado

Representa la señal operativa generada cuando una interacción es lenta, no está disponible o termina una recuperación.

| Campo | Tipo | Descripción |
|---|---|---|
| `Tipo` | `Lento`, `NoDisponible`, `Recuperado` o `NoRecuperado` | Categoría del incidente. |
| `Momento` | fecha y hora | Instante en que se detecta o se cierra el incidente. |
| `Vista` | `Catalogo` o `Detalle` | Vista afectada en ese momento. |
| `Resultado` | `Consulta lenta finalizada`, `Contenido no disponible`, `Conexión restablecida` o `Conexión no restablecida` | Descripción operativa breve y no técnica del desenlace; el valor depende de `Tipo` según la tabla siguiente. |

**Valores controlados por `Tipo`:**

| `Tipo` | `Resultado` permitido |
|---|---|
| `Lento` | `Consulta lenta finalizada` |
| `NoDisponible` | `Contenido no disponible` |
| `Recuperado` | `Conexión restablecida` |
| `NoRecuperado` | `Conexión no restablecida` |

**Reglas**: no incluye identificadores de visita, direcciones IP, nombres, correos ni valores de propiedad; no se muestra al visitante; no se persiste fuera del registro del servidor.

## Relaciones

- Una `Visita interactiva` contiene un `Contexto de navegación` y un `Estado de interacción` en un momento dado.
- El `Contexto de navegación` puede referenciar un `PropiedadId` cuando `VistaActual` es `Detalle`.
- Un `Incidente registrado` referencia la `Vista` de la visita afectada, pero nunca a la visita misma.
- Varios incidentes pueden ocurrir durante una visita; ninguno se comparte entre visitas.

## Transiciones de estado

| Estado actual | Evento | Estado nuevo | Efecto visible |
|---|---|---|---|
| `Activa` | La consulta tarda en responder | `Esperando` | Indicación de progreso en español. |
| `Esperando` | La consulta responde correctamente | `Activa` | Resultado visible; sin mensaje de incidencia. |
| `Esperando` | La consulta supera 1 segundo o falla | `Activa` o `NoDisponible` | Registro operativo; si falla, mensaje y reintento. |
| `Activa` | Se interrumpe la conexión | `Recuperando` | Modal de reconexión en español. |
| `Recuperando` | La conexión vuelve antes de 10 segundos | `Activa` | Modal desaparece; se conserva la vista. |
| `Recuperando` | La interrupción supera 10 segundos | `NoDisponible` | Mensaje de no disponibilidad y reintento. |
| `NoDisponible` | Se selecciona reintento o se restaura la conexión | `Recuperando` o `Activa` | Se reanuda la misma vista y contexto. |

## Reglas de validación

| Identificador | Regla |
|---|---|
| `ValidationRule.VAL-001` | `PaginaSolicitada` ausente, vacía, no numérica, cero o negativa produce `PaginaSaneada = 1`. |
| `ValidationRule.VAL-002` | Un valor positivo superior a la última página se conserva en la dirección de retorno y el catálogo lo resuelve a la última página efectiva. |
| `ValidationRule.VAL-003` | El `Contexto de navegación` se deriva de la dirección visible y permite enlaces directos e historial del navegador. |
| `ValidationRule.VAL-004` | El `Estado de interacción` y el `Contexto de navegación` están aislados entre visitas. |
| `ValidationRule.VAL-005` | El estado de interacción se descarta al cerrar la visita; no se usa persistencia. |
| `ValidationRule.VAL-006` | Al menos el 95 % de las acciones muestra respuesta visible en menos de 1 segundo en condiciones normales. |
| `ValidationRule.VAL-007` | Una interrupción de hasta 10 segundos se clasifica como `Recuperando`; una superior se clasifica como `NoDisponible`. |
| `ValidationRule.VAL-008` | `Incidente registrado.Tipo`, `Momento`, `Vista` y `Resultado` son los únicos campos permitidos en el registro. |
| `ValidationRule.VAL-009` | Ningún incidente registrado contiene datos personales ni detalles técnicos visibles. |
| `ValidationRule.VAL-010` | Ante acciones rápidas repetidas, solo se conserva la última intención válida; no se duplican mensajes ni contenidos. |
| `ValidationRule.VAL-011` | Todos los textos visibles del estado y del modal están en español y son comprensibles. |
| `ValidationRule.VAL-012` | El estado visible mantiene foco, contraste y adaptación a 320, 768 y 1280 px. |

## Campos canónicos para trazabilidad

| Entidad.Campo | Referencia |
|---|---|
| `Visita interactiva.VistaActual` | `data-model.md: Visita interactiva.VistaActual` |
| `Visita interactiva.UltimoResultado` | `data-model.md: Visita interactiva.UltimoResultado` |
| `Visita interactiva.Inicio` | `data-model.md: Visita interactiva.Inicio` |
| `Visita interactiva.Fin` | `data-model.md: Visita interactiva.Fin` |
| `Contexto de navegacion.PaginaSolicitada` | `data-model.md: Contexto de navegacion.PaginaSolicitada` |
| `Contexto de navegacion.PaginaSaneada` | `data-model.md: Contexto de navegacion.PaginaSaneada` |
| `Contexto de navegacion.PropiedadId` | `data-model.md: Contexto de navegacion.PropiedadId` |
| `Contexto de navegacion.DireccionRetorno` | `data-model.md: Contexto de navegacion.DireccionRetorno` |
| `Estado de interaccion` | `data-model.md: Estado de interaccion` |
| `Incidente registrado.Tipo` | `data-model.md: Incidente registrado.Tipo` |
| `Incidente registrado.Momento` | `data-model.md: Incidente registrado.Momento` |
| `Incidente registrado.Vista` | `data-model.md: Incidente registrado.Vista` |
| `Incidente registrado.Resultado` | `data-model.md: Incidente registrado.Resultado` |
| `ValidationRule.VAL-007` | `data-model.md: ValidationRule.VAL-007` |
| `ValidationRule.VAL-008` | `data-model.md: ValidationRule.VAL-008` |
