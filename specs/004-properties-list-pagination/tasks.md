# Tasks: Paginación y listado de propiedades

**Input**: Documentos de diseño desde `/specs/004-properties-list-pagination/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/list-properties.http.md`

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Dependencias de fases

- **Fase 1** no tiene dependencias.
- **Fase 2** bloquea todas las historias de usuario.
- **Fase 3 US1** constituye el MVP y depende de Fase 2.
- **Fase 4 US2** depende de US1 para reutilizar el contrato y mapping.
- **Fase 5 US3** puede desarrollarse junto con US1 una vez exista el request y puede probarse independientemente.
- **Fase 6** depende de las historias implementadas.

## Fase 1: Setup

**Propósito**: Preparar la estructura específica del caso de uso sin modificar infraestructura existente.

- [X] T001 Crear la estructura `app/backend/src/RealtorApi/Features/Properties/ListProperties/` según `specs/004-properties-list-pagination/plan.md`.
- [X] T002 [P] Crear la estructura de pruebas `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/` según `specs/004-properties-list-pagination/plan.md`.
- [X] T003 Confirmar que `ISlice`, `IHandler`, `MapSliceEndpoints`, `RegisterHandlers` y `ValidationFilterFactory` existentes cubren el registro automático sin cambios paralelos.

**Checkpoint**: La estructura específica está preparada y reutiliza la infraestructura existente.

## Fase 2: Foundational

**Propósito**: Completar los contratos comunes mínimos necesarios antes de cada historia.

**⚠️ CRÍTICA**: Ninguna historia puede comenzar hasta completar esta fase.

- [X] T004 Definir `ListPropertiesRequest.Page` y `ListPropertiesRequest.PageSize` con sus defaults y límites según `specs/004-properties-list-pagination/data-model.md` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesRequest.cs`.
- [X] T005 Definir `ListPropertiesItemResponse.Id`, `.Title`, `.Description`, `.Address`, `.Price`, `.Status`, `.BedroomCount`, `.BathroomCount`, `.AreaSquareMeters` y `.ImageUrl`, además de `ListPropertiesResponse.Items`, `.Page`, `.PageSize`, `.TotalItems`, `.TotalPages`, `.HasNext` y `.HasPrevious` según `specs/004-properties-list-pagination/data-model.md` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesResponse.cs`.
- [X] T006 Verificar que `RegisterHandlers` puede descubrir el nuevo handler sin modificación de `app/backend/src/RealtorApi/Program.cs`.

**Checkpoint**: Request y response existen y el handler puede descubrirse automáticamente.

## Fase 3: User Story 1 - Listado paginado (Priority: P1) 🎯 MVP

**Goal**: Entregar `GET /api/properties` paginado por título y dirección, con metadatos consistentes.

**Independent Test**: Consultar sin parámetros y con `page=2&pageSize=6`, verificando defaults, orden, conteo y metadatos.

### Pruebas para User Story 1

- [X] T007 [P] [US1] Crear pruebas de defaults y orden ascendente por `Property.Title` y desempate `Property.Address` en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesHandlerTests.cs`.
- [X] T008 [P] [US1] Crear pruebas de paginación, normalización de página fuera de rango y metadatos en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesHandlerTests.cs`.

### Implementación para User Story 1

- [X] T009 [US1] Implementar `ListPropertiesHandler` como clase concreta `IHandler` con lectura `AsNoTracking`, conteo y proyección EF de `Property.Id`, `Property.Title`, `Property.Description`, `Property.Address`, `Property.Price`, `Property.Status`, `Property.BedroomCount`, `Property.BathroomCount`, `Property.AreaSquareMeters` y `Property.ImageUrl` según `specs/004-properties-list-pagination/data-model.md` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs`.
- [X] T010 [US1] Implementar `ListPropertiesSlice` con `ISlice` y `GET /api/properties`, delegando al handler y propagando `CancellationToken` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesSlice.cs`.
- [X] T011 [US1] Implementar el mapping explícito de la colección y metadatos, sin entidades EF en la respuesta, en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`.
- [X] T012 [P] [US1] Crear pruebas del slice y confirmación de ruta/verb usando el patrón ISlice en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesSliceTests.cs`.

**Checkpoint**: US1 devuelve página válida, metadatos y orden sin requerir otras historias.

## Fase 4: User Story 2 - Imágenes públicas absolutas (Priority: P1)

**Goal**: Construir `imageUrl` absoluta para cada propiedad desde el scheme y host del request.

**Independent Test**: Consultar desde `http://localhost:5023` y verificar el patrón `http://localhost:5023/assets/properties/{fileName}` y el caso vacío.

### Pruebas para User Story 2

- [X] T013 [P] [US2] Crear pruebas de mapping con scheme y host HTTP/HTTPS y formato esperado de `ListPropertiesItemResponse.ImageUrl` según `specs/004-properties-list-pagination/data-model.md` en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`.
- [X] T014 [P] [US2] Crear pruebas para nombre de imagen vacío o inválido, esperando cadena vacía, en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`.

### Implementación para User Story 2

- [X] T015 [US2] Completar el mapping de `data-model.md` `Property.ImageUrl` para extraer el nombre de archivo y construir la URL absoluta con `Request.Scheme`, `Request.Host` y `/assets/properties/`; devolver cadena vacía si no existe un nombre válido en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`.
- [X] T016 [US2] Pasar `HttpContext` o los datos mínimos de request al mapping sin devolver rutas físicas ni relativas en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs`.

**Checkpoint**: US2 devuelve URLs absolutas válidas o cadenas vacías sin exponer persistencia física.

## Fase 5: User Story 3 - Validación de parámetros (Priority: P2)

**Goal**: Rechazar parámetros inválidos con HTTP 400 y normalizar valores válidos excedidos según la especificación.

**Independent Test**: Enviar `page=0`, `pageSize=0`, valores no enteros y `pageSize=100`, verificando 400 o normalización a 24.

### Pruebas para User Story 3

- [X] T017 [P] [US3] Crear pruebas del validator para `Page >= 1`, `PageSize >= 1` y normalización posterior a 24 en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesValidatorTests.cs`.
- [X] T018 [P] [US3] Crear pruebas de respuesta HTTP 400 con `ValidationProblemDetails` mediante el filtro global en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesSliceTests.cs`.

### Implementación para User Story 3

- [X] T019 [US3] Implementar `ListPropertiesValidator` con validaciones de enteros positivos y lectura por FluentValidation según `specs/004-properties-list-pagination/data-model.md` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesValidator.cs`.
- [X] T020 [US3] Aplicar defaults de `Page=1` y `PageSize=6` y normalización de `pageSize > 24` a 24 en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesRequest.cs` y `ListPropertiesHandler.cs`.
- [X] T021 [US3] Propagar `RequestAborted` o el `CancellationToken` recibido desde el endpoint hasta conteo y consulta EF en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs`.

**Checkpoint**: US3 devuelve errores estructurados sin consultar datos y normaliza `pageSize` permitido.

## Fase 6: Polish y compatibilidad

**Propósito**: Completar pruebas manuales, validación end to end y compatibilidad con infraestructura existente.

- [X] T022 [P] Añadir en `app/backend/src/RealtorApi/RealtorApi.http` las solicitudes `GET /api/properties` y `GET /api/properties?page=1&pageSize=6` según `specs/004-properties-list-pagination/contracts/list-properties.http.md`.
- [X] T023 [P] Añadir pruebas de integración ligera del discovery ISlice y respuesta del slice en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesSliceTests.cs`.
- [X] T024 Ejecutar build y todas las pruebas unitarias del backend, verificando `/health` y el discovery existente en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
- [X] T025 Actualizar `specs/004-properties-list-pagination/quickstart.md` con resultados reales de ejecución y URLs probadas.
- [X] T026 Revisar el diff final y confirmar que no existen controllers, modelos EF devueltos, registries manuales ni cambios innecesarios en `app/backend/src/RealtorApi/Program.cs`.
- [X] T027 Configurar la ruta pública `/assets/properties/` como destino servido de las imágenes existentes, sin exponer rutas físicas internas, en `app/backend/src/RealtorApi/Program.cs` y `app/backend/src/RealtorApi/RealtorApi.csproj`.

## Dependencias entre historias

- **US1** requiere los contratos fundacionales y constituye el MVP.
- **US2** depende de US1; puede implementarse al completar el mapping base.
- **US3** depende de Fase 2 y puede desarrollarse en paralelo con US1.
- **Fase 6** depende de US1-US3.

## Ejemplos de ejecución paralela

- T007 y T008 pueden ejecutarse juntas para probar handler y metadatos.
- T013 y T014 pueden ejecutarse juntas para probar mappings HTTP/HTTPS e imágenes inválidas.
- T017 y T018 pueden ejecutarse juntas para validator y respuesta HTTP.
- T022 y T023 pueden completarse juntas como pruebas manuales y discovery.

## Estrategia de implementación

### MVP: User Story 1

1. Completar Fases 1–2.
2. Completar US1 con handler, slice y mapping.
3. Detenerse y validar orden, metadatos y paginación.

### Incrementos

1. Añadir US2 para URLs absolutas.
2. Añadir US3 para validación y normalización.
3. Completar compatibilidad, HTTP manual y validación final.
