---
description: "Lista de tareas para la implementación de la iniciativa 013"
---

# Tareas: Experiencia interactiva en la web de propiedades

**Entrada**: Documentos de diseño de `/specs/013-signalr-interactive-server/`

**Prerrequisitos**: `plan.md` y `spec.md` requeridos; `research.md`, `data-model.md`, `contracts/`, `quickstart.md` y `.specify/memory/constitution.md` disponibles.

**Pruebas**: Se incluyen tareas de prueba porque la spec exige escenarios de pruebas y una prueba independiente por historia, y el plan define unit tests con xUnit y bUnit como verificación automatizada. La reconexión real y la concurrencia se dejan como evidencia manual en `quickstart.md`.

**Organización**: Las tareas se agrupan por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo; usa archivos distintos y no depende de tareas incompletas.
- **[Historia]**: Etiqueta US1, US2, US3 o US4 solo en fases de historias.
- Incluir rutas exactas en cada descripción.

## Convenciones de rutas

- Solución: `app/Realtor.sln`.
- Frontend: `app/frontend/src/RealtorWeb/`.
- Pruebas frontend: `app/frontend/test/RealtorWeb/`.
- Backend: `app/backend/` solo para regresión; esta iniciativa no lo modifica.
- Documentación de la feature: `specs/013-signalr-interactive-server/`.

---

## Fase 1: Configuración (infraestructura compartida)

**Propósito**: Confirmar que la implementación ocurre dentro de los proyectos existentes y sin nuevas dependencias.

- [X] T001 Verificar SDK, solución y proyectos existentes antes de tocar código en `global.json`, `app/Realtor.sln`, `app/frontend/src/RealtorWeb/RealtorWeb.csproj` y `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`
- [X] T002 [P] Confirmar alcance sin backend nuevo, sin paquetes nuevos, sin Hub propio y sin cambios de OpenAPI según `specs/013-signalr-interactive-server/plan.md`, `specs/013-signalr-interactive-server/research.md`, `specs/013-signalr-interactive-server/contracts/contrato-experiencia-interactiva.md` y `specs/013-signalr-interactive-server/contracts/contrato-estado-y-registro.md`; no modificar `app/backend/src/RealtorApi/RealtorApi.csproj` ni `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`

---

## Fase 2: Fundamentos (prerrequisitos bloqueantes)

**Propósito**: Preparar la infraestructura interactiva, el estado por visita y el registro operativo antes de iniciar cualquier historia.

⚠️ **CRÍTICO**: Ninguna historia puede comenzar hasta completar esta fase.

- [X] T003 Configurar Interactive Server sin Hub propio en `app/frontend/src/RealtorWeb/Program.cs` agregando los servicios y el modo de representación del marco según `specs/013-signalr-interactive-server/research.md`
- [X] T004 [P] Agregar el namespace `RealtorWeb.Features.Interactions` en `app/frontend/src/RealtorWeb/Components/_Imports.razor` para que los componentes y Razor puedan resolver el estado interactivo
- [X] T005 [P] Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionState.cs` con los valores de vista, resultado y estado visibles, citando `data-model.md: Visita interactiva.VistaActual`, `data-model.md: Visita interactiva.UltimoResultado` y `data-model.md: Estado de interaccion`
- [X] T006 [P] Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionMessages.cs` con los mensajes visibles en español y sin detalles técnicos, citando `data-model.md: ValidationRule.VAL-011`
- [X] T007 Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionContext.cs` con el contexto por visita y su ciclo de vida, citando `data-model.md: Visita interactiva.Inicio`, `data-model.md: Visita interactiva.Fin`, `data-model.md: Contexto de navegacion.PaginaSolicitada`, `data-model.md: Contexto de navegacion.PaginaSaneada`, `data-model.md: Contexto de navegacion.PropiedadId`, `data-model.md: Contexto de navegacion.DireccionRetorno`, `data-model.md: ValidationRule.VAL-001`, `data-model.md: ValidationRule.VAL-002`, `data-model.md: ValidationRule.VAL-003`, `data-model.md: ValidationRule.VAL-004` y `data-model.md: ValidationRule.VAL-005`
- [X] T008 Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionDiagnostics.cs` con la clasificación pura de duración y los datos operativos, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-006` y `data-model.md: ValidationRule.VAL-007`
- [X] T009 Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionCircuitHandler.cs` con el registro del ciclo de conexión por circuito, citando `data-model.md: Estado de interaccion`, `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-007`, `data-model.md: ValidationRule.VAL-008` y `data-model.md: ValidationRule.VAL-009`
- [X] T010 Crear `app/frontend/src/RealtorWeb/Features/Interactions/InteractionLoggingCatalogService.cs` como decorador de `IPropertyCatalogService` sin modificar `PropertyCatalogService.cs`, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-006`, `data-model.md: ValidationRule.VAL-008` y `data-model.md: ValidationRule.VAL-009`
- [X] T011 Registrar el contexto con ámbito de visita, el `CircuitHandler` y el decorador de `IPropertyCatalogService` en `app/frontend/src/RealtorWeb/Program.cs` sin introducir singleton con estado de visitante

**Punto de control**: Fundación lista; pueden comenzar las historias de usuario.

---

## Fase 3: Historia de usuario 1 - Interactuar con el catálogo y el detalle sin perder el contexto (Prioridad: P1) 🎯 MVP

**Objetivo**: Moverse entre catálogo y detalle con respuestas inmediatas, sin recarga completa y conservando página, propiedad y retorno.

**Prueba independiente**: Abrir el catálogo, cambiar de página, abrir una propiedad y volver al catálogo verificando que la respuesta es inmediata y que se conservan la página y la propiedad consultadas.

### Pruebas de la Historia de usuario 1

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T012 [P] [US1] Crear `app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs` con pruebas bUnit de carga, paginación y enlaces de contexto, citando `data-model.md: Contexto de navegacion.PaginaSolicitada`, `data-model.md: Contexto de navegacion.PaginaSaneada`, `data-model.md: Contexto de navegacion.DireccionRetorno`, `data-model.md: ValidationRule.VAL-001`, `data-model.md: ValidationRule.VAL-002` y `data-model.md: ValidationRule.VAL-003`
- [X] T013 [P] [US1] Crear `app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs` con pruebas bUnit de detalle directo, retorno y contexto por visita, citando `data-model.md: Visita interactiva.VistaActual`, `data-model.md: Contexto de navegacion.PropiedadId`, `data-model.md: Contexto de navegacion.DireccionRetorno`, `data-model.md: ValidationRule.VAL-003`, `data-model.md: ValidationRule.VAL-004` y `data-model.md: ValidationRule.VAL-005`

### Implementación de la Historia de usuario 1

- [X] T014 [US1] Actualizar `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` con `@rendermode InteractiveServer`, retirar `[StreamRendering]` y poblar el contexto del catálogo, citando `data-model.md: Visita interactiva.VistaActual`, `data-model.md: Contexto de navegacion.PaginaSolicitada`, `data-model.md: Contexto de navegacion.PaginaSaneada`, `data-model.md: Contexto de navegacion.DireccionRetorno` y `data-model.md: ValidationRule.VAL-001`
- [X] T015 [US1] Actualizar `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` con `@rendermode InteractiveServer`, retirar `[StreamRendering]` y poblar el contexto del detalle, citando `data-model.md: Visita interactiva.VistaActual`, `data-model.md: Contexto de navegacion.PaginaSaneada`, `data-model.md: Contexto de navegacion.PropiedadId`, `data-model.md: Contexto de navegacion.DireccionRetorno` y `data-model.md: ValidationRule.VAL-005`
- [X] T016 [US1] Conservar enlaces `href`, `CatalogNavigation` y direcciones compartibles en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, citando `data-model.md: Contexto de navegacion.DireccionRetorno`, `data-model.md: ValidationRule.VAL-002`, `data-model.md: ValidationRule.VAL-003` y `data-model.md: ValidationRule.VAL-010`
- [X] T017 [US1] Ajustar las transiciones de espera y éxito en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` para conservar solo la última intención válida, citando `data-model.md: Visita interactiva.UltimoResultado`, `data-model.md: Estado de interaccion` y `data-model.md: ValidationRule.VAL-010`

**Punto de control**: La Historia de usuario 1 es funcional y verificable de forma independiente.

---

## Fase 4: Historia de usuario 2 - Recuperarse de interrupciones breves (Prioridad: P1)

**Objetivo**: Recuperar automáticamente interrupciones de hasta 10 segundos sin perder vista, página, propiedad ni contexto de retorno.

**Prueba independiente**: Iniciar una interrupción de hasta 10 segundos durante la navegación entre catálogo y detalle, restaurar la conexión y verificar que se conserva la vista y el contexto.

### Pruebas de la Historia de usuario 2

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T018 [P] [US2] Crear `app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs` con pruebas unitarias del umbral de 10 segundos, de los campos de registro y de la correspondencia controlada entre `Tipo` y `Resultado`, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado` y `data-model.md: ValidationRule.VAL-007`
- [X] T019 [P] [US2] Crear `app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs` con pruebas bUnit del identificador, las clases, los textos, los botones y la estructura accesible del modal, citando `data-model.md: ValidationRule.VAL-011` y `data-model.md: ValidationRule.VAL-012`

### Implementación de la Historia de usuario 2

- [X] T020 [US2] Crear `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor` con el identificador y las clases del modal del marco, textos en español y botones nativos, citando `data-model.md: Estado de interaccion`, `data-model.md: ValidationRule.VAL-007` y `data-model.md: ValidationRule.VAL-011`
- [X] T021 [US2] Crear `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor.js` como puente mínimo de eventos y textos del modal sin lógica de negocio, citando `data-model.md: ValidationRule.VAL-011`
- [X] T022 [US2] Montar `<ReconnectModal />` una sola vez en `app/frontend/src/RealtorWeb/Components/App.razor` sin cambiar la representación estática de `App.razor` ni `Routes.razor`
- [X] T023 [US2] Completar `app/frontend/src/RealtorWeb/Features/Interactions/InteractionCircuitHandler.cs` para transicionar a recuperando o activa y registrar recuperado o no recuperado, citando `data-model.md: Estado de interaccion`, `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado` y `data-model.md: ValidationRule.VAL-007`
- [X] T024 [US2] Agregar estilos del modal y del estado de recuperación en `app/frontend/src/RealtorWeb/wwwroot/app.css` conservando foco, contraste y breakpoints, citando `data-model.md: ValidationRule.VAL-012`
- [X] T025 [US2] Verificar la conservación del contexto tras la recuperación en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, citando `data-model.md: Visita interactiva.VistaActual`, `data-model.md: Visita interactiva.UltimoResultado`, `data-model.md: Contexto de navegacion.PaginaSaneada`, `data-model.md: Contexto de navegacion.PropiedadId`, `data-model.md: Contexto de navegacion.DireccionRetorno` y `data-model.md: ValidationRule.VAL-007`

**Punto de control**: La Historia de usuario 2 es funcional y verificable de forma independiente.

---

## Fase 5: Historia de usuario 3 - Entender y reintentar cuando la interacción no está disponible (Prioridad: P2)

**Objetivo**: Mostrar progreso, no disponibilidad y reintento comprensibles cuando la experiencia no responde o no se recupera.

**Prueba independiente**: Forzar una respuesta lenta y una interrupción prolongada, verificando mensaje, acción ofrecida y ausencia de detalles técnicos.

### Pruebas de la Historia de usuario 3

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T026 [P] [US3] Crear `app/frontend/test/RealtorWeb/InteractionLoggingCatalogServiceTests.cs` con pruebas unitarias de registro lento y no disponible sin red, incluida la correspondencia controlada entre `Tipo` y `Resultado`, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-006`, `data-model.md: ValidationRule.VAL-008` y `data-model.md: ValidationRule.VAL-009`
- [X] T027 [P] [US3] Ampliar `app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs` con espera, no disponibilidad y reintento del catálogo, citando `data-model.md: Estado de interaccion` y `data-model.md: ValidationRule.VAL-011`
- [X] T028 [P] [US3] Ampliar `app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs` con espera, no disponibilidad y reintento del detalle, citando `data-model.md: Estado de interaccion` y `data-model.md: ValidationRule.VAL-011`

### Implementación de la Historia de usuario 3

- [X] T029 [US3] Completar `app/frontend/src/RealtorWeb/Features/Interactions/InteractionLoggingCatalogService.cs` para medir duración y resultado de cada consulta, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-006`, `data-model.md: ValidationRule.VAL-008` y `data-model.md: ValidationRule.VAL-009`
- [X] T030 [US3] Conectar espera, no disponibilidad y reintento en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` usando `InteractionMessages.cs` y `Components/Properties/PresentationState.razor`, citando `data-model.md: Estado de interaccion`, `data-model.md: ValidationRule.VAL-006` y `data-model.md: ValidationRule.VAL-011`
- [X] T031 [US3] Conectar espera, no disponibilidad y reintento en `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` usando `InteractionMessages.cs` y `Components/Properties/PresentationState.razor`, citando `data-model.md: Estado de interaccion`, `data-model.md: ValidationRule.VAL-006` y `data-model.md: ValidationRule.VAL-011`
- [X] T032 [US3] Agregar estilos del estado no disponible y de la indicación de progreso en `app/frontend/src/RealtorWeb/wwwroot/app.css`, citando `data-model.md: ValidationRule.VAL-012`

**Punto de control**: La Historia de usuario 3 es funcional y verificable de forma independiente.

---

## Fase 6: Historia de usuario 4 - Mantener una experiencia coherente, aislada y accesible (Prioridad: P2)

**Objetivo**: Conservar accesibilidad, aislamiento entre visitas, privacidad y adaptación visual sin degradar el catálogo ni el detalle aprobados.

**Prueba independiente**: Usar dos visitas independientes, recorrer las acciones con teclado y revisar las vistas a 320, 768 y 1280 px verificando aislamiento, foco visible y ausencia de desplazamiento horizontal.

### Pruebas de la Historia de usuario 4

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T033 [P] [US4] Ampliar `app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs` con aserciones de encabezado único, iconos decorativos, foco y texto en español del catálogo, citando `data-model.md: ValidationRule.VAL-011` y `data-model.md: ValidationRule.VAL-012`
- [X] T034 [P] [US4] Ampliar `app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs` con aserciones de encabezado único, iconos decorativos, foco y texto en español del detalle, citando `data-model.md: ValidationRule.VAL-011` y `data-model.md: ValidationRule.VAL-012`
- [X] T035 [P] [US4] Ampliar `app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs` con aserciones de foco, etiquetas comprensibles y texto en español del modal, citando `data-model.md: ValidationRule.VAL-011` y `data-model.md: ValidationRule.VAL-012`

### Implementación de la Historia de usuario 4

- [X] T036 [US4] Garantizar aislamiento y descarte del estado por visita en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionContext.cs` y `app/frontend/src/RealtorWeb/Program.cs`, citando `data-model.md: Visita interactiva.Inicio`, `data-model.md: Visita interactiva.Fin`, `data-model.md: Estado de interaccion`, `data-model.md: ValidationRule.VAL-004` y `data-model.md: ValidationRule.VAL-005`
- [X] T037 [US4] Revisar `app/frontend/src/RealtorWeb/Components/Pages/Home.razor`, `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` y `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor` para conservar un único `h1`, iconos `aria-hidden` y controles focusables, citando `data-model.md: ValidationRule.VAL-012`
- [X] T038 [US4] Completar reglas responsive en `app/frontend/src/RealtorWeb/wwwroot/app.css` para 320 px, 768 px y 1280 px sin desplazamiento horizontal, citando `data-model.md: ValidationRule.VAL-012`
- [X] T039 [US4] Verificar que los registros de `app/frontend/src/RealtorWeb/Features/Interactions/InteractionLoggingCatalogService.cs` y `app/frontend/src/RealtorWeb/Features/Interactions/InteractionCircuitHandler.cs` contengan solo los campos permitidos y los valores de `Resultado` controlados, citando `data-model.md: Incidente registrado.Tipo`, `data-model.md: Incidente registrado.Momento`, `data-model.md: Incidente registrado.Vista`, `data-model.md: Incidente registrado.Resultado`, `data-model.md: ValidationRule.VAL-008` y `data-model.md: ValidationRule.VAL-009`

**Punto de control**: La Historia de usuario 4 es funcional y verificable de forma independiente.

---

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: Verificar regresiones, ejecutar la guía de validación y dejar evidencia real sin modificar backend ni OpenAPI.

- [X] T040 Ejecutar pruebas unitarias frontend en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj` y corregir solo fallos causados por esta feature
- [X] T041 Ejecutar la suite completa de la solución en `app/Realtor.sln` y comprobar que no aparecen regresiones en catálogo, backend ni contratos
- [X] T042 [P] Ejecutar la regresión OpenAPI con filtro `FullyQualifiedName~OpenApi` en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` y verificar que `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` permanece sin cambios
- [X] T043 [P] Ejecutar `git diff --check` desde `app/Realtor.sln` y verificar que no aparecen cambios en `app/backend/`, `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, `IPropertiesApi.cs`, `PropertyCatalogService.cs` ni `RealtorWeb.csproj`
- [ ] T044 Ejecutar la verificación manual VM-001 a VM-014 descrita en `specs/013-signalr-interactive-server/quickstart.md`, registrando fecha, resultados y evidencia real
- [ ] T045 Ejecutar los protocolos SC-002, SC-005, SC-006 y SC-010 descritos en `specs/013-signalr-interactive-server/quickstart.md`, registrando fecha, resultados y evidencia real
- [X] T046 Actualizar la tabla de evidencia en `specs/013-signalr-interactive-server/quickstart.md` con los resultados ejecutados y verificar el estado final del árbol con `git status` y `git diff --check`

---

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1 (Configuración)**: sin dependencias; puede iniciarse de inmediato.
- **Fase 2 (Fundamentos)**: depende de Fase 1; bloquea todas las historias.
- **Historias (Fase 3 a 6)**: dependen de Fase 2; están organizadas por prioridad y pueden validarse por separado.
- **Fase 7 (Pulido)**: depende de que todas las historias deseadas estén completas.

### Dependencias entre historias

- **Historia de usuario 1 (P1)**: puede iniciar tras Fase 2; es el MVP y no depende de otras historias.
- **Historia de usuario 2 (P1)**: puede iniciar tras Fase 2; comparte contexto con US1, por lo que se recomienda ejecutarla justo después cuando haya un solo implementador.
- **Historia de usuario 3 (P2)**: puede iniciar tras Fase 2; comparte archivos de implementación con US1 y US2, por lo que se recomienda ejecutarla después de ellas cuando haya un solo implementador.
- **Historia de usuario 4 (P2)**: puede iniciar tras Fase 2; sus pruebas son independientes, pero sus ajustes tocan los mismos componentes y estilos que US1, US2 y US3.

### Grafo recomendado

```text
Fase 1 -> Fase 2 -> US1 (MVP) -> US2 -> US3 -> US4 -> Fase 7
```

Con varios implementadores, las tareas de prueba marcadas como `[P]` pueden avanzar en paralelo siempre que usen archivos distintos; las tareas que editan `Program.cs`, `app.css`, `Home.razor`, `PropertyDetails.razor` o `ReconnectModal.razor` deben coordinarse para evitar conflictos.

### Dentro de cada historia

1. Escribir primero las pruebas indicadas y confirmar que fallan.
2. Ajustar utilidades y estado antes de los componentes.
3. Ajustar el componente antes de la validación visual manual.
4. Verificar el punto de control de la historia antes de avanzar.

### Oportunidades de paralelismo

- Fase 1: T002 puede ejecutarse en paralelo con T001.
- Fase 2: T004, T005 y T006 pueden ejecutarse en paralelo.
- US1: T012 y T013 pueden ejecutarse en paralelo.
- US2: T018 y T019 pueden ejecutarse en paralelo.
- US3: T026, T027 y T028 pueden ejecutarse en paralelo.
- US4: T033, T034 y T035 pueden ejecutarse en paralelo.
- Fase 7: T042 y T043 pueden ejecutarse en paralelo.

---

## Ejemplo de paralelismo: Historia de usuario 1

```bash
# Ejecutar juntas las pruebas de US1:
Tarea: "Crear app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs"
Tarea: "Crear app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs"
```

## Ejemplo de paralelismo: Historia de usuario 2

```bash
# Ejecutar juntas las pruebas de US2:
Tarea: "Crear app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs"
Tarea: "Crear app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs"
```

## Ejemplo de paralelismo: Historia de usuario 3

```bash
# Ejecutar juntas las pruebas de US3:
Tarea: "Crear app/frontend/test/RealtorWeb/InteractionLoggingCatalogServiceTests.cs"
Tarea: "Ampliar app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs"
Tarea: "Ampliar app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs"
```

## Ejemplo de paralelismo: Historia de usuario 4

```bash
# Ejecutar juntas las pruebas de US4:
Tarea: "Ampliar app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs"
Tarea: "Ampliar app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs"
Tarea: "Ampliar app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs"
```

---

## Estrategia de implementación

### MVP primero: solo Historia de usuario 1

1. Completar Fase 1 y Fase 2.
2. Completar Fase 3.
3. **DETENERSE Y VALIDAR** la Historia de usuario 1 de forma independiente.
4. Mostrar el recorrido catálogo-detalle-retorno como incremento útil antes de continuar.
5. Incluir US2 inmediatamente después porque también es prioridad P1.

### Entrega incremental

1. Configuración y fundamentos listos.
2. US1: recorrido interactivo con contexto; validar y mostrar.
3. US2: recuperación breve; validar y mostrar.
4. US3: progreso, no disponibilidad y reintento; validar y mostrar.
5. US4: aislamiento, accesibilidad y adaptación; validar y mostrar.
6. Pulido: regresiones y evidencia real.

### Estrategia con equipo

1. El equipo completa juntos Fase 1 y Fase 2.
2. Un implementador toma US1 y luego US2.
3. Otro implementador escribe pruebas de US3 y US4 en archivos ya previstos por el plan.
4. Se integran cambios en `Program.cs`, `app.css`, `Home.razor`, `PropertyDetails.razor` y `ReconnectModal.razor` de uno en uno para evitar conflictos.

---

## Notas

- Las tareas marcadas con `[P]` usan archivos distintos y no dependen de resultados incompletos.
- La etiqueta `[USn]` permite trazar cada tarea hasta su historia en `spec.md`.
- Cada historia debe poder completarse y probarse por separado.
- Las pruebas deben fallar antes de la implementación.
- No se marcan tareas como completadas hasta verificar su resultado.
- No se crean proyectos, paquetes, backend, OpenAPI, migraciones, formularios ni capacidades de escritura.
- La spec permanece en estado `Borrador`; `speckit.implement` solo puede iniciarse tras la aprobación humana que la transicione a `Aprobada`.
- `specs/013-signalr-interactive-server/quickstart.md` debe contener evidencia ejecutada antes de declarar la iniciativa terminada.
- Evitar descripciones vagas, rutas incompletas y dependencias entre historias que rompan su independencia.
