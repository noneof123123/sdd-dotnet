# Guía de validación: 008-open-api

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Prerrequisitos

- .NET SDK 11; versión observada en el entorno: `11.0.100-rc.3.26522.148`.
- Node.js 20+ y npm; versión observada: Node `v22.23.2`, npm `10.9.8`.
- Acceso a `npx --yes @redocly/cli` (versión observada: 2.82.0).
- PostgreSQL configurado solo si se desea arrancar la API completa; la generación en compilación no debe depender de él.
- Ejecutar todos los comandos desde la raíz del repositorio.

## 1. Verificar herramientas

```bash
dotnet --version
node --version
npm --version
npx --yes @redocly/cli --version
dotnet tool list --local
```

Resultado esperado: .NET 11, Node 20+, npm disponible, Redocly disponible y `nswag` 14.8.0 listado después de crear/restaurar el manifiesto local.

## 2. Regenerar y validar el documento

```bash
support/scripts/generate-openapi-v1.sh
```

El script debe:

1. Compilar `app/Realtor.sln` y generar `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
2. Verificar que el archivo existe y no está vacío.
3. Ejecutar Redocly CLI con `.redocly.yaml`.
4. Restaurar NSwag desde `.config/dotnet-tools.json`.
5. Generar el cliente de humo en `artifacts/openapi-client-smoke/`.
6. Imprimir `OK: OpenAPI v1 regenerado y validado`.

## 3. Ejecutar pruebas de OpenAPI

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~OpenApi"
```

Cobertura esperada:

- Estructura del documento: versión 3.0, tres paths con cinco operaciones, esquemas compartidos y normalización `{id}`.
- Deriva: documento contra endpoints reales desde la asamblea de API.
- Herramientas: Redocly y NSwag invocados por el script.
- Ruta estática: contenido servido desde `wwwroot/openapi/v1.json`.

## 4. Ejecutar la suite completa

```bash
dotnet build app/Realtor.sln --no-restore
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Resultado esperado: build sin errores y toda la suite en verde; ejecución observada: 158 pruebas, 0 fallidas, 0 omitidas.

## 5. Verificar la ruta pública

La comprobación HTTP completa requiere la API en ejecución con PostgreSQL configurado, porque el arranque normal aplica migraciones:

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
curl -fsS -H 'Accept: application/json' http://localhost:5023/openapi/v1.json | jq -r '.openapi, .info.version, (.paths | keys | length)'
```

Salida esperada:

```text
3.0.x
v1
3
```

El documento contiene tres paths y cinco operaciones.

También puede verificarse el archivo directamente:

```bash
test -s app/backend/src/RealtorApi/wwwroot/openapi/v1.json
jq -e '.paths | has("/openapi/v1.json") | not' app/backend/src/RealtorApi/wwwroot/openapi/v1.json
jq -e '.components.schemas | has("PropertyStatus") and has("ProblemDetails") and has("HttpValidationProblemDetails")' app/backend/src/RealtorApi/wwwroot/openapi/v1.json
```

## 6. Comprobaciones manuales de no regresión

- Confirmar que no aparece `Swagger UI`, `ReDoc UI` ni configuración visual en el repositorio.
- Confirmar que `git status --short` no muestra cambios manuales en `v1.json` fuera de la regeneración.
- Confirmar que `artifacts/openapi-client-smoke/` no aparece como archivo versionado.
- Confirmar que el documento no incluye `GET /openapi/v1.json` en `paths`.
- Confirmar que no queda `MapOpenApi()` en `Program.cs`.
- Confirmar que `v1.json` no se edita manualmente y que una regeneración con el mismo código y dependencias reproduce el mismo contenido.

## Evidencia

- Fecha de ejecución: 2026-11-20.
- `support/scripts/generate-openapi-v1.sh`: cinco fases completadas con `[OK]`; build sin errores; Redocly válido con dos advertencias esperadas de `operation-4xx-response`; NSwag generó `artifacts/openapi-client-smoke/RealtorApiClient.cs`; reporte final `OK: OpenAPI v1 regenerado y validado` con ruta `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
- Pruebas OpenAPI: 15 pasadas, 0 fallidas, 0 omitidas.
- Suite completa: 158 pasadas, 0 fallidas, 0 omitidas.
- Verificación HTTP en proceso de `GET /openapi/v1.json`: `200 OK`, `Content-Type application/json`, `info.version` `v1`.
- Verificación directa del archivo: `openapi` `3.0.4`, `info.version` `v1`, tres paths, cinco operaciones, sin `/openapi/v1.json` en `paths`.
- `git status --short`: `v1.json` aparece como artefacto generado nuevo; `artifacts/openapi-client-smoke/` no aparece versionado; `git diff --check` sin hallazgos.
