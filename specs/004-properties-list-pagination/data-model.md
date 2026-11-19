# Modelo de datos: Paginación y listado de propiedades

## Entidades existentes

### Property

Entidad persistente existente. Se consulta mediante proyección EF Core y no se expone directamente.

Campos relevantes:

- `Id: Guid`
- `Title: string`
- `Description: string`
- `Address: string`
- `Price: decimal`
- `Status: PropertyStatus`
- `BedroomCount: int`
- `BathroomCount: int`
- `AreaSquareMeters: decimal`
- `ImageUrl: string`

## Contratos del caso de uso

### ListPropertiesRequest

- `Page: int`, opcional, default 1, entero positivo.
- `PageSize: int`, opcional, default 6, entero positivo y máximo efectivo 24.

### ListPropertiesItemResponse

- `Id: Guid`
- `Title: string`
- `Description: string`
- `Address: string`
- `Price: decimal`
- `Status: string`
- `BedroomCount: int`
- `BathroomCount: int`
- `AreaSquareMeters: decimal`
- `ImageUrl: string`, absoluta o vacía si no existe un archivo válido.

### ListPropertiesResponse

- `Items: IReadOnlyList<ListPropertiesItemResponse>`
- `Page: int`
- `PageSize: int`
- `TotalItems: int`
- `TotalPages: int`
- `HasNext: bool`
- `HasPrevious: bool`

## Reglas de consulta

1. Contar todas las propiedades con `CountAsync`.
2. Normalizar `pageSize` a 24 si supera ese límite.
3. Calcular `totalPages = ceil(totalItems / pageSize)`.
4. Si `totalPages = 0`, conservar `page = 1` y una colección vacía.
5. Si `page > totalPages`, normalizar `page = totalPages`.
6. Ordenar por `Title` ascendente y `Address` ascendente.
7. Aplicar `Skip` y `Take` sobre la proyección.

## Mapping de imagen

El nombre de archivo se obtiene del valor persistido cuando representa una ruta válida. Se construye:

```text
{Request.Scheme}://{Request.Host}/assets/properties/{fileName}
```

Si el nombre de archivo está vacío o no puede resolverse de forma segura, `imageUrl` será una cadena vacía.
