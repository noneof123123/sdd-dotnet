# Guía rápida de validación: Consulta de propiedad por identificador

**Fecha**: 2026-11-20

**Objetivo**: Validar la operación `GET /api/properties/{id}` y comprobar que no afecta a las capacidades existentes.

## Prerrequisitos

- SDK de .NET 11 disponible (`dotnet --version`).
- Solución `app/Realtor.sln` restaurable.
- PostgreSQL disponible para ejecución manual, o base en memoria para las pruebas automatizadas.
- Ejemplos HTTP de [contracts/get-property-by-id.http.md](contracts/get-property-by-id.http.md) y definición OpenAPI de [contracts/backend-properties-get-by-id.openapi.yaml](contracts/backend-properties-get-by-id.openapi.yaml).

## Comandos de validación automatizada

Desde la raíz del repositorio:

```bash
dotnet build app/Realtor.sln
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~GetPropertyById"
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Resultado esperado: compilación sin errores y todas las pruebas en verde, incluidas las de creación, listado, actualización, salud y persistencia.

## Escenarios de validación manual

1. **Consulta con identificador existente y con imagen**
   - Ejecutar el ejemplo `Consultar propiedad existente con imagen` de `RealtorApi.http`.
   - Esperado: HTTP 200, diez campos del listado más `createdAt` y `updatedAt`, `imageUrl` absoluta con el host usado en la solicitud.

2. **Consulta con identificador existente sin imagen**
   - Ejecutar el ejemplo `Consultar propiedad existente sin imagen`.
   - Esperado: HTTP 200 y `imageUrl` como cadena vacía.

3. **Consulta con identificador inexistente**
   - Ejecutar el ejemplo `Error: propiedad inexistente al consultar`.
   - Esperado: HTTP 404 con `ProblemDetails` y sin cambios en los datos.

4. **Consulta con identificador mal formado**
   - Reemplazar `{id}` por un texto no convertible a identificador.
   - Esperado: HTTP 400 con errores estructurados.

5. **Host o esquema distinto**
   - Repetir la consulta con imagen cambiando el host o usando HTTPS.
   - Esperado: `imageUrl` refleja el esquema y host de la solicitud, sin rutas físicas ni relativas.

6. **Compatibilidad y solo lectura**
   - Consultar el listado antes y después de la consulta por identificador.
   - Esperado: mismos datos, mismos metadatos de paginación y ningún archivo creado, modificado o eliminado.

7. **Cancelación**
   - Ejecutar la prueba unitaria de cancelación del handler.
   - Esperado: se propaga la cancelación y no se produce ningún cambio observable.

## Evidencia de ejecución

- **Fecha**: 2026-11-20.
- **SDK**: `dotnet --version` → `11.0.100-rc.3.26522.148`.
- **Compilación**: `dotnet build app/Realtor.sln` → `Build succeeded`, `0 Warning(s)`, `0 Error(s)`.
- **Pruebas de la característica**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~GetPropertyById"` → `Passed: 35`, `Failed: 0`, `Skipped: 0`.
- **Suite completa del backend**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build` → `Passed: 143`, `Failed: 0`, `Skipped: 0`.
- **Ruta, middleware y serialización**: verificados con `GetPropertyByIdSliceTests.Get_ReturnsOk_WithExpectedJson`, `Get_WithMalformedId_ReturnsBadRequestProblemDetails` y `Get_WithUnknownId_ReturnsNotFoundProblemDetails` sobre `WebApplication` real con base de datos en memoria. No se ejecutó `dotnet run`/`curl` porque el arranque aplica migraciones a la PostgreSQL configurada; las pruebas in-process ejercen el endpoint mapeado, el filtro de validación y el cuerpo JSON.
- **Revisión de cambios**: `git diff --check` sin hallazgos.
