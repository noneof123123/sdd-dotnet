# Quickstart: Validación de la fundación backend

## Prerequisitos

- Ejecutar desde la raíz del repositorio.
- Usar el SDK seleccionado por `global.json`.
- Tener disponibles las fuentes NuGet configuradas para restaurar FluentValidation.

## Restaurar y compilar

```bash
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
```

Resultado esperado: la solución restaura y compila sin errores y sin modificar `global.json`.

## Ejecutar pruebas backend

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-restore
```

Resultado esperado: pasan las pruebas unitarias de discovery de `ISlice`, validación con y sin validator, `ValidationProblemDetails`, mapping de `Result`, discovery de `IHandler` y `/health`.

## Validar /health

Ejecutar la API y solicitar:

```bash
curl -i http://localhost:<puerto>/health
```

Resultado esperado: HTTP 200 con el payload JSON:

```json
{"status":"healthy"}
```

## Verificación arquitectónica

Confirmar que:

- `Program.cs` no registra slices, handlers o validators individualmente.
- `/health` es una implementación de `ISlice` descubierta por el assembly.
- No existen controllers, entidades, `AppDbContext`, migraciones, seeders ni conexiones de base de datos.
- Las clases auxiliares de prueba viven bajo `app/backend/tests/RealtorApiTests`.

## Alcance fuera de la validación

No se ejecutan pruebas de integración con `WebApplicationFactory`, ni pruebas con PostgreSQL o Testcontainers, porque no están aprobadas por esta iniciativa.

## Evidencia ejecutada

**Fecha:** 2026-11-19

### Compilación completa

Comando ejecutado:

```bash
dotnet build app/Realtor.sln --no-restore
```

Resultado:

- Backend `RealtorApi`: compilado correctamente.
- Proyecto de tests backend `RealtorApiTests`: compilado correctamente.
- Frontend `RealtorWeb`: compilado correctamente.
- Proyecto de tests frontend `RealtorWebTests`: compilado correctamente.
- Resultado general: `Build succeeded`.

### Pruebas unitarias backend

Comando ejecutado:

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Resultado:

- Total: 11.
- Exitosas: 11.
- Fallidas: 0.
- Resultado general: `Build succeeded` y `Test summary succeeded`.

### Exclusiones arquitectónicas

Comando de revisión ejecutado:

```bash
grep -RniE 'AppDbContext|DbContext|Controller|EntityFramework|Npgsql|Migration|Seeder' app/backend/src/RealtorApi --include='*.cs' --include='*.csproj'
```

Resultado: sin coincidencias prohibidas en el backend.

### Integridad de global.json

Comando de revisión ejecutado:

```bash
git diff --stat -- global.json
```

Resultado: sin cambios reportados.
