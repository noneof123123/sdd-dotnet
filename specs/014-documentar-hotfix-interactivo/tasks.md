---
description: "Lista de tareas retrospectivas para el hotfix interactivo"
---

# Tareas: Documentación retrospectiva del hotfix interactivo

**Entrada**: Documentos de diseño de `/specs/014-documentar-hotfix-interactivo/`

**Prerrequisitos**: `plan.md` y `spec.md` requeridos; `research.md`, `data-model.md`, `contracts/`, `quickstart.md` y `.specify/memory/constitution.md` disponibles.

**Pruebas**: Se incluyen tareas de prueba porque la spec exige pruebas independientes por historia y el plan define unit tests con xUnit y bUnit. La reconexión real, la concurrencia, la prueba con participantes y el registro operativo en ejecución real quedaron confirmados por el responsable y registrados como evidencia manual.

**Estado**: `[X]` significa verificado mediante diff, pruebas ejecutadas, evidencia confirmada por el responsable o archivos existentes. `[ ]` significa pendiente de ejecución o evidencia real.

**Organización**: Las tareas se agrupan por historia de usuario para permitir validación independiente.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo; usa archivos distintos y no depende de tareas incompletas.
- **[Historia]**: Etiqueta US1 a US5 solo en fases de historias de usuario.
- Incluir rutas exactas en cada descripción.
- Las citas `data-model.md` señalan la definición canónica de campos restringidos.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Habilitar la infraestructura interactiva ya aplicada en el frontend.

- [X] T001 Activar componentes de Interactive Server y mapear el modo interactivo en `app/frontend/src/RealtorWeb/Program.cs`.
- [X] T002 Registrar `InteractionContext`, `InteractionCircuitHandler`, `PropertyCatalogService` y el decorador `IPropertyCatalogService` en `app/frontend/src/RealtorWeb/Program.cs`.
- [X] T003 Incorporar `@using RealtorWeb.Features.Interactions` en `app/frontend/src/RealtorWeb/Components/_Imports.razor`.

**Checkpoint**: Configuración interactiva verificada en `Program.cs` y `_Imports.razor`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Tipos, contexto, umbrales, mensajes y estilos compartidos por todas las historias.

**⚠️ CRITICAL**: Ninguna historia podía completarse antes de esta fase.

- [X] T004 [P] Definir enumeraciones e `IncidentRecord` en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionState.cs` según `data-model.md` `Estado de interacción.Valores`, `Estado de interacción.Resultados asociados`, `Estados de presentación.Valores`, `Incidencia operativa.Tipo` e `Incidencia operativa.Resultado`, reutilizando la definición de `Estados de presentación.Valores` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyViewModels.cs`.
- [X] T005 [P] Implementar `InteractionContext` en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionContext.cs` según `data-model.md` `Visita interactiva.Inicio`, `Visita interactiva.Fin`, `Contexto de navegación.VistaActual`, `Contexto de navegación.PaginaSolicitada`, `Contexto de navegación.PaginaSaneada`, `Contexto de navegación.PropiedadId` y `Contexto de navegación.DireccionRetorno`.
- [X] T006 [P] Implementar umbrales, clasificación y textos controlados en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionDiagnostics.cs` y `app/frontend/src/RealtorWeb/Features/Interactions/InteractionMessages.cs` según `data-model.md` `Incidencia operativa.Momento`, `Incidencia operativa.Vista`, `Incidencia operativa.Resultado` y las reglas de umbral.
- [X] T007 [P] Añadir estilos de modal, progreso, foco y estados en `app/frontend/src/RealtorWeb/wwwroot/app.css`.

**Checkpoint**: Base de interacción verificada; las historias pueden validarse de forma independiente.

---

## Phase 3: User Story 1 - Recorrer el catálogo y el detalle sin perder el contexto (Priority: P1) MVP

**Goal**: Mantener página, propiedad y retorno mientras el visitante navega entre catálogo y detalle.

**Independent Test**: Abrir catálogo, cambiar de página, abrir propiedad, volver al catálogo y comprobar que se restaura la página de origen; probar además enlace directo y páginas inválidas.

### Tests for User Story 1

- [X] T008 [P] [US1] Crear pruebas de contexto y retorno del catálogo en `app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs`.
- [X] T009 [P] [US1] Crear pruebas de contexto y retorno del detalle en `app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs`.
- [X] T010 [US1] Actualizar pruebas existentes de navegación y paginación en `app/frontend/test/RealtorWeb/CatalogNavigationTests.cs`, `app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs`, `app/frontend/test/RealtorWeb/PropertyDetailNavigationTests.cs` y `app/frontend/test/RealtorWeb/Components/PropertyDetailsBackLinkTests.cs`.

### Implementation for User Story 1

- [X] T011 [P] [US1] Habilitar carga interactiva del catálogo en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor`.
- [X] T012 [P] [US1] Habilitar carga interactiva del detalle en `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`.
- [X] T013 [US1] Aplicar el control de intención vigente con `data-model.md` `Intención vigente.ClaveCargada` e `Intención vigente.ContadorDeIntencion` en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`.

### Validation for User Story 1

- [X] T014 [US1] Ejecutar VM-001, VM-002, VM-003, VM-004, SC-002 y SC-005 y registrar evidencia real en `specs/013-signalr-interactive-server/quickstart.md` y `specs/014-documentar-hotfix-interactivo/quickstart.md`.

**Checkpoint**: Navegación y conservación de contexto verificadas por diff, pruebas y validación manual confirmada.

---

## Phase 4: User Story 2 - Recuperarse de interrupciones breves (Priority: P1)

**Goal**: Recuperar automáticamente interrupciones de hasta 10 segundos conservando la vista y el contexto.

**Independent Test**: Detener y reiniciar el frontend en menos de 10 segundos durante una visita y comprobar que vuelve la misma vista sin recarga manual.

### Tests for User Story 2

- [X] T015 [P] [US2] Crear pruebas del modal de reconexión en `app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs`.
- [X] T016 [US2] Crear pruebas unitarias del ciclo de conexión en `app/frontend/test/RealtorWeb/InteractionCircuitHandlerTests.cs`.

### Implementation for User Story 2

- [X] T017 [P] [US2] Implementar el ciclo de conexión en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionCircuitHandler.cs` según `data-model.md` `Visita interactiva.Fin`, `Incidencia operativa.Momento`, `Incidencia operativa.Vista`, `Incidencia operativa.Tipo` e `Incidencia operativa.Resultado`.
- [X] T018 [P] [US2] Crear el modal de reconexión en `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor` y `app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor.js` con los estados y acciones de `specs/014-documentar-hotfix-interactivo/contracts/contrato-experiencia-interactiva.md`.
- [X] T019 [US2] Montar `<ReconnectModal />` en `app/frontend/src/RealtorWeb/Components/App.razor`.

### Validation for User Story 2

- [X] T020 [US2] Ejecutar VM-005, VM-006 y VM-007 y registrar evidencia real en `specs/013-signalr-interactive-server/quickstart.md`.

**Checkpoint**: Modal y ciclo implementados; recuperación real confirmada y pruebas del ciclo completadas.

---

## Phase 5: User Story 3 - Entender y reintentar cuando la interacción no está disponible (Priority: P2)

**Goal**: Mostrar progreso, explicar la no disponibilidad y permitir reintentar la misma intención.

**Independent Test**: Provocar una respuesta lenta y una interrupción prolongada, comprobar mensajes en español, acciones ofrecidas y reintento con el mismo contexto.

### Tests for User Story 3

- [X] T021 [US3] Crear pruebas de espera y reintento en `app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs` y `app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs`.

### Implementation for User Story 3

- [X] T022 [P] [US3] Añadir la acción `OnRetry` en `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`, conservando el enlace alternativo `RetryUrl`.
- [X] T023 [US3] Conectar mensajes de espera, estados de error y reintento en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`.

### Validation for User Story 3

- [X] T024 [US3] Ejecutar VM-008 y registrar evidencia real en `specs/013-signalr-interactive-server/quickstart.md` y `specs/014-documentar-hotfix-interactivo/quickstart.md`.

**Checkpoint**: Mensajes y reintento verificados por pruebas; validación con API detenida confirmada.

---

## Phase 6: User Story 4 - Mantener una experiencia coherente, aislada y accesible (Priority: P2)

**Goal**: Conservar accesibilidad, aislamiento por visita, coherencia visual y ausencia de desplazamiento horizontal.

**Independent Test**: Usar dos visitas independientes, recorrer acciones con teclado y revisar las vistas a 320, 768 y 1280 px.

### Tests for User Story 4

- [X] T025 [P] [US4] Actualizar pruebas de accesibilidad en `app/frontend/test/RealtorWeb/Components/PropertyDetailsAccessibilityTests.cs`, `app/frontend/test/RealtorWeb/Components/PresentationStateTests.cs` y `app/frontend/test/RealtorWeb/Components/PropertyDetailsStateTests.cs`.
- [X] T026 [P] [US4] Crear pruebas de aislamiento de contextos en `app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs`.

### Implementation for User Story 4

- [X] T027 [P] [US4] Ajustar estilos accesibles, responsive y de movimiento reducido en `app/frontend/src/RealtorWeb/wwwroot/app.css`.

### Validation for User Story 4

- [X] T028 [US4] Ejecutar VM-009, VM-010, VM-011, VM-012, VM-013 y SC-006 y registrar evidencia real en `specs/013-signalr-interactive-server/quickstart.md`.

**Checkpoint**: Accesibilidad y aislamiento cubiertos por pruebas unitarias; validación visual y concurrente confirmada.

---

## Phase 7: User Story 5 - Registrar incidencias sin exponerlas al visitante (Priority: P3)

**Goal**: Registrar internamente incidencias de respuesta lenta o recuperación fallida sin datos personales ni exposición visible.

**Independent Test**: Provocar una consulta lenta, un error de contenido y una recuperación fallida; comprobar los campos del registro y que la vista no muestra el registro.

### Tests for User Story 5

- [X] T029 [P] [US5] Crear pruebas de umbrales y resultados controlados en `app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs`.
- [X] T030 [P] [US5] Crear pruebas del decorador de registro en `app/frontend/test/RealtorWeb/InteractionLoggingCatalogServiceTests.cs`.

### Implementation for User Story 5

- [X] T031 [P] [US5] Implementar `InteractionLoggingCatalogService` en `app/frontend/src/RealtorWeb/Features/Interactions/InteractionLoggingCatalogService.cs` según `data-model.md` `Incidencia operativa.Tipo`, `Incidencia operativa.Momento`, `Incidencia operativa.Vista` e `Incidencia operativa.Resultado`.

### Validation for User Story 5

- [X] T032 [US5] Ejecutar VM-014 y SC-010 y registrar evidencia real en `specs/013-signalr-interactive-server/quickstart.md`.

**Checkpoint**: Registro verificado por pruebas unitarias y verificación en ejecución real confirmada.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentación retrospectiva, evidencia automatizada y cierre de gobernanza.

- [X] T033 Documentar la especificación retrospectiva en `specs/014-documentar-hotfix-interactivo/spec.md`.
- [X] T034 Documentar el plan técnico retrospectivo en `specs/014-documentar-hotfix-interactivo/plan.md`.
- [X] T035 Documentar decisiones retrospectivas en `specs/014-documentar-hotfix-interactivo/research.md`.
- [X] T036 Documentar el modelo de estado e incidencias en `specs/014-documentar-hotfix-interactivo/data-model.md`.
- [X] T037 [P] Documentar contratos en `specs/014-documentar-hotfix-interactivo/contracts/contrato-experiencia-interactiva.md` y `specs/014-documentar-hotfix-interactivo/contracts/contrato-estado-y-registro.md`.
- [X] T038 Documentar la guía de validación en `specs/014-documentar-hotfix-interactivo/quickstart.md`.
- [X] T039 Generar esta lista de tareas en `specs/014-documentar-hotfix-interactivo/tasks.md`.
- [X] T040 Registrar pruebas automatizadas ejecutadas: 111/111 frontend, 17/17 OpenAPI y 324/324 solución en `specs/014-documentar-hotfix-interactivo/quickstart.md`.
- [X] T041 Verificar `git diff --check` y ausencia de cambios en `app/backend/`, `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, `app/frontend/src/RealtorWeb/Features/Properties/IPropertiesApi.cs`, `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` y `app/frontend/src/RealtorWeb/RealtorWeb.csproj`.
- [X] T042 Consolidar la evidencia manual confirmada por el responsable en `specs/013-signalr-interactive-server/quickstart.md` y `specs/014-documentar-hotfix-interactivo/quickstart.md`.
- [X] T043 Registrar la transición de `specs/014-documentar-hotfix-interactivo/spec.md` a `Implementada`, autorizada por la constitución v1.2.0 para iniciativas retrospectivas con entregable documental completo y evidencia registrada en `quickstart.md`.

**Checkpoint**: Documentación retrospectiva verificada y cierre registrado; evidencia manual confirmada y consolidada.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias; verificado.
- **Foundational (Phase 2)**: Depende de Setup; verificado y bloquea el resto de fases.
- **User Stories (Phase 3 a 7)**: Dependen de Foundational; pueden validarse por separado.
- **Polish (Phase 8)**: Completado; T042 y T043 verificadas.

### User Story Dependencies

- **US1 (P1)**: Sin dependencias entre historias; navegación y contexto verificados por diff, pruebas y evidencia manual confirmada.
- **US2 (P1)**: Puede integrarse con US1 pero conserva prueba independiente; recuperación confirmada por el responsable y pruebas del ciclo completadas.
- **US3 (P2)**: Usa los contextos de la fase fundamental; mensajes y reintento verificados por pruebas y evidencia manual confirmada.
- **US4 (P2)**: Aplica a todas las vistas; accesibilidad y aislamiento verificados por pruebas y validación visual confirmada.
- **US5 (P3)**: Registro verificado por pruebas unitarias y validación real confirmada.

### Within Each User Story

- Las tareas marcadas `[X]` tienen evidencia en el diff, en pruebas ejecutadas, en archivos existentes o en la confirmación del responsable.
- Las tareas marcadas `[ ]` requieren ejecución, creación de archivo o evidencia adicional.
- No se añade trabajo de implementación nuevo; todas las tareas están completadas y la evidencia unitaria y manual quedó registrada.

### Parallel Opportunities

- 19 tareas están marcadas `[P]`: T004, T005, T006, T007, T008, T009, T011, T012, T015, T017, T018, T021, T025, T026, T027, T029, T030, T031 y T037.
- Las fases fundamentales y las pruebas de historias pueden ejecutarse en paralelo cuando usan archivos distintos.
- Las validaciones manuales de US1 a US5 pueden ejecutarse en sesiones independientes, salvo SC-006 que requiere 20 visitas concurrentes.

---

## Parallel Example: User Story 1

```text
Task: "Crear pruebas de contexto y retorno del catálogo en app/frontend/test/RealtorWeb/Components/HomeInteractionTests.cs"
Task: "Crear pruebas de contexto y retorno del detalle en app/frontend/test/RealtorWeb/Components/PropertyDetailsInteractionTests.cs"
Task: "Habilitar carga interactiva del catálogo en app/frontend/src/RealtorWeb/Components/Pages/Home.razor"
Task: "Habilitar carga interactiva del detalle en app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor"
```

## Parallel Example: User Story 2

```text
Task: "Crear pruebas del modal de reconexión en app/frontend/test/RealtorWeb/Components/ReconnectModalTests.cs"
Task: "Implementar el ciclo de conexión en app/frontend/src/RealtorWeb/Features/Interactions/InteractionCircuitHandler.cs"
Task: "Crear el modal de reconexión en app/frontend/src/RealtorWeb/Components/Layout/ReconnectModal.razor"
```

## Parallel Example: User Story 3

```text
Task: "Añadir la acción OnRetry en app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor"
```

## Parallel Example: User Story 4

```text
Task: "Actualizar pruebas de accesibilidad en app/frontend/test/RealtorWeb/Components/PropertyDetailsAccessibilityTests.cs"
Task: "Crear pruebas de aislamiento de contextos en app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs"
Task: "Ajustar estilos accesibles y responsive en app/frontend/src/RealtorWeb/wwwroot/app.css"
```

## Parallel Example: User Story 5

```text
Task: "Crear pruebas de umbrales y resultados controlados en app/frontend/test/RealtorWeb/InteractionDiagnosticsTests.cs"
Task: "Crear pruebas del decorador de registro en app/frontend/test/RealtorWeb/InteractionLoggingCatalogServiceTests.cs"
Task: "Implementar InteractionLoggingCatalogService en app/frontend/src/RealtorWeb/Features/Interactions/InteractionLoggingCatalogService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Confirmar Setup y Foundational como verificados.
2. Validar US1 de forma independiente con VM-001 a VM-004, SC-002 y SC-005.
3. Mantener el MVP documentado como completado con la evidencia manual confirmada por el responsable.

### Incremental Delivery

1. US1: recorrido principal y contexto.
2. US2: recuperación breve; también prioritaria P1.
3. US3: espera, error y reintento.
4. US4: accesibilidad, aislamiento y responsive.
5. US5: registro operativo.
6. Cierre: consolidar evidencia y transición de estado.

### Parallel Team Strategy

- Un equipo puede ejecutar las validaciones manuales de US1/US2 mientras otro revisa US4/US5.
- SC-006 requiere coordinación por ser una prueba con 20 visitas concurrentes.
- Las tareas marcadas `[P]` pueden ejecutarse en paralelo solo si no comparten archivo.

---

## Notes

- No se propone implementación nueva: las tareas `[X]` documentan cambios ya presentes y evidencia confirmada.
- Todas las tareas están marcadas `[X]`; no queda trabajo pendiente en esta lista.
- No se requiere regenerar OpenAPI porque el backend y el contrato HTTP no cambiaron.
- `git diff --check` y las pruebas automatizadas están registradas como evidencia del 2026-11-22.
- La transición a `Implementada` quedó registrada conforme a la constitución v1.2.0 para iniciativas retrospectivas con entregable documental completo y evidencia registrada en `quickstart.md`.
