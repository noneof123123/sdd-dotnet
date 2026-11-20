---
description: "Lista de tareas para la implementación de actualización de propiedades"
---

# Tareas: Actualización de propiedades

**Entrada**: Documentos de diseño desde `/specs/006-properties-update/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/update-property.http.md`, `quickstart.md` y `.specify/memory/constitution.md`

**Pruebas**: Se incluyen pruebas unitarias y de componente en memoria porque la especificación define una prueba independiente por historia de usuario y SC-009 exige conservar las regresiones de las iniciativas anteriores.

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: puede ejecutarse en paralelo porque usa archivos distintos y no depende de tareas incompletas.
- **[Story]**: identifica la historia de usuario únicamente en las fases de usuario.
- Todas las descripciones incluyen rutas de archivo exactas.
- Los campos restringidos se citan como `data-model.md` (`Entidad.campo`) en las tareas de implementación que los materializan.

## Convenciones de rutas

- Código fuente: `app/backend/src/RealtorApi/`.
- Pruebas: `app/backend/tests/RealtorApiTests/`.
- Caso de uso: `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/`.
- Imágenes públicas: `app/backend/src/RealtorApi/wwwroot/assets/properties/`.

## Fase 1: Configuración

**Propósito**: Preparar la estructura específica del caso de uso sin crear soluciones paralelas.

- [X] T001 Crear los directorios `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/` y `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/` siguiendo `specs/006-properties-update/plan.md`.
- [X] T002 [P] Verificar en `app/backend/src/RealtorApi/Program.cs` que `RegisterSlices`, `RegisterHandlers`, `AddValidatorsFromAssembly` y `AddHttpContextAccessor` cubren el descubrimiento automático y la validación dependiente de la solicitud, sin agregar registros manuales de slice, handler ni validator.
- [X] T003 [P] Verificar en `app/backend/src/RealtorApi/Program.cs` y `app/backend/src/RealtorApi/RealtorApi.csproj` que el destino público `/assets/properties` y la copia de activos en publicación permanecen disponibles sin cambios funcionales.

**Punto de control**: La estructura existe y reutiliza la infraestructura de descubrimiento y almacenamiento actual.

## Fase 2: Fundacional

**Propósito**: Completar contratos, validación y comprobaciones compartidas que bloquean todas las historias de usuario.

**CRÍTICA**: Ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T004 [P] Definir `UpdatePropertyRequest` con el identificador de ruta y los campos individuales de `multipart/form-data`, aplicando `data-model.md` (`UpdatePropertyRequest.Id`), `data-model.md` (`UpdatePropertyRequest.Title`), `data-model.md` (`UpdatePropertyRequest.Description`), `data-model.md` (`UpdatePropertyRequest.Address`), `data-model.md` (`UpdatePropertyRequest.Price`), `data-model.md` (`UpdatePropertyRequest.Status`), `data-model.md` (`UpdatePropertyRequest.BedroomCount`), `data-model.md` (`UpdatePropertyRequest.BathroomCount`), `data-model.md` (`UpdatePropertyRequest.AreaSquareMeters`) y `data-model.md` (`UpdatePropertyRequest.Image`) en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyRequest.cs`.
- [X] T005 [P] Definir `UpdatePropertyResponse` como representación pública separada de la entidad, aplicando `data-model.md` (`UpdatePropertyResponse.Id`), `data-model.md` (`UpdatePropertyResponse.Title`), `data-model.md` (`UpdatePropertyResponse.Description`), `data-model.md` (`UpdatePropertyResponse.Address`), `data-model.md` (`UpdatePropertyResponse.Price`), `data-model.md` (`UpdatePropertyResponse.Status`), `data-model.md` (`UpdatePropertyResponse.BedroomCount`), `data-model.md` (`UpdatePropertyResponse.BathroomCount`), `data-model.md` (`UpdatePropertyResponse.AreaSquareMeters`) y `data-model.md` (`UpdatePropertyResponse.ImageUrl`) en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyResponse.cs`.
- [X] T006 [P] Confirmar en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`, `app/backend/src/RealtorApi/Domain/Properties/Property.cs` y `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/` que no se requiere migración, conservando las definiciones canónicas de `data-model.md` (`Property.Id`), `data-model.md` (`Property.Title`), `data-model.md` (`Property.Description`), `data-model.md` (`Property.Address`), `data-model.md` (`Property.Price`), `data-model.md` (`Property.Status`), `data-model.md` (`Property.BedroomCount`), `data-model.md` (`Property.BathroomCount`), `data-model.md` (`Property.AreaSquareMeters`), `data-model.md` (`Property.ImageUrl`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.UpdatedAt`).
- [X] T007 [P] Verificar en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs` y `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMapperTests.cs` que `Error.NotFound` y `Error.Internal` se traducen a HTTP 404 y 500 genérico sin detalles técnicos, ajustando solo lo que falte.
- [X] T008 [P] Verificar en `app/backend/src/RealtorApi/Infrastructure/Storage/IPropertyImageStorage.cs`, `app/backend/src/RealtorApi/Infrastructure/Storage/PropertyImageStorage.cs` y `app/backend/src/RealtorApi/Program.cs` que `StoreAsync` y `RemoveAsync` están disponibles y registrados, sin modificar su comportamiento ni añadir validación.
- [X] T009 Implementar `UpdatePropertyValidator` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyValidator.cs`, aplicando las reglas canónicas de `data-model.md` (`UpdatePropertyRequest.Id`), `data-model.md` (`UpdatePropertyRequest.Title`), `data-model.md` (`UpdatePropertyRequest.Description`), `data-model.md` (`UpdatePropertyRequest.Address`), `data-model.md` (`UpdatePropertyRequest.Price`), `data-model.md` (`UpdatePropertyRequest.Status`), `data-model.md` (`UpdatePropertyRequest.BedroomCount`), `data-model.md` (`UpdatePropertyRequest.BathroomCount`), `data-model.md` (`UpdatePropertyRequest.AreaSquareMeters`) y `data-model.md` (`UpdatePropertyRequest.Image`) mediante FluentValidation, inyectando `IHttpContextAccessor` para inspeccionar multipart y limitando sus fallos a HTTP 400 sin introducir 413 ni 415; debe completarse antes de iniciar US1.

**Punto de control**: Los contratos, el validator, la configuración persistente y la infraestructura compartida están listos; las historias pueden comenzar.

## Fase 3: Historia de usuario 1 - Actualizar datos sin cambiar la imagen (Prioridad: P1) — MVP

**Objetivo**: Corregir los datos comerciales y físicos de una propiedad existente sin perder su imagen asociada ni sus marcas originales.

**Prueba independiente**: Enviar `PUT /api/properties/{id}` válido sin parte `image`, verificar HTTP 200, representación pública actualizada, conservación de `Id`, `CreatedAt` e `ImageUrl`, nuevo `UpdatedAt` en UTC y ninguna operación de almacenamiento.

**Prueba independiente**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~UpdateProperty"`.

### Pruebas para la historia de usuario 1

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T010 [P] [US1] Crear pruebas del handler sin imagen que verifiquen conservación de `data-model.md` (`Property.Id`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.ImageUrl`), asignación de `data-model.md` (`Property.UpdatedAt`) en UTC, reemplazo de campos editables, cero llamadas de almacenamiento y respuesta con `data-model.md` (`UpdatePropertyResponse.ImageUrl`) igual a la previa en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs`.
- [X] T011 [P] [US1] Crear pruebas de mapping explícito que comprueben la proyección de todos los campos públicos, `status` como texto, ausencia de entidad persistente, rutas físicas y marcas temporales en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyMappingTests.cs`.
- [X] T012 [P] [US1] Crear pruebas del slice que comprueben `ISlice`, `PUT /api/properties/{id}`, enlace de `data-model.md` (`UpdatePropertyRequest.Id`), HTTP 200, cuerpo actualizado y delegación al handler sin lógica propia en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs`.

### Implementación para la historia de usuario 1

- [X] T013 [P] [US1] Implementar `UpdatePropertyMapping.ToResponse` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyMapping.cs`, proyectando `data-model.md` (`UpdatePropertyResponse.Id`), `data-model.md` (`UpdatePropertyResponse.Title`), `data-model.md` (`UpdatePropertyResponse.Description`), `data-model.md` (`UpdatePropertyResponse.Address`), `data-model.md` (`UpdatePropertyResponse.Price`), `data-model.md` (`UpdatePropertyResponse.Status`), `data-model.md` (`UpdatePropertyResponse.BedroomCount`), `data-model.md` (`UpdatePropertyResponse.BathroomCount`), `data-model.md` (`UpdatePropertyResponse.AreaSquareMeters`) y `data-model.md` (`UpdatePropertyResponse.ImageUrl`) sin validar, persistir ni escribir archivos.
- [X] T014 [US1] Implementar `UpdatePropertyHandler.HandleAsync` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyHandler.cs`, cargando la propiedad por identificador, capturando una instantánea, conservando `data-model.md` (`Property.Id`), `data-model.md` (`Property.CreatedAt`) y `data-model.md` (`Property.ImageUrl`), reemplazando `data-model.md` (`Property.Title`), `data-model.md` (`Property.Description`), `data-model.md` (`Property.Address`), `data-model.md` (`Property.Price`), `data-model.md` (`Property.Status`), `data-model.md` (`Property.BedroomCount`), `data-model.md` (`Property.BathroomCount`) y `data-model.md` (`Property.AreaSquareMeters`), asignando `data-model.md` (`Property.UpdatedAt`) en UTC, persistiendo con `SaveChangesAsync`, propagando cancelación y devolviendo `Result<UpdatePropertyResponse>` o `Error.NotFound` cuando no exista la propiedad.
- [X] T015 [US1] Implementar `UpdatePropertySlice.AddEndpoint` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs`, enlazando `[AsParameters] UpdatePropertyRequest`, inyectando `UpdatePropertyHandler`, propagando `CancellationToken`, delegando la lógica al handler y devolviendo `Results.Ok(response)` cuando el resultado es exitoso o `ProblemDetailsMapper.ToProblemDetails(result)` cuando falla, sin validación propia de `data-model.md` (`UpdatePropertyRequest.Id`).

**Punto de control**: US1 cumple el MVP con validación disponible desde la Fase 2 y sin depender del flujo de imagen ni de las historias posteriores.

## Fase 4: Historia de usuario 2 - Reemplazar la imagen de una propiedad (Prioridad: P1)

**Objetivo**: Adjuntar una imagen PNG o JPG/JPEG válida, publicarla con nombre seguro y dejar una sola imagen activa asociada.

**Prueba independiente**: Actualizar una propiedad con PNG y con JPG/JPEG válidos, verificar HTTP 200, `imageUrl` relativa, retirada de la imagen anterior, compensación ante fallos controlados y ausencia de rutas físicas o nombres originales.

### Pruebas para la historia de usuario 2

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T016 [P] [US2] Crear pruebas del handler con `data-model.md` (`UpdatePropertyRequest.Image`) válido, PNG y JPG/JPEG, que verifiquen publicación, persistencia de `data-model.md` (`Property.ImageUrl`), retirada de la imagen anterior, fallo antes de publicar, fallo de persistencia, fallo de retirada con restauración de la instantánea y cancelación en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs`.
- [X] T017 [P] [US2] Crear pruebas del slice con imagen que comprueben HTTP 200, `data-model.md` (`UpdatePropertyResponse.ImageUrl`) relativa, exactamente una imagen activa y ausencia de `wwwroot`, `ContentRootPath` y nombres originales en el cuerpo, en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs`.

### Implementación para la historia de usuario 2

- [X] T018 [US2] Ampliar `UpdatePropertyHandler.HandleAsync` en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyHandler.cs` para publicar `data-model.md` (`UpdatePropertyRequest.Image`) solo después de encontrar la propiedad, persistir la nueva `data-model.md` (`Property.ImageUrl`), retirar la imagen anterior únicamente tras confirmar la persistencia, y compensar fallos de publicación, persistencia, retirada y cancelación restaurando la instantánea y manteniendo cero archivos nuevos huérfanos observables.

**Punto de control**: US2 reemplaza imágenes de forma independiente y mantiene consistencia entre registro y archivo.

## Fase 5: Historia de usuario 3 - Rechazar una actualización inválida sin efectos parciales (Prioridad: P2)

**Objetivo**: Devolver errores claros y estructurados antes de modificar la propiedad o publicar archivos.

**Prueba independiente**: Enviar identificadores inválidos o inexistentes, datos vacíos o negativos, estado desconocido, imágenes no admitidas, tamaño excedido y dos partes `image`; verificar HTTP 400 o 404 según corresponda, sin cambios observables.

### Pruebas para la historia de usuario 3

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T019 [P] [US3] Crear pruebas del validator para identificador vacío y campos inválidos, cubriendo `data-model.md` (`UpdatePropertyRequest.Id`), `data-model.md` (`UpdatePropertyRequest.Title`), `data-model.md` (`UpdatePropertyRequest.Description`), `data-model.md` (`UpdatePropertyRequest.Address`), `data-model.md` (`UpdatePropertyRequest.Price`), `data-model.md` (`UpdatePropertyRequest.Status`), `data-model.md` (`UpdatePropertyRequest.BedroomCount`), `data-model.md` (`UpdatePropertyRequest.BathroomCount`) y `data-model.md` (`UpdatePropertyRequest.AreaSquareMeters`) en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs`.
- [X] T020 [US3] Crear pruebas del validator para `Content-Type` no multipart, MIME no admitido, extensión no admitida, firma incompatible, tamaño superior al límite, parte `image` no archivo y dos partes `image`, cubriendo `data-model.md` (`UpdatePropertyRequest.Image`) en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs`.
- [X] T021 [P] [US3] Crear pruebas del slice para solicitudes mal formadas, `data-model.md` (`UpdatePropertyRequest.Id`) inexistente, `data-model.md` (`UpdatePropertyRequest.Image`) duplicada, HTTP 400 con `ValidationProblemDetails`, HTTP 404 y HTTP 500 genérico, comprobando que no se invocan persistencia ni almacenamiento en los casos inválidos, en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs`.

### Verificación para la historia de usuario 3

- [X] T022 [US3] Confirmar en `app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertySlice.cs` y `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapper.cs` que el slice no contiene comprobaciones de validación y que los fallos del handler se traducen a 404 o 500 sin detalles técnicos.

**Punto de control**: US3 bloquea entradas inválidas antes de cualquier efecto persistente o de archivo.

## Fase 6: Historia de usuario 4 - Mantener compatibilidad del sistema existente (Prioridad: P3)

**Objetivo**: Incorporar la actualización sin alterar creación, listado paginado, salud, persistencia ni descubrimiento automático.

**Prueba independiente**: Ejecutar las pruebas existentes de salud, creación, listado, persistencia y descubrimiento después de la actualización y comprobar que siguen pasando sin cambios de contrato.

### Pruebas para la historia de usuario 4

> Escribir primero las pruebas y confirmar que fallan antes de la implementación.

- [X] T023 [P] [US4] Añadir pruebas de descubrimiento que comprueben el registro automático de `UpdatePropertySlice` y `UpdatePropertyHandler` junto a `CreatePropertySlice`, `ListPropertiesSlice` y `HealthSlice`, sin controllers, en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs` y `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T024 [P] [US4] Añadir una prueba de componente en memoria que actualice con `UpdatePropertyHandler` y consulte con `ListPropertiesHandler`, verificando que `data-model.md` (`Property.ImageUrl`) sigue generando la URL absoluta vigente del listado, en `app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs`.

### Implementación para la historia de usuario 4

- [X] T025 [US4] Verificar en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`, `app/backend/src/RealtorApi/Features/Properties/CreateProperty/CreatePropertySlice.cs`, `app/backend/src/RealtorApi/Features/Health/HealthSlice.cs` y `app/backend/src/RealtorApi/Program.cs` que la nueva capacidad no modifica sus contratos, su registro automático ni su comportamiento.

**Punto de control**: Todas las historias funcionan de forma independiente y las funcionalidades previas conservan su contrato.

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: Completar ejemplos HTTP, validación real y revisión de gobernanza.

- [X] T026 [P] Añadir las solicitudes `PUT /api/properties/{id}` sin imagen, con PNG y con JPG/JPEG, además de ejemplos de error, en `app/backend/src/RealtorApi/RealtorApi.http` según `specs/006-properties-update/contracts/update-property.http.md`.
- [X] T027 Ejecutar `dotnet build app/Realtor.sln` y `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` desde la raíz del repositorio, corrigiendo cualquier fallo en los archivos implicados por `specs/006-properties-update/quickstart.md`.
- [X] T028 Registrar en `specs/006-properties-update/quickstart.md` la evidencia real de ejecución con fecha, comandos y resultados, sin declarar supuestos como validaciones ejecutadas.
- [X] T029 Revisar el diff completo y confirmar en `app/backend/src/RealtorApi/` que no hay controllers, autorización, eliminación, actualización parcial, control de versión, HTTP 409, migración, lógica de negocio en `UpdatePropertySlice.cs` o `UpdatePropertyMapping.cs`, entidades EF en respuestas ni rutas físicas expuestas.

**Punto de control**: La implementación está validada y lista para decisión de cierre conforme a la constitución.

## Dependencias y orden de ejecución

### Dependencias de fases

- **Fase 1**: sin dependencias.
- **Fase 2**: depende de Fase 1 y bloquea todas las historias de usuario.
- **Fases 3-6**: dependen de Fase 2 y se ejecutan en orden US1 → US2 → US3 → US4 porque comparten `UpdatePropertyHandler.cs`, `UpdatePropertySlice.cs` y sus pruebas; `UpdatePropertyValidator.cs` se implementa en Fase 2.
- **Fase 7**: depende de las historias que se vayan a entregar.

### Dependencias entre historias

- **US1 (P1)**: constituye el MVP y no depende de otras historias.
- **US2 (P1)**: amplía `UpdatePropertyHandler.cs` con almacenamiento y compensación; depende del handler y del mapping de US1.
- **US3 (P2)**: depende de T009, del slice y del handler; verifica rechazos y que el slice no tenga reglas propias, sin implementar validación.
- **US4 (P3)**: depende de la existencia del caso de uso completo para validar compatibilidad; no modifica otras features.

### Dentro de cada historia

1. Escribir primero las pruebas y verificar que fallan.
2. Definir o ampliar contratos y mapping.
3. Implementar el handler antes del slice.
4. Confirmar que la validación permanece concentrada en el validator de la Fase 2.
5. Verificar el punto de control de la historia antes de pasar a la siguiente.

## Oportunidades de ejecución paralela

- T002 y T003 pueden ejecutarse juntos.
- T004, T005, T006, T007 y T008 pueden ejecutarse juntos al completar la Fase 1; T009 debe ejecutarse después de T004 y antes del punto de control de la Fase 2.
- T010, T011 y T012 pueden ejecutarse juntos al iniciar US1.
- T013 puede ejecutarse en paralelo con la preparación de T014, pero T014 y T015 deben ejecutarse después de T013.
- T016 y T017 pueden ejecutarse juntos al iniciar US2; T018 debe ejecutarse después de ambos y de T014.
- T019 y T021 pueden ejecutarse juntos al iniciar US3; T020 comparte archivo con T019 y debe ejecutarse después; T022 debe ejecutarse después de las pruebas y T009 ya debe estar completado en la Fase 2.
- T023 y T024 pueden ejecutarse juntos al iniciar US4; T025 debe ejecutarse después.
- T026 puede ejecutarse en paralelo con la validación final.
- No se ejecutan historias diferentes en paralelo cuando compartan archivos; la serialización US1 → US2 → US3 → US4 es obligatoria.

## Ejemplos de ejecución paralela

### Historia de usuario 1

```text
Tarea: "Crear pruebas del handler sin imagen en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs"
Tarea: "Crear pruebas de mapping explícito en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyMappingTests.cs"
Tarea: "Crear pruebas del slice PUT /api/properties/{id} en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs"
Tarea: "Definir UpdatePropertyResponse en app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyResponse.cs"
```

### Historia de usuario 2

```text
Tarea: "Crear pruebas del handler con imagen y compensación en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs"
Tarea: "Crear pruebas del slice con imagen en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs"

# Después de las pruebas y de T014:
Tarea: "Ampliar UpdatePropertyHandler en app/backend/src/RealtorApi/Features/Properties/UpdateProperty/UpdatePropertyHandler.cs"
```

### Historia de usuario 3

```text
Tarea: "Crear pruebas del validator para datos inválidos en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs"
Tarea: "Crear pruebas del slice para solicitudes mal formadas en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs"

# Después de T019, en el mismo archivo de pruebas del validator:
Tarea: "Crear pruebas del validator para imagen inválida en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyValidatorTests.cs"
```

### Historia de usuario 4

```text
Tarea: "Añadir pruebas de descubrimiento en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertySliceTests.cs"
Tarea: "Añadir prueba de componente actualización-listado en app/backend/tests/RealtorApiTests/Features/Properties/UpdateProperty/UpdatePropertyHandlerTests.cs"
```

## Estrategia de implementación

### MVP primero: historia de usuario 1

1. Completar Fase 1: Configuración.
2. Completar Fase 2: Fundacional.
3. Completar Fase 3: US1.
4. Detener y validar US1 de forma independiente.
5. Demostrar el flujo de actualización sin imagen antes de continuar.

### Entrega incremental

1. Configuración + Fundacional → base lista.
2. US1 → validar → MVP.
3. US2 → validar reemplazo de imagen → demostrar.
4. US3 → validar rechazos y errores → demostrar.
5. US4 → validar compatibilidad → demostrar.
6. Cada historia agrega valor sin romper las anteriores.

### Estrategia con equipo paralelo

1. El equipo completa Fase 1 y Fase 2 conjuntamente.
2. Después de la Fase 2, un desarrollador toma US1 y otro prepara las pruebas de US2.
3. La integración de US2, US3 y US4 se realiza en orden porque comparten archivos del caso de uso.

## Notas

- `[P]` significa archivos distintos y sin dependencias incompletas.
- `[Story]` solo aparece en las fases de usuario.
- Cada historia debe ser completable y validable de forma independiente.
- Verificar que las pruebas fallan antes de implementar.
- Detenerse en cada punto de control para validar la historia.
- Evitar tareas vagas, conflictos sobre el mismo archivo y dependencias cruzadas que rompan la independencia de las historias.
