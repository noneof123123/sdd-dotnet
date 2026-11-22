# Investigación: Experiencia interactiva en la web de propiedades

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md)

Este documento resuelve las decisiones técnicas necesarias para la planificación. No modifica el alcance de la spec y no registra evidencia de implementación.

## R001 — Modo de representación por página

- **Decisión**: Aplicar `InteractiveServer` únicamente a `Home.razor` y `PropertyDetails.razor`, sin prerenderizado. `Error.razor`, `NotFound.razor`, `App.razor` y `Routes.razor` permanecen estáticos.
- **Justificación**: Limita los circuitos a las dos vistas con valor interactivo, evita que las páginas de error abran circuitos innecesarios y elimina la ejecución duplicada de `OnInitializedAsync` entre prerenderizado y conexión del circuito, que contradiría FR-012.
- **Alternativas consideradas**: modo global en `Routes.razor` (rechazado por crear circuitos en todas las rutas); prerenderizado con transferencia de estado (rechazado por añadir complejidad no cubierta por pruebas en proceso).

## R002 — Registro del transporte interactivo

- **Decisión**: Usar `AddInteractiveServerComponents()` y `AddInteractiveServerRenderMode()` del marco en `Program.cs`, sin Hub propio, sin `MapHub` y sin paquetes nuevos.
- **Justificación**: SignalR ya forma parte del transporte del modo Interactive Server; un Hub propio duplicaría infraestructura y ampliaría el alcance hacia el backend.
- **Alternativas consideradas**: Hub personalizado (rechazado); WebAssembly o modo Auto (rechazados por introducir otro modelo de ejecución); `HttpClient` directo (rechazado por sustituir Refit).

## R003 — Navegación y contexto

- **Decisión**: Mantener enlaces `<a href>` para paginación, apertura de detalle, retorno y reintento; conservar `?page=N` y las utilidades `CatalogNavigation`.
- **Justificación**: Preserva direcciones compartibles, historial del navegador, foco con teclado, las pruebas existentes y FR-005, FR-009 y FR-013.
- **Alternativas consideradas**: controladores `@onclick` con `NavigationManager` (rechazados por romper enlaces directos y aumentar el riesgo de pérdida de contexto); estado oculto en el componente (rechazado por no ser compartible).

## R004 — Carga inicial

- **Decisión**: Retirar `[StreamRendering]` de las dos páginas interactivas y usar los estados `Cargando` ya definidos para la espera inicial.
- **Justificación**: `StreamRendering` pertenece a la representación estática; mezclarlo con un circuito activo crearía dos señales de carga diferentes y dificultaría FR-006.
- **Alternativas consideradas**: conservar `StreamRendering` y el modo interactivo (rechazado por solapamiento de mecanismos); mostrar contenido vacío (rechazado por FR-006).

## R005 — Modal de reconexión

- **Decisión**: Crear `Components/Layout/ReconnectModal.razor` con el identificador `components-reconnect-modal` y las clases que el marco alterna, más un módulo JavaScript adjunto para exponer textos en español.
- **Justificación**: El modal del marco no admite localización mediante recursos de servidor porque se ejecuta en el navegador cuando el circuito no está disponible. Proporcionar el marcado propio permite cumplir FR-008, FR-016 y el umbral de 10 segundos sin exponer detalles técnicos.
- **Alternativas consideradas**: modal predeterminado en inglés (rechazado por FR-016); localización con `IStringLocalizer` (rechazada porque el circuito puede estar caído); paquete de terceros (rechazado por el stack obligatorio y por evitar dependencias nuevas).

## R006 — Umbral de recuperación

- **Decisión**: Clasificar la duración de la interrupción con una función pura: hasta 10 segundos es `Recuperando`; más de 10 segundos es `NoDisponible`.
- **Justificación**: Hace comprobable FR-007 y FR-008 con unit tests, aunque la visualización real se valide manualmente. El mismo criterio se usa para el registro de incidencias de FR-019.
- **Alternativas consideradas**: dejar el criterio solo en JavaScript (rechazado por no ser comprobable en unit tests); usar un valor distinto (rechazado por contradecir la spec).

## R007 — Ámbito del estado

- **Decisión**: Registrar un contexto de interacción con ámbito de visita o circuito y no usar variables estáticas, singletons con estado de persona, cookies ni sesión HTTP.
- **Justificación**: Cumple FR-010, FR-011 y SC-009 sin almacenar datos personales ni filtrar el contexto entre visitantes.
- **Alternativas consideradas**: singleton (rechazado por mezclar visitas); `ProtectedSessionStorage` (rechazado por persistir más allá de la visita y añadir superficie de datos).

## R008 — Registro de consultas

- **Decisión**: Añadir un decorador de `IPropertyCatalogService` que mide duración y resultado, y usa `ILogger<T>` para emitir incidencias `Lento` y `NoDisponible` con tipo, momento, vista y resultado.
- **Justificación**: Cubre FR-019 y SC-010 sin modificar `PropertyCatalogService`, sin tocar backend y sin exponer información al visitante.
- **Alternativas consideradas**: editar directamente el servicio existente (rechazado por ampliar el riesgo de regresión); registrar solo desde el navegador (rechazado porque no puede enviar registros con el circuito caído); añadir endpoints nuevos (rechazado por FR-014).

## R009 — Backend y contratos HTTP

- **Decisión**: No modificar `app/backend`, `openapi/v1.json`, `IPropertiesApi.cs` ni los slices `GET /api/properties` y `GET /api/properties/{id}`.
- **Justificación**: La interactividad es una capacidad de la aplicación web; los datos y reglas aprobados no cambian.
- **Alternativas consideradas**: crear un endpoint de estado de conexión (rechazado por innecesario y por cambiar el contrato público); mover consultas al cliente (rechazado por romper la arquitectura canónica).

## R010 — Estilos

- **Decisión**: Añadir los estilos del modal y de los estados interactivos en `wwwroot/app.css`, usando tokens existentes y conservando `:focus-visible`, `sr-only`, `state-panel` y los breakpoints de 48 rem y 80 rem.
- **Justificación**: Mantiene el sistema visual centralizado y evita regresiones de accesibilidad y adaptación.
- **Alternativas consideradas**: estilos inline (rechazados por la constitución); nuevo archivo CSS o framework (rechazados por el stack obligatorio).

## R011 — Estrategia de pruebas

- **Decisión**: Usar xUnit y bUnit para lógica pura, decorador y render de estados; dejar reconexión real, concurrencia y recorrido visual como evidencia manual de `quickstart.md`.
- **Justificación**: bUnit no levanta un circuito SignalR ni simula pérdida de red. La spec exige evidencia real para esos criterios, y la constitución permite solo unit tests automatizados salvo justificación explícita.
- **Alternativas consideradas**: pruebas de integración con `WebApplicationFactory` (rechazadas por requerir otro tipo de prueba automatizada no justificado); Playwright (rechazado por añadir dependencia y tipo de prueba no aprobado); afirmar cobertura sin evidencia (rechazado por la gobernanza de estado).

## R012 — Accesibilidad

- **Decisión**: Conservar un único `h1` por vista, foco visible, iconos decorativos con `aria-hidden`, alternativas de imagen y el objetivo interno WCAG 2.1 AA ya adoptado en la iniciativa 012.
- **Justificación**: Evita que la interactividad degrade los comportamientos aprobados y cumple FR-017 y SC-008.
- **Alternativas consideradas**: componentes interactivos sin `role` ni etiquetas (rechazados); cambiar enlaces por elementos no focusables (rechazado).

## R013 — Reutilización de capacidades aprobadas

- **Decisión**: No modificar `PropertyViewModels.cs`, `CatalogNavigation.cs`, `PropertyPresentation.cs`, `PropertyStatusLabels.cs`, `IPropertiesApi.cs` ni `PropertyCatalogService.cs`.
- **Justificación**: Estas piezas ya implementan mapeo, saneamiento, estados y mensajes aprobados; la iniciativa 013 solo añade interactividad y registro.
- **Alternativas consideradas**: reescribir la capa de presentación (rechazado por riesgo y falta de valor); duplicar mensajes en componentes (rechazado por FR-016).

## R014 — Privacidad del registro

- **Decisión**: Los registros contendrán únicamente tipo de incidencia, momento, vista y resultado. No se registrarán identificadores de visita, direcciones IP, nombres, correos ni valores de propiedad.
- **Justificación**: Responde a la clarificación aceptada el 2026-11-21 y a FR-015, FR-016 y FR-019.
- **Alternativas consideradas**: registrar el contexto completo de navegación (rechazado por incluir identificadores de propiedad innecesarios); no registrar nada (rechazado por FR-019).
