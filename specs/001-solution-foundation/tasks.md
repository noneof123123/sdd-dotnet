# Tasks: Base de la solución Realtor

**Input**: Design documents from `/specs/001-solution-foundation/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Organization**: Las tareas se agrupan por historia de usuario para permitir una implementación independiente y verificable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Puede ejecutarse en paralelo sin depender de otras tareas incompletas
- **[Story]**: Historia de usuario asociada: US1, US2 o US3
- Las descripciones incluyen rutas de archivo exactas

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar la estructura inicial sin implementar lógica de negocio

- [X] T001 Verificar que `global.json` existe en la raíz y que conserva su configuración actual sin modificarlo
- [X] T002 Crear el directorio base `app/` y la solución `app/Realtor.sln` con `dotnet new sln --name Realtor --output app`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Crear los proyectos de aplicación que utilizan todas las historias de usuario

**⚠️ CRITICAL**: Esta fase debe completarse antes de comenzar las historias de usuario

- [ ] T003 Crear el proyecto backend Minimal API `app/backend/src/RealtorApi/` con `dotnet new webapi --name RealtorApi --output app/backend/src/RealtorApi --use-minimal-apis`
- [ ] T004 Crear el proyecto frontend Blazor `app/frontend/src/RealtorWeb/` con `dotnet new blazor --name RealtorWeb --output app/frontend/src/RealtorWeb`
- [ ] T005 Incorporar `app/backend/src/RealtorApi/RealtorApi.csproj` y `app/frontend/src/RealtorWeb/RealtorWeb.csproj` a `app/Realtor.sln`

**Checkpoint**: Foundation ready - la solución contiene los proyectos principales y puede comenzar el trabajo por historias

---

## Phase 3: User Story 1 - Preparar el repositorio para desarrollar Realtor (Priority: P1) 🎯 MVP

**Goal**: Disponer de una solución compartida compilable con backend y frontend

**Independent Test**: Ejecutar `dotnet restore app/Realtor.sln` y `dotnet build app/Realtor.sln --no-restore`; comprobar que termina sin errores y no modifica `global.json`

### Implementation for User Story 1

- [ ] T006 [US1] Limitar `app/backend/src/RealtorApi/Program.cs` a servicios base, middleware base y configuración mínima sin lógica de negocio ni entidades de dominio
- [ ] T007 [US1] Configurar `app/frontend/src/RealtorWeb/Program.cs` con el arranque base de Blazor Web App y sin features de negocio
- [ ] T008 [US1] Verificar que `app/Realtor.sln` contiene únicamente los proyectos aprobados de la iniciativa y sus proyectos de pruebas
- [ ] T009 [US1] Ejecutar restauración y compilación mediante `dotnet restore app/Realtor.sln` y `dotnet build app/Realtor.sln --no-restore`

**Checkpoint**: La solución base es restaurable y compilable sin lógica de negocio ni entidades de dominio

---

## Phase 4: User Story 2 - Establecer los puntos de inicio base de backend y frontend (Priority: P2)

**Goal**: Dejar los puntos de entrada backend y frontend preparados con configuración arquitectónica mínima

**Independent Test**: Revisar `Program.cs` de ambos proyectos y confirmar Minimal APIs, Blazor Web App, ausencia de controllers y ausencia de lógica de negocio

### Implementation for User Story 2

- [ ] T010 [P] [US2] Revisar y ajustar `app/backend/src/RealtorApi/Program.cs` para que use únicamente servicios, middleware y mapeo inicial de endpoints
- [ ] T011 [P] [US2] Revisar y ajustar `app/frontend/src/RealtorWeb/Program.cs` para que configure únicamente el arranque base de Razor Components
- [ ] T012 [US2] Eliminar de `app/backend/src/RealtorApi/` cualquier estructura de controllers o endpoint de demostración ajeno a la configuración base
- [ ] T013 [US2] Confirmar que los proyectos usan el SDK seleccionado por `global.json` sin crear ni modificar `global.json`

**Checkpoint**: Backend y frontend cumplen la configuración mínima aprobada y respetan el stack obligatorio

---

## Phase 5: User Story 3 - Incorporar pruebas de infraestructura desde el inicio (Priority: P3)

**Goal**: Disponibilizar proyectos de pruebas separados para backend y frontend

**Independent Test**: Crear y compilar ambos proyectos de pruebas, añadirlos a la solución y verificar que no contienen lógica de negocio

### Implementation for User Story 3

- [ ] T014 [P] [US3] Crear el proyecto de pruebas unitarias backend `app/backend/tests/RealtorApiTests/` con una plantilla de pruebas unitarias compatible con la solución
- [ ] T015 [P] [US3] Crear el proyecto de pruebas unitarias frontend `app/frontend/test/RealtorWeb/` con una plantilla de pruebas unitarias compatible con la solución
- [ ] T016 [US3] Añadir referencias de proyecto desde `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` hacia `app/backend/src/RealtorApi/RealtorApi.csproj`
- [ ] T017 [US3] Añadir referencias de proyecto desde `app/frontend/test/RealtorWeb/RealtorWeb.csproj` hacia `app/frontend/src/RealtorWeb/RealtorWeb.csproj`
- [ ] T018 [US3] Incorporar ambos proyectos de pruebas a `app/Realtor.sln`

**Checkpoint**: Los proyectos de pruebas están asociados a sus aplicaciones y reconocidos por la solución

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validación final de estructura, compilación y límites constitucionales

- [ ] T019 [P] Revisar rutas y nombres de `app/Realtor.sln`, `app/backend/src/RealtorApi/`, `app/backend/tests/RealtorApiTests/`, `app/frontend/src/RealtorWeb/` y `app/frontend/test/RealtorWeb/`
- [ ] T020 [P] Revisar el repositorio para confirmar que no se añadieron entidades de dominio, persistencia, migraciones, controllers o features
- [ ] T021 Ejecutar `dotnet restore app/Realtor.sln` y `dotnet build app/Realtor.sln --no-restore` como validación final
- [ ] T022 Validar los criterios de aceptación documentados en `specs/001-solution-foundation/quickstart.md` y actualizar el estado de implementación si corresponde

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sin dependencias
- **Foundational (Phase 2)**: Depende de completar Phase 1 y bloquea todas las historias
- **User Stories (Phase 3+)**: Dependen de completar Phase 2
- **Polish (Final Phase)**: Depende de completar las historias deseadas

### User Story Dependencies

- **US1 (P1)**: Puede comenzar después de Phase 2 y no depende de US2 ni US3
- **US2 (P2)**: Puede comenzar después de Phase 2 y modifica principalmente los puntos de entrada creados por la fundación
- **US3 (P3)**: Puede comenzar después de crear los proyectos principales y no requiere historias de negocio

### Parallel Opportunities

- T003 y T004 pueden ejecutarse en paralelo después de T002
- T010 y T011 pueden ejecutarse en paralelo
- T014 y T015 pueden ejecutarse en paralelo
- T019 y T020 pueden ejecutarse en paralelo

---

## Parallel Example: User Story 3

```text
T014: Crear app/backend/tests/RealtorApiTests/
T015: Crear app/frontend/test/RealtorWeb/
T018: Incorporar ambos proyectos a app/Realtor.sln
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Setup
2. Completar Phase 2: Foundational
3. Completar Phase 3: User Story 1
4. Validar restauración y compilación de la solución

### Incremental Delivery

1. Completar Setup y Foundational
2. Añadir US1 y validar el árbol base compartido
3. Añadir US2 y validar los puntos de entrada
4. Añadir US3 y validar los proyectos de pruebas
5. Ejecutar la validación final

---

## Notes

- Las tareas marcadas `[P]` afectan archivos o proyectos diferentes
- Las tareas de historias permiten trazabilidad directa a US1, US2 y US3
- No se han añadido pruebas funcionales porque la especificación no solicita casos de prueba adicionales
- Esta iniciativa no incluye migraciones, persistencia, entidades ni lógica de negocio
