# Modelo de datos: Alta de propiedades

## Entidades existentes modificadas

### Property

Entidad persistente existente. Esta feature modifica únicamente la nulabilidad de `ImageUrl`.

| Campo | Tipo | Persistencia | Restricciones |
|---|---|---|---|
| Id | Guid | PK | Obligatorio |
| Title | string | VARCHAR 200 | Obligatorio; no vacío; máximo 200 |
| Description | string | VARCHAR 4000 | Obligatorio; no vacío; máximo 4000 |
| Address | string | VARCHAR 500 | Obligatorio; no vacío; máximo 500 |
| Price | decimal | decimal(18,2) | Obligatorio; mayor o igual a cero |
| Status | PropertyStatus | VARCHAR 32 | Available, Rented o Maintenance |
| BedroomCount | int | INTEGER | Mayor o igual a cero |
| BathroomCount | int | INTEGER | Mayor o igual a cero |
| AreaSquareMeters | decimal | decimal(12,2) | Mayor o igual a cero |
| ImageUrl | string? | VARCHAR 1000 nullable | Ruta relativa `/assets/properties/{nombreSeguro}` o null |
| CreatedAt | DateTime | TIMESTAMP WITH TIME ZONE | Asignado por el handler al crear |
| UpdatedAt | DateTime? | TIMESTAMP WITH TIME ZONE | null al crear |

### Propiedad creada sin imagen

- `ImageUrl` tiene valor `null`.
- No se escribe ningún archivo.
- La respuesta conserva `imageUrl = null`.

### Propiedad creada con imagen

- Se genera un nombre seguro único con extensión PNG/JPG según el contenido validado.
- Se almacena bajo `wwwroot/assets/properties/{nombreSeguro}`.
- Se persiste `/assets/properties/{nombreSeguro}`.
- La respuesta devuelve esa ruta relativa.

## Contratos del caso de uso

### CreatePropertyRequest

Campos recibidos mediante `multipart/form-data`:

- `Title: string`
- `Description: string`
- `Address: string`
- `Price: decimal`
- `Status: PropertyStatus`
- `BedroomCount: int`
- `BathroomCount: int`
- `AreaSquareMeters: decimal`
- `Image: IFormFile?` opcional

### CreatePropertyResponse

- `Id: Guid`
- `Title: string`
- `Description: string`
- `Address: string`
- `Price: decimal`
- `Status: string`
- `BedroomCount: int`
- `BathroomCount: int`
- `AreaSquareMeters: decimal`
- `ImageUrl: string?`

## Validación

- Todos los campos de texto y números obligatorios deben cumplir el modelo persistente.
- El estado debe pertenecer al enum `PropertyStatus`.
- Precio, habitaciones, baños y superficie deben ser mayores o iguales a cero.
- Solo puede existir una parte `image`.
- La imagen debe tener MIME y firma compatibles con PNG, JPG o JPEG.
- El tamaño máximo inclusivo es 5 × 1024 × 1024 bytes.

## Migración

Esta feature requiere una única migración coherente que cambie la columna `ImageUrl` de no nullable a nullable. No se agregan entidades ni tablas nuevas.
