# Tasks: Fundación interna del backend

**Input**: Documentos de diseño de `/specs/002-foundation-backend/`

**Prerequisites**: `plan.md` requerido, `spec.md` requerido para las historias de usuario, `research.md`, `data-model.md`, `contracts/`

**Tests**: Los tests están incluidos porque la especificación los solicita explícitamente.

**Organization**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: puede ejecutarse en paralelo con otras tareas de archivos distintos sin depender de tareas incompletas.
- **[Historia]**: identifica la historia de usuario, por ejemplo `[US1]`, `[US2]`, `[US3]`, `[US4]` o `[US5]`.
- Todas las tareas incluyen rutas de archivos exactas.

## Phase 1: Setup (Infraestructura compartida)

**Purpose**: Preparar únicamente las dependencias y carpetas necesarias para la fundación backend.

- [X] T001 Crear las carpetas base de infraestructura y features en `app/backend/src/RealtorApi/Infrastructure/`, `app/backend/src/RealtorApi/Features/` y `app/backend/tests/RealtorApiTests/Infrastructure/`.
- [X] T002 [P] Agregar la referencia de `FluentValidation` al proyecto `app/backend/src/RealtorApi/RealtorApi.csproj`.
- [X] T003 [P] Configurar el proyecto de tests backend para acceso a tipos públicos de infraestructura en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.

---

## Phase 2: Foundational (Prerrequisitos bloqueantes)

**Purpose**: Establecer los contratos compartidos que serán usados por todas las historias.

**⚠️ CRITICAL**: Ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T004 Crear el contrato `ISlice` en `app/backend/src/RealtorApi/Infrastructure/Endpoints/ISlice.cs`.
- [X] T005 Crear el marker interface `IHandler` en `app/backend/src/RealtorApi/Infrastructure/Handlers/IHandler.cs`.
- [X] T006 Crear el modelo compartido mínimo `Result` y sus estados en `app/backend/src/RealtorApi/Infrastructure/Results/Result.cs`.
- [X] T007 Crear el modelo compartido de errores de resultado en `app/backend/src/RealtorApi/Infrastructure/Results/Error.cs`.
- [X] T008 Crear la conversión centralizada base de errores a `ProblemDetails` en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs`.
- [X] T009 Configurar FluentValidation y el escaneo automático de validators sin registros individuales en `app/backend/src/RealtorApi/Program.cs`.

**Checkpoint**: Contratos, modelos de resultado y registro base preparados; las historias pueden implementarse.

---

## Phase 3: User Story 1 - Descubrir endpoints sin modificar el arranque (Priority: P1) 🎯 MVP

**Goal**: Permitir que clases `ISlice` públicas y concretas sean descubiertas y mapeadas automáticamente sin modificar `Program.cs`.

**Independent Test**: Una clase `ISlice` definida en el proyecto de tests se registra una sola vez y queda disponible para el mapeo centralizado.

### Tests for User Story 1

- [X] T010 [P] [US1] Crear una clase `ISlice` de prueba sin lógica de negocio en `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/TestSlice.cs`.
- [X] T011 [P] [US1] Crear pruebas unitarias de descubrimiento y ausencia de duplicados para `RegisterSlices` en `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/RegisterSlicesTests.cs`.
- [X] T012 [P] [US1] Crear pruebas unitarias del mapeo centralizado de slices en `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/MapSliceEndpointsTests.cs`.

### Implementation for User Story 1

- [X] T013 [US1] Implementar el descubrimiento por assembly de clases públicas, concretas y no abstractas que implementan `ISlice` sin duplicados en `app/backend/src/RealtorApi/Infrastructure/Endpoints/RegisterSlices.cs`.
- [X] T014 [US1] Implementar el mapeo centralizado que resuelve `IEnumerable<ISlice>` y llama a `AddEndpoint` en `app/backend/src/RealtorApi/Infrastructure/Endpoints/MapSliceEndpoints.cs`.
- [X] T015 [US1] Registrar y ejecutar `RegisterSlices` y `MapSliceEndpoints` desde `app/backend/src/RealtorApi/Program.cs` sin registro manual de endpoints individuales.

**Checkpoint**: El descubrimiento y mapeo de `ISlice` funciona y puede demostrarse de forma independiente.

---

## Phase 4: User Story 2 - Validar requests automáticamente (Priority: P1)

**Goal**: Detectar `IValidator<T>` por convención y devolver errores de validación consistentes con HTTP 400.

**Independent Test**: Tests con tipos que tienen validator y tipos sin validator prueban la ejecución y el pass-through; un fallo devuelve `ValidationProblemDetails`.

### Tests for User Story 2

- [X] T016 [P] [US2] Crear tipos request y un `IValidator<T>` de prueba en `app/backend/tests/RealtorApiTests/Infrastructure/Validation/TestValidationTypes.cs`.
- [X] T017 [P] [US2] Crear pruebas de detección y ejecución de `IValidator<T>` en `app/backend/tests/RealtorApiTests/Infrastructure/Validation/ValidationFilterFactoryTests.cs`.
- [X] T018 [P] [US2] Crear pruebas de pass-through para tipos sin validator en `app/backend/tests/RealtorApiTests/Infrastructure/Validation/ValidationFilterFactoryTests.cs`.
- [X] T019 [P] [US2] Crear pruebas de transformación de fallos a `ValidationProblemDetails` con HTTP 400 en `app/backend/tests/RealtorApiTests/Infrastructure/Validation/ValidationProblemDetailsTests.cs`.

### Implementation for User Story 2

- [X] T020 [US2] Implementar `ValidationFilterFactory.Create` consultando `IValidator<T>` mediante convención y `IServiceProviderIsService` en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`.
- [X] T021 [US2] Integrar el filtro de validación automáticamente con el mapeo central de slices en `app/backend/src/RealtorApi/Infrastructure/Endpoints/MapSliceEndpoints.cs`.
- [X] T022 [US2] Usar `ValidateAsync`, propagación de `CancellationToken`, agrupación por propiedad y `ValidationProblemDetails` HTTP 400 en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`.

**Checkpoint**: La validación automática falla con 400 cuando corresponde y hace pass-through sin validator.

---

## Phase 5: User Story 5 - Mantener /health como demostración end to end (Priority: P1)

**Goal**: Migrar `/health` al patrón `ISlice` y dejarlo descubierto sin modificar individualmente `Program.cs`.

**Independent Test**: El slice de `/health` queda mapeado y su respuesta devuelve HTTP 200 con `{ "status": "healthy" }`.

### Tests for User Story 5

- [X] T023 [P] [US5] Crear pruebas unitarias del slice de `/health`, método HTTP, ruta y payload en `app/backend/tests/RealtorApiTests/Features/Health/HealthSliceTests.cs`.

### Implementation for User Story 5

- [X] T024 [US5] Implementar el único slice de infraestructura `/health` con HTTP 200 y payload `{ "status": "healthy" }` en `app/backend/src/RealtorApi/Features/Health/HealthSlice.cs`.
- [X] T025 [US5] Verificar que `/health` sea descubierto sin agregar una llamada manual en `app/backend/src/RealtorApi/Program.cs` y conservar el contrato definido en `specs/002-foundation-backend/contracts/health.http`.

**Checkpoint**: `/health` demuestra el pipeline completo de discovery y mapeo sin persistencia ni dominio.

---

## Phase 6: User Story 3 - Comunicar errores esperados y registrar eventos base (Priority: P2)

**Goal**: Proporcionar `Result`, categorías iniciales de error, `ProblemDetails` seguros y logging estructurado compartido.

**Independent Test**: Tests unitarios verifican 400, 404 y 409; los payloads contienen `title`, `detail` y `status` sin datos sensibles.

### Tests for User Story 3

- [X] T026 [P] [US3] Crear pruebas unitarias de mapeo de Validation, NotFound y Conflict a los status HTTP correspondientes en `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMapperTests.cs`.
- [X] T027 [P] [US3] Crear pruebas del payload seguro de `ProblemDetails` para 404 y 409 en `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMapperTests.cs`.

### Implementation for User Story 3

- [X] T028 [US3] Completar `Result` y `Error` con códigos, categorías y metadatos mínimos para Validation, NotFound y Conflict en `app/backend/src/RealtorApi/Infrastructure/Results/Result.cs` y `app/backend/src/RealtorApi/Infrastructure/Results/Error.cs`.
- [X] T029 [US3] Implementar el mapeo centralizado Validation → 400, NotFound → 404 y Conflict → 409 en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs`.
- [X] T030 [US3] Incorporar logging estructurado mediante `ILogger`, eventos nombrados y exclusión de datos sensibles en `app/backend/src/RealtorApi/Infrastructure/`.

**Checkpoint**: Los errores esperados tienen mapping HTTP y payload consistentes con logging base seguro.

---

## Phase 7: User Story 4 - Registrar casos de uso sin enlazado manual (Priority: P2)

**Goal**: Preparar el marker `IHandler` y el assembly scanning sin crear handlers concretos de negocio.

**Independent Test**: Una implementación de prueba de `IHandler` se descubre una sola vez y no existe ninguna implementación concreta en producción.

### Tests for User Story 4

- [X] T031 [P] [US4] Crear una implementación de prueba de `IHandler` sin lógica de negocio en `app/backend/tests/RealtorApiTests/Infrastructure/Handlers/TestHandler.cs`.
- [X] T032 [P] [US4] Crear pruebas unitarias de descubrimiento y ausencia de duplicados de implementaciones `IHandler` en `app/backend/tests/RealtorApiTests/Infrastructure/Handlers/RegisterHandlersTests.cs`.

### Implementation for User Story 4

- [X] T033 [US4] Implementar el assembly scanning de clases concretas que implementan `IHandler` y su registro sin duplicados en `app/backend/src/RealtorApi/Infrastructure/Handlers/RegisterHandlers.cs`.
- [X] T034 [US4] Integrar `RegisterHandlers` en `app/backend/src/RealtorApi/Program.cs` sin registrar handlers individuales ni agregar handlers de producción.

**Checkpoint**: El registro futuro de handlers queda preparado sin clases de negocio ni registro manual.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Validar integración final, documentación y cumplimiento de exclusiones.

- [X] T035 [P] Ejecutar restauración y compilación completas de `app/Realtor.sln` desde la raíz del repositorio.
- [X] T036 [P] Ejecutar todas las pruebas unitarias backend de `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
- [X] T037 [P] Revisar que no existan controllers, entidades, `AppDbContext`, migraciones, seeders ni conexión de base de datos en `app/backend/src/RealtorApi/`.
- [X] T038 [P] Revisar que las clases `ISlice`, `IValidator<T>` e `IHandler` exclusivas de pruebas estén solo en `app/backend/tests/RealtorApiTests/`.
- [X] T039 Confirmar que `Program.cs` contiene solo configuración, registro de infraestructura y mapeo centralizado en `app/backend/src/RealtorApi/Program.cs`.
- [X] T040 Actualizar `specs/002-foundation-backend/quickstart.md` con la evidencia real de comandos, fecha y resultados obtenidos.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 - Setup**: puede comenzar inmediatamente y no tiene dependencias previas.
- **Phase 2 - Foundational**: depende de Phase 1 y bloquea todas las historias de usuario.
- **User Stories (Phases 3-7)**: dependen de Phase 2 y pueden ejecutarse en paralelo una vez completada la fundación.
- **Phase 8 - Polish**: depende de todas las historias deseadas completadas.

### User Story Dependencies

- **US1 (P1)**: depende de los contratos base de `ISlice` y del registro en `Program.cs`; no depende de otras historias.
- **US2 (P1)**: depende de la infraestructura de mapeo US1 para aplicar filtros automáticamente; puede implementarse junto con US1 tras los prerequisitos.
- **US5 (P1)**: depende de US1 para que el slice sea descubierto y puede implementarse independientemente de US2, US3 y US4.
- **US3 (P2)**: depende de `Result` y `Error` fundacionales; no depende de features de negocio.
- **US4 (P2)**: depende del marker `IHandler` fundacional; no requiere ninguna clase concreta de producción.

### Within Each User Story

- Las pruebas solicitadas deben escribirse antes de la implementación correspondiente y comprobarse como fallidas antes de implementar.
- Contratos y modelos antes de servicios o mapeos.
- Implementación central antes de integración en `Program.cs`.
- Cada checkpoint se valida antes de pasar a la siguiente historia.

---

## Parallel Example: User Story 1

```text
T010: Crear TestSlice en app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/TestSlice.cs
T011: Crear RegisterSlicesTests en app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/RegisterSlicesTests.cs
T012: Crear MapSliceEndpointsTests en app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/MapSliceEndpointsTests.cs
```

## Parallel Example: User Story 2

```text
T016: Crear tipos y validator de prueba en app/backend/tests/RealtorApiTests/Infrastructure/Validation/TestValidationTypes.cs
T017: Crear pruebas de ejecución con validator
T018: Crear pruebas de pass-through sin validator
T019: Crear pruebas de ValidationProblemDetails
```

## Parallel Example: User Story 5

```text
T023: Crear pruebas unitarias de HealthSlice
T024: Implementar HealthSlice
```

---

## Implementation Strategy

### MVP First (User Story 1 + /health)

1. Completar Phase 1: Setup.
2. Completar Phase 2: Foundational.
3. Completar Phase 3: US1; validar discovery y mapeo.
4. Completar Phase 5: US5; validar `/health` como demostración end to end.
5. Ejecutar tests MVP antes de continuar con validación y errores compartidos.

### Incremental Delivery

1. Completar Setup + Foundational.
2. Agregar US1 + US5 y validar el MVP sin persistencia.
3. Agregar US2 y validar validación con y sin validator.
4. Agregar US3 y validar el mapping central de errores.
5. Agregar US4 y validar el discovery de handlers futuros.
6. Cerrar con polish y evidencia en `quickstart.md`.

### Parallel Team Strategy

- Equipo completo: Phase 1 y Phase 2.
- Desarrollador A: US1 y US5.
- Desarrollador B: US2.
- Desarrollador C: US3 y US4.
- Todos: Phase 8 tras completar las historias seleccionadas.

---

## Notes

- [P] indica archivos independientes sin dependencias sobre tareas incompletas.
- Las tareas `[USn]` aparecen únicamente en las fases de historias de usuario.
- Las tareas de Setup, Foundational y Polish no llevan label de historia.
- Todas las tareas incluyen una ruta exacta y pueden completarse sin inventar alcance adicional.
- No crear migraciones, entidades, persistencia, controllers ni features de producto.
