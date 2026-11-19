# Validación rápida: Persistencia y seed de propiedades

## Requisitos previos

- SDK .NET indicado por `global.json`.
- PostgreSQL accesible mediante `ConnectionStrings:DefaultConnection`.
- Archivos presentes en `support/seed-data`:
  - `seed-manifest.json`
  - `images/properties.json`
  - `images/properties-statuses.json`
  - `images/properties/1.png` hasta `10.png`

## Validar compilación y modelo

1. Restaurar y compilar la solución Realtor.
2. Revisar que existe una única migración EF Core para esta feature.
3. Revisar `Up()`, `Down()` y el snapshot antes de iniciar la aplicación.
4. Confirmar que `Property.Status` usa conversión enum-string y que `PropertyConfiguration` aplica las restricciones declaradas.

## Validar publicación de assets

1. Ejecutar build y publish del proyecto `RealtorApi`.
2. Confirmar que el manifiesto y los dos JSON quedan disponibles en el resultado publicado.
3. Confirmar que las imágenes quedan bajo `wwwroot/images/properties` o en el destino publicado declarado por el manifiesto.
4. Confirmar que no existen rutas físicas de `support/` persistidas en `Property.ImageUrl`.

## Validar arranque y migración

1. Iniciar con una base PostgreSQL nueva.
2. Confirmar que `MigrateAsync()` crea la tabla `properties` y aplica una sola migración.
3. Reiniciar la aplicación con la base ya migrada.
4. Confirmar que el arranque continúa y que `UseAsyncSeeding` se ejecuta aunque no existan migraciones pendientes.

## Validar seed idempotente

1. Ejecutar el arranque y contar los estados y propiedades iniciales.
2. Reiniciar varias veces.
3. Confirmar que no aparecen propiedades duplicadas ni cambios incorrectos en registros existentes válidos.
4. Confirmar que las propiedades usan Ids del JSON y rutas `/images/properties/{archivo}`.

## Validar pruebas unitarias

1. Ejecutar las pruebas del proyecto `RealtorApiTests`.
2. Confirmar cobertura de configuración EF, conversión enum-string, revisión del esquema de migración, idempotencia, ejecución asincrónica, lectura del manifiesto y sincronización de imágenes.
3. Confirmar que las pruebas existentes de `/health` siguen pasando.

## Evidencia esperada

- Compilación correcta sin migraciones duplicadas.
- Una tabla `properties` con las restricciones revisadas.
- Seed reproducible y sin duplicados.
- Manifiesto, JSON e imágenes disponibles en build/publish.
- `ImageUrl` público válido para cada propiedad inicial.
- `/health` operativo sin cambios funcionales.

## Evidencia ejecutada

**Fecha:** 2026-11-19

- `dotnet build app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-restore` — éxito; backend y proyecto de pruebas compilaron.
- `dotnet ef migrations add AddPropertiesPersistence ...` — éxito; se generó una única migración `20261119185758_AddPropertiesPersistence` y su snapshot.
- `dotnet publish app/backend/src/RealtorApi/RealtorApi.csproj --no-restore -o /tmp/realtor-publish` — éxito; build publicado sin errores.
- Publicación revisada: `seed-manifest.json`, `images/properties.json`, `images/properties-statuses.json` y 10 imágenes quedaron bajo `seed-data/images/properties/`; las 10 imágenes quedaron además bajo `wwwroot/images/properties/`.
- Validación del manifiesto: manifiesto, JSON de propiedades, JSON de estados y las 10 imágenes existen y son resolubles.
- `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build` — éxito: 16 pruebas aprobadas.
- `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` — éxito final: 18 pruebas aprobadas, 0 fallidas.
- Revisión del código: una migración, sin `HasData`, sin controllers, sin llamadas manuales al seeder desde migraciones ni arranque, y `/health` conserva su implementación existente.
