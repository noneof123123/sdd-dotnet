---
description: "Lista de tareas para la implementación de consulta de propiedad por identificador"
---

# Tareas: Consulta de propiedad por identificador

**Entrada**: Documentos de diseño desde `/specs/007-properties-get-by-id/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/backend-properties-get-by-id.openapi.yaml`, `contracts/get-property-by-id.http.md`, `quickstart.md` y `.specify/memory/constitution.md`

**Pruebas**: Se incluyen pruebas unitarias y unitarias del slice en memoria porque la especificación define una prueba independiente por historia de usuario y SC-006 exige conservar las regresiones de listado, creación, actualización, persistencia y salud.

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo porque usa archivos distintos y no depende de tareas incompletas.
- **[Story]**: identifica la historia de usuario únicamente en las fases de usuario.
- Todas las descripciones incluyen rutas de archivo exactas.
- Los campos restringidos se citan como `data-model.md` (`Entidad.campo`) en las tareas que los materializan.

## Convenciones de rutas

- Código fuente: `app/backend/src/RealtorApi/`.
- Pruebas: `app/backend/tests/RealtorApiTests/`.
- Caso de uso: `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/`.
- Contratos: `specs/007-properties-get-by-id/contracts/`.

## Fase 1: Configuración

**Propósito**: Preparar la estructura específica del caso de uso sin crear soluciones paralelas.

- [X] T001 Crear los directorios `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/` y `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/` siguiendo `specs/007-properties-get-by-id/plan.md`.
- [X] T002 [P] Verificar en `app/backend/src/RealtorApi/Program.cs` que `RegisterSlices`, `RegisterHandlers`, `AddValidatorsFromAssembly` y `AddHttpContextAccessor` cubren el descubrimiento automático y la validación dependiente de la solicitud, sin agregar registros manuales de slice, handler ni validator.
- [X] T003 [P] Verificar en `app/backend/src/RealtorApi/Program.cs` que `UseStaticFiles` mantiene disponible `/assets/properties` y que no se requiere nueva configuración de almacenamiento ni de host para la operación de solo lectura.

**Punto de control**: La estructura existe y reutiliza la infraestructura de descubrimiento, validación y activos públicos actual.

## Fase 2: Fundacional

**Propósito**: Completar contratos, validación y comprobaciones compartidas que bloquean todas las historias de usuario.

**CRÍTICA**: Ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T004 [P] Definir `GetPropertyByIdRequest` con el identificador de ruta, aplicando `data-model.md` (`GetPropertyByIdRequest.Id`) en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdRequest.cs`.
- [X] T005 [P] Definir `GetPropertyByIdResponse` como representación pública separada de la entidad, aplicando `data-model.md` (`GetPropertyByIdResponse.id`), `data-model.md` (`GetPropertyByIdResponse.title`), `data-model.md` (`GetPropertyByIdResponse.description`), `data-model.md` (`GetPropertyByIdResponse.address`), `data-model.md` (`GetPropertyByIdResponse.price`), `data-model.md` (`GetPropertyByIdResponse.status`), `data-model.md` (`GetPropertyByIdResponse.bedroomCount`), `data-model.md` (`GetPropertyByIdResponse.bathroomCount`), `data-model.md` (`GetPropertyByIdResponse.areaSquareMeters`), `data-model.md` (`GetPropertyByIdResponse.imageUrl`), `data-model.md` (`GetPropertyByIdResponse.createdAt`) y `data-model.md` (`GetPropertyByIdResponse.updatedAt`) en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdResponse.cs`.
- [X] T006 [P] Confirmar en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`, `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs` y `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/` que no se requiere migración, conservando las definiciones canónicas de `data-model.md` (`Property.Id`), `data-model.md` (`Property.Title`), `data-model.md` (`Property.Description`), `data-model.md` (`Property.Address`), `data-model.md` (`Property.Price`), `data-model.md` (`Property.Status`), `data-model.md` (`Property.BedroomCount`), `data-model.md` (`Property.BathroomCount`), `data-model.md` (`Property.AreaSquareMeters`), `data-model.md` (`Property.ImageUrl`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.UpdatedAt`).
- [X] T007 [P] Verificar en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs` y `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMapperTests.cs` que la validación produce HTTP 400 y que `Error.NotFound` y `Error.Internal` se traducen a HTTP 404 y 500 genérico sin detalles técnicos, ajustando solo lo que falte.
- [X] T008 [P] Añadir un evento de logging estructurado con el identificador de la propiedad como parámetro, sin rutas ni datos sensibles, en `app/backend/src/RealtorApi/Infrastructure/Logging/InfrastructureLog.cs`, siguiendo el patrón `[LoggerMessage]` existente.
- [X] T009 Implementar `GetPropertyByIdValidator` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdValidator.cs`, aplicando la regla canónica de `data-model.md` (`GetPropertyByIdRequest.Id`) mediante FluentValidation; debe completarse antes de iniciar US1.

**Punto de control**: Los contratos, el validator, la configuración persistente, el mapeo de errores y el logging están listos; las historias pueden comenzar.

## Fase 3: Historia de usuario 1 - Obtener el detalle de una propiedad existente (Prioridad: P1) — MVP

**Objetivo**: Solicitar una propiedad concreta por su identificador y recibir todos sus datos públicos, con o sin imagen, sin alterar ningún dato.

**Prueba independiente**: Enviar `GET /api/properties/{id}` sobre una propiedad existente con imagen y otra sin imagen; verificar HTTP 200, los diez campos del listado más marcas temporales, `imageUrl` absoluta o cadena vacía según corresponda, y ausencia de cambios en la propiedad, sus archivos y sus marcas.

**Prueba independiente**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~GetPropertyById"`.

### Pruebas para la historia de usuario 1

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T010 [P] [US1] Crear pruebas de mapping que comprueben la proyección de `data-model.md` (`GetPropertyByIdResponse.id`), `data-model.md` (`GetPropertyByIdResponse.title`), `data-model.md` (`GetPropertyByIdResponse.description`), `data-model.md` (`GetPropertyByIdResponse.address`), `data-model.md` (`GetPropertyByIdResponse.price`), `data-model.md` (`GetPropertyByIdResponse.status`), `data-model.md` (`GetPropertyByIdResponse.bedroomCount`), `data-model.md` (`GetPropertyByIdResponse.bathroomCount`), `data-model.md` (`GetPropertyByIdResponse.areaSquareMeters`), `data-model.md` (`GetPropertyByIdResponse.imageUrl`), `data-model.md` (`GetPropertyByIdResponse.createdAt`) y `data-model.md` (`GetPropertyByIdResponse.updatedAt`), incluyendo `status` como texto, URL absoluta, cadena vacía, marcas UTC y ausencia de entidad, rutas físicas y nombres originales, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs`.
- [X] T011 [P] [US1] Crear pruebas del handler que verifiquen lectura por `data-model.md` (`Property.Id`), resultado correcto, ausencia de escritura, conservación de `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.UpdatedAt`), propagación de cancelación y ausencia de operaciones de almacenamiento, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs`.
- [X] T012 [P] [US1] Crear pruebas del slice que comprueben `ISlice`, `GET /api/properties/{id}`, enlace de `data-model.md` (`GetPropertyByIdRequest.Id`), HTTP 200 con cuerpo y delegación al handler sin lógica propia, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdSliceTests.cs`.

### Implementación para la historia de usuario 1

- [X] T013 [P] [US1] Implementar `GetPropertyByIdMapping.ToResponse` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs`, proyectando `data-model.md` (`GetPropertyByIdResponse.id`), `data-model.md` (`GetPropertyByIdResponse.title`), `data-model.md` (`GetPropertyByIdResponse.description`), `data-model.md` (`GetPropertyByIdResponse.address`), `data-model.md` (`GetPropertyByIdResponse.price`), `data-model.md` (`GetPropertyByIdResponse.status`), `data-model.md` (`GetPropertyByIdResponse.bedroomCount`), `data-model.md` (`GetPropertyByIdResponse.bathroomCount`), `data-model.md` (`GetPropertyByIdResponse.areaSquareMeters`), `data-model.md` (`GetPropertyByIdResponse.imageUrl`), `data-model.md` (`GetPropertyByIdResponse.createdAt`) y `data-model.md` (`GetPropertyByIdResponse.updatedAt`) desde `data-model.md` (`Property.ImageUrl`), con normalización UTC y sin validar, persistir ni escribir archivos.
- [X] T014 [US1] Implementar `GetPropertyByIdHandler.HandleAsync` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdHandler.cs`, consultando `data-model.md` (`Property.Id`) con solo lectura, propagando cancelación, devolviendo `Result<GetPropertyByIdResponse>` o `Error.NotFound`, registrando el evento de logging y conservando `data-model.md` (`Property.CreatedAt`), `data-model.md` (`Property.UpdatedAt`) y `data-model.md` (`Property.ImageUrl`) sin modificaciones.
- [X] T015 [US1] Implementar `GetPropertyByIdSlice.AddEndpoint` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdSlice.cs`, enlazando `[AsParameters] GetPropertyByIdRequest`, inyectando `GetPropertyByIdHandler`, propagando `CancellationToken`, delegando la lógica al handler y devolviendo `Results.Ok(response)` cuando el resultado es exitoso o `ProblemDetailsMapper.ToProblemDetails(result)` cuando falla, sin validación propia de `data-model.md` (`GetPropertyByIdRequest.Id`).

**Punto de control**: US1 cumple el MVP con validación disponible desde la Fase 2 y sin depender de las historias posteriores.

## Fase 4: Historia de usuario 2 - Obtener un resultado coherente con el catálogo (Prioridad: P2)

**Objetivo**: Garantizar que la consulta por identificador use la misma interpretación de datos, estados e imágenes que el listado existente.

**Prueba independiente**: Mapear la misma propiedad con `ListPropertiesMapping` y `GetPropertyByIdMapping` ante el mismo request, comparando los diez campos compartidos; después, consultar el mismo registro con ambos handlers y comprobar que los valores coinciden sin escrituras.

### Pruebas para la historia de usuario 2

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T016 [P] [US2] Crear pruebas de equivalencia entre `ListPropertiesMapping` y `GetPropertyByIdMapping` para `data-model.md` (`GetPropertyByIdResponse.id`), `data-model.md` (`GetPropertyByIdResponse.title`), `data-model.md` (`GetPropertyByIdResponse.description`), `data-model.md` (`GetPropertyByIdResponse.address`), `data-model.md` (`GetPropertyByIdResponse.price`), `data-model.md` (`GetPropertyByIdResponse.status`), `data-model.md` (`GetPropertyByIdResponse.bedroomCount`), `data-model.md` (`GetPropertyByIdResponse.bathroomCount`), `data-model.md` (`GetPropertyByIdResponse.areaSquareMeters`) y `data-model.md` (`GetPropertyByIdResponse.imageUrl`), usando valores de `data-model.md` (`Property.ImageUrl`) nulos y relativos, incluyendo rutas relativas limpias y nombres de archivo seguros, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs`.
- [X] T017 [P] [US2] Crear pruebas de componente en memoria que consulten el mismo registro con `ListPropertiesHandler` y `GetPropertyByIdHandler`, verificando coincidencia de los campos compartidos y ausencia de cambios en `data-model.md` (`Property.Id`), `data-model.md` (`Property.CreatedAt`), `data-model.md` (`Property.UpdatedAt`) y `data-model.md` (`Property.ImageUrl`), en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs`.

### Implementación para la historia de usuario 2

- [X] T018 [US2] Ajustar `GetPropertyByIdMapping` en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs` para garantizar que `data-model.md` (`GetPropertyByIdResponse.imageUrl`) use exactamente el algoritmo del listado desde `data-model.md` (`Property.ImageUrl`), sin modificar `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`.
- [X] T019 [US2] Confirmar en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdResponse.cs` y `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs` que `data-model.md` (`GetPropertyByIdResponse.createdAt`) y `data-model.md` (`GetPropertyByIdResponse.updatedAt`) conservan UTC y nulo cuando corresponde, y que la respuesta no incluye metadatos de paginación.

**Punto de control**: US2 ofrece coherencia con el listado sin alterar el contrato de las capacidades previas.

## Fase 5: Historia de usuario 3 - Recibir errores claros y sin efectos (Prioridad: P3)

**Objetivo**: Devolver errores comprensibles para identificadores inexistentes o inválidos y para fallos internos, sin efectos observables.

**Prueba independiente**: Enviar identificadores mal formados, vacíos e inexistente, además de provocar un fallo interno controlado y una cancelación; verificar HTTP 400, 404 o 500 genérico según corresponda y que la propiedad, sus marcas y sus archivos permanezcan sin cambios.

### Pruebas para la historia de usuario 3

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T020 [P] [US3] Crear pruebas del validator para `Guid.Empty` y valores no válidos de `data-model.md` (`GetPropertyByIdRequest.Id`), documentando que un path sin segmento `id` no alcanza el endpoint y queda resuelto por el routing, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdValidatorTests.cs`.
- [X] T021 [P] [US3] Crear pruebas del slice para HTTP 400 con errores estructurados, HTTP 404 con `ProblemDetails`, HTTP 500 con mensaje genérico, cancelación y ausencia de detalles técnicos, en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdSliceTests.cs`.
- [X] T022 [P] [US3] Crear pruebas de solo lectura que verifiquen que, tras cualquier error o cancelación, permanecen sin cambios `data-model.md` (`Property.Id`), `data-model.md` (`Property.Title`), `data-model.md` (`Property.Description`), `data-model.md` (`Property.Address`), `data-model.md` (`Property.Price`), `data-model.md` (`Property.Status`), `data-model.md` (`Property.BedroomCount`), `data-model.md` (`Property.BathroomCount`), `data-model.md` (`Property.AreaSquareMeters`), `data-model.md` (`Property.ImageUrl`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.UpdatedAt`), en `app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs`.

### Verificación para la historia de usuario 3

- [X] T023 [US3] Confirmar en `app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdSlice.cs` y `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs` que el slice no contiene comprobaciones de validación ni manipulación de errores y que los fallos se traducen a 400, 404 o 500 sin introducir 413, 415 ni 409.

**Punto de control**: US3 bloquea entradas inválidas y fallos sin producir efectos observables ni información sensible.

## Fase 6: Pulido y preocupaciones transversales

**Propósito**: Completar ejemplos HTTP, validación real y revisión de gobernanza.

- [X] T024 [P] Añadir las solicitudes `GET /api/properties/{id}` con imagen, sin imagen, con identificador inexistente y con identificador mal formado en `app/backend/src/RealtorApi/RealtorApi.http`, según `specs/007-properties-get-by-id/contracts/get-property-by-id.http.md`.
- [X] T025 Ejecutar `dotnet build app/Realtor.sln` y `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` desde la raíz del repositorio, corrigiendo cualquier fallo en los archivos implicados por `specs/007-properties-get-by-id/quickstart.md`.
- [X] T026 Registrar en `specs/007-properties-get-by-id/quickstart.md` la evidencia real de ejecución con fecha, comandos y resultados, sin declarar supuestos como validaciones ejecutadas.
- [X] T027 Revisar el diff completo y confirmar en `app/backend/src/RealtorApi/` que no hay controllers, autorización, escritura, eliminación, paginación, migración, lógica de negocio en `GetPropertyByIdSlice.cs` o `GetPropertyByIdMapping.cs`, entidades EF en respuestas ni rutas físicas expuestas; verificar que `specs/007-properties-get-by-id/contracts/backend-properties-get-by-id.openapi.yaml` coincide con `specs/007-properties-get-by-id/contracts/get-property-by-id.http.md` y que `specs/007-properties-get-by-id/spec.md` conserva su estado sin transiciones manuales.

**Punto de control**: La implementación está validada y lista para decisión de cierre conforme a la constitución.

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1**: sin dependencias.
- **Fase 2**: depende de Fase 1 y bloquea todas las historias de usuario.
- **Fases 3-5**: dependen de Fase 2. Se ejecutan en orden US1 → US2 → US3 porque comparten `GetPropertyByIdMapping.cs`, `GetPropertyByIdHandler.cs`, `GetPropertyByIdSlice.cs` y sus pruebas; `GetPropertyByIdValidator.cs` se implementa en Fase 2.
- **Fase 6**: depende de las historias que se vayan a entregar.

### Dependencias entre historias

- **US1 (P1)**: constituye el MVP y no depende de otras historias.
- **US2 (P2)**: depende del mapping y del handler de US1 para comparar con el listado; no modifica la feature 004.
- **US3 (P3)**: depende de T009, del slice y del handler; verifica rechazos y solo lectura sin introducir validación propia en el slice.

### Dentro de cada historia

1. Escribir primero las pruebas y verificar que fallan.
2. Implementar el mapping antes del handler y el handler antes del slice.
3. Mantener la validación concentrada en el validator de la Fase 2.
4. Confirmar el punto de control de la historia antes de pasar a la siguiente.

## Oportunidades de ejecución paralela

- T002 y T003 pueden ejecutarse juntos tras T001.
- T004, T005, T006, T007 y T008 pueden ejecutarse juntos al completar Fase 1; T009 debe ejecutarse después de T004 y antes del punto de control de Fase 2.
- T010, T011 y T012 pueden ejecutarse juntos al iniciar US1.
- T013 puede prepararse en paralelo, pero T014 depende del mapping y T015 depende del handler.
- T016 y T017 pueden ejecutarse juntos al iniciar US2; T018 depende de T013 y T016; T019 depende de T005 y T018.
- T020, T021 y T022 pueden ejecutarse juntos al iniciar US3; T023 debe ejecutarse después de las pruebas y con T009 ya completado.
- T024 puede ejecutarse en paralelo con la validación final; T026 depende de T025; T027 depende de todas las historias entregadas.
- No se ejecutan historias diferentes en paralelo cuando compartan archivos; la serialización US1 → US2 → US3 es obligatoria.

## Ejemplos de ejecución paralela

### Historia de usuario 1

```text
Tarea: "Crear pruebas de mapping en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs"
Tarea: "Crear pruebas del handler en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs"
Tarea: "Crear pruebas del slice GET /api/properties/{id} en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdSliceTests.cs"
Tarea: "Definir GetPropertyByIdResponse en app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdResponse.cs"
```

### Historia de usuario 2

```text
Tarea: "Crear pruebas de equivalencia de mapping en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdMappingTests.cs"
Tarea: "Crear pruebas de componente listado-detalle en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs"

# Después de T013 y T016:
Tarea: "Ajustar GetPropertyByIdMapping en app/backend/src/RealtorApi/Features/Properties/GetPropertyById/GetPropertyByIdMapping.cs"
```

### Historia de usuario 3

```text
Tarea: "Crear pruebas del validator en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdValidatorTests.cs"
Tarea: "Crear pruebas del slice para errores en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdSliceTests.cs"
Tarea: "Crear pruebas de solo lectura en app/backend/tests/RealtorApiTests/Features/Properties/GetPropertyById/GetPropertyByIdHandlerTests.cs"
```

## Estrategia de implementación

### MVP primero (solo historia de usuario 1)

1. Completar Fase 1: Configuración.
2. Completar Fase 2: Fundacional.
3. Completar Fase 3: US1.
4. **DETENERSE Y VALIDAR**: ejecutar la prueba independiente de US1.
5. Mostrar el endpoint de detalle como incremento funcional mínimo.

### Entrega incremental

1. Configuración + Fundacional → base lista.
2. US1 → validar y mostrar MVP.
3. US2 → validar equivalencia con el listado sin regresiones.
4. US3 → validar errores, cancelación y solo lectura.
5. Cada historia añade valor sin romper las anteriores.

### Estrategia con equipo paralelo

1. El equipo completa juntas las Fases 1 y 2.
2. Tras la Fase 2:
   - Desarrollador A: US1.
   - Desarrollador B: prepara pruebas de equivalencia de US2 sin tocar los archivos de US1 hasta que esté completo.
   - Desarrollador C: prepara pruebas de errores de US3 en archivos de prueba distintos.
3. Las historias se integran en orden US1 → US2 → US3 por compartir archivos de origen.

## Notas

- `[P]` significa archivos distintos y sin dependencias incompletas.
- `[Story]` mapea tareas a una historia específica para trazabilidad.
- Cada historia puede completarse y probarse de forma independiente.
- Verificar que las pruebas fallan antes de implementar.
- Confirmar los puntos de control antes de continuar.
- Evitar tareas vagas, conflictos en el mismo archivo y dependencias cruzadas entre historias que rompan la independencia.
