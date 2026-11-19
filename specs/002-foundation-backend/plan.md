# Implementation Plan: Fundación interna del backend

**Branch**: `002-foundation-backend` | **Date**: 2026-11-19 | **Spec**: [spec.md](spec.md)

**Input**: Especificación de la característica en `/specs/002-foundation-backend/spec.md`

## Summary

Ampliar el backend existente de Realtor con infraestructura transversal reutilizable: descubrimiento y mapeo de `ISlice`, auto-registro de `IHandler`, validación automática con FluentValidation, conversión centralizada de resultados esperados a `ProblemDetails`, logging estructurado y migración de `/health`. La implementación no crea dominio, persistencia, handlers de negocio ni entidades.

## Technical Context

**Language/Version**: C# sobre .NET 11, definido por `global.json`.

**Primary Dependencies**: ASP.NET Core Minimal APIs, FluentValidation 12.2.0 y las APIs existentes de Microsoft.Extensions.DependencyInjection y Microsoft.Extensions.Logging.

**Storage**: N/A. No se crea EF Core, Npgsql, PostgreSQL, DbContext, migraciones ni seeders.

**Testing**: xUnit en `app/backend/tests/RealtorApiTests`; únicamente unit tests. Se podrán usar clases de prueba en ese proyecto para `ISlice`, `IValidator<T>` e implementaciones de prueba de `IHandler`.

**Target Platform**: Linux, aplicación web ASP.NET Core net11.0.

**Project Type**: Backend dentro de una solución compartida con Blazor frontend.

**Performance Goals**: Sin objetivos de rendimiento numéricos. El assembly scanning debe ejecutarse una sola vez durante el arranque y no introducir registries manuales.

**Constraints**: Sin controllers, sin lógica de negocio, sin persistencia, sin endpoints de producto, sin modificar `global.json`, sin integración HTTP real obligatoria y sin dependencias de contenedores.

**Scale/Scope**: Infraestructura transversal pequeña dentro de `RealtorApi` y una sonda `/health`; sin features futuras implementadas.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Solución única**: aprobado. La iniciativa modifica únicamente el backend dentro de `app/Realtor.sln`.
- **Spec-Driven Development**: aprobado. Todas las decisiones y archivos se derivan de `spec.md` y están destinados a tareas posteriores.
- **Backend Minimal APIs**: aprobado. No se usan controllers y todos los endpoints se implementarán mediante `ISlice`.
- **Vertical Slice**: aprobado con la excepción fundacional explícita. No existen features ni casos de uso; la infraestructura compartida se limita a `Infrastructure/Endpoints`, `Infrastructure/Validation` y `Infrastructure/Handlers`.
- **Stack obligatorio**: aprobado. `.NET 11` proviene de `global.json`; FluentValidation y logging estructurado se incorporan según la constitución.
- **Persistencia**: aprobado con no creación de persistencia. Esta iniciativa no introduce EF Core, PostgreSQL ni AppDbContext.
- **Calidad y pruebas**: aprobado. Las pruebas serán unitarias y los fixtures o tipos de prueba permanecerán en el proyecto de tests.
- **Idioma**: aprobado. Los artefactos Markdown estarán en español.

## Project Structure

### Documentation (this feature)

```text
specs/002-foundation-backend/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── health.http
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
app/backend/src/RealtorApi/
├── Infrastructure/
│   ├── Endpoints/
│   │   ├── ISlice.cs
│   │   ├── RegisterSlices.cs
│   │   └── MapSliceEndpoints.cs
│   ├── Handlers/
│   │   ├── IHandler.cs
│   │   └── RegisterHandlers.cs
│   ├── Validation/
│   │   └── ValidationFilterFactory.cs
│   └── Results/
│       ├── Result.cs
│       ├── Error.cs
│       └── ProblemDetailsMapper.cs
├── Features/
│   └── Health/              # único slice funcional de infraestructura
│       └── HealthSlice.cs
├── Program.cs
└── RealtorApi.csproj

app/backend/tests/RealtorApiTests/
├── Infrastructure/
│   ├── Endpoints/
│   ├── Validation/
│   ├── Handlers/
│   └── Results/
└── RealtorApiTests.csproj
```

**Structure Decision**: Mantener la infraestructura transversal realmente compartida bajo `Infrastructure/` y ubicar `/health` como `Features/Health/HealthSlice.cs`. Los tipos usados exclusivamente para probar discovery, validación y handlers deben vivir en el proyecto de tests.

## Complexity Tracking

No existe una desviación constitucional que requiera justificación. Una sola implementación por infraestructura evita duplicar scanners o registros manuales.
