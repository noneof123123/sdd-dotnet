# Plan de implementación: Experiencia interactiva en la web de propiedades

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/013-signalr-interactive-server/spec.md`

**Estado de la especificación**: `Borrador`. La aclaración operativa ya quedó integrada; este plan no altera el estado de la spec.

## Resumen

Activar una experiencia interactiva en el servidor para las dos vistas públicas existentes: el catálogo y el detalle de propiedades. La interactividad se limitará a esas dos páginas, conservará las direcciones y el contexto `?page=N` actuales, mostrará estados claros durante la espera, la recuperación y la no disponibilidad, y registrará internamente las incidencias sin exponer datos personales. No se crea un proyecto nuevo, no se cambia el backend, no se añaden paquetes y no se modifica el contrato HTTP existente. El diseño reutiliza `RealtorWeb`, `app.css`, Lucide Icons, `CatalogNavigation`, los view models aprobados y el proyecto de pruebas frontend.

## Contexto técnico

- **Lenguaje y versión**: C# con .NET 11; `global.json` fija `11.0.100-rc.3.26522.148` y no se modifica.
- **Dependencias principales**: Blazor Web App con Razor Components y modo de representación Interactive Server del marco; SignalR se usa únicamente como transporte interno del circuito, sin Hub propio ni paquete adicional. Se conservan Refit `17.0.0`, `Refit.HttpClientFactory` `17.0.0`, `Refit.HttpClientFactory.Reflection` `17.0.0` y LucideBlazor `1.0.7`.
- **Almacenamiento**: no hay almacenamiento nuevo. El estado de interacción vive en el ámbito de la visita y del circuito; no se usan cookies, sesión HTTP, tablas ni persistencia. Las propiedades siguen proviniendo del backend existente.
- **Pruebas**: xUnit `2.9.3` y bUnit `2.14.0` en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`. Solo unit tests automatizados en proceso. La caída real del circuito, la concurrencia de visitas y el modal de reconexión del navegador se validan manualmente en `quickstart.md`; no se introduce WebApplicationFactory, Playwright, SignalR de prueba ni otro marco automatizado.
- **Plataforma de destino**: aplicación web hospedada en servidor con circuitos Interactive Server del marco; navegadores modernos; comprobación visual a 320 px, 768 px y 1280 px.
- **Tipo de proyecto**: aplicación web; porción frontend de la solución compartida `app/Realtor.sln`.
- **Objetivos de rendimiento**: SC-002 exige respuesta visible en menos de 1 segundo en al menos el 95 % de las acciones probadas; SC-003 exige recuperación automática en interrupciones de hasta 10 segundos; SC-006 exige que al menos el 95 % de las tareas con 20 visitas concurrentes terminen sin errores visibles ni pérdida de contexto.
- **Restricciones**: stack obligatorio de la constitución; CSS propio centralizado en `wwwroot/app.css`; sin estilos inline, frameworks CSS ni librerías de iconos distintas de Lucide; textos visibles en español; objetivo interno WCAG 2.1 AA; sin cambios en backend, dominio, persistencia, OpenAPI ni contratos Refit ya aprobados; sin estado de visitante en singletons ni variables estáticas.
- **Alcance y escala**: dos páginas interactivas, un modal de reconexión compartido, un contexto de interacción por visita, un registrador de incidencias y validación manual con hasta 20 visitas concurrentes. Sin formularios, autenticación, CRUD ni datos personales.
- **Incógnitas pendientes**: ninguna. Las decisiones de diseño quedan resueltas en `research.md`.

## Comprobación de la constitución previa al diseño

_PUERTA: todos los principios deben cumplirse antes de continuar con la Fase 0._

| Principio | Estado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | Los cambios se limitan a `RealtorWeb` y `RealtorWebTests`, dentro de `app/Realtor.sln`. |
| II. Spec-Driven Development | CUMPLE | Existen `spec.md`, clarificación integrada y checklist 16/16; el flujo especificar → clarificar → plan se ejecuta sin saltos y el plan no edita el estado `Borrador`. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Se mantiene Blazor Web App con Razor Components; el backend y sus vertical slices no se tocan. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit, CSS centralizado y Lucide Icons ya están adoptados. Interactive Server usa capacidades del marco, no un stack paralelo. |
| V. Calidad de Dominio y Contratos | CUMPLE | La navegación, los formatos y los estados siguen fuera del componente; el registro de incidencias usa datos operativos sin información personal. La validación manual exigida por la spec es evidencia, no un nuevo tipo de prueba automatizada. |
| VI. Idioma y Documentación | CUMPLE | Toda la documentación de planeación está en español; se conservan identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | No se editan estado, scripts ni plantillas upstream; no se mueven artefactos fuera de la ruta canónica. |

Resultado: **PUERTA CUMPLIDA**.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/013-signalr-interactive-server/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── contrato-experiencia-interactiva.md
│   └── contrato-estado-y-registro.md
├── checklists/
│   └── requirements.md
└── tasks.md             # No se crea en esta fase; lo genera /speckit.tasks
```

### Código fuente (raíz del repositorio)

```text
app/frontend/src/RealtorWeb/
├── Components/
│   ├── App.razor                              # Incluir el modal de reconexión compartido
│   ├── Layout/ReconnectModal.razor            # Nuevo: modal del cliente con textos en español
│   ├── Layout/ReconnectModal.razor.js         # Nuevo: puente mínimo con eventos del marco
│   ├── Pages/Home.razor                       # Modo interactivo, estados y contexto de catálogo
│   ├── Pages/PropertyDetails.razor            # Modo interactivo, estados y contexto de detalle
│   ├── Pages/Error.razor                      # Sin cambios
│   ├── Pages/NotFound.razor                   # Sin cambios
│   ├── Properties/PresentationState.razor     # Reutilizar para estados terminales
│   └── Properties/PropertyCard.razor          # Sin cambios
├── Features/
│   ├── Interactions/
│   │   ├── InteractionState.cs                # Nuevo: estados, vistas y resultados
│   │   ├── InteractionContext.cs              # Nuevo: contexto por visita
│   │   ├── InteractionDiagnostics.cs          # Nuevo: clasificación de duración y datos de registro
│   │   ├── InteractionCircuitHandler.cs       # Nuevo: registro del ciclo de conexión
│   │   ├── InteractionLoggingCatalogService.cs# Nuevo: decorador que mide y registra consultas
│   │   └── InteractionMessages.cs             # Nuevo: mensajes visibles en español
│   └── Properties/                            # Sin cambios en view models, navegación ni mapeo
├── wwwroot/app.css                            # Añadir estilos del modal y estados interactivos
└── Program.cs                                 # Registrar interactividad, contexto y servicios

app/frontend/test/RealtorWeb/
├── InteractionDiagnosticsTests.cs             # Nuevo: clasificación de duración y campos de registro
├── InteractionLoggingCatalogServiceTests.cs   # Nuevo: decorador sin red
├── Components/ReconnectModalTests.cs          # Nuevo: textos y estructura accesible del modal
├── Components/HomeInteractionTests.cs         # Nuevo: estados interactivos del catálogo
├── Components/PropertyDetailsInteractionTests.cs # Nuevo: estados interactivos del detalle
└── Components/PropertyCatalogServiceTests.cs  # Regresión existente, sin cambios previstos
```

**Decisión estructural**: reutilizar los proyectos existentes y añadir una carpeta `Features/Interactions` dentro de `RealtorWeb` para la lógica transversal de la visita. No se crea proyecto, ruta pública, paquete, archivo CSS ni Hub nuevo. Las páginas `Home.razor` y `PropertyDetails.razor` obtienen el modo `InteractiveServer` de forma individual; `App.razor`, `Routes.razor`, `Error.razor` y `NotFound.razor` permanecen estáticos.

## Fase 0: Investigación

Artefacto: `specs/013-signalr-interactive-server/research.md`.

Decisiones registradas:

- R001: modo `InteractiveServer` por página en `Home` y `PropertyDetails`, sin prerenderizado; `Error` y `NotFound` siguen estáticos.
- R002: registro de servicios y puntos de conexión del circuito solo mediante el marco; sin Hub personalizado ni dependencia nueva.
- R003: navegación principal mediante enlaces y direcciones existentes; `?page=N` sigue siendo la única fuente de contexto de retorno.
- R004: se retira `StreamRendering` de las dos páginas interactivas para evitar dos mecanismos de carga incompatibles.
- R005: modal de reconexión propio con el identificador y las clases que espera el marco, textos en español y botones nativos que funcionan con el circuito caído.
- R006: el umbral de 10 segundos se decide con una función pura y se refleja en el modal; el ciclo de conexión se registra desde un `CircuitHandler` con ámbito de circuito.
- R007: contexto de interacción con ámbito de visita; sin estado estático, singleton ni persistencia.
- R008: registro de consultas lentas o no disponibles mediante un decorador de `IPropertyCatalogService`, sin modificar el servicio aprobado.
- R009:backend, OpenAPI, Refit y entidades de dominio sin cambios.
- R010: estilos nuevos solo en `app.css`, con tokens existentes; se conservan `:focus-visible`, `sr-only`, breakpoints y Lucide decorativo.
- R011: cobertura automática con xUnit y bUnit; reconexión real, aislamiento y concurrencia quedan como evidencia manual de `quickstart.md`.
- R012: objetivo interno WCAG 2.1 AA; se conserva un único `h1`, foco visible y alternativas textuales.
- R013: view models, `CatalogNavigation`, `PropertyPresentation` y `PropertyStatusLabels` no cambian.
- R014: no se recolectan datos personales; los registros contienen tipo, momento, vista y resultado.

## Fase 1: Diseño y contratos

### D001 — Superficie web

`/` y `/properties/{id}` mantienen sus direcciones, parámetros y contenidos. La interacción se manifiesta como respuesta sin recarga completa, indicación de espera, modal de reconexión y estados terminales comprensibles. Los enlaces de paginación, detalle, retorno y reintento conservan `href` válidos para admitir enlaces directos, historial del navegador y teclado.

### D002 — Contexto y estados de interacción

El contexto de la visita contiene la vista actual y el último resultado observable. Los estados son `Activa`, `Esperando`, `Recuperando` y `NoDisponible`. La transición a `NoDisponible` ocurre cuando la espera o la desconexión supera 10 segundos; la recuperación vuelve a `Activa` sin recargar manualmente. El contexto se descarta al cerrar la visita.

### D003 — Reconexión

`ReconnectModal.razor` se monta una sola vez en `App.razor`. Su JavaScript adjunto solo traduce y expone estados del modal del marco; no implementa lógica de negocio. El botón de reintento invoca la reconexión del marco y el botón de recarga recarga la página. Los textos son: «Reconectando…», «No se pudo mantener la conexión.», «Reintentar» y «Recargar página».

### D004 — Registro de incidencias

El decorador mide cada consulta al catálogo o detalle. Registra `Lento` cuando supera 1 segundo y `NoDisponible` cuando la operación termina en error. El `CircuitHandler` registra `Recuperado` o `NoRecuperado` según la duración de la caída. Cada registro incluye tipo de incidencia, momento, vista y resultado, sin identificadores de visita, direcciones IP, nombres ni datos de propiedad.

### D005 — Pruebas y evidencia

Las pruebas unitarias cubren clasificación de duración, decorador, textos del modal y render de estados. La reconexión real, el umbral visual de 10 segundos, el aislamiento entre visitas, la concurrencia de 20 visitas y los anchos 320/768/1280 px se comprueban manualmente y se registran en la tabla de evidencia de `quickstart.md`. No se marcan criterios como superados sin evidencia ejecutada.

## Comprobación de la constitución posterior al diseño

| Principio | Estado | Justificación tras el diseño |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | La carpeta `Features/Interactions` pertenece al proyecto frontend existente. |
| II. Spec-Driven Development | CUMPLE | El diseño cubre FR-001 a FR-019 y SC-001 a SC-010 sin ampliar el alcance aprobado. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | La interactividad se implementa dentro de Blazor Web App; no aparecen controladores ni slices nuevos. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | No se añaden paquetes, frameworks CSS ni servicios externos; SignalR queda como transporte del marco. |
| V. Calidad de Dominio y Contratos | CUMPLE | La lógica de interacción y registro es auxiliar y no contiene reglas de negocio; solo unit tests automatizados. |
| VI. Idioma y Documentación | CUMPLE | Todos los artefactos generados están en español. |
| VII. Gobernanza y Divergencias | CUMPLE | El estado de la spec permanece `Borrador`; no se tocan scripts, plantillas ni rutas canónicas. |

Resultado: **PUERTA CUMPLIDA**.

## Complejidad

No hay violaciones de la constitución que requieran justificación. El modal de reconexión y el `CircuitHandler` son los únicos componentes nuevos y responden a requisitos visibles y operativos de la spec, no a preferencias de implementación.
