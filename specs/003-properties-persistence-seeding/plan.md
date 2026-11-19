# Implementation Plan: Persistencia y seed de propiedades

**Branch**: `003-properties-persistence-seeding` | **Date**: 2026-11-19 | **Spec**: [spec.md](spec.md)

**Input**: Especificación de la característica en `/specs/003-properties-persistence-seeding/spec.md`

## Summary

Implementar el modelo persistente inicial de `Property` y el enum persistido `PropertyStatus`, una única migración EF Core, seeding idempotente mediante `UseSeeding`/`UseAsyncSeeding`, migración automática al arrancar y publicación consumible de los JSON, manifiesto e imágenes. Mantener `/health` y el patrón de slices existente sin cambios funcionales.

## Technical Context

**Language/Version**: C# sobre .NET 11, según `global.json`.

**Primary Dependencies**: ASP.NET Core Minimal APIs, EF Core .NET 11, Npgsql.EntityFrameworkCore.PostgreSQL, FluentValidation existente y System.Text.Json.

**Storage**: PostgreSQL mediante Npgsql y una sola tabla `properties`; `PropertyStatus` se almacenará como texto por conversión EF Core sin tabla independiente.

**Testing**: xUnit en `app/backend/tests/RealtorApiTests`; unit tests únicamente, sin obligatoriedad de PostgreSQL real ni Testcontainers.

**Target Platform**: Linux, backend ASP.NET Core en `net11.0`.

**Project Type**: Backend dentro de la solución compartida, sin crear proyectos paralelos.

**Performance Goals**: Sin objetivos numéricos adicionales; el seeding debe leer pocos archivos iniciales y evitar consultas redundantes por propiedad.

**Constraints**: No controllers, no `HasData`, una sola migración funcional, `MigrateAsync` siempre al arranque, seed idempotente, rutas públicas no físicas y no invocar el seeder manualmente desde migración o fallback.

**Scale/Scope**: Una entidad persistida, hasta 10 propiedades seed iniciales y 10 imágenes versionadas.

## Constitution Check

*GATE: Debe pasar antes de la investigación y volver a evaluarse después del diseño.*

- **Solución única**: aprobado. Los cambios pertenecen a `app/backend` y `support/seed-data` dentro de la solución existente.
- **Spec-Driven Development**: aprobado. El diseño deriva de la especificación ya aclarada y todos los cambios deberán tener tareas propias.
- **Backend y Vertical Slice**: aprobado. No se crean endpoints paralelos técnicos; la persistencia queda en Domain e Infrastructure y los futuros slices bajo `Features/Properties`.
- **Stack obligatorio**: aprobado. Se usa .NET 11, Minimal APIs, EF Core, Npgsql y unit tests existentes.
- **Persistencia**: aprobado. Se siguen configuraciones separadas, una migración y el mecanismo nativo de seeding.
- **Archivos externos**: aprobado. Los JSON y el manifiesto quedan como contenido; las imágenes se sincronizan a una ruta pública estable.
- **Idioma**: aprobado. Todos los artefactos Markdown estarán en español.

### Reevaluación posterior al diseño

- **Revisión final**: aprobado. El modelo contiene una sola entidad persistente, una configuración separada y una migración funcional única.
- **Persistencia**: aprobado. `PropertyStatus` permanece como enum convertido a texto y no introduce una entidad relacional adicional.
- **Seeding**: aprobado. Los delegados EF Core conservan el flujo nativo y el seeder central será idempotente en sus versiones síncrona y asincrónica.
- **Activos**: aprobado. El manifiesto será la única fuente de rutas para JSON e imágenes, mientras `ImageUrl` usará una ruta pública estable.
- **Arquitectura**: aprobado. No se crean controllers ni endpoints paralelos; las futuras operaciones de propiedades podrán vivir en slices bajo `Features/Properties`.

## Project Structure

### Documentation (this feature)

```text
specs/003-properties-persistence-seeding/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── seed-manifest.schema.json
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
support/seed-data/
├── seed-manifest.json
├── images/
│   ├── properties.json
│   ├── properties-statuses.json
│   └── properties/
│       └── 1.png ... 10.png

app/backend/src/RealtorApi/
├── Domain/
│   └── Properties/
│       ├── Property.cs
│       └── PropertyStatus.cs
├── Features/
│   └── Properties/          # reservada para casos de uso futuros autorizados
├── Infrastructure/
│   └── Persistence/
│       ├── AppDbContext.cs
│       ├── DatabaseSeeder.cs
│       ├── MigrationExtensions.cs
│       ├── Configurations/
│       │   └── PropertyConfiguration.cs
│       └── Migrations/
├── wwwroot/
│   └── images/
│       └── properties/
├── Program.cs
└── RealtorApi.csproj

app/backend/tests/RealtorApiTests/
└── Infrastructure/
    └── Persistence/
        ├── PropertyConfigurationTests.cs
        └── DatabaseSeederTests.cs
```

**Structure Decision**: Mantener Domain, Features e Infrastructure según la arquitectura canónica. Toda persistencia compartida vive bajo `Infrastructure/Persistence`; no se crean repositories ni endpoints técnicos paralelos.

## Complexity Tracking

No existen desviaciones constitucionales que requieran justificación. La complejidad se limita a una entidad persistente, su configuración, una migración, el seeder y la integración de activos externos.
