---

description: "Lista de tareas de implementación para la característica"
---

# Tareas: Documento OpenAPI v1 de la API pública

**Entrada**: Documentos de diseño de `/specs/008-open-api/`

**Prerrequisitos**: plan.md (obligatorio), spec.md (obligatorio para historias de usuario), research.md, data-model.md, contracts/

**Pruebas**: incluidas porque la spec aprobada las exige explícitamente en FR-002, FR-005, FR-011, FR-012, FR-013, FR-014 y FR-016, y porque la sección «Renuncia explícita al principio V» autoriza pruebas CLI y en proceso.

**Organización**: las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo (archivos distintos, sin dependencias incompletas)
- **[Story]**: historia de usuario a la que pertenece (US1, US2, US3)
- Incluir rutas de archivo exactas en las descripciones
- Incluir trazabilidad `(FR-..., SC-...)` cuando la tarea verifique un requisito aprobado

## Convenciones de rutas

- API: `app/backend/src/RealtorApi/`
- Pruebas: `app/backend/tests/RealtorApiTests/`
- Configuración de herramientas: raíz del repositorio
- Documento generado: `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`
- Cliente de humo: `artifacts/openapi-client-smoke/`

---

## Phase 1: Configuración (infraestructura compartida)

**Propósito**: preparar dependencias de compilación, configuración de herramientas y directorio de salida.

- [X] T001 Añadir `Microsoft.Extensions.ApiDescription.Server` 11.0.0 con `PrivateAssets=all`, `OpenApiGenerateDocuments=true`, `OpenApiGenerateDocumentsOnBuild=true`, `OpenApiDocumentsDirectory=wwwroot/openapi` y opciones `--document-name v1 --file-name v1 --openapi-version OpenApi3_0` en `app/backend/src/RealtorApi/RealtorApi.csproj` (FR-001, FR-003)
- [X] T002 [P] Crear `.redocly.yaml` con `extends: recommended`, `rules.security-defined: off`, `rules.operation-4xx-response: warn` y sin configuración visual, según `data-model.md` (`RedoclyConfiguration.Extends`, `RedoclyConfiguration.SecurityDefined`, `RedoclyConfiguration.Operation4xxResponse`) (FR-015, SC-007)
- [X] T003 [P] Crear `.config/dotnet-tools.json` fijando `NSwag.ConsoleCore` 14.8.0 y el comando local `nswag`, según `data-model.md` (`NswagConfiguration.ToolVersion`) (FR-014)
- [X] T004 [P] Crear `support/scripts/openapi-v1.nswag.json` leyendo el documento generado, usando `openapi2csclient` y escribiendo en `artifacts/openapi-client-smoke/`, según `data-model.md` (`NswagConfiguration.InputDocument`, `NswagConfiguration.OutputDirectory`, `NswagConfiguration.Command`) (FR-014, SC-005)

---

## Phase 2: Fundacional (prerrequisitos bloqueantes)

**Propósito**: infraestructura que DEBE completarse antes de cualquier historia de usuario.

**CRÍTICO**: ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T005 Configurar `app/backend/src/RealtorApi/Program.cs` para usar OpenAPI 3.0, eliminar `MapOpenApi()` del bloque Development, conservar `UseStaticFiles()` como única fuente de `GET /openapi/v1.json` y excluir el host de generación de `app.MigrateAsync()`, según `data-model.md` (`OpenApiDocumentV1.SpecVersion`, `OpenApiDocumentV1.PublicRoute`) (FR-004, FR-005)
- [X] T006 Crear `app/backend/src/RealtorApi/Infrastructure/OpenApi/OpenApiConstants.cs` con nombre de documento `v1`, título de RealtorApi, `info.version` exactamente `v1`, tags `Salud` y `Propiedades`, nombres de operación y nombres de esquemas compartidos, según `data-model.md` (`OpenApiDocumentV1.DocumentName`, `OpenApiDocumentV1.InfoTitle`, `OpenApiDocumentV1.InfoVersion`) (FR-001, FR-022, SC-011)
- [X] T007 Crear `app/backend/src/RealtorApi/Infrastructure/OpenApi/OpenApiDocumentTransformer.cs` para fijar metadatos de documento, excluir `/openapi/v1.json` de `paths`, exponer las cinco operaciones, documentar `page` y `pageSize` en `GetProperties`, documentar `500` en `GetHealth` y canonicalizar salida, según `data-model.md` (`OpenApiDocumentV1.OutputPath`, `OpenApiDocumentV1.Paths`, `OpenApiDocumentV1.Schemas`, `OpenApiDocumentV1.PublicRoute`, `OpenApiDocumentV1.InfoTitle`, `OpenApiDocumentV1.InfoVersion`, `DocumentedEndpoint.QueryParameters`, `DocumentedEndpoint.ErrorResponses`) (FR-006, FR-007, FR-008, FR-009)
- [X] T008 [P] Crear `app/backend/src/RealtorApi/Infrastructure/OpenApi/OpenApiSchemaCatalog.cs` registrando `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` en `components.schemas` y sustituyendo duplicados inline por referencias `$ref`, según `data-model.md` (`SharedSchema.Name`, `SharedSchema.Shape`, `SharedSchema.References`, `OpenApiDocumentV1.Schemas`) (FR-008, FR-009, SC-008)
- [X] T009 [P] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiTestApp.cs` con una factoría en proceso que registre slices y handlers solo desde la asamblea de `RealtorApi` para excluir `TestSlice` de inventarios OpenAPI (FR-011)

**Punto de control**: base lista; las historias de usuario pueden comenzar.

---

## Phase 3: Historia de usuario 1 - Obtener y consumir el contrato v1 (Prioridad: P1), entrega mínima

**Objetivo**: publicar un documento OpenAPI v1 válido, estable y consumible desde `GET /openapi/v1.json`, sin correcciones manuales.

**Prueba independiente**: solicitar `GET /openapi/v1.json`, verificar `200 OK` y `Content-Type application/json`, validar con Redocly, generar un cliente tipado con NSwag y repetir la generación byte-idéntica sin editar el documento.

### Pruebas de la historia de usuario 1

- [X] T010 [US1] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDocumentTests.cs` verificando versión 3.0, `info.version` exactamente `v1`, cinco paths y operaciones, ausencia de `/openapi/v1.json`, normalización `{id}`, `page` y `pageSize` en `GetProperties`, `500` en `GetHealth` y los tres esquemas compartidos referenciados con `$ref` (FR-001, FR-006, FR-007, FR-008, FR-009, FR-010, FR-022, SC-001, SC-008, SC-011)
- [X] T011 [US1] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiHttpContractTests.cs` verificando que `GET /openapi/v1.json` sirve el archivo estático con `200 OK` y `Content-Type application/json` usando la factoría de `app/backend/tests/RealtorApiTests/OpenApi/OpenApiTestApp.cs` (FR-004, FR-005, SC-002)

### Implementación de la historia de usuario 1

- [X] T012 [P] [US1] Añadir metadatos OpenAPI a `app/backend/src/RealtorApi/Features/Health/HealthSlice.cs` con método `GET`, ruta `/health`, `operationId` `GetHealth`, tag `Salud`, respuesta 200 y error 500 referenciado con `$ref`, según `data-model.md` (`DocumentedEndpoint.Method`, `DocumentedEndpoint.RouteTemplate`, `DocumentedEndpoint.OperationId`, `DocumentedEndpoint.Tag`, `DocumentedEndpoint.SuccessResponse`, `DocumentedEndpoint.ErrorResponses`) (FR-006, FR-007, FR-008)
- [X] T013 [P] [US1] Añadir metadatos OpenAPI a `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs` con método `GET`, ruta `/api/properties`, `operationId` `GetProperties`, tag `Propiedades`, parámetros de consulta `page` y `pageSize`, respuesta 200 y error 500 referenciado con `$ref`, según `data-model.md` (`DocumentedEndpoint.Method`, `DocumentedEndpoint.RouteTemplate`, `DocumentedEndpoint.QueryParameters`, `DocumentedEndpoint.OperationId`, `DocumentedEndpoint.Tag`, `DocumentedEndpoint.SuccessResponse`, `DocumentedEndpoint.ErrorResponses`) (FR-006, FR-007, FR-008)
- [X] T014 [P] [US1] Añadir metadatos OpenAPI a `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdSlice.cs` con método `GET`, ruta `/api/properties/{id}`, `operationId` `GetPropertyById`, tag `Propiedades`, ruta publicada `{id}`, respuesta 200 y error 404 `ProblemDetails`, según `data-model.md` (`DocumentedEndpoint.Method`, `DocumentedEndpoint.RouteTemplate`, `DocumentedEndpoint.OperationId`, `DocumentedEndpoint.Tag`, `DocumentedEndpoint.SuccessResponse`, `DocumentedEndpoint.ErrorResponses`, `RouteNormalizationRule.AppliesTo`) (FR-006, FR-007, FR-008, FR-010)
- [X] T015 [P] [US1] Añadir metadatos OpenAPI a `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs` con método `POST`, ruta `/api/properties`, `operationId` `CreateProperty`, tag `Propiedades`, cuerpo de entrada declarado, respuesta 201 y errores 400/409 referenciados con `$ref`, según `data-model.md` (`DocumentedEndpoint.Method`, `DocumentedEndpoint.RouteTemplate`, `DocumentedEndpoint.OperationId`, `DocumentedEndpoint.Tag`, `DocumentedEndpoint.RequestSchema`, `DocumentedEndpoint.SuccessResponse`, `DocumentedEndpoint.ErrorResponses`) (FR-006, FR-007, FR-008)
- [X] T016 [P] [US1] Añadir metadatos OpenAPI a `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs` con método `PUT`, ruta `/api/properties/{id}`, `operationId` `UpdateProperty`, tag `Propiedades`, ruta publicada `{id}`, cuerpo de entrada declarado, respuesta 200 y errores 400/404/409 referenciados con `$ref`, según `data-model.md` (`DocumentedEndpoint.Method`, `DocumentedEndpoint.RouteTemplate`, `DocumentedEndpoint.OperationId`, `DocumentedEndpoint.Tag`, `DocumentedEndpoint.RequestSchema`, `DocumentedEndpoint.SuccessResponse`, `DocumentedEndpoint.ErrorResponses`, `RouteNormalizationRule.AppliesTo`) (FR-006, FR-007, FR-008, FR-010)
- [X] T017 [US1] Generar `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` ejecutando `dotnet build app/Realtor.sln` y verificar que el archivo existe, no está vacío, declara `info.version` `v1` y contiene exactamente los cinco paths esperados, según `data-model.md` (`OpenApiDocumentV1.OutputPath`, `OpenApiDocumentV1.Paths`, `OpenApiDocumentV1.InfoVersion`) (FR-001, FR-002, FR-003, FR-006, SC-001, SC-011)
- [X] T018 [US1] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiToolingTests.cs` ejecutando `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json` y `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json` contra `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, sin correcciones manuales del contrato (FR-013, FR-014, FR-015, SC-003, SC-005, SC-007)
- [X] T019 [US1] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDeterminismTests.cs` regenerando `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` dos veces con código, configuración y dependencias idénticos y verificando contenido byte-idéntico y ausencia de edición manual (FR-002)

**Punto de control**: US1 es funcional y validable de forma independiente.

---

## Phase 4: Historia de usuario 2 - Detectar desalineación entre implementación y documento (Prioridad: P2)

**Objetivo**: fallar automáticamente cuando el documento deje de coincidir con los endpoints reales.

**Prueba independiente**: inducir una desalineación controlada, verificar que la comprobación falla, restaurar el estado y verificar que pasa; además, comprobar que `{id:guid}` y `{id}` se tratan como equivalentes en el documento y en el inventario.

### Pruebas de la historia de usuario 2

- [X] T020 [P] [US2] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDriftTests.cs` con casos para inventario alineado, endpoint nuevo, endpoint renombrado, endpoint eliminado y equivalencia de constraint `{id:guid}` frente a `{id}` (FR-010, FR-011, FR-012, SC-004)

### Implementación de la historia de usuario 2

- [X] T021 [US2] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiEndpointInventory.cs` para leer `EndpointDataSource`, parsear `RoutePattern`, normalizar constraints del enrutador y devolver pares método/ruta, aplicando la misma normalización a los paths de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, según `data-model.md` (`RouteNormalizationRule.Source`, `RouteNormalizationRule.Normalized`, `RouteNormalizationRule.AppliesTo`) (FR-010, FR-011)
- [X] T022 [US2] Conectar `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDriftTests.cs` con `app/backend/tests/RealtorApiTests/OpenApi/OpenApiTestApp.cs`, `app/backend/tests/RealtorApiTests/OpenApi/OpenApiEndpointInventory.cs` y `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, excluyendo explícitamente `GET /openapi/v1.json` y `TestSlice`, y haciendo la comparación insensible al orden (FR-006, FR-011, FR-012, SC-001, SC-004)
- [X] T023 [US2] Añadir en `app/backend/tests/RealtorApiTests/OpenApi/OpenApiDriftTests.cs` una comprobación de restauración que garantice que, tras inducir y revertir la desalineación, la verificación vuelve a pasar (FR-012, SC-004)

**Punto de control**: US1 y US2 funcionan de forma independiente.

---

## Phase 5: Historia de usuario 3 - Regenerar y validar de forma reproducible (Prioridad: P3)

**Objetivo**: ofrecer un único comando que regenere, valide y reporte el documento OpenAPI v1.

**Prueba independiente**: ejecutar `support/scripts/generate-openapi-v1.sh` desde una máquina preparada y verificar que genera `v1.json`, valida con Redocly, genera el cliente NSwag y termina con reporte de éxito; provocar fallos controlados y verificar salida `[ERROR]` y código distinto de cero.

### Pruebas de la historia de usuario 3

- [X] T024 [US3] Crear `app/backend/tests/RealtorApiTests/OpenApi/OpenApiScriptTests.cs` con una copia temporal de la estructura mínima del repositorio y casos para ejecución correcta, documento ausente, `.redocly.yaml` inválido, manifiesto NSwag inválido y artefacto visual inesperado, verificando mensajes `[ERROR]` y código distinto de cero (FR-016, FR-017, FR-018, FR-019, SC-006)

### Implementación de la historia de usuario 3

- [X] T025 [US3] Completar `support/scripts/generate-openapi-v1.sh` con secuencia fail-fast `dotnet build app/Realtor.sln` → verificación de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` → `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json` → `dotnet tool restore` → `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json` → verificación de artefactos visuales → reporte, ejecutándolo desde la raíz del repositorio, según `data-model.md` (`RegenerationScript.Path`, `RegenerationScript.WorkingDirectory`, `RegenerationScript.Phases`, `RegenerationScript.UiArtifacts`) (FR-016, FR-017, FR-020, SC-006)
- [X] T026 [US3] Añadir en `support/scripts/generate-openapi-v1.sh` mensajes `[OK]`/`[ERROR]` por fase, ruta final del documento y código de salida distinto de cero ante cualquier fallo, coincidiendo con `specs/008-open-api/contracts/generate-openapi-v1.cli.md`, según `data-model.md` (`RegenerationScript.ExitCode`) (FR-017, FR-018, SC-006)
- [X] T027 [US3] Integrar `support/scripts/generate-openapi-v1.sh` con `.redocly.yaml`, `.config/dotnet-tools.json` y `support/scripts/openapi-v1.nswag.json`, verificando que las rutas y los comandos coinciden con `specs/008-open-api/contracts/generate-openapi-v1.cli.md` y que `artifacts/openapi-client-smoke/` solo contiene el cliente generado (FR-013, FR-014, FR-015, FR-019, FR-020, SC-003, SC-005, SC-007)
- [X] T028 [US3] Ejecutar `support/scripts/generate-openapi-v1.sh`, confirmar que `artifacts/openapi-client-smoke/` se crea fuera del control de versiones, que no hay artefactos visuales y que el reporte final incluye `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` (FR-016, FR-017, FR-019, SC-006)

**Punto de control**: las tres historias son funcionales y validables de forma independiente.

---

## Phase 6: Pulido y preocupaciones transversales

**Propósito**: verificación final, evidencia y ausencia de regresiones.

- [X] T029 [P] Verificar en `app/backend/src/RealtorApi/Program.cs`, `.redocly.yaml`, `support/scripts/generate-openapi-v1.sh` y `specs/008-open-api/quickstart.md` que no existen Swagger UI, ReDoc UI, endpoints de exploración interactiva ni configuración visual (FR-019, FR-020, SC-009)
- [X] T030 Actualizar `specs/008-open-api/quickstart.md` con fecha, salida resumida de `support/scripts/generate-openapi-v1.sh`, resultados de pruebas OpenAPI, suite completa y salida de la verificación de `GET /openapi/v1.json`
- [X] T031 Ejecutar `dotnet build app/Realtor.sln` y `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`, confirmando que las verificaciones existentes de salud, listado, creación, actualización y consulta por identificador continúan pasando (FR-021, SC-010)
- [X] T032 Verificar con `git status --short` que `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` solo cambió por regeneración, que no se editó manualmente y que `artifacts/openapi-client-smoke/` no aparece como artefacto versionado (FR-002, FR-003)
- [X] T033 Revisar consistencia final entre `specs/008-open-api/spec.md`, `specs/008-open-api/plan.md`, `specs/008-open-api/research.md`, `specs/008-open-api/data-model.md`, `specs/008-open-api/contracts/openapi-v1-document.md`, `specs/008-open-api/contracts/get-openapi-v1.http.md`, `specs/008-open-api/contracts/generate-openapi-v1.cli.md` y `specs/008-open-api/quickstart.md`, confirmando trazabilidad FR-001–FR-022 y SC-001–SC-011, comandos canónicos y `info.version` exactamente `v1` (FR-001, FR-022, SC-011)

---

## Dependencias y orden de ejecución

### Dependencias de fases

- **Phase 1 (Configuración)**: sin dependencias; puede comenzar de inmediato.
- **Phase 2 (Fundacional)**: depende de Phase 1; bloquea todas las historias de usuario.
- **Phase 3 (US1)**: depende de Phase 2; produce el documento que consumen US2 y US3.
- **Phase 4 (US2)**: depende de Phase 2 y del documento generado en T017; T020 puede prepararse antes de T017.
- **Phase 5 (US3)**: depende de Phase 2 y del documento generado en T017; T024 puede prepararse antes de T025.
- **Phase 6 (Pulido)**: depende de las historias incluidas en el alcance que se vayan a entregar.

### Dependencias entre historias

- **US1 (P1)**: comienza tras Phase 2; no depende de US2 ni US3.
- **US2 (P2)**: comienza tras Phase 2; depende del documento generado por T017 para comparar contra el artefacto real.
- **US3 (P3)**: comienza tras Phase 2; depende de la configuración de Redocly/NSwag de Phase 1 y del documento de T017.

### Dentro de cada historia

- Las pruebas de estructura y contrato HTTP se escriben antes de la implementación correspondiente.
- Las pruebas que consumen el documento generado o ejecutan herramientas externas se sitúan después de T017.
- La infraestructura de prueba debe existir antes de las pruebas de deriva y CLI.
- Los metadatos de slices deben existir antes de generar el documento final.
- El documento final debe existir antes de las verificaciones de deriva, herramientas y determinismo.
- El script debe completarse antes de la validación reproducible final.

### Oportunidades de paralelo

- Phase 1: T002, T003 y T004 pueden ejecutarse en paralelo.
- Phase 2: T008 y T009 pueden ejecutarse en paralelo; T005, T006 y T007 deben secuenciarse por dependencia de configuración.
- US1: T012-T016 pueden ejecutarse en paralelo; T017 depende de T005-T016; T018 y T019 dependen de T017.
- US2: T020 puede ejecutarse en paralelo con la preparación de US1; T021 depende de T009; T022 depende de T017 y T021; T023 depende de T022.
- US3: T024 puede ejecutarse en paralelo con T020; T025-T028 deben secuenciarse.
- Phase 6: T029 puede ejecutarse en paralelo con la preparación de evidencia; T031 depende de T017, T023 y T028.

## Ejemplo de paralelo: US1

```text
Task: T012 Añadir metadatos en HealthSlice.cs
Task: T013 Añadir metadatos en ListPropertiesSlice.cs
Task: T014 Añadir metadatos en GetPropertyByIdSlice.cs
Task: T015 Añadir metadatos en CreatePropertySlice.cs
Task: T016 Añadir metadatos en UpdatePropertySlice.cs
```

## Ejemplo de paralelo: US2

```text
Task: T020 Crear OpenApiDriftTests.cs
```

## Ejemplo de paralelo: US3

```text
Task: T024 Crear OpenApiScriptTests.cs
```

## Estrategia de implementación

### Entrega mínima primero (solo US1)

1. Completar Phase 1: Configuración.
2. Completar Phase 2: Fundacional.
3. Completar Phase 3: US1.
4. Detener y validar: `GET /openapi/v1.json`, Redocly, cliente NSwag, regeneración byte-idéntica y cinco paths documentados.
5. Demostrar el contrato v1 publicado.

### Entrega incremental

1. Configuración + Fundacional → base lista.
2. US1 → documento publicado y consumible → entrega mínima.
3. US2 → barrera automática de deriva → demostración de confianza del contrato.
4. US3 → regeneración reproducible → demostración de pipelines locales.
5. Cada historia aporta valor sin romper la anterior.

### Estrategia con equipo paralelo

1. El equipo completa Phase 1 y Phase 2 juntas.
2. Desarrollador A: US1.
3. Desarrollador B: prepara T020 y T021 de US2; ejecuta T022-T023 cuando T017 esté listo.
4. Desarrollador C: prepara T024 de US3; ejecuta T025-T028 cuando T017 esté listo.
5. Integrar en Phase 6.

## Notas

- `[P]` indica archivos distintos y sin dependencias incompletas.
- `[Story]` marca la trazabilidad hacia la historia aprobada en `spec.md`.
- Cada historia debe poder completarse y probarse de forma independiente.
- No editar manualmente `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
- No crear Swagger UI, ReDoc UI ni herramientas visuales en la aplicación en ejecución.
- La renuncia al principio V está aprobada en `spec.md` y debe permanecer reflejada en `plan.md` y en este archivo.
- Confirmar que todas las citas a campos restringidos usan `data-model.md` y el nombre exacto `Entidad.campo`.
- No hacer commit salvo solicitud explícita del usuario.
