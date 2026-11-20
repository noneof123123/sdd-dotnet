# Investigación: 008-open-api

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`  
Objetivo: resolver las incógnitas técnicas de generación, publicación, validación y detección de deriva del documento OpenAPI v1.

## R001 — Generación en tiempo de compilación

**Decisión:** usar `Microsoft.Extensions.ApiDescription.Server` con `OpenApiGenerateDocuments=true`, `OpenApiGenerateDocumentsOnBuild=true`, `OpenApiDocumentsDirectory=wwwroot/openapi`, `--document-name v1` y `--file-name v1`. La dependencia se añade con `PrivateAssets=all` para no convertirse en dependencia runtime.

**Justificación:** la spec exige un artefacto estático versionado, generado desde la implementación real y listo para validación offline. El servidor de descripción de API de ASP.NET Core está soportado oficialmente, se integra con `AddOpenApi` y produce los archivos durante `dotnet build`. La documentación oficial confirma que la generación en compilación lanza la aplicación con un servidor simulado para obtener los documentos.

**Alternativas consideradas:**
- `MapOpenApi()` en runtime guardando el JSON con una petición HTTP: rechazada porque acopla la generación a un host en ejecución y a la base de datos.
- Generador propio por reflexión sobre slices: rechazada porque duplicaría la lógica de ASP.NET Core y aumentaría la deriva.
- Acción manual de copia/pegado: rechazada por prohibición expresa de edición manual en FR-002.

**Fuente:** `https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Tools/Extensions.ApiDescription.Server/src/build/Microsoft.Extensions.ApiDescription.Server.targets` y `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-11.0`.

## R002 — Versión del documento

**Decisión:** fijar `OpenApiSpecVersion.OpenApi3_0` en la configuración de `AddOpenApi` y `--openapi-version OpenApi3_0` en `OpenApiGenerateDocumentsOptions`.

**Justificación:** `Microsoft.AspNetCore.OpenApi` 11 genera OpenAPI 3.2 por defecto, pero NSwag todavía no ofrece soporte completo para 3.1/3.2 en todas las rutas de generación de cliente. La spec exige un único documento consumible por Redocly y NSwag; fijar 3.0 garantiza compatibilidad sin mantener dos artefactos.

**Alternativas consideradas:**
- Mantener 3.2 y generar un documento 3.0 solo para NSwag: rechazada porque rompe FR-022 (una única versión de contrato) y duplica validaciones.
- Convertir el documento justo antes de NSwag: rechazada porque añade otra transformación manual y ocultaría diferencias.

## R003 — Publicación estática y ruta duplicada

**Decisión:** eliminar `app.MapOpenApi()` del bloque Development en `Program.cs` y servir `GET /openapi/v1.json` únicamente desde `UseStaticFiles()` y `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.

**Justificación:** .NET 11 mapea por defecto el documento OpenAPI en `/openapi/v1.json`. Si se mantiene junto al archivo estático, existen dos productores para la misma ruta: `UseStaticFiles()` normalmente gana antes de routing, pero el endpoint dinámico permanece en `EndpointDataSource` y contamina la detección de deriva. La aclaración A excluye la ruta de documentación del documento y de la comparación; eliminar el endpoint dinámico hace que esa exclusión sea coherente con runtime.

**Alternativas consideradas:**
- Mantener ambos y excluir solo por patrón en pruebas: rechazada porque deja comportamiento ambiguo según orden de middleware.
- Cambiar la ruta dinámica a otro valor: rechazada porque crea una segunda superficie de documentación no contemplada en la spec.

## R004 — Seguridad del arranque durante generación

**Decisión:** condicionar `await app.MigrateAsync()` para que no se ejecute cuando el entry point sea el host de generación de documentos (`GetDocument.Insider`). El arranque normal de la API conserva la migración actual.

**Justificación:** `Program.cs:51-53` ejecuta migraciones antes de `app.Run()`. La generación en compilación lanza la aplicación con un host simulado; sin esta protección, `dotnet build` podría intentar conexión a PostgreSQL y provocar migraciones no deseadas. La comprobación es una preocupación de infraestructura y no altera casos de uso ni reglas de negocio.

**Alternativas consideradas:**
- Usar variable de entorno exclusiva de CI: rechazada porque no protege generaciones locales y añade estado externo.
- Crear un `DbContext` de diseño sin migraciones: rechazada porque duplica configuración de persistencia para una característica de documentación.

## R005 — Metadatos y esquemas compartidos

**Decisión:** añadir metadatos `WithName`, `WithTags`, `WithSummary`, `Produces`, `ProducesProblem`, `ProducesValidationProblem` y declaración del cuerpo donde corresponda en las cinco slices; usar un transformador de documento y un catálogo de esquemas en `Infrastructure/OpenApi` para garantizar `components/schemas.PropertyStatus`, `components/schemas.ProblemDetails` y `components/schemas.HttpValidationProblemDetails` con referencias `$ref`.

**Justificación:** la inferencia de ASP.NET Core no garantiza nombres de operación estables, presencia de `HttpValidationProblemDetails` ni referencias compartidas. Los tipos de dominio exponen `Status` como `string`, por lo que `PropertyStatus` no aparecería automáticamente. Un transformador permite documentar la forma real sin cambiar tipos runtime.

**Alternativas consideradas:**
- Cambiar tipos de respuesta a enum y atributos de anotaciones: rechazada porque modificaría contratos de serialización más allá del alcance aprobado.
- Editar `v1.json` después de generarlo: rechazada por FR-002.
- Confiar en la inferencia: rechazada porque SC-008 exige ausencia de duplicación inline.

## R006 — Detección de deriva

**Decisión:** en pruebas xUnit en proceso, construir un `WebApplication` que registre slices solo desde la asamblea de `RealtorApi`, leer `EndpointDataSource`, parsear cada `RoutePattern` y comparar el conjunto `(método HTTP, ruta normalizada)` contra los paths/operaciones del JSON. Normalizar `{id:guid}` a `{id}`, ignorar la ruta `/openapi/v1.json` y excluir `TestSlice`, registrado desde la asamblea de pruebas. La comparación debe ser insensible al orden.

**Justificación:** la spec exige que la deriva se detecte desde la implementación real, no desde una lista escrita a mano. `RoutePattern` es la representación canónica de ASP.NET Core y evita regex frágiles. Registrar únicamente la asamblea de API evita falsos positivos por endpoints de prueba.

**Alternativas consideradas:**
- Cadena regex sobre `RouteEndpoint.RoutePattern.RawText`: rechazada porque no normaliza restricciones ni grupos.
- Comparar contra documentación estática: rechazada porque dejaría de validar la implementación real exigida por FR-001 y FR-011.
- Levantar el host real con HTTP: rechazada porque introduce dependencia de PostgreSQL y puertos.

## R007 — Configuración de Redocly

**Decisión:** crear `.redocly.yaml` en la raíz con `extends: ["recommended"]`, `rules.security-defined: off`, `rules.operation-4xx-response: warn` y reglas incompatibles con el contrato aprobado desactivadas (`no-empty-servers`, `operation-summary`, `info-license`, `tag-description`).

**Justificación:** FR-015 exige desactivar `security-defined` porque la API no define esquemas de seguridad y degradar `operation-4xx-response` a advertencia porque algunas respuestas 4xx se documentan mediante catálogo/transformadores y no necesariamente como metadata literal en cada operación. Redocly CLI 2.82.0 está disponible mediante `npx`.

**Alternativas consideradas:**
- Añadir `security: []` global: rechazada porque modificaría el documento para satisfacer una regla de linting.
- Usar `recommended-strict`: rechazada porque elevaría advertencias a errores sin aportar al alcance aprobado.
- Configurar UI de Redoc: rechazada por FR-019.

## R008 — Configuración de NSwag y herramientas locales

**Decisión:** crear `.config/dotnet-tools.json` fijando `NSwag.ConsoleCore` 14.8.0 y `support/scripts/openapi-v1.nswag.json` para generar un cliente C# de humo en `artifacts/openapi-client-smoke/`. El script invoca `dotnet tool restore` y `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json`.

**Justificación:** `dotnet tool list --local` devolvió una lista vacía y `dotnet tool list --global` solo mostró `dotnet-ef 11.0.0-rc.3.26...`; NSwag no está instalado. Fijarlo localmente hace reproducible la validación y evita depender de una instalación global.

**Alternativas consideradas:**
- Instalar NSwag globalmente: rechazada por no reproducible.
- Usar `NSwag.MSBuild`: rechazada porque acoplaría la validación al build de la solución, mientras la spec define un script de verificación separado.
- Usar solo Redocly: rechazada porque FR-014/SC-005 exigen consumibilidad por NSwag.

**Estado observado:**
- `node --version` → `v22.23.2`.
- `npm --version` → `10.9.8`.
- `npx --yes @redocly/cli --version` → `2.82.0` (con avisos npm; ejecutado con caché temporal aislada).
- `dotnet --version` → `11.0.100-rc.3.26522.148`.
- `dotnet tool list --local` → ninguna herramienta.
- `dotnet tool list --global` → solo `dotnet-ef`.

## R009 — Inventario de endpoints y estado del repositorio

**Decisión:** basar el documento y la deriva en estos cinco endpoints reales: `GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties`, `PUT /api/properties/{id}`.

**Justificación:** son los únicos endpoints de negocio/salud mapeados por `MapSliceEndpoints()` en `app/backend/src/RealtorApi/Program.cs:49` y las slices existentes. `GET /openapi/v1.json` queda excluido por aclaración A.

**Hallazgos relevantes:**
- `Program.cs:13` registra `AddOpenApi()`.
- `Program.cs:29-32` mapea actualmente `MapOpenApi()` solo en Development.
- `Program.cs:35` registra `UseStaticFiles()` antes del proveedor estático personalizado de imágenes.
- `Program.cs:36-48` sirve `/assets/properties`.
- `Program.cs:51-53` ejecuta `MigrateAsync()` en arranque normal.
- `app/backend/src/RealtorApi/RealtorApi.csproj` tiene `net11.0` y `Microsoft.AspNetCore.OpenApi` 11.0.0; no tiene `Microsoft.Extensions.ApiDescription.Server`.
- `app/backend/src/RealtorApi/wwwroot/` solo contiene `assets/properties/.gitkeep`; `openapi/v1.json` no existe.
- `artifacts/` está ignorado en `.gitignore:41`.
- `.github/skills/dotnet-webapi/SKILL.md` confirma que la ruta dinámica por defecto de .NET es `/openapi/v1.json`.

**Alternativas consideradas:** incluir `/assets/properties/{fileName}` en el documento: rechazada porque es contenido estático, no un endpoint de la API contemplado en la spec.

## Resultado

Todas las incógnitas técnicas están resueltas. No quedan marcadores `[NEEDS CLARIFICATION]`. Las decisiones R001–R009 alimentan directamente las fases D001–D009 de `specs/008-open-api/plan.md`.
