# Modelo de datos: Actualización de propiedades

## Entidades existentes

### Property

No se agregan tablas, columnas ni migraciones. La entidad persistente existente se actualiza en memoria y se guarda mediante `AppDbContext`.

| Campo | Tipo | Persistencia | Comportamiento en esta feature |
|---|---|---|---|
| Id | Guid | PK | Se conserva sin cambios; identifica la propiedad de la ruta |
| Title | string | VARCHAR 200 | Reemplazable; no vacío; máximo 200 |
| Description | string | VARCHAR 4000 | Reemplazable; no vacío; máximo 4000 |
| Address | string | VARCHAR 500 | Reemplazable; no vacío; máximo 500 |
| Price | decimal | decimal(18,2) | Reemplazable; mayor o igual a cero |
| Status | PropertyStatus | VARCHAR 32 | Reemplazable; `Available`, `Rented` o `Maintenance` |
| BedroomCount | int | INTEGER | Reemplazable; mayor o igual a cero |
| BathroomCount | int | INTEGER | Reemplazable; mayor o igual a cero |
| AreaSquareMeters | decimal | decimal(12,2) | Reemplazable; mayor o igual a cero |
| ImageUrl | string? | VARCHAR 1000 nullable | Se conserva si no hay imagen nueva; se sustituye por ruta relativa si la hay |
| CreatedAt | DateTime | TIMESTAMP WITH TIME ZONE | Inmutable |
| UpdatedAt | DateTime? | TIMESTAMP WITH TIME ZONE | Nueva marca UTC tras persistir el cambio |

### PropertyImage

Archivo opcional validado, no una entidad persistente. Si se adjunta, se publica bajo `wwwroot/assets/properties` con nombre seguro generado y se persiste como `/assets/properties/{nombreSeguro}`. El nombre original se descarta.

## Contratos del caso de uso

### UpdatePropertyRequest

Campos enlazados desde `PUT /api/properties/{id}`:

| Campo | Origen | Tipo | Obligatorio |
|---|---|---|---|
| Id | Ruta `id` | Guid | Sí, no vacío |
| Title | Formulario `title` | string | Sí |
| Description | Formulario `description` | string | Sí |
| Address | Formulario `address` | string | Sí |
| Price | Formulario `price` | decimal | Sí |
| Status | Formulario `status` | PropertyStatus | Sí |
| BedroomCount | Formulario `bedroomCount` | int | Sí |
| BathroomCount | Formulario `bathroomCount` | int | Sí |
| AreaSquareMeters | Formulario `areaSquareMeters` | decimal | Sí |
| Image | Formulario `image` | IFormFile? | No |

### UpdatePropertyResponse

| Campo | Tipo | Descripción |
|---|---|---|
| Id | Guid | Identificador conservado |
| Title | string | Valor vigente |
| Description | string | Valor vigente |
| Address | string | Valor vigente |
| Price | decimal | Valor vigente |
| Status | string | Texto del enum, no número |
| BedroomCount | int | Valor vigente |
| BathroomCount | int | Valor vigente |
| AreaSquareMeters | decimal | Valor vigente |
| ImageUrl | string? | Ruta relativa pública o `null`; nunca ruta física ni nombre original |

## Validación

- `Id` debe ser un `Guid` válido y distinto de vacío.
- `Title`: no vacío, máximo 200 caracteres.
- `Description`: no vacío, máximo 4000 caracteres.
- `Address`: no vacío, máximo 500 caracteres.
- `Price`, `BedroomCount`, `BathroomCount` y `AreaSquareMeters`: mayores o iguales a cero.
- `Status`: debe pertenecer a `PropertyStatus`.
- La solicitud debe usar `multipart/form-data`.
- Solo puede existir una parte `image`; una parte textual con ese nombre se rechaza.
- Si hay imagen, debe ser PNG o JPG/JPEG por extensión, MIME y firma de contenido, con tamaño mayor que cero y menor o igual a 5 × 1024 × 1024 bytes.
- Toda la validación se ejecuta antes de publicar archivos o modificar la propiedad.

## Transiciones de imagen

| Estado previo | Solicitud | Estado posterior | Archivos |
|---|---|---|---|
| `ImageUrl = null` | Sin `image` | `ImageUrl = null` | Sin cambios |
| `ImageUrl` relativo | Sin `image` | Mismo `ImageUrl` | Sin cambios |
| `ImageUrl = null` | Con imagen válida | Nueva ruta relativa | Se publica un archivo nuevo |
| `ImageUrl` relativo | Con imagen válida | Nueva ruta relativa | Se publica uno nuevo y se retira el anterior tras confirmar la persistencia |

La eliminación explícita de imagen no forma parte de esta feature. No existe un campo para solicitarla y una parte `image` vacía se trata como imagen inválida.

## Concurrencia

No se introduce token de concurrencia, número de versión ni error 409. Si dos solicitudes válidas se persisten sobre la misma propiedad, prevalece la última escritura confirmada, de acuerdo con FR-021.

## Migración

No se requiere ninguna migración. El esquema actual ya admite `ImageUrl` nullable y `UpdatedAt` nullable.
