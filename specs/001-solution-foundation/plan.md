# Implementation Plan: Base de la solución Realtor

**Branch**: `001-prompt-spec` | **Date**: 2026-11-19 | **Spec**: [spec.md](spec.md)

**Input**: Especificación de la característica en `/specs/001-solution-foundation/spec.md`

## Summary

Crear la estructura fundacional compartida de Realtor en `app/Realtor.sln`, con backend ASP.NET Core Minimal APIs, frontend Blazor Web App y proyectos de pruebas separados, sin implementar lógica de negocio ni entidades de dominio. La versión de SDK se tomará exclusivamente de `global.json` y la solución deberá restaurar y compilar correctamente.

## Technical Context

**Language/Version**: C# sobre .NET 11, definido por `global.json` como SDK `11.0.100-rc.3.26522.148`

**Primary Dependencies**: ASP.NET Core, Blazor Web App, Razor Components y frameworks de pruebas de la plantilla de .NET

**Storage**: N/A en esta iniciativa; no se crea persistencia ni entidades de dominio

**Testing**: Un proyecto de pruebas backend y otro frontend asociados a sus aplicaciones respectivas

**Target Platform**: Linux para desarrollo y ejecución; aplicaciones web ASP.NET Core y Blazor

**Project Type**: Aplicación web compartida con backend y frontend

**Performance Goals**: Sin objetivos de rendimiento específicos; la aceptación exige compilación y restauración correctas

**Constraints**: No controllers, no lógica de negocio, no features, no entidades de dominio y no modificación de `global.json`

**Scale/Scope**: Cuatro proyectos iniciales dentro de una solución compartida

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Solución única y compartida**: aprobado. Todos los proyectos se incluirán en `app/Realtor.sln`.
- **Spec-Driven Development**: aprobado. El diseño se deriva de `spec.md` y no introduce alcance adicional.
- **Backend Minimal APIs**: aprobado. El proyecto backend no usará controllers.
- **Frontend Blazor**: aprobado. El proyecto frontend usará Blazor Web App con Razor Components.
- **Stack obligatorio**: aprobado. La versión .NET se deriva de `global.json`; EF Core, Npgsql, Refit y demás tecnologías no se inicializan porque no corresponden a esta fundación.
- **Sin lógica de dominio**: aprobado. No se crearán entidades, servicios de negocio ni features.
- **Idioma**: aprobado. Todos los artefactos Markdown de esta iniciativa estarán en español.

## Project Structure

### Documentation (this feature)

```text
specs/001-solution-foundation/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   └── tests/
│       └── RealtorApiTests/
└── frontend/
    ├── src/
    │   └── RealtorWeb/
    └── test/
        └── RealtorWeb/
```

**Structure Decision**: Se adopta una única solución compartida con proyectos backend, frontend y pruebas separados según las rutas aprobadas por la especificación.

## Complexity Tracking

No existen justificaciones de desviación constitucional. No se incorporan patrones adicionales ni complejidad técnica fuera del alcance fundacional.
