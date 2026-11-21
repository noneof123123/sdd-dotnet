# Investigación: Detalle de propiedad en la experiencia web

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

## Propósito

Resolver las incógnitas técnicas de la fase de diseño usando solo el código existente, los contratos ya publicados y la especificación aprobada. Todas las decisiones quedan registradas aquí para que `/speckit.tasks` pueda crear tareas sin volver a preguntar al usuario.

## Decisiones

### R001 — Estrategia de implementación

- **Decisión**: mejorar la vista `/properties/{id}` ya existente dentro de `RealtorWeb` y `RealtorWebTests`; no crear proyectos, soluciones ni aplicaciones nuevas.
- **Justificación**: la ruta, el servicio, los view models, los estados compartidos y el enlace desde el catálogo ya están implementados. La spec 012 describe una maduración de esa superficie, no una nueva capacidad estructural.
- **Alternativas consideradas**: crear un proyecto frontend independiente (descartado: rompe el principio de solución única compartida); reemplazar la página actual (descartado: desperdicia contratos y pruebas existentes); implementar una API gateway nueva (descartado: duplicaría el consumo Refit ya aprobado).

### R002 — Modelo de renderizado

- **Decisión**: conservar Blazor Web App con Razor Components, Static SSR y `@attribute [StreamRendering]`; no activar modos interactivos ni componentes servidor interactivos.
- **Justificación**: es el modelo obligatorio de la constitución y es suficiente para una pantalla de solo lectura que obtiene datos del backend y navega mediante enlaces.
- **Alternativas consideradas**: Interactive Server (descartado: añade circuito SignalR sin beneficio para el caso de uso); Interactive WebAssembly (descartado: require descarga adicional y expone más superficie de cliente); prerender + hydratación parcial (descartado: innecesario para contenido estático por solicitud).

### R003 — Acceso al backend

- **Decisión**: usar el cliente Refit tipado existente `IPropertiesApi` registrado mediante `IHttpClientFactory` con el nombre `RealtorApi` y `BaseAddress` desde configuración.
- **Justificación**: es la integración ya adoptada por el catálogo y por el propio detalle; mantiene la serialización, la administración del cliente HTTP y el contrato OpenAPI en un solo lugar.
- **Alternativas consideradas**: `HttpClient` directo (descartado: rompe la convención de clientes tipados); `RestService.For` (descartado: elude `IHttpClientFactory`); acceso directo a la base de datos desde el frontend (descartado: viola la arquitectura); llamadas desde JavaScript (descartado: duplica contrato y sincronización).

### R004 — Contexto de paginación

- **Decisión**: transportar el contexto de retorno en el parámetro de consulta `page` y sanearlo en `CatalogNavigation` a un entero positivo, con valor por defecto 1.
- **Justificación**: `CatalogNavigation.DetailsUrl` y `PropertyCard.razor` ya construyen la dirección con `?page=N`; el catálogo ya interpreta ese parámetro y produce una página efectiva. No hace falta un parámetro nuevo ni estado del lado cliente.
- **Alternativas consideradas**: cookie o sesión (descartado: acopla navegación a estado del servidor); ruta con segmento `/pagina/N` (descartado: cambia contratos de enlaces existentes); guardar el contexto en JavaScript (descartado: rompe Static SSR y añade fragilidad).

### R005 — Página superior a la última disponible

- **Decisión**: no calcular la última página en la vista de detalle. Si `?page=N` es mayor que la última página, el enlace de retorno conserva `N` y el catálogo de destino aplica la página efectiva ya definida en la iniciativa 011.
- **Justificación**: el detalle no conoce el total de propiedades sin hacer una petición adicional. El catálogo sí lo conoce y ya resuelve el desbordamiento a la última página, evitando una vista vacía y cumpliendo FR-007.
- **Alternativas consideradas**: consultar el catálogo desde el detalle solo para obtener metadata (descartado: añade una petición por carga y acopla dos pantallas); limitar el valor en el servidor del detalle (descartado: no existe esa información en la respuesta de detalle); dejar una vista vacía (descartado: incumple la spec).

### R006 — Identificadores ausentes o mal formados

- **Decisión**: mantener la restricción de ruta `{id:guid}`. Los identificadores mal formados o ausentes son atendidos por `NotFound.razor`, que ya muestra «Página no encontrada», «No encontramos la página que buscas.» y «Volver al catálogo». Un GUID válido sin propiedad asociada produce el estado `NoEncontrado` dentro del detalle.
- **Justificación**: la página global ya es un estado de no encontrado en español, con encabezado, acción de retorno y sin exponer el valor técnico. Cambiar la firma del componente a `string` parseado manualmente añadiría pruebas y rutas sin beneficio para el requisito.
- **Alternativas consideradas**: convertir `Guid Id` en `string?` y parsear en `OnInitializedAsync` (descartado: amplía el alcance y crea una segunda implementación de validación); añadir `@page "/properties"` (descartado: crea una ruta nueva que la spec no pide); mostrar el GUID crudo (descartado: incumple FR-012).

### R007 — Formato y localización

- **Decisión**: calcular precio, etiqueta de estado, fechas y alternativa de imagen en `PropertyCatalogService`/`PropertyPresentation`, no en Razor. Los precios usan dos decimales y cultura `es-ES`; las fechas usan `dd/MM/yyyy HH:mm`; la fecha de actualización ausente se muestra como «No disponible».
- **Justificación**: centraliza reglas que ya son verificables por unit tests y evita duplicar formato entre el catálogo y el detalle.
- **Alternativas consideradas**: formatear con `@string.Format` en el componente (descartado: dispersa la lógica y debilita las pruebas); depender de la cultura del host (descartado: salida no determinista); usar formato invariant (descartado: incumple los ejemplos en español).

### R008 — Estados de presentación

- **Decisión**: reutilizar `PresentationState` sin redefinir sus valores. `Cargando` usa el mensaje «Cargando propiedad…»; `Contenido` muestra la información; `NoEncontrado` usa «No encontramos esta propiedad.»; `Error` usa «No se pudo cargar la información. Inténtalo de nuevo.».
- **Justificación**: el componente compartido ya cubre iconos Lucide, roles ARIA y botones de acción; la spec solo exige que el detalle use esos estados de forma coherente.
- **Alternativas consideradas**: crear un componente de estados exclusivo para detalle (descartado: duplicación); eliminar el estado de carga (descartado: empeora percepción con `StreamRendering`); mostrar datos parciales en error (descartado: incumple FR-015).

### R009 — Accesibilidad de estados no satisfactorios

- **Decisión**: en `PropertyDetails.razor`, garantizar un encabezado `h1` accesible también cuando el estado no es `Contenido`, y usar un `id` estable como destino de `FocusOnNavigate`. No se modifica `Routes.razor` ni se cambia el comportamiento del catálogo.
- **Justificación**: `Routes.razor` enfoca `h1` tras la navegación, pero la sección actual solo referencia `h1` dentro de la rama de contenido. Sin un encabezado alternativo, carga, error y no encontrado carecen de objetivo de foco coherente.
- **Alternativas consideradas**: cambiar `FocusOnNavigate` a otro selector (descartado: afecta a toda la aplicación); añadir el encabezado dentro de `PresentationState.razor` (descartado: alteraría el catálogo y otros usos compartidos); dejar el problema sin corregir (descartado: incumple FR-015 y SC-006).

### R010 — Reintento

- **Decisión**: mantener «Reintentar» como enlace a la misma dirección de detalle, incluido `?page=N` cuando exista. La navegación vuelve a ejecutar la obtención de datos; no se añade JavaScript ni recarga manual del usuario.
- **Justificación**: es compatible con Static SSR y con la constitución. En una pantalla de solo lectura, una navegación al mismo recurso es la forma más simple y accesible de repetir la solicitud.
- **Alternativas consideradas**: botón con `@onclick` e interactividad (descartado: requiere modo interactivo); recarga por JavaScript (descartado: fuera del stack permitido); mensaje sin acción (descartado: incumple FR-011).

### R011 — Diseño visual e iconografía

- **Decisión**: conservar `app.css`, sus tokens y las clases `details-*`, `state-panel`, `back-link` y `focus-visible`; usar LucideBlazor para todos los iconos y `aria-hidden="true"` en los decorativos.
- **Justificación**: evita estilos inline, frameworks CSS y librerías de iconos nuevas, todos prohibidos por la constitución.
- **Alternativas consideradas**: Tailwind u otro framework de utilidades (descartado: prohibido y añadiría dependencias); iconos SVG inline o Font Awesome (descartado: hay una librería obligatoria ya adoptada); colores literales nuevos (descartado: rompen los tokens centralizados).

### R012 — Estrategia de pruebas

- **Decisión**: usar solo xUnit y bUnit ya instalados, sin servidor real, navegador automatizado, base de datos de prueba ni `HttpClient` real. Las verificaciones manuales de `quickstart.md` son compuertas de evidencia de la spec, no pruebas automatizadas.
- **Justificación**: cubre mapeo, render, estados, navegación y accesibilidad estructural con dependencias mínimas y deterministas.
- **Alternativas consideradas**: Playwright o Selenium (descartado: añadiría dependencias y ejecución fuera de proceso sin una necesidad aprobada por la spec); pruebas de integración con Kestrel (descartado: prohibidas por la constitución salvo aprobación explícita, que no existe); pruebas de base de datos (descartado: no hay cambios de persistencia).

### R013 — Backend, contratos y persistencia

- **Decisión**: no modificar `app/backend`, OpenAPI, entidades de dominio, migraciones ni la iniciativa 007. La API `GET /api/properties/{id}` ya entrega todos los datos requeridos. Para esta iniciativa, una propiedad publicada equivale a una propiedad presente en la respuesta pública de la API.
- **Justificación**: la feature es de presentación y navegación. Cualquier cambio del contrato HTTP incrementaría el alcance y podría romper consumidores actuales.
- **Alternativas consideradas**: añadir campos al endpoint de detalle (descartado: los campos ya existen); filtrar propiedades publicadas en backend (descartado: la spec define “publicada” como presencia en la respuesta pública y no solicita incorporar un estado de publicación); versionar el contrato (descartado: sin cambios que versionar).

## Resultado

No quedan incógnitas técnicas pendientes ni marcadores de aclaración. Las decisiones R001 a R013 son suficientes para completar el diseño de Fase 1 y para generar `tasks.md` en el siguiente paso del flujo.
