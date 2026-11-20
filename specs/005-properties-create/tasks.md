---

description: "Lista de tareas para la implementación de alta de propiedades"
---

# Tareas: Alta de propiedades

**Entrada**: Documentos de diseño desde `/specs/005-properties-create/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/create-property.http.md`, `quickstart.md` y `.specify/memory/constitution.md`

**Pruebas**: Se incluyen pruebas unitarias y una prueba de componente en memoria porque FR-023 las exige explícitamente y el plan las restringe a `app/backend/tests/RealtorApiTests`.

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo porque usa archivos distintos y no depende de tareas incompletas.
- **[Story]**: identifica la historia de usuario únicamente en las fases de usuario.
- Todas las descripciones incluyen rutas de archivo exactas.

## Convenciones de rutas

- Código fuente: `app/backend/src/RealtorApi/`.
- Pruebas: `app/backend/tests/RealtorApiTests/`.
- Caso de uso: `app/backend/src/RealtorApi/Features/Properties/CreateProperty/`.
- Imágenes públicas: `app/backend/src/RealtorApi/wwwroot/assets/properties/`.

## Fase 1: Configuración

**Propósito**: Preparar la estructura específica del caso de uso sin crear soluciones paralelas.

- [X] T001 Crear los directorios `app/backend/src/RealtorApi/Features/Properties/CreateProperty/` y `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/` siguiendo `specs/005-properties-create/plan.md`.
- [X] T002 [P] Asegurar el directorio `app/backend/src/RealtorApi/wwwroot/assets/properties/` con un archivo de marcador y conservar su copia en publicación mediante `app/backend/src/RealtorApi/RealtorApi.csproj`, sin tocar `wwwroot/images/properties/`.
- [X] T003 [P] Verificar en `app/backend/src/RealtorApi/Program.cs` que `RegisterSlices`, `RegisterHandlers` y `AddValidatorsFromAssembly` cubren el descubrimiento automático, sin agregar registros manuales de slice, handler ni validator.

**Punto de control**: La estructura existe y reutiliza la infraestructura de descubrimiento actual.

## Fase 2: Fundacional

**Propósito**: Completar contratos, persistencia e infraestructura compartida que bloquean todas las historias.

**⚠️ CRÍTICA**: Ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T004 Definir `CreatePropertyRequest` con campos individuales de `multipart/form-data` y una imagen opcional, aplicando `data-model.md` (`CreatePropertyRequest.Title`), `data-model.md` (`CreatePropertyRequest.Description`), `data-model.md` (`CreatePropertyRequest.Address`), `data-model.md` (`CreatePropertyRequest.Price`), `data-model.md` (`CreatePropertyRequest.Status`), `data-model.md` (`CreatePropertyRequest.BedroomCount`), `data-model.md` (`CreatePropertyRequest.BathroomCount`), `data-model.md` (`CreatePropertyRequest.AreaSquareMeters`) y `data-model.md` (`CreatePropertyRequest.Image`) en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyRequest.cs`.
- [X] T005 Definir `CreatePropertyResponse` como representación pública separada de la entidad, aplicando `data-model.md` (`CreatePropertyResponse.Id`), `data-model.md` (`CreatePropertyResponse.Title`), `data-model.md` (`CreatePropertyResponse.Description`), `data-model.md` (`CreatePropertyResponse.Address`), `data-model.md` (`CreatePropertyResponse.Price`), `data-model.md` (`CreatePropertyResponse.Status`), `data-model.md` (`CreatePropertyResponse.BedroomCount`), `data-model.md` (`CreatePropertyResponse.BathroomCount`), `data-model.md` (`CreatePropertyResponse.AreaSquareMeters`) y `data-model.md` (`CreatePropertyResponse.ImageUrl`) en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyResponse.cs`.
- [X] T006 Cambiar `Property.ImageUrl` a `string?` sin valor inicial vacío, según `data-model.md` (`Property.ImageUrl`), en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`.
- [X] T007 Conservar las definiciones canónicas de `data-model.md` (`Property.Id`), `data-model.md` (`Property.Title`), `data-model.md` (`Property.Description`), `data-model.md` (`Property.Address`), `data-model.md` (`Property.Price`), `data-model.md` (`Property.Status`), `data-model.md` (`Property.BedroomCount`), `data-model.md` (`Property.BathroomCount`), `data-model.md` (`Property.AreaSquareMeters`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.UpdatedAt`) en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`, cambiando únicamente `data-model.md` (`Property.ImageUrl`) a `IsRequired(false)`.
- [X] T008 Generar con `dotnet ef migrations add MakePropertyImageUrlNullable --project app/backend/src/RealtorApi/RealtorApi.csproj --startup-project app/backend/src/RealtorApi/RealtorApi.csproj --output-dir Infrastructure/Persistence/Migrations` la migración, el diseñador y la actualización del snapshot en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/`, reflejando `data-model.md` (`Property.ImageUrl`).
- [X] T009 Reemplazar la comprobación de migración única por una comprobación ordenada de dos migraciones y añadir la verificación de nulidad en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`, reflejando `data-model.md` (`Property.ImageUrl`).
- [X] T010 Añadir `ErrorType.Internal` y `Error.Internal` sin detalles técnicos en `app/backend/src/RealtorApi/Infrastructure/Results/Error.cs`, mapearlo a HTTP 500 con `ProblemDetails` en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs` y verificar el cuerpo genérico en `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMapperTests.cs`.
- [X] T011 Registrar `IHttpContextAccessor` en `app/backend/src/RealtorApi/Program.cs` para que `CreatePropertyValidator` inspeccione metadatos multipart, conservando `UseStaticFiles`, la ruta `/assets/properties`, `MapSliceEndpoints` y `MigrateAsync` sin cambios funcionales.

**Punto de control**: Los contratos, la nulibilidad persistida y la infraestructura de errores y validación están listos; las historias pueden comenzar.

## Fase 3: Historia de usuario 1 - Crear una propiedad sin imagen (Prioridad: P1) — MVP

**Objetivo**: Registrar una propiedad válida sin fotografía y devolver su representación pública con `imageUrl` nulo.

**Prueba independiente**: Enviar `POST /api/properties` sin parte `image`, verificar HTTP 201, `Location: /api/properties/{id}`, persistencia del registro, `imageUrl` nulo y ausencia de archivos creados.

**Prueba independiente**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~CreateProperty"`.

### Pruebas para la historia de usuario 1

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T012 [P] [US1] Crear pruebas del handler sin imagen que verifiquen persistencia, `data-model.md` (`Property.ImageUrl`) nulo, `data-model.md` (`Property.CreatedAt`) asignado, `data-model.md` (`Property.UpdatedAt`) nulo, `data-model.md` (`CreatePropertyResponse.ImageUrl`) nulo y ninguna llamada de almacenamiento en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyHandlerTests.cs`.
- [X] T013 [P] [US1] Crear pruebas de mapping explícito sin imagen, comprobando que no se expone la entidad `Property` en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyMappingTests.cs`.
- [X] T014 [P] [US1] Crear pruebas del slice que comprueben `ISlice`, `POST /api/properties`, HTTP 201, encabezado `Location: /api/properties/{id}` y cuerpo con `imageUrl` nulo en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs`.

### Implementación para la historia de usuario 1

- [X] T015 [US1] Implementar `CreatePropertyMapping.ToResponse` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyMapping.cs`, proyectando `data-model.md` (`CreatePropertyResponse.Id`), `data-model.md` (`CreatePropertyResponse.Title`), `data-model.md` (`CreatePropertyResponse.Description`), `data-model.md` (`CreatePropertyResponse.Address`), `data-model.md` (`CreatePropertyResponse.Price`), `data-model.md` (`CreatePropertyResponse.Status`), `data-model.md` (`CreatePropertyResponse.BedroomCount`), `data-model.md` (`CreatePropertyResponse.BathroomCount`), `data-model.md` (`CreatePropertyResponse.AreaSquareMeters`) y `data-model.md` (`CreatePropertyResponse.ImageUrl`) sin validar, persistir ni escribir archivos.
- [X] T016 [US1] Implementar `CreatePropertyHandler.HandleAsync` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyHandler.cs`, devolviendo `Result<CreatePropertyResponse>`, asignando `data-model.md` (`Property.Id`) con `Guid` nuevo, `data-model.md` (`Property.CreatedAt`) con fecha UTC, `data-model.md` (`Property.UpdatedAt`) como nulo y `data-model.md` (`Property.ImageUrl`) como nulo cuando no haya imagen, con `SaveChangesAsync`, cancelación propagada y `Error.Internal` genérico ante fallo de persistencia.
- [X] T017 [US1] Implementar `CreatePropertySlice.AddEndpoint` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs`, enlazando `[AsParameters] CreatePropertyRequest`, inyectando `CreatePropertyHandler`, propagando `CancellationToken`, delegando la lógica al handler y devolviendo `Results.Created($"/api/properties/{response.Id}", response)` cuando el resultado es exitoso o `ProblemDetailsMapper.ToProblemDetails(result)` cuando falla.

**Punto de control**: US1 cumple el MVP sin depender del flujo de imagen ni de las historias posteriores.

## Fase 4: Historia de usuario 2 - Crear una propiedad con una imagen válida (Prioridad: P1)

**Objetivo**: Adjuntar una imagen PNG/JPG válida, publicarla bajo una ruta segura y persistir su URL relativa.

**Prueba independiente**: Enviar una propiedad válida con PNG y con JPG/JPEG de hasta 5 MB, verificar un único archivo seguro, `imageUrl` relativa y consumo desde el listado.

### Pruebas para la historia de usuario 2

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T018 [P] [US2] Crear pruebas unitarias de almacenamiento con PNG y JPG/JPEG ya validados, nombres generados únicos, confinamiento en `wwwroot/assets/properties`, URL relativa devuelta, eliminación del archivo y señalización de fallo de infraestructura en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/PropertyImageStorageTests.cs`.
- [X] T019 [P] [US2] Crear pruebas del handler con imagen válida, fallo de persistencia posterior, fallo de almacenamiento y cancelación, verificando la ruta persistida, la limpieza del archivo y el error interno genérico, en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyHandlerTests.cs`.
- [X] T020 [P] [US2] Crear pruebas del slice con PNG y JPG/JPEG que comprueben HTTP 201, `Location`, `data-model.md` (`CreatePropertyResponse.ImageUrl`) relativa y ausencia de rutas físicas en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs`.

### Implementación para la historia de usuario 2

- [X] T021 [P] [US2] Definir `IPropertyImageStorage` en `app/backend/src/RealtorApi/Infrastructure/Storage/IPropertyImageStorage.cs` con operaciones asíncronas para escribir `data-model.md` (`CreatePropertyRequest.Image`) ya validado y eliminar la ruta devuelta, sin implementar `ISlice` ni `IHandler` y sin reglas de validación.
- [X] T022 [US2] Implementar `PropertyImageStorage` en `app/backend/src/RealtorApi/Infrastructure/Storage/PropertyImageStorage.cs`, escribiendo solo bytes ya validados, generando un nombre seguro único con la extensión validada sin conservar el nombre original, resolviendo el mismo destino público que `Program.cs`, devolviendo `data-model.md` (`Property.ImageUrl`) con el patrón `/assets/properties/{nombreSeguro}`, propagando cancelación, soportando eliminación y registrando `IPropertyImageStorage` en `app/backend/src/RealtorApi/Program.cs` después de crear la implementación.
- [X] T023 [US2] Integrar `IPropertyImageStorage` en `CreatePropertyHandler.HandleAsync` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyHandler.cs`, almacenando solo cuando exista `data-model.md` (`CreatePropertyRequest.Image`), persistiendo la ruta devuelta, eliminando el archivo si falla `SaveChangesAsync` o se cancela la operación y devolviendo `Error.Internal` sin detalles técnicos.

**Punto de control**: US2 crea propiedades con imagen de forma independiente y mantiene consistencia entre registro y archivo.

## Fase 5: Historia de usuario 3 - Rechazar datos o imágenes inválidos (Prioridad: P2)

**Objetivo**: Devolver HTTP 400 estructurado sin persistencia ni archivos huérfanos ante datos inválidos, formatos no admitidos, tamaño excedido o partes duplicadas.

**Prueba independiente**: Enviar campos obligatorios inválidos, negativos, estado desconocido, GIF/PDF, contenido incompatible, extensión no admitida, archivo mayor de 5 MB y dos partes `image`; verificar HTTP 400 y ningún efecto observable.

### Pruebas para la historia de usuario 3

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T024 [P] [US3] Crear pruebas del validator para textos vacíos o excedidos, números negativos y estados no permitidos, cubriendo `data-model.md` (`CreatePropertyRequest.Title`), `data-model.md` (`CreatePropertyRequest.Description`), `data-model.md` (`CreatePropertyRequest.Address`), `data-model.md` (`CreatePropertyRequest.Price`), `data-model.md` (`CreatePropertyRequest.Status`), `data-model.md` (`CreatePropertyRequest.BedroomCount`), `data-model.md` (`CreatePropertyRequest.BathroomCount`) y `data-model.md` (`CreatePropertyRequest.AreaSquareMeters`) en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyValidatorTests.cs`.
- [X] T025 [US3] Crear pruebas del validator para MIME no admitido, extensión no admitida, firma incompatible y tamaño superior al límite de `data-model.md` (`CreatePropertyRequest.Image`) en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyValidatorTests.cs`.
- [X] T026 [P] [US3] Crear pruebas del slice para solicitudes mal formadas, dos partes `image`, HTTP 400 con `ValidationProblemDetails` y HTTP 500 genérico ante fallo interno, comprobando que no se invoca persistencia ni almacenamiento en los casos inválidos, en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs`.

### Implementación para la historia de usuario 3

- [X] T027 [US3] Implementar `CreatePropertyValidator` en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertyValidator.cs`, aplicando las reglas canónicas de `data-model.md` (`CreatePropertyRequest.Title`), `data-model.md` (`CreatePropertyRequest.Description`), `data-model.md` (`CreatePropertyRequest.Address`), `data-model.md` (`CreatePropertyRequest.Price`), `data-model.md` (`CreatePropertyRequest.Status`), `data-model.md` (`CreatePropertyRequest.BedroomCount`), `data-model.md` (`CreatePropertyRequest.BathroomCount`), `data-model.md` (`CreatePropertyRequest.AreaSquareMeters`) y `data-model.md` (`CreatePropertyRequest.Image`) mediante FluentValidation, e inyectando `IHttpContextAccessor` para validar `Content-Type` y la existencia de exactamente una parte `image`, sin duplicar validación en el slice, el mapping ni el almacenamiento.
- [X] T028 [US3] Confirmar en `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs` que `AddEndpoint` no contiene comprobaciones de validación y se limita a enlazar, delegar y traducir el resultado del handler.

**Punto de control**: US3 bloquea entradas inválidas antes de cualquier efecto persistente o de archivo.

## Fase 6: Historia de usuario 4 - Mantener compatibilidad con el sistema existente (Prioridad: P3)

**Objetivo**: Incorporar el alta sin alterar `/health`, el listado paginado, el descubrimiento automático ni la persistencia aprobada.

**Prueba independiente**: Ejecutar las pruebas existentes de salud, listado, persistencia y descubrimiento después del alta y comprobar que siguen pasando sin cambios de contrato.

### Pruebas para la historia de usuario 4

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T029 [P] [US4] Añadir pruebas de descubrimiento que comprueben el registro automático de `CreatePropertySlice` y `CreatePropertyHandler`, sin afectar a `HealthSlice` ni `ListPropertiesSlice`, en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs`.
- [X] T030 [P] [US4] Añadir una prueba de componente en memoria que cree una propiedad con `CreatePropertyHandler` y la consulte con `ListPropertiesHandler`, además de pruebas de `ListPropertiesMapping` con `Property.ImageUrl` nulo y relativa, en `app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyHandlerTests.cs` y `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`.

### Implementación para la historia de usuario 4

- [X] T031 [US4] Ajustar la firma de `ListPropertiesMapping.ToResponse` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs` para aceptar `data-model.md` (`Property.ImageUrl`) como nulo, conservando la conversión a URL absoluta y el valor vacío actual del listado.
- [X] T032 [US4] Verificar en `app/backend/src/RealtorApi/Program.cs` y `app/backend/src/RealtorApi/RealtorApi.csproj` que el alta no modifica `/health`, `GET /api/properties`, los archivos servidos desde `/images/properties/` ni el registro automático de casos de uso.

**Punto de control**: Todas las historias funcionan de forma independiente y las funcionalidades previas conservan su contrato.

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: Completar ejemplos HTTP, validación real y revisión de gobernanza.

- [X] T033 [P] Añadir las solicitudes `POST /api/properties` sin imagen, con PNG y con JPG/JPEG, además de ejemplos de error, en `app/backend/src/RealtorApi/RealtorApi.http` según `specs/005-properties-create/contracts/create-property.http.md`.
- [X] T034 Ejecutar `dotnet build app/Realtor.sln` y `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` desde la raíz del repositorio, corrigiendo cualquier fallo en los archivos implicados por `specs/005-properties-create/quickstart.md`.
- [X] T035 Registrar en `specs/005-properties-create/quickstart.md` la evidencia real de ejecución con fecha, comandos y resultados, sin declarar supuestos como validaciones ejecutadas.
- [X] T036 Revisar el diff completo y confirmar en `app/backend/src/RealtorApi/` que no hay controllers, rutas de actualización, eliminación o autorización, lógica de negocio en `CreatePropertySlice.cs` ni `CreatePropertyMapping.cs`, entidades EF en respuestas, rutas físicas expuestas ni cambios de contrato en el listado.

**Punto de control**: La implementación está validada y lista para decisión de cierre conforme a la constitución.

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1**: sin dependencias.
- **Fase 2**: depende de Fase 1 y bloquea todas las historias de usuario.
- **Fases 3-6**: dependen de Fase 2 y se ejecutan en orden US1 → US2 → US3 → US4 porque comparten `CreatePropertyHandler.cs`, `CreatePropertySlice.cs` y sus pruebas.
- **Fase 7**: depende de las historias que se vayan a entregar.

### Dependencias entre historias

- **US1 (P1)**: constituye el MVP y no depende de otras historias.
- **US2 (P1)**: depende del request, response, handler y mapping compartidos; amplía `CreatePropertyHandler.cs` y registra el almacenamiento en T022.
- **US3 (P2)**: depende del request y del slice; concentra toda la validación en `CreatePropertyValidator.cs` y verifica que el slice no tenga reglas propias.
- **US4 (P3)**: depende de la existencia del alta para validar compatibilidad; toca `ListPropertiesMapping.cs` de forma aislada, pero sus pruebas de descubrimiento requieren el slice completo.

### Dentro de cada historia

1. Escribir primero las pruebas y verificar que fallan.
2. Definir o ampliar modelos y contratos.
3. Implementar almacenamiento o mapping antes del handler.
4. Implementar el handler antes del slice.
5. Verificar el punto de control de la historia antes de pasar a la siguiente.

## Oportunidades de ejecución paralela

- T002 y T003 pueden ejecutarse juntos.
- T012, T013 y T014 pueden ejecutarse juntos al iniciar US1.
- T018, T019 y T020 pueden ejecutarse juntos al iniciar US2.
- T021 puede ejecutarse junto con las pruebas T018-T020.
- T024 y T026 pueden ejecutarse juntos al iniciar US3; T025 comparte archivo con T024 y debe ejecutarse después.
- T029 y T030 pueden ejecutarse juntos al iniciar US4.
- T033 puede ejecutarse en paralelo con la validación final.
- No se ejecutan historias diferentes en paralelo cuando compartan archivos; la serialización US1 → US2 → US3 → US4 es obligatoria.

## Ejemplos de ejecución paralela

### Historia de usuario 1

```text
Tarea: "Crear pruebas del handler sin imagen en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyHandlerTests.cs"
Tarea: "Crear pruebas de mapping explícito sin imagen en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyMappingTests.cs"
Tarea: "Crear pruebas del slice POST /api/properties en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs"
```

### Historia de usuario 2

```text
Tarea: "Crear pruebas unitarias de almacenamiento en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/PropertyImageStorageTests.cs"
Tarea: "Crear pruebas del handler con imagen válida y fallos parciales en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyHandlerTests.cs"
Tarea: "Definir IPropertyImageStorage en app/backend/src/RealtorApi/Infrastructure/Storage/IPropertyImageStorage.cs"
```

### Historia de usuario 3

```text
Tarea: "Crear pruebas del validator para datos inválidos en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyValidatorTests.cs"
Tarea: "Crear pruebas del slice para solicitudes mal formadas y dos partes image en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertySliceTests.cs"

# Después de T024, en el mismo archivo de pruebas del validator:
Tarea: "Crear pruebas del validator para imagen inválida en app/backend/tests/RealtorApiTests/Features/Properties/CreateProperty/CreatePropertyValidatorTests.cs"
```

## Estrategia de implementación

### MVP primero: historia de usuario 1

1. Completar Fases 1 y 2.
2. Completar Fase 3.
3. Detenerse y validar creación sin imagen, HTTP 201, `Location` y `imageUrl` nulo.
4. Entregar o demostrar el MVP antes de continuar.

### Entrega incremental

1. Añadir US2 y validar PNG/JPG, ruta pública y limpieza.
2. Añadir US3 y validar errores estructurados sin efectos parciales.
3. Añadir US4 y ejecutar regresiones de salud, listado y descubrimiento.
4. Completar Fase 7 y registrar evidencia real de validación.

### Estrategia con equipo paralelo

1. El equipo completa junto Fases 1 y 2.
2. Un desarrollador ejecuta US1, US2 y US3 de forma secuencial porque comparten el slice y sus pruebas.
3. Otro desarrollador puede preparar T031 y T033 después de US1 siempre que no modifique los archivos del slice.
4. US4 se integra después de US3 para incluir descubrimiento y visibilidad en el listado.
5. Integrar cada historia solo después de su punto de control.

## Notas

- `[P]` indica archivos distintos y ausencia de dependencias entre tareas.
- Las etiquetas `[US1]` a `[US4]` permiten rastrear cada tarea hasta `spec.md`.
- Las restricciones canónicas de campos se citan desde `data-model.md`; no se duplican en esta lista.
- Solo se usan pruebas unitarias y de componente en memoria, conforme a la constitución y a `specs/005-properties-create/plan.md`.
- No se marca ninguna tarea como completada hasta verificar su resultado.
