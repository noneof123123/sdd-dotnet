# Implementation Plan: Paginación y listado de propiedades

**Branch**: `004-properties-list-pagination` | **Date**: 2026-11-19 | **Spec**: [spec.md](spec.md)

**Input**: Especificación de la característica en `/specs/004-properties-list-pagination/spec.md`

## Summary

Agregar `GET /api/properties` como Minimal API dentro de `Features/Properties/ListProperties`, con validación de paginación, handler concreto registrado mediante `IHandler`, proyección EF Core con `AsNoTracking`, contratos explícitos de respuesta y construcción de `imageUrl` absoluta desde el request actual.

## Technical Context

**Language/Version**: C# sobre .NET 11 según `global.json`.

**Primary Dependencies**: ASP.NET Core Minimal APIs, EF Core, Npgsql, FluentValidation, infraestructura ISlice/IHandler existente.

**Storage**: PostgreSQL mediante `AppDbContext.Properties`; no se modifica el modelo persistente ni se genera migración.

**Testing**: xUnit en `app/backend/tests/RealtorApiTests`; unit tests de handler, mapping, validator y slice, sin Testcontainers obligatorios.

**Target Platform**: Linux, backend ASP.NET Core `net11.0`.

**Project Type**: Backend de la solución compartida.

**Performance Goals**: Dos consultas acotadas: conteo total y proyección de la página solicitada. Sin carga completa de entidades EF.

**Constraints**: No controllers, no MediatR, no AutoMapper, no repositorios triviales, no cambios a `/health`, propagación de `CancellationToken` y no modificación de `Program.cs` para registrar el slice.

**Scale/Scope**: Una ruta GET, un slice, un handler, un validator, mapping explícito y pruebas asociadas.

## Constitution Check

*GATE: Debe pasar antes de la investigación y volver a evaluarse después del diseño.*

- **Solución única**: aprobado. El cambio pertenece al backend existente.
- **Spec-Driven Development**: aprobado. Todo cambio deriva de `spec.md` y deberá tener tareas.
- **Vertical Slice**: aprobado. El endpoint y su caso de uso viven bajo `Features/Properties/ListProperties`.
- **Minimal APIs**: aprobado. Se implementa mediante `ISlice` y no se usa controller.
- **Persistencia**: aprobado. Se reutiliza `AppDbContext` y no se modifica el modelo ni se crea migración.
- **Validación y errores**: aprobado. Se usa FluentValidation y `ValidationProblemDetails`.
- **Calidad y pruebas**: aprobado. Se usan unit tests compatibles con la suite existente.
- **Idioma**: aprobado. La documentación Markdown está en español.

### Reevaluación posterior al diseño

- La separación endpoint/handler/mapping/validator respeta Vertical Slice Architecture.
- La proyección EF evita cargar entidades completas y mantiene `AsNoTracking`.
- No se introduce persistencia nueva ni cambio de migración.
- Las decisiones de normalización y respuesta inválida quedan trazadas en el contrato.
- `/health` y el discovery de ISlice permanecen sin cambios funcionales.

## Project Structure

### Documentation (this feature)

```text
specs/004-properties-list-pagination/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── list-properties.http.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
app/backend/src/RealtorApi/
├── Features/
│   └── Properties/
│       └── ListProperties/
│           ├── ListPropertiesSlice.cs
│           ├── ListPropertiesHandler.cs
│           ├── ListPropertiesRequest.cs
│           ├── ListPropertiesResponse.cs
│           ├── ListPropertiesMapping.cs
│           └── ListPropertiesValidator.cs
├── Infrastructure/
│   ├── Endpoints/
│   ├── Handlers/
│   └── Validation/
├── RealtorApi.http
└── Program.cs

app/backend/tests/RealtorApiTests/
└── Features/
    └── Properties/
        └── ListProperties/
            ├── ListPropertiesSliceTests.cs
            ├── ListPropertiesHandlerTests.cs
            ├── ListPropertiesMappingTests.cs
            └── ListPropertiesValidatorTests.cs
```

**Structure Decision**: Mantener el caso de uso completo dentro de `Features/Properties/ListProperties`. `ISlice`, `IHandler` y validación global se reutilizan desde Infrastructure.

## Complexity Tracking

No existen desviaciones constitucionales. No se necesitan repositories, MediatR, mapeadores automáticos ni extensiones paralelas de registro.
