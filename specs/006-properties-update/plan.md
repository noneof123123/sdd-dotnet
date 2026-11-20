# Plan de implementación: Actualización de propiedades

**Rama**: `006-properties-update` | **Fecha**: 2026-11-20 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/006-properties-update/spec.md`

## Resumen

Agregar `PUT /api/properties/{id}` como Minimal API dentro de `Features/Properties/UpdateProperty`, con reemplazo completo de los campos editables mediante `multipart/form-data` y una imagen opcional. Un `ISlice` enlazará la ruta, un `IValidator<UpdatePropertyRequest>` concentrará toda la validación, un `IHandler` cargará la propiedad, publicará la imagen nueva si existe, persistirá los cambios con `UpdatedAt` en UTC y retirará la imagen anterior solo después de confirmar la persistencia. La respuesta será HTTP 200 con una representación pública; los fallos se traducirán a 400, 404 o 500 mediante el pipeline existente. No se requieren migraciones, nuevos paquetes ni cambios en creación, listado o salud.

## Contexto técnico

**Lenguaje/versión**: C# sobre .NET 11, según `global.json` (`11.0.100-rc.3.26522.148`).

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core con Npgsql, FluentValidation, `IHttpContextAccessor`, infraestructura `ISlice`/`IHandler`, `Result<T>`/`Error` y `IPropertyImageStorage`.

**Almacenamiento**: PostgreSQL existente mediante `AppDbContext.Properties`, sin cambios de esquema; archivos públicos en `wwwroot/assets/properties`, persistiendo rutas relativas `/assets/properties/{nombreSeguro}`.

**Pruebas**: xUnit 2.9.3 en `app/backend/tests/RealtorApiTests`, con EF Core InMemory, `DefaultHttpContext`, stubs de `IPropertyImageStorage` y multipart construido en bytes. Se mantienen las pruebas unitarias y de componente en memoria; no se agrega PostgreSQL real, servidor HTTP ni Testcontainers.

**Plataforma de destino**: Linux, backend ASP.NET Core `net11.0`.

**Tipo de proyecto**: Backend dentro de la solución compartida `app/Realtor.sln`.

**Objetivos de rendimiento**: El 95 % de las actualizaciones válidas debe completarse en menos de 2 segundos. Cada operación realiza una lectura y una escritura de la entidad, como máximo una escritura de archivo y, si corresponde, una retirada de archivo posterior a la confirmación.

**Restricciones**: Sin controllers, sin registros manuales de slices o handlers, sin repositories triviales, sin MediatR ni AutoMapper; sin lógica de negocio en el slice, el mapping, la entidad ni el almacenamiento; sin autenticación, actualización parcial, eliminación explícita de imagen ni código 409 por concurrencia; toda validación de entrada en `UpdatePropertyValidator`; errores internos sin detalles técnicos.

**Alcance**: Una ruta `PUT`, un slice, un request, un response, un handler, un validator, un mapping, ejemplos HTTP y pruebas asociadas. No se modifica el esquema persistente ni ninguna feature existente.

## Comprobación de la constitución

*COMPUERTA: Debe pasar antes de la investigación y volver a evaluarse después del diseño.*

- **Solución única**: aprobado. Todos los cambios quedan dentro del backend existente y de su directorio público de activos.
- **Desarrollo guiado por especificaciones**: aprobado para planificación. La spec es la fuente de verdad y los artefactos derivan de sus FR y SC; la transición de estado a `Aprobada` no se ejecuta en este comando y debe resolverse por revisión humana antes de implementar.
- **Vertical Slice**: aprobado. El caso de uso vive en `Features/Properties/UpdateProperty` y no introduce carpetas técnicas globales nuevas.
- **Minimal APIs**: aprobado. `UpdatePropertySlice` implementa `ISlice`; el descubrimiento automático existente registra slice y handler sin listas manuales.
- **Stack tecnológico**: aprobado. No se incorporan lenguajes, frameworks ni paquetes fuera del stack obligatorio.
- **Persistencia**: aprobado. Se reutiliza `AppDbContext` y el modelo actual; no se agrega migración porque no cambia el esquema.
- **Validación y errores**: aprobado. FluentValidation produce 400 vía `ValidationFilterFactory`; `Error.NotFound` y `Error.Internal` se traducen a 404 y 500 con `ProblemDetailsMapper`.
- **Calidad de dominio y contratos**: aprobado. Request y response son tipos explícitos distintos de la entidad; mapping solo proyecta; handler coordina; slice solo enlaza y traduce.
- **Logging estructurado**: aprobado. Se conserva `InfrastructureLog` y cualquier registro de rechazo o fallo usa mensajes estructurados sin exponer datos sensibles.
- **Pruebas**: aprobado. Solo se planifican pruebas unitarias y de componente en memoria, coherentes con la constitución y con `005`.
- **Idioma**: aprobado. Toda la documentación de esta iniciativa está en español, salvo identificadores técnicos y nombres de API permitidos.
- **Gobernanza y divergencias**: aprobado. No se detectan conflictos entre el plan, la spec y la constitución; no se editan scripts ni plantillas upstream.

### Reevaluación posterior al diseño

- El contrato `PUT`, el identificador de ruta y los códigos 400/404/500 quedan documentados sin introducir 413 ni 415.
- `UpdatePropertyRequest` incluye el `id` de ruta para que participe en el filtro de validación consistente.
- Las reglas de imagen y campos se concentran en `UpdatePropertyValidator`, sin tocar `CreatePropertyValidator`.
- La estrategia de imagen publica primero, persiste después y solo retira la anterior tras confirmar; los fallos controlados incluyen compensación.
- `Id` y `CreatedAt` son inmutables; `UpdatedAt` se asigna en UTC en el handler.
- La respuesta replica el contrato público de creación con HTTP 200, sin marcas temporales ni datos internos.
- La concurrencia aplica última escritura gana sin token de versión ni HTTP 409.
- No se requiere migración, nuevo servicio ni modificación de `Program.cs`.
- El listado, la creación, `/health` y el descubrimiento automático permanecen sin cambios funcionales.

## Fase 0: Investigación

La investigación resolvió todas las incógnitas sin dejar marcadores pendientes:

1. Contrato y método HTTP: `PUT` con reemplazo completo y respuesta 200.
2. Enlace y validación del identificador de ruta dentro del request.
3. Reglas de validación duplicadas en el nuevo validator, sin refactorizar la creación.
4. Códigos de error 400, 404 y 500 conforme al pipeline existente.
5. Orden de publicación, persistencia, retirada y compensación de imágenes.
6. Marcas temporales y política de última escritura gana.
7. Forma de la respuesta pública.
8. Estrategia de pruebas unitarias y de componente en memoria.

Detalles en `research.md`.

## Fase 1: Diseño y contratos

- `data-model.md`: entidades, contratos, validación, transiciones de imagen, concurrencia y ausencia de migración.
- `contracts/update-property.http.md`: endpoint, campos, respuestas, errores y ejemplos de solicitud.
- `quickstart.md`: pasos ejecutables de validación, escenarios esperados y evidencia pendiente para la implementación.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/006-properties-update/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── update-property.http.md
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
│       └── UpdateProperty/
│           ├── UpdatePropertySlice.cs
│           ├── UpdatePropertyHandler.cs
│           ├── UpdatePropertyRequest.cs
│           ├── UpdatePropertyResponse.cs
│           ├── UpdatePropertyMapping.cs
│           └── UpdatePropertyValidator.cs
├── Domain/
│   └── Properties/
│       └── Property.cs
├── Infrastructure/
│   ├── Endpoints/
│   ├── Handlers/
│   ├── Logging/
│   ├── Persistence/
│   ├── Results/
│   ├── Storage/
│   └── Validation/
├── Program.cs
└── RealtorApi.http

app/backend/tests/RealtorApiTests/
└── Features/
    └── Properties/
        └── UpdateProperty/
            ├── UpdatePropertySliceTests.cs
            ├── UpdatePropertyHandlerTests.cs
            ├── UpdatePropertyMappingTests.cs
            └── UpdatePropertyValidatorTests.cs
```

**Decisión estructural**: Mantener el caso de uso completo en `Features/Properties/UpdateProperty`, siguiendo la convención de seis archivos por slice. La entidad, el `DbContext`, el mapper de errores y el almacenamiento de imágenes se reutilizan sin cambios. No se crean migración, repositorio, controlador ni registro manual en `Program.cs`, y solo se añaden ejemplos HTTP en `RealtorApi.http`, sin cambios de configuración.

## Seguimiento de complejidad

No existen desviaciones constitucionales que requieran justificación.
