---
description: "Lista de tareas para la implementación de 009-update-status"
---

# Tareas: 009-update-status

**Entrada**: Documentos de diseño de `/specs/009-update-status/`

**Prerrequisitos**: plan.md (obligatorio), spec.md (obligatorio para historias), research.md, data-model.md, contracts/, quickstart.md

**Pruebas**: Se incluyen tareas de prueba porque la spec define criterios de prueba independientes por historia y la constitución exige cobertura unitaria.

**Excepción de pruebas heredada (principio V)**: La spec aprobada de 008 justifica explícitamente pruebas de documento, deriva, CLI y herramientas OpenAPI. Esta característica reutiliza esa excepción de forma declarada para las tareas de validación del contrato `v1`; el principio V queda registrado como desviación justificada en `specs/009-update-status/plan.md`.

**Organización**: Las tareas se agrupan por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias incompletas)
- **[Historia]**: Historia de usuario a la que pertenece (US1, US2, US3, US4)
- Incluir rutas exactas en cada descripción

## Convenciones de rutas

- Backend: `app/backend/src/RealtorApi/`
- Pruebas: `app/backend/tests/RealtorApiTests/`
- Contrato generado: `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`
- Documentación de característica: `specs/009-update-status/`

---

## Fase 1: Configuración (infraestructura compartida)

**Propósito**: Preparar la estructura mínima y confirmar la línea base existente.

- [X] T001 Crear el directorio de la slice en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/` y el directorio de pruebas en `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/` según `specs/009-update-status/plan.md`
- [X] T002 [P] Confirmar la línea base ejecutando `dotnet build app/Realtor.sln` y verificando que no existan errores previos en `app/Realtor.sln`

---

## Fase 2: Cimientos (prerrequisitos bloqueantes)

**Propósito**: Elementos compartidos que deben existir antes de cualquier historia.

**CRÍTICO**: Ninguna historia puede comenzar hasta completar esta fase.

- [X] T003 Añadir `ChangePropertyStatusOperationId` en `app/backend/src/RealtorApi/Infrastructure/OpenApi/OpenApiConstants.cs`
- [X] T004 [P] Añadir el evento estructurado `PropertyStatusChanged` con id `1004` en `app/backend/src/RealtorApi/Infrastructure/Logging/InfrastructureLog.cs`
- [X] T005 [P] Definir los registros de solicitud y respuesta en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusRequest.cs` y `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusResponse.cs`, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusRequest.Body`, `ChangePropertyStatusBody.status`, `ChangePropertyStatusResponse.id`, `ChangePropertyStatusResponse.title`, `ChangePropertyStatusResponse.description`, `ChangePropertyStatusResponse.address`, `ChangePropertyStatusResponse.price`, `ChangePropertyStatusResponse.status`, `ChangePropertyStatusResponse.bedroomCount`, `ChangePropertyStatusResponse.bathroomCount`, `ChangePropertyStatusResponse.areaSquareMeters`, `ChangePropertyStatusResponse.imageUrl`, `ChangePropertyStatusResponse.createdAt`, `ChangePropertyStatusResponse.updatedAt`)
- [X] T006 [P] Confirmar que no se requiere migración revisando `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs` frente a `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)

**Punto de control**: Cimientos listos; las historias pueden comenzar.

---

## Fase 3: Historia de usuario 1 - Cambiar el estado sin reenviar el resto de datos (Prioridad: P1) MVP

**Objetivo**: Cambiar el estado de una propiedad existente enviando solo identificador y estado, devolviendo la representación pública completa.

**Prueba independiente**: Enviar un identificador válido y un estado distinto del vigente; verificar que solo cambian `status` y `updatedAt`, que la respuesta es `200` y que el resto de datos e imagen permanecen iguales.

### Pruebas de la historia 1

- [X] T007 [P] [US1] Crear `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandlerTests.cs` con casos de cambio correcto, preservación de campos, no encontrado, cancelación y fallo interno, según `specs/009-update-status/data-model.md` (`Property.Id`, `Property.Status`, `Property.UpdatedAt`, `Property.Title`, `Property.Description`, `Property.Address`, `Property.Price`, `Property.BedroomCount`, `Property.BathroomCount`, `Property.AreaSquareMeters`, `Property.ImageUrl`, `Property.CreatedAt`)
- [X] T008 [P] [US1] Crear `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusMappingTests.cs` para imagen absoluta, imagen vacía y fechas UTC, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusResponse.imageUrl`, `ChangePropertyStatusResponse.createdAt`, `ChangePropertyStatusResponse.updatedAt`, `Property.ImageUrl`, `Property.CreatedAt`, `Property.UpdatedAt`)
- [X] T009 [P] [US1] Crear `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSliceTests.cs` para ruta `PATCH`, `operationId`, `200` y representación completa, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusBody.status`, `ChangePropertyStatusResponse.status`)

### Implementación de la historia 1

- [X] T010 [US1] Implementar `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusMapping.cs`, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusResponse.imageUrl`, `ChangePropertyStatusResponse.createdAt`, `ChangePropertyStatusResponse.updatedAt`, `Property.ImageUrl`, `Property.CreatedAt`, `Property.UpdatedAt`)
- [X] T011 [US1] Implementar la ruta de cambio en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandler.cs`, según `specs/009-update-status/data-model.md` (`Property.Id`, `Property.Status`, `Property.UpdatedAt`, `Property.Title`, `Property.Description`, `Property.Address`, `Property.Price`, `Property.BedroomCount`, `Property.BathroomCount`, `Property.AreaSquareMeters`, `Property.ImageUrl`, `Property.CreatedAt`)
- [X] T012 [US1] Implementar `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSlice.cs` con `MapPatch`, nombre OpenAPI, etiqueta, `Accepts`, `Produces` y mapeo de errores, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusRequest.Body`, `ChangePropertyStatusResponse.status`)
- [X] T013 [US1] Añadir el ejemplo manual de cambio correcto a `app/backend/src/RealtorApi/RealtorApi.http`
- [X] T014 [P] [US1] Añadir una comprobación de paridad de representación pública en `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusMappingTests.cs`, comparando los doce campos y las reglas de imagen y UTC con `GetPropertyByIdResponse`, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusResponse.id`, `ChangePropertyStatusResponse.title`, `ChangePropertyStatusResponse.description`, `ChangePropertyStatusResponse.address`, `ChangePropertyStatusResponse.price`, `ChangePropertyStatusResponse.status`, `ChangePropertyStatusResponse.bedroomCount`, `ChangePropertyStatusResponse.bathroomCount`, `ChangePropertyStatusResponse.areaSquareMeters`, `ChangePropertyStatusResponse.imageUrl`, `ChangePropertyStatusResponse.createdAt`, `ChangePropertyStatusResponse.updatedAt`)
- [X] T015 [P] [US1] Añadir casos de concurrencia de última escritura gana a `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandlerTests.cs`, verificando que no aparece versión ni `409` y que el estado final corresponde a la última escritura confirmada, según `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)
- [X] T016 [P] [US1] Añadir un caso de fallo interno a `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSliceTests.cs`, verificando `500` con `ProblemDetails` genérico, sin detalles técnicos y sin cambios observables, según `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)
- [X] T017 [US1] Hacer explícita la política de última escritura gana en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandler.cs`, sin token de concurrencia ni `409`, según `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)
- [X] T018 [US1] Actualizar `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSlice.cs` para declarar `ProducesProblem(500)` y mapear fallos internos con `ProblemDetailsMapper`, según `specs/009-update-status/contracts/contrato-openapi-v1.md`
- [X] T019 [US1] Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~ChangePropertyStatus"` y verificar que las pruebas de la historia 1 pasen en `app/backend/tests/RealtorApiTests/`

**Punto de control**: La historia 1 es funcional y verificable de forma independiente; este es el MVP.

---

## Fase 4: Historia de usuario 2 - Repetir el estado vigente sin provocar errores (Prioridad: P2)

**Objetivo**: Aceptar reintentos con el mismo estado sin cambios observables ni falsos errores.

**Prueba independiente**: Enviar el estado vigente sobre una propiedad existente; verificar `200`, representación vigente e `updatedAt` inalterado.

### Pruebas de la historia 2

- [X] T020 [P] [US2] Añadir casos de idempotencia a `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandlerTests.cs`, verificando ausencia de guardado y `updatedAt` inalterado según `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)
- [X] T021 [P] [US2] Añadir un caso de estado repetido a `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSliceTests.cs`, verificando `200` e `updatedAt` inalterado según `specs/009-update-status/data-model.md` (`ChangePropertyStatusResponse.updatedAt`)

### Implementación de la historia 2

- [X] T022 [US2] Añadir la guarda de idempotencia en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusHandler.cs`, según `specs/009-update-status/data-model.md` (`Property.Status`, `Property.UpdatedAt`)
- [X] T023 [US2] Añadir el ejemplo manual de estado repetido a `app/backend/src/RealtorApi/RealtorApi.http`
- [X] T024 [US2] Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~ChangePropertyStatus"` y verificar que las historias 1 y 2 pasen en `app/backend/tests/RealtorApiTests/`

**Punto de control**: Las historias 1 y 2 funcionan de forma independiente.

---

## Fase 5: Historia de usuario 3 - Rechazar entradas inválidas sin efectos (Prioridad: P2)

**Objetivo**: Devolver `400` o `404` estructurados sin modificar datos ante identificadores o estados inválidos.

**Prueba independiente**: Enviar identificador mal formado, identificador inexistente, estado ausente, vacío o desconocido; verificar el código de error, el cuerpo estructurado y la ausencia de cambios.

### Pruebas de la historia 3

- [X] T025 [P] [US3] Crear `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusValidatorTests.cs` con casos de `Id` ausente o mal formado, `Body` ausente y `status` ausente, vacío, inválido y válido, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusRequest.Body`, `ChangePropertyStatusBody.status`)
- [X] T026 [P] [US3] Añadir casos de `400` y `404` estructurados a `app/backend/tests/RealtorApiTests/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSliceTests.cs`, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusBody.status`, `Property.Id`)

### Implementación de la historia 3

- [X] T027 [US3] Implementar `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusValidator.cs`, según `specs/009-update-status/data-model.md` (`ChangePropertyStatusRequest.Id`, `ChangePropertyStatusRequest.Body`, `ChangePropertyStatusBody.status`)
- [X] T028 [US3] Verificar en `app/backend/src/RealtorApi/Features/Properties/ChangePropertyStatus/ChangePropertyStatusSlice.cs` que los fallos usen `ProblemDetailsMapper`, que el identificador mal formado sea rechazado por el validador antes del enlace y que no se modifique `Program.cs`
- [X] T029 [US3] Añadir los ejemplos manuales de identificador mal formado, estado ausente, estado inválido y propiedad inexistente a `app/backend/src/RealtorApi/RealtorApi.http`
- [X] T030 [US3] Ejecutar `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~ChangePropertyStatus"` y verificar que las historias 1, 2 y 3 pasen en `app/backend/tests/RealtorApiTests/`

**Punto de control**: Las historias 1, 2 y 3 funcionan de forma independiente.

---

## Fase 6: Historia de usuario 4 - Mantener la coherencia del catálogo y su contrato público (Prioridad: P3)

**Objetivo**: Publicar la nueva operación en el contrato vigente sin regresiones en capacidades existentes.

**Prueba independiente**: Ejecutar las comprobaciones de salud, listado, creación, actualización, consulta por identificador y contrato público tras incorporar el cambio de estado.

### Pruebas de la historia 4

- [X] T031 [P] [US4] Actualizar `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDocumentTests.cs`, `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDriftTests.cs`, `app/backend/tests/RealtorApiTests/OpenApi/OpenApiToolingTests.cs` y `app/backend/tests/RealtorApiTests/OpenApi/OpenApiContractCliTests.cs` para 4 rutas, 6 operaciones y `ChangePropertyStatus`

### Implementación de la historia 4

- [X] T032 [US4] Actualizar los inventarios de `specs/008-open-api/contracts/get-openapi-v1.http.md` y `specs/008-open-api/contracts/openapi-v1-document.md` con la nueva ruta, el nuevo `operationId` y los conteos 4/6
- [X] T033 [US4] Verificar que los mapeos existentes en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs` y `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs` interpreten el nuevo valor de estado sin cambios de código
- [X] T034 [US4] Ejecutar `dotnet test app/Realtor.sln` y verificar que salud, listado, creación, actualización, consulta y OpenAPI conserven su resultado en `app/Realtor.sln`

**Punto de control**: Todas las historias son funcionales y verificables de forma independiente.

---

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: Cierre de contrato, documentación, calidad y verificación final.

- [X] T035 [P] Regenerar y validar el contrato OpenAPI v1 ejecutando `./support/scripts/generate-openapi-v1.sh`, versionando `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` y comprobando deriva, Redocly y NSwag según `specs/009-update-status/contracts/contrato-openapi-v1.md`
- [X] T036 [P] Ejecutar la secuencia completa de `specs/009-update-status/quickstart.md` y registrar que todos los comandos terminan correctamente
- [X] T037 [P] Revisar que `specs/009-update-status/plan.md`, `specs/009-update-status/research.md`, `specs/009-update-status/data-model.md`, `specs/009-update-status/quickstart.md`, `specs/009-update-status/contracts/` y `app/backend/src/RealtorApi/RealtorApi.http` permanezcan en español y sin contradicciones
- [X] T038 Ejecutar `dotnet test app/Realtor.sln` y `git diff --check` desde la raíz del repositorio y verificar resultado limpio

---

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1**: Sin dependencias; puede comenzar de inmediato.
- **Fase 2**: Depende de Fase 1; bloquea todas las historias.
- **Fase 3 (US1)**: Depende de Fase 2; entrega el MVP.
- **Fase 4 (US2)**: Depende de Fase 2 y de US1; puede ejecutarse en paralelo con US3 si hay capacidad.
- **Fase 5 (US3)**: Depende de Fase 2 y de US1; puede ejecutarse en paralelo con US2, pero antes del cierre de contrato.
- **Fase 6 (US4)**: Depende de que la operación, la validación y el contrato de la nueva ruta estén implementados.
- **Fase 7**: Depende de todas las historias deseadas.

Grafo de finalización de historias:

```text
Configuración -> Cimientos -> US1 (MVP) -> US2 -> US4 -> Pulido
                                      \-> US3 ->/
```

### Dependencias entre historias

- **US1 (P1)**: Sin dependencias de otras historias.
- **US2 (P2)**: Depende de US1; reutiliza el handler y la slice creados en esa historia.
- **US3 (P2)**: Depende de US1; reutiliza el tipo de solicitud y la slice creados en esa historia.
- **US2 y US3 (P2)**: Pueden ejecutarse en paralelo después de US1, pero antes de US4.
- **US4 (P3)**: Depende de US1, US2 y US3 para validar el contrato completo.

### Dentro de cada historia

- Las pruebas se escriben antes de la implementación que las satisface.
- Los tipos de solicitud y respuesta existen antes que handler, validador y slice.
- El handler y el mapeo existen antes de la validación HTTP final de la slice.
- Cada historia se cierra con su ejecución de pruebas.

### Oportunidades de paralelismo

- T002 puede ejecutarse junto con T001.
- T003, T004, T005 y T006 pueden ejecutarse en paralelo.
- Las pruebas de cada historia marcadas `[P]` pueden crearse en paralelo.
- US2 y US3 pueden avanzar en paralelo tras completar US1, con propietarios distintos.
- Las actualizaciones de pruebas OpenAPI y de contratos heredados pueden prepararse en paralelo tras la definición de la operación.

---

## Ejemplos de ejecución paralela

### Historia 1

```text
T007 Crear ChangePropertyStatusHandlerTests.cs
T008 Crear ChangePropertyStatusMappingTests.cs
T009 Crear ChangePropertyStatusSliceTests.cs
T014 Verificar paridad de representación pública
T015 Añadir casos de concurrencia al handler
T016 Añadir caso de fallo interno a la slice
```

### Historia 2

```text
T020 Añadir casos de idempotencia al handler
T021 Añadir caso de estado repetido a la slice
```

### Historia 3

```text
T025 Crear ChangePropertyStatusValidatorTests.cs
T026 Añadir casos de 400 y 404 a la slice
```

### Historia 4

```text
T031 Actualizar pruebas OpenAPI
T032 Actualizar contratos heredados de 008
```

---

## Estrategia de implementación

### Primero el MVP

1. Completar Fase 1 y Fase 2.
2. Completar Fase 3 (US1).
3. Detener y validar US1 de forma independiente.
4. Demo o despliegue del MVP si procede.

### Entrega incremental

1. Configuración y cimientos listos.
2. US1: cambio de estado y representación completa.
3. US2: idempotencia.
4. US3: errores estructurados.
5. US4: coherencia de contrato y regresiones.
6. Pulido: OpenAPI, quickstart, idioma y verificación final.

### Equipo en paralelo

1. El equipo completa juntos configuración y cimientos.
2. Un propietario toma US1 y luego US2.
3. Otro propietario prepara US3 y US4 sin solapar archivos de US1.
4. El cierre de Fase 7 consolida contrato y verificación.

---

## Notas

- `[P]` indica archivos distintos y sin dependencias incompletas.
- `[USn]` traza cada tarea a su historia de usuario.
- Cada historia tiene criterio de prueba independiente y punto de control.
- La spec `Borrador` fue revisada y aprobada antes de ejecutar `/speckit.implement`.
- No confirmar cambios salvo solicitud explícita.

## Trazabilidad de requisitos

| Clave | Tareas | Estado |
|---|---|---|
| FR-001 | T005, T009, T011, T012 | Sí |
| FR-002 | T025, T027 | Sí |
| FR-003 | T025-T028 | Sí |
| FR-004 | T025-T028 | Sí |
| FR-005 | T007, T026 | Sí |
| FR-006 | T025-T027 | Sí |
| FR-007 | T007, T008, T010, T011 | Sí |
| FR-008 | T007, T010, T011 | Sí |
| FR-009 | T020-T022 | Sí |
| FR-010 | T008, T009, T010, T012, T014 | Parcial |
| FR-011 | T025-T028 | Sí |
| FR-012 | T007, T016, T018 | Parcial |
| FR-013 | T007 | Parcial |
| FR-014 | T015, T017, T019 | Sí |
| FR-015 | T011, T033, T034 | Parcial |
| FR-016 | — | No aplicable; alcance negativo no buildable |
| FR-017 | T009, T012, T028 | Parcial |
| FR-018 | T003, T031, T032, T035 | Sí |
| FR-019 | T004 | Parcial |
| FR-020 | T008, T010, T021 | Sí |
| SC-001 | T007, T009, T011, T012, T019 | Sí |
| SC-002 | T020, T021, T022, T024, T025 | Sí |
| SC-003 | T025-T028, T030 | Sí |
| SC-004 | T007, T016, T018 | Parcial |
| SC-005 | — | Sin tarea; definir condiciones de medición |
| SC-006 | T007, T008, T010, T011 | Sí |
| SC-007 | T008, T009, T010, T014 | Parcial |
| SC-008 | T031, T033, T034 | Sí |
| SC-009 | T007 | Parcial |
| SC-010 | T015, T017, T019 | Sí |
