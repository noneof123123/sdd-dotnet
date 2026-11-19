# Tasks: Persistencia y seed de propiedades

**Input**: Documentos de diseño desde `/specs/003-properties-persistence-seeding/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/seed-manifest.schema.json`

**Organización**: Las tareas están agrupadas por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Story] Descripción`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos y sin dependencias pendientes).
- **[Story]**: Historia de usuario relacionada: US1, US2, US3 o US4.
- Toda descripción incluye la ruta exacta del archivo o componente afectado.

## Dependencias de fases

- **Fase 1** no tiene dependencias y puede iniciarse inmediatamente.
- **Fase 2** bloquea todas las historias de usuario.
- **Fase 3 US1** puede iniciar tras la fase 2 y constituye el MVP.
- **Fase 4 US2** depende de US1.
- **Fase 5 US3** depende de US1 y usa la infraestructura base de la fase 2.
- **Fase 6 US4** depende de US1, US2 y US3; principalmente valida compatibilidad.
- **Fase 7** es el cierre final y depende de las fases de historias deseadas.

## Fase 1: Setup

**Propósito**: Preparar dependencias y activos externos sin implementar lógica de persistencia.

- [X] T001 Añadir referencias de EF Core 11 y Npgsql Entity Framework Core compatibles con `net11.0` en `app/backend/src/RealtorApi/RealtorApi.csproj`.
- [X] T002 [P] Crear la estructura de carpetas `Domain/Properties`, `Features/Properties`, `Infrastructure/Persistence/Configurations` y `Infrastructure/Persistence/Migrations` bajo `app/backend/src/RealtorApi/`.
- [X] T003 [P] Referenciar los datos existentes en `support/seed-data/images/properties.json` y `support/seed-data/images/properties-statuses.json`, manteniendo las imágenes en `support/seed-data/images/properties/`.
- [X] T004 [P] Crear `support/seed-data/seed-manifest.json` con rutas declarativas para JSON, imágenes fuente, destino público y base URL según `specs/003-properties-persistence-seeding/contracts/seed-manifest.schema.json`.
- [X] T005 Integrar JSON, manifiesto e imágenes de `support/seed-data/` como contenido disponible en build y publish en `app/backend/src/RealtorApi/RealtorApi.csproj`.

**Checkpoint**: Dependencias y activos preparados.

## Fase 2: Foundational

**Propósito**: Completar infraestructura compartida que bloquea todas las historias de persistencia.

**⚠️ CRÍTICA**: Ninguna historia puede comenzar hasta completar esta fase.

- [X] T006 Registrar `Microsoft.EntityFrameworkCore` y Npgsql en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs` con resolución automática de `IEntityTypeConfiguration` mediante `ApplyConfigurationsFromAssembly`.
- [X] T007 Crear la extensión base de ejecución de migraciones en `app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs`, sin invocar manualmente `DatabaseSeeder`.
- [X] T008 Configurar el DbContext con `UseSeeding` y `UseAsyncSeeding` delegando a `DatabaseSeeder.Seed` y `DatabaseSeeder.SeedAsync` en `app/backend/src/RealtorApi/Program.cs`.
- [X] T009 Ejecutar siempre `await app.MigrateAsync()` antes de `app.Run()` en `app/backend/src/RealtorApi/Program.cs` y resolver el `DbContext` desde un scope; el método debe migrar bases relacionales sin fallback manual.

**Checkpoint**: Fundación EF Core y arranque de migración lista.

## Fase 3: User Story 1 - Modelo persistente de propiedades (Priority: P1) 🎯 MVP

**Goal**: Definir la entidad, su enum convertido a texto, su configuración EF y una migración única revisable.

**Independent Test**: Compilar el modelo, revisar el esquema generado y comprobar la conversión enum-string con el modelo EF y la migración.

### Pruebas para User Story 1

- [X] T010 [P] [US1] Crear pruebas unitarias de la configuración y conversión enum-string de `Property.Status` en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T011 [P] [US1] Crear una prueba que inspeccione el snapshot y/o el modelo para validar el esquema esperado de `Property` según `specs/003-properties-persistence-seeding/data-model.md`.

### Implementación para User Story 1

- [X] T012 [P] [US1] Crear el enum `PropertyStatus` con `Available`, `Rented` y `Maintenance` en `app/backend/src/RealtorApi/Domain/Properties/PropertyStatus.cs`.
- [X] T013 [P] [US1] Crear `Property` con `Property.Id`, `Property.Title`, `Property.Description`, `Property.Address`, `Property.Price`, `Property.Status`, `Property.BedroomCount`, `Property.BathroomCount`, `Property.AreaSquareMeters`, `Property.ImageUrl`, `Property.CreatedAt` y `Property.UpdatedAt` según `specs/003-properties-persistence-seeding/data-model.md` en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`.
- [X] T014 [US1] Registrar `DbSet<Property> Properties` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`.
- [X] T015 [US1] Crear `PropertyConfiguration` aplicando `data-model.md`: `Property.Id`, `Property.Title`, `Property.Description`, `Property.Address`, `Property.Price`, `Property.Status`, `Property.BedroomCount`, `Property.BathroomCount`, `Property.AreaSquareMeters`, `Property.ImageUrl`, `Property.CreatedAt` y `Property.UpdatedAt` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`; no incluir configuración inline en `OnModelCreating`.
- [X] T016 [US1] Generar exactamente una migración EF Core coherente para el esquema completo en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/` y actualizar el snapshot sin crear migraciones separadas por entidad.
- [X] T017 [US1] Revisar el `Up()`, `Down()`, nombres de tabla y columna, nulabilidad, precisión, índices y snapshot de la migración generada en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/`.

**Checkpoint**: El MVP US1 compila y su migración única contiene el modelo revisado.

## Fase 4: User Story 2 - Arranque con migración automática (Priority: P1)

**Goal**: Aplicar migraciones siempre al iniciar y ejecutar el seeding asincrónico mediante EF Core, incluso sin migraciones pendientes.

**Independent Test**: Iniciar la aplicación con base nueva y base ya migrada y verificar que el esquema queda actualizado y el seed asíncrono se ejecuta.

### Pruebas para User Story 2

- [X] T018 [P] [US2] Crear pruebas unitarias del delegado asincrónico y del flujo de migración sin invocar manualmente el seeder desde `MigrationExtensions` en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T019 [P] [US2] Crear una prueba de arranque que confirme que `MigrateAsync` se llama antes de `app.Run()` y con bases relacionales en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T020 [US2] Completar `MigrationExtensions.MigrateAsync` en `app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs` para ejecutar `Database.MigrateAsync` siempre en proveedores relacionales y permitir que `UseAsyncSeeding` ejecute el seed al finalizar.
- [X] T021 [US2] Completar el registro de `UseSeeding` y `UseAsyncSeeding` en `app/backend/src/RealtorApi/Program.cs`, propagando `CancellationToken` al delegado asincrónico y delegando ambas rutas a `DatabaseSeeder`.

**Checkpoint**: US2 verifica migración y seeding nativo en arranque nuevo y reinicio.

## Fase 5: User Story 3 - Seed idempotente y activos externos (Priority: P1)

**Goal**: Sembrar estados y propiedades desde archivos externos y sincronizar imágenes a rutas públicas sin duplicados ni rutas físicas de soporte.

**Independent Test**: Ejecutar el seed varias veces, leer el manifiesto y confirmar que propiedades e imágenes quedan consistentes y públicas.

### Pruebas para User Story 3

- [X] T022 [P] [US3] Crear pruebas de lectura del manifiesto y resolución de assets conforme al contrato `specs/003-properties-persistence-seeding/contracts/seed-manifest.schema.json` en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T023 [P] [US3] Crear pruebas de idempotencia síncrona y asincrónica para propiedades identificadas por `Property.Id` y archivos iniciales en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.
- [X] T024 [P] [US3] Crear pruebas de sincronización de imágenes y consistencia de `Property.ImageUrl` con `/images/properties/{archivo}` en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyPersistenceTests.cs`.

### Implementación para User Story 3

- [X] T025 [US3] Implementar lectura central de `seed-manifest.json`, `properties.json` y `properties-statuses.json` desde rutas relativas resueltas por el manifiesto en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`.
- [X] T026 [US3] Implementar sincronía entre `Seed` y `SeedAsync` en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`, incluyendo actualización por `Property.Id` y validación de `Property.Status` sin usar `HasData`.
- [X] T027 [US3] Sincronizar imágenes hacia el destino público declarado y asignar `Property.ImageUrl` con `/images/properties/{archivo}` sin rutas físicas de soporte en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`.
- [X] T028 [US3] Manejar JSON inválido o ausente e imagen ausente con logging estructurado y fallo seguro sin persistir datos incompletos en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs` y `app/backend/src/RealtorApi/Infrastructure/Logging/InfrastructureLog.cs`.

**Checkpoint**: US3 ejecuta seed reproducible y mantiene imágenes públicas consistentes.

## Fase 6: User Story 4 - Compatibilidad con infraestructura existente (Priority: P2)

**Goal**: Confirmar que la persistencia no modifica `/health`, el discovery de slices ni el comportamiento existente.

**Independent Test**: Ejecutar las pruebas existentes de `/health`, endpoints, validación y handlers después de incorporar EF Core y assets.

### Pruebas para User Story 4

- [X] T029 [P] [US4] Ejecutar o ampliar las pruebas existentes de `Features/Health`, discovery y mapeo en `app/backend/tests/RealtorApiTests/Features/Health/HealthSliceTests.cs` y `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/`.
- [X] T030 [P] [US4] Añadir una prueba de estructura que confirme que no existen endpoints técnicos paralelos fuera de `Features/Properties/` ni cambios manuales de registro en `app/backend/src/RealtorApi/Program.cs`.

### Implementación para User Story 4

- [X] T031 [US4] Revisar `app/backend/src/RealtorApi/Features/Health/HealthSlice.cs`, `app/backend/src/RealtorApi/Infrastructure/Endpoints/` y `app/backend/src/RealtorApi/Program.cs`; realizar solo ajustes mínimos necesarios para compatibilidad sin cambiar la respuesta de `/health` ni el discovery.
- [X] T032 [US4] Crear únicamente la carpeta reservada `app/backend/src/RealtorApi/Features/Properties/` sin endpoints ni handlers hasta que exista una tarea específica de caso de uso posterior.

**Checkpoint**: La infraestructura existente sigue aprobada y no se crean endpoints paralelos.

## Fase 7: Polish y validación final

**Propósito**: Completar publicación, documentación de ejecución y validación cruzada.

- [X] T033 [P] Actualizar `specs/003-properties-persistence-seeding/quickstart.md` con los comandos y resultados reales usados para compilar, generar la migración, publicar assets y ejecutar las pruebas.
- [X] T034 [P] Revisar `support/seed-data/seed-manifest.json` contra `specs/003-properties-persistence-seeding/contracts/seed-manifest.schema.json` y comprobar que todos los archivos referenciados existen.
- [X] T035 Revisar y ejecutar build/publish de `app/backend/src/RealtorApi/RealtorApi.csproj`, verificando JSON, manifiesto e imágenes en la salida publicada.
- [X] T036 Ejecutar todas las pruebas unitarias de `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`, incluyendo configuración EF, migración, seed, activos y regresiones existentes.
- [X] T037 Revisar el diff final para confirmar una sola migración, ausencia de `HasData`, ausencia de seeder manual fuera de EF Core, ausencia de controllers y trazabilidad de todos los cambios a estas tareas.

## Dependencias entre historias

- **US1** desbloquea el modelo EF y debe completarse antes de generar la migración y sembrar propiedades.
- **US2** depende de US1 y activa el arranque mediante migración y delegados EF.
- **US3** depende de US1 y del contexto fundacional US2, pero puede desarrollarse en paralelo con parte de US2 después de completar el modelo.
- **US4** depende de US1-US3 para validar compatibilidad y no necesita modificar la arquitectura existente.

## Ejemplos de ejecución paralela

### Después de Fase 2

- T010/T011 pueden ejecutar pruebas de US1 en paralelo.
- T012/T013 pueden crear enum y entidad en archivos diferentes en paralelo.
- T018/T019 pueden ejecutar pruebas de arranque de US2 en paralelo.
- T022/T023/T024 pueden ejecutar pruebas de assets y seed de US3 en paralelo.
- T029/T030 pueden ejecutar regresiones de compatibilidad de US4 en paralelo.

### Historias independientes

US1 y US3 pueden prepararse parcialmente en paralelo una vez exista el contexto base, pero la migración de US1 debe cerrar antes de ejecutar seeds dependientes del esquema.

## Estrategia de implementación

### MVP: User Story 1

1. Completar Fases 1 y 2.
2. Completar US1 y revisar una única migración.
3. Detenerse y validar el modelo, configuración y esquema.

### Incrementos

1. Añadir US2 para arranque y migración automática.
2. Añadir US3 para datos iniciales e imágenes.
3. Añadir US4 para validar compatibilidad end to end.
4. Completar Fase 7 y publicar evidencia en `quickstart.md`.
