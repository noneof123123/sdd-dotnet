# Modelo de datos: Consulta de propiedad por identificador

**Fecha**: 2026-11-20

**Alcance**: Solo lectura. No se incorporan tablas, columnas ni migraciones nuevas.

## Entidad persistente reutilizada

### Property

| Campo | Tipo | Regla vigente | Uso en esta característica |
|-------|------|---------------|----------------------------|
| `Id` | `Guid` | Identificador único | Clave de búsqueda; se devuelve sin cambios |
| `Title` | `string` | Obligatorio, según modelo vigente | Se expone en la respuesta |
| `Description` | `string` | Obligatoria, según modelo vigente | Se expone en la respuesta |
| `Address` | `string` | Obligatoria, según modelo vigente | Se expone en la respuesta |
| `Price` | `decimal` | No negativo | Se expone en la respuesta |
| `Status` | enumeración | `Available`, `Rented`, `Maintenance` | Se expone como texto |
| `BedroomCount` | `int` | No negativo | Se expone en la respuesta |
| `BathroomCount` | `int` | No negativo | Se expone en la respuesta |
| `AreaSquareMeters` | `decimal` | No negativa | Se expone en la respuesta |
| `ImageUrl` | `string?` | Ruta relativa o `null` | No se expone directamente; origen de `imageUrl` pública |
| `CreatedAt` | `DateTime` | UTC | Se expone como `createdAt` |
| `UpdatedAt` | `DateTime?` | UTC o `null` | Se expone como `updatedAt` |

La entidad no se modifica. No se crea, actualiza ni elimina ningún registro ni archivo.

## Contrato de entrada

### GetPropertyByIdRequest

| Campo | Tipo | Origen | Regla |
|-------|------|--------|-------|
| `Id` | `Guid` | Ruta `{id}` | Obligatorio, no `Guid.Empty`; un valor mal formado se rechaza en el enlace |

Un path sin segmento `id` no alcanza el endpoint y queda resuelto por el routing general. La validación de esta operación aplica cuando la ruta coincide y el valor enlazado es inválido o vacío.

## Contrato de salida

### GetPropertyByIdResponse

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `Guid` | Identificador de la propiedad |
| `title` | `string` | Título público |
| `description` | `string` | Descripción pública |
| `address` | `string` | Dirección pública |
| `price` | `number` | Precio de la propiedad |
| `status` | `string` | Estado textual del catálogo |
| `bedroomCount` | `integer` | Cantidad de dormitorios |
| `bathroomCount` | `integer` | Cantidad de baños |
| `areaSquareMeters` | `number` | Superficie en metros cuadrados |
| `imageUrl` | `string` | URL absoluta pública o cadena vacía |
| `createdAt` | `date-time` | Fecha de creación en UTC |
| `updatedAt` | `date-time` o `null` | Última modificación en UTC, o `null` si nunca se actualizó |

Los diez primeros campos conservan nombre, tipo y significado del item del listado 004. Esta respuesta añade únicamente `createdAt` y `updatedAt`, sin metadatos de paginación.

## Reglas de proyección

1. `status` se obtiene del nombre textual del valor persistido.
2. Para `imageUrl`:
   - Se recorta el valor persistido.
   - Se extrae solo el nombre de archivo seguro.
   - Se rechazan nombres vacíos, `.`, `..` y nombres con caracteres inválidos.
   - Con un nombre válido se construye `{scheme}://{host}/assets/properties/{fileName}` usando el esquema y host de la solicitud actual.
   - Sin un nombre válido se devuelve cadena vacía.
   - Nunca se exponen rutas físicas, rutas relativas ni nombres originales inseguros.
3. `createdAt` y `updatedAt` se normalizan a UTC en la proyección. Un `updatedAt` nulo permanece nulo; no se inventa un valor igual a `createdAt`.
4. La respuesta es un tipo explícito separado de la entidad persistente.

## Resultados y errores

| Situación | Resultado interno | Respuesta pública |
|-----------|-------------------|-------------------|
| Identificador válido y existente | `Result<GetPropertyByIdResponse>` correcto | 200 con la representación pública |
| Identificador mal formado o vacío | Validación del pipeline | 400 con errores estructurados |
| Identificador válido sin propiedad | `Error.NotFound` | 404 con `ProblemDetails` |
| Fallo inesperado | `Error.Internal` | 500 con mensaje genérico |
| Cancelación | Excepción de cancelación propagada | Sin cambios observables |

## Transiciones de estado

No existen transiciones de estado. La operación es de solo lectura.

## Migración

No se requiere migración: el esquema actual ya contiene todos los campos necesarios.
