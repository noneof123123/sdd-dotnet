# Modelo de datos: 009-update-status

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`
Objetivo: describir las estructuras persistentes y de contrato utilizadas por la operación de cambio de estado, sin introducir cambios de esquema.

## Modelo persistido existente

| Entidad | Campo | Tipo | Uso en esta característica |
|---|---|---|---|
| `Property` | `Id` | `Guid` | Identificador de ruta; selecciona la fila a actualizar. |
| `Property` | `Status` | `PropertyStatus` | Único campo modificable. |
| `Property` | `UpdatedAt` | `DateTime?` | Se escribe solo cuando el estado cambia; se conserva en operaciones idempotentes. |
| `Property` | `Title` | `string` | Se conserva sin modificaciones. |
| `Property` | `Description` | `string` | Se conserva sin modificaciones. |
| `Property` | `Address` | `string` | Se conserva sin modificaciones. |
| `Property` | `Price` | `decimal` | Se conserva sin modificaciones. |
| `Property` | `BedroomCount` | `int` | Se conserva sin modificaciones. |
| `Property` | `BathroomCount` | `int` | Se conserva sin modificaciones. |
| `Property` | `AreaSquareMeters` | `decimal` | Se conserva sin modificaciones. |
| `Property` | `ImageUrl` | `string?` | Se conserva sin modificaciones y se normaliza solo en la respuesta. |
| `Property` | `CreatedAt` | `DateTime` | Se conserva y se normaliza a UTC en la respuesta. |

No existen nuevas entidades, relaciones, índices ni columnas. No se requiere migración.

## Catálogo de estados

| Nombre expuesto | Valor de dominio | Aceptado |
|---|---|---|
| `Available` | `PropertyStatus.Available` | Sí |
| `Rented` | `PropertyStatus.Rented` | Sí |
| `Maintenance` | `PropertyStatus.Maintenance` | Sí |
| `null`, cadena vacía o espacios | ninguno | No; `400` |
| Cualquier otro texto | ninguno | No; `400` |
| Minúsculas o mayúsculas distintas a las anteriores | ninguno | No; `400` |

## Contrato de solicitud

### `ChangePropertyStatusRequest`

Tipo de slice enlazado desde ruta y cuerpo mediante `[AsParameters]`.

| Miembro | Origen | Tipo | Requisito |
|---|---|---|---|
| `Id` | Ruta `{id}` | `string?` | No vacío y convertible a `Guid`; se enlaza como texto para que un formato inválido produzca `400` estructurado. |
| `Body` | Cuerpo JSON | `ChangePropertyStatusBody?` | Debe existir; si falta, el validador produce `400`. |

### `ChangePropertyStatusBody`

| Campo JSON | Tipo | Requisito |
|---|---|---|
| `status` | `string?` | Obligatorio; uno de los tres nombres exactos del catálogo. |

Representación JSON:

```json
{
  "status": "Rented"
}
```

Se usa un registro anidado anulable en lugar de una cadena suelta para que el enlace lea un objeto JSON, distinga cuerpo ausente, permita el validador global producir `HttpValidationProblemDetails` y mantenga `Id` y `Status` bajo el mismo validador de solicitud.

## Contrato de respuesta

### `ChangePropertyStatusResponse`

| Campo JSON | Tipo | Origen |
|---|---|---|
| `id` | `Guid` | `Property.Id` |
| `title` | `string` | `Property.Title` |
| `description` | `string` | `Property.Description` |
| `address` | `string` | `Property.Address` |
| `price` | `decimal` | `Property.Price` |
| `status` | `string` | Estado persistido convertido a texto. |
| `bedroomCount` | `int` | `Property.BedroomCount` |
| `bathroomCount` | `int` | `Property.BathroomCount` |
| `areaSquareMeters` | `decimal` | `Property.AreaSquareMeters` |
| `imageUrl` | `string` | URL absoluta derivada del nombre de archivo persistido, o cadena vacía. |
| `createdAt` | `DateTime` | `Property.CreatedAt` normalizado a UTC. |
| `updatedAt` | `DateTime?` | `Property.UpdatedAt` normalizado a UTC, o `null`. |

La respuesta es equivalente a la representación pública de `GetPropertyByIdResponse`, pero se define dentro de esta slice para evitar acoplamiento entre características.

## Reglas de transición

1. Se carga la propiedad por `Id` con seguimiento de Entity Framework.
2. Si no existe, se devuelve un error de dominio `NotFound`.
3. Si `Status` ya es igual al solicitado, no se muta la entidad, no se llama a `SaveChangesAsync` y `UpdatedAt` permanece intacto.
4. Si `Status` difiere, se asigna el valor de dominio correspondiente, se fija `UpdatedAt` y se guarda.
5. Cualquier transición entre los tres estados válidos está permitida.
6. No se registra historial, versión ni conflicto de concurrencia; última escritura gana.

## Errores

| Situación | Respuesta | Formato |
|---|---|---|
| `id` vacío, cuerpo ausente, `status` ausente, vacío o inválido | `400` | `HttpValidationProblemDetails` con claves `Id` o `Status`. |
| Propiedad inexistente | `404` | `ProblemDetails` con detalle estable. |
| Error inesperado de persistencia | `500` | `ProblemDetails` genérico sin detalles técnicos. |
| Estado repetido | `200` | Representación pública sin cambios en `updatedAt`. |

## Mapeo

`ChangePropertyStatusMapping.ToResponse(Property, HttpRequest)` aplica las mismas reglas que la consulta por id:

- Obtiene solo el nombre de archivo de `ImageUrl`; valores vacíos, inválidos o `.`/`..` producen `imageUrl` vacía.
- Compone la URL como `{scheme}://{host}/assets/properties/{fileName}`.
- Normaliza fechas `Utc`, `Local` y `Unspecified` a `DateTimeKind.Utc`.
- No expone la ruta física de la imagen.

## Trazabilidad

El handler registra el evento estructurado `1004` con identificador de propiedad y estado destino solo cuando la operación termina correctamente, incluida la repetición de estado.
