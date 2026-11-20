# Plan de implementación: Consulta de propiedad por identificador

**Rama**: `007-properties-get-by-id` | **Fecha**: 2026-11-20 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/007-properties-get-by-id/spec.md`

## Resumen

Agregar `GET /api/properties/{id}` como una nueva Vertical Slice dentro de `Features/Properties/GetPropertyById`. El slice enlazará el identificador de ruta, el validator garantizará que no esté vacío y el handler realizará una única lectura sin seguimiento de cambios, devolverá un `Result<GetPropertyByIdResponse>` y registrará el evento de consulta con logging estructurado. El mapping proyectará una representación pública explícita con los diez campos del listado más `createdAt` y `updatedAt`, aplicando el mismo algoritmo de `imageUrl` absoluta o cadena vacía ya usado por 004. Los errores se traducirán a 400, 404 o 500 mediante `ProblemDetailsMapper`. No se requieren migraciones, nuevos paquetes, controladores, repositorios ni cambios en `Program.cs`.

## Contexto técnico

**Lenguaje/versión**: C# sobre .NET 11, según `global.json` (`11.0.100-rc.3.26522.148`).

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core con Npgsql, FluentValidation, `ProblemDetailsMapper`, infraestructura `ISlice`/`IHandler` con descubrimiento por ensamblado, `Result<T>`/`Error`, `IHttpContextAccessor` y `ILogger<T>` con `LoggerMessage`.

**Almacenamiento**: PostgreSQL existente mediante `AppDbContext.Properties`, sin cambios de esquema. La persistencia conserva `ImageUrl` relativa o nula; la representación pública de esta consulta usa `imageUrl` absoluta o cadena vacía.

**Pruebas**: xUnit 2.9.3 en `app/backend/tests/RealtorApiTests`, con EF Core InMemory, `DefaultHttpContext`, stubs manuales y `NullLogger<T>`. Se planifican pruebas unitarias de validator, mapping y handler, además de pruebas unitarias del slice mediante `WebApplication` en memoria e invocación directa del delegado de la ruta, sin Kestrel, PostgreSQL real ni Testcontainers.

**Plataforma de destino**: Linux, backend ASP.NET Core `net11.0`.

**Tipo de proyecto**: Backend dentro de la solución compartida `app/Realtor.sln`.

**Objetivos de rendimiento**: El 95 % de las consultas válidas debe resolverse en menos de 1 segundo. Cada consulta realiza una sola lectura de la entidad, sin escrituras ni operaciones de archivos.

**Restricciones**: Sin controllers, sin registros manuales de slices o handlers, sin repositories triviales, sin MediatR ni AutoMapper; sin lógica de negocio en el slice, el mapping, la entidad ni la infraestructura; sin autenticación, paginación, escritura, eliminación ni gestión de imágenes; errores internos sin detalles técnicos; documentación en español.

**Alcance**: Una ruta `GET`, un slice, un request, un response, un handler, un validator, un mapping, un evento de logging, ejemplos HTTP, contrato OpenAPI y pruebas asociadas. No se modifica el esquema persistente ni el comportamiento de salud, listado, creación o actualización.

## Comprobación de la constitución

*COMPUERTA: Debe pasar antes de la investigación y volver a evaluarse después del diseño.*

- **Solución única**: aprobado. Todos los cambios quedan dentro del backend existente y de la solución compartida.
- **Desarrollo guiado por especificaciones**: aprobado para planificación. La spec 007 es la fuente de verdad; el campo `Estado` permanece en `Borrador` y la aprobación humana debe ocurrir en el flujo de revisión antes de implementar. Este comando no edita ese estado.
- **Vertical Slice**: aprobado. El caso de uso vive en `Features/Properties/GetPropertyById` y no introduce carpetas técnicas globales nuevas.
- **Minimal APIs**: aprobado. `GetPropertyByIdSlice` implementa `ISlice`; `MapSliceEndpoints()` lo descubre automáticamente sin tocar `Program.cs`.
- **Stack tecnológico**: aprobado. No se incorporan lenguajes, frameworks ni paquetes fuera del stack obligatorio.
- **Persistencia**: aprobado. Se reutiliza `AppDbContext` y el modelo actual; no se agrega migración porque no cambia el esquema.
- **Validación y errores**: aprobado. FluentValidation produce 400 vía `ValidationFilterFactory`; `Error.NotFound` y `Error.Internal` se traducen a 404 y 500 con `ProblemDetailsMapper`.
- **Calidad de dominio y contratos**: aprobado. Request y response son tipos explícitos distintos de la entidad; el mapping solo proyecta; el handler coordina la lectura; el slice solo enlaza y traduce.
- **Logging estructurado**: aprobado. Se añade un evento `LoggerMessage` para la consulta, con el identificador como parámetro estructurado y sin datos sensibles.
- **Pruebas**: aprobado. Solo se planifican pruebas unitarias y unitarias del slice en memoria; esta caracterización se explicita porque la constitución exige justificar cualquier prueba que no sea estrictamente unitaria.
- **Idioma**: aprobado. Toda la documentación de esta iniciativa está en español, salvo identificadores técnicos y nombres de API permitidos.
- **Gobernanza y divergencias**: aprobado. El prompt operativo enumera diez campos de respuesta y la spec clarificada exige además marcas temporales; se conservan los diez campos compartidos y se añaden `createdAt`/`updatedAt` por ser la spec la fuente de verdad. No se editan scripts ni plantillas upstream.

### Reevaluación posterior al diseño

- La ruta `GET /api/properties/{id}` queda documentada en contrato HTTP y OpenAPI.
- La respuesta conserva los diez campos del listado y añade solo las marcas temporales exigidas por FR-004.
- `imageUrl` usa la convención del listado: URL absoluta del origen actual o cadena vacía; prevalece sobre la convención relativa/nula de create/update.
- El resolvedor se replica en el nuevo mapping y se verifica mediante pruebas de equivalencia, sin refactorizar `ListPropertiesMapping.cs`.
- Un identificador ausente en la ruta no alcanza el endpoint y queda resuelto por el routing; un identificador mal formado o vacío dentro de la ruta produce 400.
- El handler solo lee con `AsNoTracking()`, propaga cancelación y no modifica entidades, archivos ni marcas.
- El evento de logging usa el identificador como parámetro estructurado y no expone imágenes, rutas ni datos sensibles.
- Las pruebas cubren 200, 400, 404, cancelación, solo lectura, equivalencia de imagen y descubrimiento automático sin afectar las capacidades existentes.
- No se requiere migración, nuevo servicio, repositorio, controlador ni modificación de `Program.cs`.

## Fase 0: Investigación

La investigación resolvió todas las incógnitas sin dejar marcadores pendientes:

1. Contrato y método: `GET` sobre `/api/properties/{id}`, sin paginación.
2. Enlace y validación del identificador de ruta dentro de un request explícito.
3. Forma de la respuesta: diez campos del listado más marcas temporales.
4. Representación de imagen: absoluta/cadena vacía, equivalente al listado.
5. Decisión de no refactorizar el listado; duplicación controlada con prueba de equivalencia.
6. Acceso a datos de solo lectura y propagación de cancelación.
7. Traducción de errores a 400/404/500 mediante el pipeline existente.
8. Logging estructurado con un nuevo evento.
9. Estrategia de pruebas unitarias y de componente en memoria.
10. Generación de contrato OpenAPI y guía HTTP de validación.

Detalles en `research.md`.

## Fase 1: Diseño y contratos

- `data-model.md`: entidades de solicitud/respuesta, campos públicos, formatos, reglas de imagen, errores y ausencia de migración.
- `contracts/backend-properties-get-by-id.openapi.yaml`: definición OpenAPI de la operación y sus respuestas.
- `contracts/get-property-by-id.http.md`: ejemplo legible de solicitudes y respuestas esperadas.
- `quickstart.md`: pasos ejecutables de validación, escenarios esperados y evidencia pendiente para la implementación.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/007-properties-get-by-id/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── backend-properties-get-by-id.openapi.yaml
│   └── get-property-by-id.http.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

`tasks.md` se genera en la fase `/speckit.tasks`; `/speckit.plan` no lo crea.

### Código fuente (raíz del repositorio)

```text
app/backend/src/RealtorApi/
├── Features/
│   └── Properties/
│       ├── CreateProperty/
│       ├── ListProperties/
│       ├── UpdateProperty/
│       └── GetPropertyById/
│           ├── GetPropertyByIdSlice.cs
│           ├── GetPropertyByIdHandler.cs
│           ├── GetPropertyByIdRequest.cs
│           ├── GetPropertyByIdResponse.cs
│           ├── GetPropertyByIdMapping.cs
│           └── GetPropertyByIdValidator.cs
├── Infrastructure/
│   └── Logging/
│       └── InfrastructureLog.cs
├── Program.cs
└── RealtorApi.http

app/backend/tests/RealtorApiTests/
└── Features/
    └── Properties/
        └── GetPropertyById/
            ├── GetPropertyByIdSliceTests.cs
            ├── GetPropertyByIdHandlerTests.cs
            ├── GetPropertyByIdMappingTests.cs
            └── GetPropertyByIdValidatorTests.cs
```

**Decisión estructural**: Mantener el caso de uso completo en `Features/Properties/GetPropertyById`, siguiendo la convención de seis archivos por slice. La entidad, el `DbContext`, el mapper de errores y el descubrimiento automático se reutilizan sin cambios. `InfrastructureLog.cs` recibe un único evento nuevo y `RealtorApi.http` ejemplos adicionales. No se crean migración, repositorio, controlador ni registro manual en `Program.cs`.

## Seguimiento de complejidad

No existen desviaciones constitucionales que requieran justificación.
