# Plan de implementación: Documentación retrospectiva del hotfix interactivo

**Rama**: `hotfix/import-razor-utils` | **Rama de documentación**: `014-documentar-hotfix-interactivo` | **Fecha**: 2026-11-22 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación en `/specs/014-documentar-hotfix-interactivo/spec.md`

**Nota**: Plan retrospectivo. Documenta una implementación ya aplicada; no prescribe trabajo nuevo ni modifica código.

## Summary

El hotfix elimina la experiencia estática del catálogo y del detalle de propiedades y la sustituye por una experiencia interactiva con contexto por visita, recuperación ante interrupciones, mensajes comprensibles en español, reintento en la misma vista y registro interno de incidencias. La rama funcional observada es `hotfix/import-razor-utils`; el cambio principal está en el commit `bdb714e`.

### Causa raíz

- Las páginas de catálogo y detalle usaban renderizado por streaming y cargaban datos una sola vez durante la inicialización, por lo que cada acción relevante exigía una navegación completa y podía perder el lugar de origen.
- No existía un contexto de interacción por visita: la página consultada, la propiedad abierta, la dirección de retorno y el estado de espera o error no se conservaban de forma coherente durante la interacción.
- La recuperación ante caídas de conexión dependía del comportamiento genérico del entorno, sin mensajes propios en español, acciones de reintento/recarga ni registro operativo.
- Los estados alternativos solo ofrecían un enlace de reintento; no permitían repetir la intención vigente dentro de la misma vista.
- No había una clasificación centralizada de interacciones lentas, contenido no disponible, conexión restablecida o conexión no restablecida.

### Componentes afectados

- `app/frontend/src/RealtorWeb/Program.cs`: activación del modo interactivo, registro del contexto por visita, del gestor de ciclo de conexión y del servicio de catálogo decorado.
- `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `PropertyDetails.razor`: paso a renderizado interactivo, carga tras renderizado, estados de espera y error, reintento y conservación de intención.
- `app/frontend/src/RealtorWeb/Components/App.razor`: montaje del modal de reconexión.
- `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor` y `ReconnectModal.razor.js`: estados de reconexión, mensajes y acciones de reintento o recarga.
- `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`: acción de reintento interactiva conservando el enlace alternativo existente.
- `app/frontend/src/RealtorWeb/Features/Interactions/`: contexto de visita, estados y resultados, umbrales, clasificación de incidencias, gestión del ciclo de conexión y decorador de registro.
- `app/frontend/src/RealtorWeb/wwwroot/app.css`: estilos propios del modal, del estado de espera y de las acciones accesibles.
- `app/frontend/test/RealtorWeb/`: pruebas nuevas y ajustes en pruebas existentes de navegación, estado y accesibilidad.
- `app/frontend/src/RealtorWeb/Features/_Imports.razor`: archivo sin seguimiento presente en el working tree actual; no forma parte del commit `bdb714e`.
- Backend, OpenAPI y contratos HTTP: sin cambios.

### Solución aplicada

- Se activó el renderizado interactivo solo en las páginas de catálogo y detalle, manteniendo estáticas las páginas de error y no encontrado.
- Se sustituyó la carga inicial única por una carga tras renderizado, con una clave por vista que evita recargas innecesarias y un contador de intención que descarta respuestas tardías de acciones superadas.
- Se incorporó un contexto de interacción con ámbito por visita para registrar vista actual, página solicitada y saneada, propiedad, dirección de retorno, estado y resultado.
- Se añadió un modal de reconexión con identificadores y estados reconocibles, textos en español, foco visible y acciones «Reintentar» y «Recargar página».
- Se convirtió el reintento de estados de error en una acción interactiva dentro de la vista, conservando el enlace de reintento como alternativa.
- Se decoró el servicio de catálogo para medir duración, detectar respuestas lentas y errores, y emitir registros internos acotados.
- Se registró un gestor de ciclo de conexión para marcar recuperación, clasificar la interrupción según el umbral de 10 segundos y cerrar el contexto al finalizar la visita.
- Se centralizaron mensajes, estados y resultados controlados para evitar textos técnicos; el modal conserva textos literales propios para sus estados y acciones.

### Decisiones técnicas

- Se eligió interactividad por página en lugar de interactividad global, para limitar el cambio a catálogo y detalle.
- Se mantuvo el backend y el contrato HTTP sin cambios: la solución actúa únicamente en la capa de presentación y en la composición de servicios del frontend.
- Se arbitró el servicio de catálogo con un decorador en lugar de modificar el servicio concreto, preservando su responsabilidad original.
- Se usó un contexto con ámbito de visita para aislar el estado entre circuitos y descartarlo al cerrarse.
- Se emplearon umbrales explícitos: respuesta lenta cuando supera 1 segundo; recuperación breve cuando la interrupción dura 10 segundos o menos.
- Se adoptaron cuatro resultados controlados para el registro interno: consulta lenta finalizada, contenido no disponible, conexión restablecida y conexión no restablecida.
- Se priorizó la acción de reintento interactiva sobre el enlace alternativo cuando ambos están disponibles.
- Se mantuvieron los estilos en el CSS propio centralizado y se evitaron estilos en línea.
- No se añadieron paquetes, hubs propios, formularios, autenticación ni persistencia nueva.

### Riesgos

- El modo interactivo depende de una conexión sostenida; las caídas cortas se mitigan con el modal, pero la recuperación real requiere validación manual.
- El contexto reside en memoria por visita; un reinicio completo del circuito puede hacer que la vista vuelva a una página válida por defecto en lugar de la anterior exacta.
- La concurrencia y el aislamiento están cubiertos por pruebas unitarias de contexto y por la validación manual confirmada por el responsable, con 20 visitas concurrentes.
- Si algún consumidor solicitara el servicio concreto en lugar de la interfaz, el registro de incidencias podría omitirse; el código actual inyecta la interfaz.
- Los umbrales exactos pueden clasificar de forma distinta interacciones muy cercanas a 1 segundo o 10 segundos por variación temporal normal.
- El modal depende de identificadores y estados del marco de trabajo; una actualización futura del entorno podría alterar esos contratos.
- La aparición del modal puede superponerse temporalmente durante interrupciones leves y afectar la percepción de fluidez.
- El archivo `Features/_Imports.razor` permanece sin seguimiento; si se considera parte del hotfix, su inclusión en el historial no quedó documentada en el commit principal.

### Pruebas realizadas

- Pruebas unitarias del frontend: 111/111 en `RealtorWebTests`, incluidas las nuevas pruebas de contexto, espera, reintento, modal, ciclo de conexión, accesibilidad, umbrales y registro.
- Pruebas de regresión OpenAPI: 17/17 con filtro `FullyQualifiedName~OpenApi`.
- Pruebas completas de la solución: 324/324, con 111 pruebas frontend y 213 backend.
- `git diff --check`: sin diferencias de whitespace.
- Verificación de alcance: backend, OpenAPI, `IPropertiesApi.cs`, `PropertyCatalogService.cs` y `RealtorWeb.csproj` sin cambios.
- Protocolos manuales VM-001 a VM-014 y SC-002/SC-005/SC-006/SC-010: ejecución confirmada por el responsable con resultado satisfactorio; evidencia registrada en las guías 013 y 014.

### Posibles efectos secundarios

- Mayor consumo de recursos del servidor al mantener interacciones por visita frente al renderizado estático previo.
- Cambio en la semántica de navegación: el reintento de error pasa de enlace a acción dentro de la vista cuando el visitante ya está en una página interactiva.
- Aumento del volumen de registros operativos por incidencias lentas o no disponibles.
- Posible reposicionamiento visual leve por los nuevos estilos de modal, progreso y botones en tamaños pequeños.
- El modal puede impedir temporalmente interactuar con el contenido cuando la conexión oscila.
- La lógica de intención evita respuestas obsoletas, pero incrementa la complejidad interna de las páginas.
- No hay efectos sobre las reglas de negocio, la información publicada ni el contrato HTTP.

## Technical Context

**Language/Version**: C# con SDK .NET 11, según la constitución del proyecto.

**Primary Dependencies**: ASP.NET Core Blazor Web App con modo de renderizado Interactive Server, Refit para el cliente de propiedades, LucideBlazor para iconos, xUnit y bUnit para pruebas unitarias. FluentValidation, ProblemDetails, EF Core, Npgsql y PostgreSQL permanecen en backend sin cambios.

**Storage**: Sin almacenamiento nuevo. El estado de interacción vive en memoria durante la visita; la persistencia existente de propiedades no se modifica.

**Testing**: `dotnet test` sobre `app/Realtor.sln`, proyecto `RealtorWebTests` y filtro OpenAPI; protocolos manuales VM-001 a VM-014 y SC-002/SC-005/SC-006/SC-010 ejecutados y documentados en la Guía 013, con confirmación del responsable.

**Target Platform**: Aplicación web atendida por ASP.NET Core, navegadores modernos con conexión activa, vistas comprobadas a 320 px, 768 px y 1280 px.

**Project Type**: Aplicación web dentro de una solución única compartida, con frontend Blazor y backend de Minimal APIs.

**Performance Goals**: Respuesta visible en menos de 1 segundo para al menos el 95 % de las acciones probadas; recuperación automática para interrupciones de hasta 10 segundos.

**Constraints**: Sin modificación de código en este encargo retrospectivo; CSS propio centralizado; logging estructurado; unit tests como forma principal de prueba; backend, OpenAPI y contrato HTTP intactos.

**Scale/Scope**: Dos vistas interactivas, catálogo y detalle; hasta 20 visitas concurrentes para validación; sin formularios, autenticación ni datos personales.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Solución única y compartida**: PASS. Los artefactos se documentan dentro de la solución existente; no se propone una solución paralela.
- **II. Spec-Driven Development**: PASS. La documentación retrospectiva inicia con `speckit.specify` y continúa con `speckit.plan`; no genera implementación nueva ni saltos de fase.
- **III. Arquitectura canónica**: PASS. Se describe el frontend Blazor existente; no se introducen controladores ni estructura técnica global nueva.
- **IV. Stack tecnológico obligatorio**: PASS. .NET 11, Blazor Web App, CSS propio y Lucide Icons; el cambio implementado no añadió frameworks CSS ni paquetes nuevos.
- **V. Calidad de dominio y contratos**: PASS. La lógica de negocio y los endpoints permanecen sin cambios; se documenta logging estructurado acotado y pruebas unitarias.
- **VI. Idioma y documentación**: PASS. Todos los artefactos están en español.
- **VII. Gobernanza y divergencias**: PASS. No se editan scripts ni plantillas upstream y no se alteran specs existentes.

Resultado: sin violaciones. No requiere justificaciones en Complexity Tracking.

### Reevaluación posterior al diseño

Tras definir research, modelo, contratos y guía de validación, el resultado se mantiene sin violaciones. Los artefactos solo documentan el estado implementado, no introducen dependencias, no modifican código y no alteran specs existentes.

## Project Structure

### Documentation (this feature)

```text
specs/014-documentar-hotfix-interactivo/
├── plan.md              # Este archivo (/speckit.plan)
├── research.md          # Decisiones retrospectivas (/speckit.plan)
├── data-model.md        # Modelo de estado e incidencias (/speckit.plan)
├── quickstart.md        # Guía de validación (/speckit.plan)
├── contracts/           # Contratos observables (/speckit.plan)
│   ├── contrato-experiencia-interactiva.md
│   └── contrato-estado-y-registro.md
├── checklists/requirements.md
└── spec.md
```

### Source Code (repository root)

```text
app/frontend/src/RealtorWeb/
├── Components/
│   ├── App.razor
│   ├── Layout/ReconnectModal.razor
│   ├── Layout/ReconnectModal.razor.js
│   ├── Pages/Home.razor
│   ├── Pages/PropertyDetails.razor
│   └── Properties/PresentationState.razor
├── Features/Interactions/
│   ├── InteractionContext.cs
│   ├── InteractionCircuitHandler.cs
│   ├── InteractionDiagnostics.cs
│   ├── InteractionLoggingCatalogService.cs
│   ├── InteractionMessages.cs
│   └── InteractionState.cs
├── Program.cs
└── wwwroot/app.css

app/frontend/test/RealtorWeb/
├── Components/HomeInteractionTests.cs
├── Components/PropertyDetailsInteractionTests.cs
├── Components/ReconnectModalTests.cs
├── InteractionCircuitHandlerTests.cs
├── InteractionDiagnosticsTests.cs
└── InteractionLoggingCatalogServiceTests.cs
```

**Structure Decision**: La documentación sigue la estructura real de la solución. El frontend concentra el cambio en componentes de página, componentes de estado, servicios de interacción y composición de dependencias; el backend y la solución no se modifican.

## Complexity Tracking

No aplica: no hay violaciones de la constitución que justificar.
