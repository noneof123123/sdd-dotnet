# Research: Base de la solución Realtor

## Decisión: Usar el SDK indicado por global.json

**Decisión**: Los proyectos se crearán con SDK .NET 11 y usarán como referencia el SDK efectivo seleccionado por `global.json`, actualmente `11.0.100-rc.3.26522.148`.

**Rationale**: `global.json` es una precondición bloqueante y la fuente de verdad exigida por la iniciativa.

**Alternativas consideradas**: Usar una versión fija distinta en los proyectos fue descartada porque contradice la especificación y el mecanismo de selección centralizado.

## Decisión: Backend como ASP.NET Core Minimal API

**Decisión**: Crear `RealtorApi` como proyecto Web para Minimal APIs, sin controllers.

**Rationale**: Es el modelo requerido por la constitución y permite mantener `Program.cs` como punto exclusivo de configuración base.

**Alternativas consideradas**: Controllers y aplicaciones MVC fueron descartados por estar explícitamente prohibidos.

## Decisión: Frontend como Blazor Web App

**Decisión**: Crear `RealtorWeb` como Blazor Web App con Razor Components.

**Rationale**: Coincide con el stack obligatorio y permite definir el shell inicial sin introducir funcionalidad de negocio.

**Alternativas consideradas**: Blazor WebAssembly aislado y Blazor Server independiente fueron descartados por no representar el modelo obligatorio de Blazor Web App.

## Decisión: Plantillas estándar de prueba

**Decisión**: Crear proyectos de pruebas backend y frontend asociados a sus respectivas aplicaciones y reconocerlos dentro de la solución.

**Rationale**: El alcance exige proyectos de pruebas separados, sin agregar suites de integración o dependencias externas no aprobadas.

**Alternativas consideradas**: Una sola solución de pruebas compartida fue descartada por la estructura obligatoria de rutas separadas.

## Decisión: Sin contratos ni modelo de datos externos

**Decisión**: Omitir contratos HTTP y `data-model.md` detallado porque esta fundación no expone features ni entidades.

**Rationale**: Un contrato solo puede describir interfaces externas existentes; no existen endpoints de negocio ni entidades en esta iniciativa.

**Alternativas consideradas**: Crear contratos vacíos o modelos simulados fue descartado por ser artificioso y aumentar la complejidad sin valor verificable.
