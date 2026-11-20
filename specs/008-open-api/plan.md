# Plan de implementación: 008-open-api

Rama lógica: `008-open-api`  
Rama local actual: `007-properties-get-by-id`  
Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Resumen

`008-open-api` publica un único documento OpenAPI v1 para RealtorApi, generado automáticamente desde la implementación real de los cinco endpoints funcionales (`GET /health`, `GET /api/properties`, `GET /api/properties/{id}`, `POST /api/properties`, `PUT /api/properties/{id}`). El documento se escribe como archivo estático versionado en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` y se sirve en `GET /openapi/v1.json`. No se publica Swagger UI ni ReDoc UI.

La spec aprobada es la fuente de verdad del proyecto; dentro de su alcance, el código de la API es la entrada de generación del documento. La generación ocurre en tiempo de compilación con `Microsoft.Extensions.ApiDescription.Server`, se normalizan las restricciones de ruta `{id:guid}` a `{id}`, se canonicalizan los esquemas compartidos `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` en `components/schemas`, y se valida con Redocly CLI y NSwag CLI de forma reproducible desde `support/scripts/generate-openapi-v1.sh`.

La ruta de documentación `GET /openapi/v1.json` queda explícitamente fuera del propio documento y de la comparación de deriva, según la aclaración A aceptada en `specs/008-open-api/spec.md`. El alcance únicamente cubre la API HTTP; no modifica reglas de negocio, persistencia ni contratos de dominio.

## Contexto técnico

- Lenguaje/versión: C# del SDK .NET 11; SDK observado `11.0.100-rc.3.26522.148`.
- Framework principal: ASP.NET Core 11 con Minimal APIs y `Microsoft.AspNetCore.OpenApi` 11.0.0.
- Herramienta de generación: `Microsoft.Extensions.ApiDescription.Server` 11.0.0, referencia de solo compilación (`PrivateAssets=all`), sin exposición runtime.
- Versión del documento: OpenAPI 3.0, fijada en servicios y en la opción `--openapi-version OpenApi3_0` para compatibilidad con NSwag.
- Salida generada: `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, ruta pública `GET /openapi/v1.json`.
- Validadores: `@redocly/cli` 2.82.0 observado vía `npx`, NSwag.ConsoleCore 14.8.0 fijado en `.config/dotnet-tools.json`.
- Entorno Node: Node `v22.23.2` y npm `10.9.8` disponibles; la spec exige Node 20+.
- Persistencia: sin nuevas tablas, migraciones ni contexto de base de datos; PostgreSQL sigue siendo necesidad del arranque normal de la API.
- Pruebas: xUnit 2.9.3; pruebas unitarias/en proceso para estructura, normalización y deriva; pruebas de humo de CLI justificadas explícitamente por la sección «Renuncia explícita al principio V» de la spec aprobada.
- Configuración de herramientas: `.redocly.yaml`, `.config/dotnet-tools.json` y `support/scripts/openapi-v1.nswag.json` son nuevos archivos de infraestructura de desarrollo.
- Artefactos temporales: `artifacts/openapi-client-smoke/` queda ignorado por `.gitignore:41`.
- Determinismo: la generación debe ser byte-idéntica para un mismo código, configuración y dependencias.
- Plataforma objetivo: Linux; el entorno actual no dispone de `pwsh`. El usuario dio instrucción humana explícita para continuar emulando las acciones de los scripts PowerShell de Spec Kit desde `.specify/feature.json`; esta decisión queda registrada aquí conforme al principio VII.
- Estado de aclaraciones: sin marcadores `[NEEDS CLARIFICATION]`; aclaración A integrada y Checklist 16/16.

## Comprobación de la constitución

### Pre-diseño

| Principio | Resultado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | PASS | La característica usa la solución `app/Realtor.sln` existente; no crea bifurcaciones ni estructuras paralelas. |
| II. Spec-Driven Development | PASS | `spec.md` está `Aprobada`; el flujo spec → plan → tasks → analyze se ha completado sin saltar fases obligatorias. |
| III. Arquitectura Canónica | PASS | No se introducen controladores ni carpetas técnicas para orquestar dominio; `Program.cs` solo configura OpenAPI, middleware estático, migraciones y mapeo de slices. |
| IV. Stack Tecnológico Obligatorio | PASS | Se conservan .NET 11, Minimal APIs, EF Core/Npgsql/PostgreSQL y FluentValidation; Node, Redocly y NSwag son herramientas de validación, no runtime. |
| V. Calidad de Dominio y Contratos | DEVIACIÓN JUSTIFICADA | La spec aprobada contiene una renuncia explícita en «Renuncia explícita al principio V»: las pruebas CLI y de deriva no son unitarias porque requieren herramientas externas y `EndpointDataSource` real. |
| VI. Idioma y Documentación | PASS | Toda documentación de specs, plan, contratos y guías está en español; se permiten nombres técnicos, comandos y paquetes. |
| VII. Gobernanza y Divergencias | PASS | La divergencia `pwsh` fue reportada y resuelta mediante instrucción humana explícita registrada en el contexto técnico. |

### Posterior al diseño

| Principio | Resultado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | PASS | El script compila `app/Realtor.sln`; el cliente de humo queda en `artifacts/`, fuera de la solución. |
| II. Spec-Driven Development | PASS | El plan cubre FR-001 a FR-022 y SC-001 a SC-011 sin ampliar el alcance aprobado. |
| III. Arquitectura Canónica | PASS | Los metadatos OpenAPI se añaden a las cinco slices existentes; la infraestructura de OpenAPI solo presenta el contrato HTTP. |
| IV. Stack Tecnológico Obligatorio | PASS | `Microsoft.Extensions.ApiDescription.Server` queda confinado a MSBuild; no se sustituye ningún componente del stack. |
| V. Calidad de Dominio y Contratos | DEVIACIÓN JUSTIFICADA | La renuncia aprobada se refleja en `tasks.md`; las pruebas externas validan consumibilidad, linting y deriva real. |
| VI. Idioma y Documentación | PASS | Los artefactos generados en esta fase están en español y usan términos técnicos solo cuando corresponden. |
| VII. Gobernanza y Divergencias | PASS | No se han introducido cambios silenciosos de estado ni de scripts; las decisiones humanas quedan registradas. |

## Fase 0: Investigación

Los resultados se registran en `specs/008-open-api/research.md` con formato Decisión / Justificación / Alternativas.

- R001: generación en compilación mediante `Microsoft.Extensions.ApiDescription.Server`, `OpenApiGenerateDocuments=true`, `OpenApiGenerateDocumentsOnBuild=true`, `OpenApiDocumentsDirectory=wwwroot/openapi` y `--file-name v1 --document-name v1`.
- R002: fijar OpenAPI 3.0 en `AddOpenApi` y en la opción del generador para que el único documento sea consumible por NSwag.
- R003: eliminar `app.MapOpenApi()` del arranque en Development; `UseStaticFiles()` debe ser la única fuente de `GET /openapi/v1.json` en runtime.
- R004: proteger `await app.MigrateAsync()` con una comprobación de host de generación (`GetDocument.Insider`) para que compilar no requiera PostgreSQL ni produzca migraciones.
- R005: usar metadatos de Minimal API y transformadores de documento/esquema para nombres de operación, respuestas, códigos de error y `$ref` de esquemas compartidos.
- R006: detectar deriva desde `EndpointDataSource` en pruebas en proceso, normalizando `RoutePattern` y excluyendo la ruta de documentación y endpoints de prueba.
- R007: confirmar que `.redocly.yaml` desactiva `security-defined` y degrada `operation-4xx-response` a `warn`, según FR-015.
- R008: confirmar inventario de herramientas: Node/npm/npx y Redocly disponibles; NSwag requiere manifiesto local fijado.
- R009: inventariar los cinco endpoints reales y sus tipos de respuesta/validación para la comparación de deriva.

Resultado: las incógnitas técnicas de `spec.md` quedan resueltas sin requerir una nueva aclaración al usuario.

## Fase 1: Diseño y contratos

### D001 — Configuración de generación

Modificar `app/backend/src/RealtorApi/RealtorApi.csproj`:

- Añadir `Microsoft.Extensions.ApiDescription.Server` 11.0.0 con `PrivateAssets=all`.
- Activar generación en compilación y directorio de salida `wwwroot/openapi`.
- Fijar `--document-name v1`, `--file-name v1` y `--openapi-version OpenApi3_0`.
- No añadir dependencias de runtime ni reemplazar `Microsoft.AspNetCore.OpenApi` 11.0.0.

### D002 — Arranque de la API

Modificar `app/backend/src/RealtorApi/Program.cs`:

- Configurar `AddOpenApi` con versión 3.0, metadatos de documento y transformadores de `Infrastructure/OpenApi`.
- Mantener `UseStaticFiles()` antes del mapeo de slices.
- Eliminar el bloque Development que mapea `MapOpenApi()` para evitar una segunda fuente de `/openapi/v1.json`.
- Condicionar `app.MigrateAsync()` al host normal, excluyendo el host de generación de documentos.

### D003 — Infraestructura OpenAPI

Crear `app/backend/src/RealtorApi/Infrastructure/OpenApi/`:

- `OpenApiDocumentTransformer.cs`: título, descripción y versión del documento; canonicalización de respuestas y esquemas compartidos; exclusión de la ruta de documentación.
- `OpenApiSchemaCatalog.cs`: registro determinista de `PropertyStatus`, `ProblemDetails` y `HttpValidationProblemDetails` en `components/schemas`.
- `OpenApiConstants.cs`: nombres de documento, esquemas, tags y prefijos de operación.

Esta infraestructura es de presentación de contrato HTTP; no contiene reglas de negocio ni orquestación de casos de uso.

### D004 — Metadatos por slice

Actualizar los cinco slices existentes con metadatos OpenAPI:

- `GET /health`: `GetHealth`, tag `Salud`, respuestas 200/500 con `ProblemDetails`.
- `GET /api/properties`: `GetProperties`, tag `Propiedades`, parámetros `page` y `pageSize`, respuestas 200/500 con `ProblemDetails`.
- `GET /api/properties/{id}`: `GetPropertyById`, tag `Propiedades`, respuestas 200/404.
- `POST /api/properties`: `CreateProperty`, tag `Propiedades`, respuestas 201/400/409; tipo de cuerpo de entrada declarado.
- `PUT /api/properties/{id}`: `UpdateProperty`, tag `Propiedades`, respuestas 200/400/404/409; tipo de cuerpo de entrada declarado.

Los metadatos deben conservar el comportamiento runtime y los `ProblemDetails` existentes.

### D005 — Configuración de Redocly

Crear `.redocly.yaml` en la raíz:

- Extender `recommended`.
- `rules.security-defined: off`.
- `rules.operation-4xx-response: warn`.
- Desactivar reglas incompatibles con el contrato aprobado: `no-empty-servers`, `operation-summary`, `info-license` y `tag-description`.
- No incluir configuración de UI, temas ni servidores visuales.

### D006 — Configuración de NSwag

Crear `.config/dotnet-tools.json` y `support/scripts/openapi-v1.nswag.json`:

- Fijar `NSwag.ConsoleCore` 14.8.0 como herramienta local.
- Generar un cliente C# de humo desde `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` hacia `artifacts/openapi-client-smoke/`.
- El cliente generado es artefacto de validación, ignorado por Git y no referenciado por la solución.

### D007 — Script de regeneración

Completar `support/scripts/generate-openapi-v1.sh` con secuencia fail-fast:

1. `dotnet build app/Realtor.sln`.
2. Verificar existencia y no vaciedad de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
3. `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.
4. `dotnet tool restore`.
5. `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json`.
6. Reporte final con mensajes `[OK]`/`[ERROR]`, ruta del artefacto y estado de cada fase.

El script debe funcionar desde la raíz del repositorio, usar rutas relativas y devolver código distinto de cero ante cualquier fallo.

### D008 — Pruebas

Crear `app/backend/tests/RealtorApiTests/OpenApi/`:

- `OpenApiDocumentTests.cs`: versión 3.0, `info.version` igual a `v1`, cinco paths y operaciones, esquemas compartidos con `$ref`, ausencia de ruta de documentación, normalización `{id}` y respuesta HTTP `200` con `Content-Type application/json` para `GET /openapi/v1.json`.
- `OpenApiDriftTests.cs`: compara el documento con `EndpointDataSource` de la asamblea de API, ignorando la ruta de documentación y endpoints de prueba; la comparación es insensible al orden de rutas/operaciones.
- `OpenApiToolingTests.cs`: ejecuta Redocly y NSwag con los comandos canónicos; usa tiempo de espera acotado y reporta claramente prerrequisitos ausentes.
- `OpenApiScriptTests.cs`: verifica éxito del script y fallos explícitos inducidos con configuración temporal, documento inválido o PATH simulado.
- Helper de aplicación de pruebas que registre slices únicamente desde la asamblea de `RealtorApi` para no incluir `TestSlice`.

### D009 — Guía de validación

Actualizar `specs/008-open-api/quickstart.md` con prerrequisitos, comandos canónicos de regeneración, validación y verificación de la ruta estática, manteniendo la sección de evidencia pendiente para implementación.

### Orden y dependencias

- R001–R004 preceden a D001–D004 porque fijan el mecanismo de generación y la seguridad del arranque.
- D001, D003 y D004 preceden a D008 porque producen el documento que se valida.
- D005 y D006 preceden a la validación CLI directa de D008 en US1; D007 se valida de extremo a extremo en US3.
- D009 se actualiza al final para reflejar los comandos reales y la evidencia de implementación.

## Estructura del proyecto

### Documentación

```text
specs/008-open-api/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/
│   └── requirements.md
├── contracts/
│   ├── get-openapi-v1.http.md
│   ├── openapi-v1-document.md
│   └── generate-openapi-v1.cli.md
└── tasks.md
```

`tasks.md` ya fue generado con `/speckit.tasks` y debe regenerarse tras cambios de plan o spec.

### Código y configuración

```text
.redocly.yaml
.config/dotnet-tools.json
support/scripts/generate-openapi-v1.sh
support/scripts/openapi-v1.nswag.json
app/backend/src/RealtorApi/
├── Program.cs
├── RealtorApi.csproj
├── Infrastructure/OpenApi/
│   ├── OpenApiConstants.cs
│   ├── OpenApiDocumentTransformer.cs
│   └── OpenApiSchemaCatalog.cs
└── wwwroot/openapi/v1.json
artifacts/openapi-client-smoke/
app/backend/tests/RealtorApiTests/OpenApi/
├── OpenApiDocumentTests.cs
├── OpenApiDriftTests.cs
├── OpenApiEndpointInventory.cs
├── OpenApiScriptTests.cs
├── OpenApiTestApp.cs
└── OpenApiToolingTests.cs
```

`artifacts/openapi-client-smoke/` queda ignorado por Git. No se crean controladores, repositorios, MediatR, AutoMapper ni nuevos proyectos de solución.

## Fase 2: Descomposición

La descomposición en tareas, estimaciones y dependencias se genera con `/speckit.tasks` a partir de `plan.md`, `spec.md`, `contracts/` y `research.md`.

## Seguimiento de complejidad

| Violación | Por qué es necesaria | Alternativa más simple rechazada |
|---|---|---|
| Pruebas xUnit que invocan Redocly CLI y NSwag CLI y comparan deriva real de endpoints. | La sección «Renuncia explícita al principio V» de la spec aprobada justifica esta excepción; FR-011, FR-013, FR-014, SC-003, SC-004 y SC-005 exigen demostrar consumibilidad, linting y deriva real. Una prueba puramente unitaria no puede validar esas condiciones. | Solo pruebas unitarias que lean el JSON generado; rechazadas porque no detectarían errores de linting, generación de cliente ni desalineación con `EndpointDataSource`. |
