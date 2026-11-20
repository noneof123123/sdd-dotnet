# Investigación: Fundación del frontend web

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Especificación: `specs/010-foundation-blazor-frontend/spec.md`
Objetivo: resolver las decisiones técnicas necesarias para diseñar la fundación web sin ambigüedades pendientes.

## R001 — Modo de representación

Decisión: usar Static SSR con navegación mejorada para catálogo, detalle y estados. Se conservan `AddRazorComponents()`, `MapRazorComponents<App>()` y el script `blazor.web.js`; no se añaden modos interactivos ni `@onclick`.

Justificación: la spec describe exploración pública, paginación por enlaces y reintento por navegación. Es el caso de uso recomendado para Static SSR y evita circuitos SignalR o carga WebAssembly en la fundación. El usuario confirmó esta opción de forma explícita.

Alternativas consideradas:

- Interactive Server por página: rechazada porque añadiría circuitos sin requisito funcional.
- Auto con Server y WebAssembly: rechazada porque duplica infraestructura y contradice la decisión aprobada.
- WebAssembly aislado: rechazada porque no es el modelo de Blazor Web App de la constitución.

## R002 — Estado de carga

Decisión: usar `@attribute [StreamRendering]` en las páginas con ruta y mostrar una indicación de carga mientras se obtiene el contenido.

Justificación: permite enviar una primera respuesta y parchear el contenido cuando termina la consulta, sin interactividad de componente. Cumple FR-009 sin JavaScript propio.

Alternativas consideradas:

- Spinner interactivo con `@onclick` o estado de componente: rechazado porque exige un modo interactivo.
- Esperar la respuesta completa sin indicación: rechazado porque rompe FR-009 y SC-006.
- JavaScript propio: rechazado porque añade complejidad y no está en la spec.

## R003 — Contexto de navegación

Decisión: transportar la página del catálogo en la query string. Catálogo canónico `/?page=N`; detalle `/properties/{id:guid}?page=N`. El valor se enlaza como texto, se sanea a entero positivo y solo el valor saneado se usa en enlaces. El orden no se transporta porque la API ya lo define y lo reproduce.

Justificación: es una URL compartible, compatible con navegación mejorada, sin sesión, sin estado oculto y sin historial imperativo. El botón Atrás del navegador recupera la URL completa cuando el `href` incluye la query.

Alternativas consideradas:

- Estado en sesión, cookie, campo oculto o `TempData`: rechazado por estado oculto y manipulable.
- `history.back()` o JavaScript: rechazado porque falla en entrada directa y no es semántico.
- Segmentos de ruta para la página: rechazado porque mezcla identidad del recurso con estado de navegación.
- Añadir `?sort=`: rechazado porque la API no expone orden configurable y FR-020 prohíbe redefinir reglas de negocio.

## R004 — Cliente HTTP

Decisión: instalar Refit `17.0.0`, `Refit.HttpClientFactory` `17.0.0` y `Refit.HttpClientFactory.Reflection` `17.0.0`; registrar `IPropertiesApi` con nombre de cliente `RealtorApi` y `BaseAddress` desde `ApiClients:RealtorApi:BaseAddress`. Propagar `HttpContext.RequestAborted` como `CancellationToken`.

Justificación: cumple la constitución de Refit con `IHttpClientFactory` y evita `HttpClient` directo en componentes. La versión 17 declara compatibilidad `net11.0` y funciona con el SDK RC3 instalado sin cambiar `global.json`.

Alternativas consideradas:

- `RestService.For<T>()` o `new HttpClient()`: rechazadas por prohibidas en las instrucciones del frontend.
- Refit 16.x: rechazada porque no declara destino `net11.0` explícito.
- Cliente source-generated: rechazado para esta fundación porque añade contextos JSON sin beneficio inmediato en ejecución servidor.
- `CancellationToken.None`: rechazado porque ignora la cancelación de la petición SSR.

## R005 — Separación de capas

Decisión: crear view models y mapeadores en `Features/Properties`; los componentes Razor solo reciben view models y renderizan enlaces o mensajes.

Justificación: evita lógica de presentación en componentes, mantiene los DTO de transporte separados de la vista y permite probar el mapeo con xUnit sin renderizar Razor.

Alternativas consideradas:

- Mapear dentro de cada `.razor`: rechazado por baja testabilidad y duplicación.
- Reutilizar DTO de la API como modelo de vista: rechazado porque expondría nombres y formatos técnicos al visitante.
- Crear un proyecto de aplicación independiente: rechazado por violar la solución única.

## R006 — Etiquetas de estado

Decisión: mapear `Available`, `Rented` y `Maintenance` a «Disponible», «Alquilada» y «Mantenimiento». Cualquier valor no asignado se muestra como «Estado no disponible» sin exponer el valor técnico.

Justificación: cumple FR-013, FR-023 y SC-014; los textos visibles son españoles y desconocer un valor no rompe la tarjeta ni el detalle.

Alternativas consideradas:

- Mostrar los valores publicados tal cual: rechazada por el usuario y por FR-013.
- Mostrar solo color o icono: rechazada porque no es comprensible por sí solo.
- Mostrar ambos valores: rechazada porque expondría el valor técnico.

## R007 — Estilos y accesibilidad

Decisión: eliminar Bootstrap, centralizar estilos en `wwwroot/app.css`, definir tokens para color, tipografía, espaciado, foco e iconos, y verificar foco, contraste y alternativas textuales contra WCAG 2.1 AA interno a 320, 768 y 1280 px.

Justificación: la constitución exige CSS propio y prohíbe frameworks CSS; la spec fijó el objetivo AA y los anchos de verificación. Los tokens evitan estilos hardcodeados en componentes.

Alternativas consideradas:

- Conservar Bootstrap: rechazado por violar la constitución.
- Usar otro framework CSS: rechazado por la misma razón.
- Contratar auditoría externa: rechazada porque la spec la excluye explícitamente.

## R008 — Iconografía

Decisión: usar `LucideBlazor` `1.0.7`, registrar `@using LucideBlazor` y usar `LucideIcon` con nombres Lucide en minúsculas/kebab-case y clases `.icon`, `.icon-sm`, `.icon-md` y `.icon-lg`.

Justificación: expone la API `<LucideIcon Name="..." />` descrita en las instrucciones del frontend, renderiza SVG inline con `currentColor`, no requiere JavaScript ni CSS externo y es compatible con `net11.0` por su destino `net9.0`.

Alternativas consideradas:

- `BlazorBlueprint.Icons.Lucide`: rechazada porque su documentación está acoplada a otro ecosistema visual, aunque tiene más adopción.
- `Blazicons.Lucide`: rechazada por su API distinta y dependencia de bundle CSS adicional.
- `Blazora.Icons` y `Lucide.NET.Blazor`: rechazadas por baja adopción o versión pre-1.0.
- Iconos SVG copiados a mano: rechazados porque no dan una integración componentizada trazable.

## R009 — Estrategia de pruebas

Decisión: usar xUnit `2.9.3` y bUnit `2.14.0` en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`, cambiando su SDK a `Microsoft.NET.Sdk.Razor`. La lógica pura se prueba con xUnit; el render de componentes con bUnit. No se inicia Kestrel, navegador, PostgreSQL ni `HttpClient` real.

Justificación: bUnit es una biblioteca de unit tests para Razor Components compatible con `net11.0` en la versión `2.14.0`, que depende de componentes `11.0.0-rc.2` y por tanto funciona con el SDK RC3 del repositorio. No se modifica `global.json`.

Alternativas consideradas:

- Solo xUnit sin bUnit: rechazada porque no verificaría `href`, `alt`, `aria-*` ni mensajes renderizados.
- bUnit `2.15.2`: rechazada porque empujaría a cambiar `global.json` a SDK GA, una decisión global fuera del alcance de 010.
- WebApplicationFactory, Playwright o PostgreSQL real: rechazadas porque serían pruebas de integración o extremo a extremo sin aprobación explícita.

## R010 — Errores y regresión

Decisión: traducir fallos de transporte y códigos no exitosos a estados `Error` o `NoEncontrado` con texto fijo en español y enlace de reintento o retorno. La verificación incluye la solución completa y las pruebas OpenAPI existentes para confirmar que no hay regresión.

Justificación: FR-011, FR-021 y SC-006 exigen mensajes accionables sin detalles técnicos. El backend y su contrato son fuente de verdad de 008 y no deben cambiar.

Alternativas consideradas:

- Mostrar `ProblemDetails` o mensajes de excepción: rechazado por FR-021.
- Dejar pantalla vacía: rechazado por FR-012.
- Regenerar o editar OpenAPI: rechazado porque esta iniciativa solo consume el contrato existente.

## R011 — Persistencia y backend

Decisión: no crear migraciones, entidades, endpoints, controladores ni proyectos. El único cambio de backend es ninguno.

Justificación: la información ya existe en las capacidades 004, 007 y 009; la spec 010 es de presentación y prohíbe redefinir reglas de negocio.

Alternativas consideradas:

- Añadir un endpoint agregado para la web: rechazado por ampliar el alcance y modificar OpenAPI.
- Guardar preferencias de navegación en base de datos: rechazado por estado innecesario.
- Modificar el orden del listado: rechazado por FR-020.

## Resultado

Todas las incógnitas técnicas quedan resueltas. No permanecen marcadores pendientes. Estas decisiones alimentan `plan.md`, `data-model.md`, los contratos y `quickstart.md`.
