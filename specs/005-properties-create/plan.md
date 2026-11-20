# Plan de implementación: Alta de propiedades

**Rama**: `005-properties-create` | **Fecha**: 2026-11-20 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/005-properties-create/spec.md`

## Resumen

Agregar `POST /api/properties` como Minimal API dentro de `Features/Properties/CreateProperty`, mediante solicitud `multipart/form-data`, un slice `ISlice`, un handler concreto `IHandler`, validación FluentValidation centralizada, persistencia EF Core y almacenamiento público seguro de imágenes PNG/JPG. La propiedad se crea con `imageUrl = null` cuando no se adjunta imagen. El flujo exitoso devuelve HTTP 201, `Location: /api/properties/{id}` y una respuesta explícita; el handler devuelve `Result<CreatePropertyResponse>` y el slice traduce fallos internos a HTTP 500 con `ProblemDetails` mediante `ProblemDetailsMapper`.

## Contexto técnico

**Lenguaje/versión**: C# sobre .NET 11 según `global.json`.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core, Npgsql.EntityFrameworkCore.PostgreSQL, FluentValidation, `IHttpContextAccessor` e infraestructura `ISlice`/`IHandler` existente.

**Almacenamiento**: PostgreSQL mediante `AppDbContext.Properties` y archivos en `wwwroot/assets/properties`; el modelo persistente cambia para admitir `ImageUrl` nullable mediante una migración EF Core.

**Pruebas**: xUnit en `app/backend/tests/RealtorApiTests`; pruebas unitarias de slice, handler, mapping, validator y almacenamiento, además de una prueba de componente en memoria que crea con `CreatePropertyHandler` y consulta con `ListPropertiesHandler`. No se incorpora Testcontainers ni servidor HTTP real.

**Plataforma de destino**: Linux, backend ASP.NET Core `net11.0`.

**Tipo de proyecto**: Backend dentro de la solución compartida.

**Objetivos de rendimiento**: Escritura de un único registro, una operación de persistencia por creación y un único almacenamiento de archivo válido. Sin carga ni lectura adicional innecesaria.

**Restricciones**: No controllers, no repositories triviales, no MediatR, no AutoMapper, no lógica de negocio en mapping, endpoint ni almacenamiento; no nombres de archivo inseguros; límite inclusivo de 5 × 1024 × 1024 bytes; cancelación propagada; toda validación de solicitud se resuelve en `CreatePropertyValidator` antes del handler.

**Alcance**: Una ruta POST, un slice, un request, una response, un handler, un validator, mapping explícito, un almacenamiento de imágenes, una modificación de configuración EF y una migración funcional.

## Comprobación de la constitución

*COMPUERTA: Debe pasar antes de la investigación y volver a evaluarse después del diseño.*

- **Solución única**: aprobado. Los cambios están limitados al backend existente y al directorio público existente de activos.
- **Desarrollo guiado por especificaciones**: aprobado. La especificación está aprobada y todos los cambios derivan de tareas trazables en `tasks.md`.
- **Vertical Slice**: aprobado. El caso de uso vive bajo `Features/Properties/CreateProperty`.
- **Minimal APIs**: aprobado. El endpoint implementa `ISlice` y no usa controllers.
- **Persistencia**: aprobado. Se mantiene `AppDbContext`, se modifica únicamente la nulabilidad de `ImageUrl` y se genera una migración versionada.
- **Validación y errores**: aprobado. Se usa FluentValidation como único propietario de la validación de entrada; `ValidationProblemDetails` cubre HTTP 400 y `ProblemDetailsMapper` convierte fallos internos en HTTP 500 sin detalles técnicos.
- **Archivos públicos**: aprobado. Las imágenes válidas se escriben únicamente bajo `wwwroot/assets/properties` y se persiste una ruta relativa segura.
- **Calidad y pruebas**: aprobado. Se usan pruebas unitarias y una prueba de componente en memoria; no se introduce un servidor HTTP real ni frameworks de prueba adicionales.
- **Idioma**: aprobado. Toda documentación propia de esta iniciativa está escrita en español, conservando solo identificadores técnicos en inglés.
- **Gobernanza de estado**: aprobado. La transición `Borrador → Aprobada` se realiza mediante la revisión explícita de la especificación, sin edición manual del estado.

### Reevaluación posterior al diseño

- El contrato multipart queda localizado en el slice y su modelo de datos explícito.
- La nulabilidad de `ImageUrl` queda reflejada en el dominio, EF Core y la migración.
- Toda validación de la solicitud, incluidos MIME, extensión, firma, tamaño, tipo de contenido y cantidad de partes `image`, reside en `CreatePropertyValidator`.
- El almacenamiento recibe bytes ya validados, genera un nombre seguro único y solo señala fallos de infraestructura.
- El handler coordina persistencia y limpieza y devuelve un resultado explícito de éxito o error interno.
- El slice solo enlaza, delega y traduce el resultado; no contiene validación ni lógica de negocio.
- El listing existente conserva compatibilidad y el resultado sin imagen se representa como `null` en la respuesta de creación y con el contrato vigente en el listado.
- `/health` y el descubrimiento automático se mantienen sin modificación funcional.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/005-properties-create/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── create-property.http.md
└── checklists/
    └── requirements.md
```

### Código fuente (raíz del repositorio)

```text
app/backend/src/RealtorApi/
├── Features/
│   └── Properties/
│       └── CreateProperty/
│           ├── CreatePropertySlice.cs
│           ├── CreatePropertyHandler.cs
│           ├── CreatePropertyRequest.cs
│           ├── CreatePropertyResponse.cs
│           ├── CreatePropertyMapping.cs
│           └── CreatePropertyValidator.cs
├── Domain/
│   └── Properties/
│       └── Property.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/PropertyConfiguration.cs
│   │   └── Migrations/
│   ├── Results/
│   │   ├── Error.cs
│   │   └── ProblemDetailsMapper.cs
│   └── Storage/
│       ├── IPropertyImageStorage.cs
│       └── PropertyImageStorage.cs
└── wwwroot/
    └── assets/
        └── properties/

app/backend/tests/RealtorApiTests/
└── Features/
    └── Properties/
        └── CreateProperty/
            ├── CreatePropertySliceTests.cs
            ├── CreatePropertyHandlerTests.cs
            ├── CreatePropertyMappingTests.cs
            ├── CreatePropertyValidatorTests.cs
            └── PropertyImageStorageTests.cs
```

**Decisión estructural**: Mantener el caso de uso completo dentro de `Features/Properties/CreateProperty`. La entidad persistente y su configuración continúan en Domain e Infrastructure; el almacenamiento de imágenes se ubica en Infrastructure como servicio de infraestructura, no como caso de uso; no se crean repositories ni registros manuales.

## Seguimiento de complejidad

No existen desviaciones constitucionales que requieran justificación. La única modificación persistente necesaria es hacer nullable `Property.ImageUrl` y generar una migración compatible con el modelo actual.
