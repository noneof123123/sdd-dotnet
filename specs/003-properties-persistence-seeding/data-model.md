# Modelo de datos: Persistencia y seed de propiedades

## Property

Entidad persistente principal ubicada en `Domain/Properties/Property.cs`.

| Campo | Tipo | Persistencia | Restricciones |
|---|---|---|---|
| Id | Guid | PK | Obligatorio; identidad estable del seed |
| Title | string | NVARCHAR 200 | Obligatorio |
| Description | string | NVARCHAR 4000 | Obligatorio |
| Address | string | NVARCHAR 500 | Obligatorio |
| Price | decimal | decimal(18,2) | Obligatorio; no negativo |
| Status | PropertyStatus | VARCHAR 32 | Obligatorio; enum convertido a texto |
| BedroomCount | int | INTEGER | Obligatorio; valor mínimo 0 |
| BathroomCount | int | INTEGER | Obligatorio; valor mínimo 0 |
| AreaSquareMeters | decimal | decimal(12,2) | Obligatorio; valor mínimo 0 |
| ImageUrl | string | NVARCHAR 1000 | Obligatorio; ruta pública `/images/properties/{archivo}` |
| CreatedAt | DateTime | TIMESTAMP WITH TIME ZONE | Obligatorio |
| UpdatedAt | DateTime? | TIMESTAMP WITH TIME ZONE | Opcional |

### Índices

- Clave primaria sobre `Id`.
- Índice sobre `Status` para futuras filtraciones por estado.
- Índice compuesto opcional sobre `Status` y `Price` si la evaluación posterior lo exige.

## PropertyStatus

Enum de dominio persistido como texto mediante conversión EF Core:

- `Available`
- `Rented`
- `Maintenance`

No se crea tabla independiente ni DbSet para estados.

## Tabla properties

La configuración EF Core crea una única tabla `properties` sin relaciones externas. El `Status` usa texto en columna y no una clave foránea.

## SeedManifest

Objeto JSON documentado mediante el contrato `contracts/seed-manifest.schema.json`.

Campos mínimos:

- `propertiesFile`: ruta relativa del JSON de propiedades.
- `statusesFile`: ruta relativa del JSON de estados.
- `imagesSourceDirectory`: ruta relativa del directorio fuente de imágenes.
- `imagesPublicDirectory`: ruta relativa pública final, por ejemplo `images/properties`.
- `imagesPublicBaseUrl`: ruta base pública, por ejemplo `/images/properties`.

## Reglas de idempotencia

- Las propiedades se identifican por `Id` y se actualizan al coincidir.
- Los estados se validan contra el enum y no generan entidad persistida.
- Las imágenes se sobrescriben de forma consistente en el destino público sin crear referencias duplicadas.
- Ningún registro seed debe eliminar o modificar registros no incluidos en el conjunto inicial salvo los campos iniciales de una propiedad coincidente.

## Transiciones de estado

La spec no define transiciones HTTP. El modelo admite cualquier cambio directo entre `Available`, `Rented` y `Maintenance` en futuras operaciones autorizadas.
