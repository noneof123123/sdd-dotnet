# Contrato HTTP: cambiar estado de propiedad

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`

## Operación

| Elemento | Valor |
|---|---|
| Método | `PATCH` |
| Ruta | `/api/properties/{id}/status` |
| Autenticación | Ninguna; operación pública |
| Content-Type de solicitud | `application/json` |
| Content-Type de respuesta | `application/json` |
| Identificador OpenAPI | `ChangePropertyStatus` |
| Etiqueta OpenAPI | `Propiedades` |
| Cuerpo | Objeto JSON con `status` |
| Éxito | `200 OK` con representación pública completa |

## Solicitud

### Parámetros

| Nombre | Origen | Tipo | Obligatorio | Descripción |
|---|---|---|---|---|
| `id` | Ruta | `string` con formato `Guid` | Sí | Identificador de la propiedad existente; un formato inválido produce `400` estructurado. |
| `status` | Cuerpo | `string` | Sí | Uno de `Available`, `Rented` o `Maintenance`. |

### Ejemplo válido

```http
PATCH /api/properties/11111111-1111-1111-1111-111111111111/status HTTP/1.1
Content-Type: application/json

{
  "status": "Rented"
}
```

### Ejemplo con cuerpo ausente

```http
PATCH /api/properties/11111111-1111-1111-1111-111111111111/status HTTP/1.1
Content-Type: application/json
```

Respuesta: `400 Bad Request` con `HttpValidationProblemDetails`.

## Respuesta correcta

Ejemplo de `200 OK`:

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "title": "Piso céntrico",
  "description": "Piso luminoso junto al parque.",
  "address": "Calle Mayor 10",
  "price": 125000.00,
  "status": "Rented",
  "bedroomCount": 2,
  "bathroomCount": 1,
  "areaSquareMeters": 78.50,
  "imageUrl": "http://localhost:5085/assets/properties/piso-centrico.jpg",
  "createdAt": "2026-11-20T10:00:00Z",
  "updatedAt": "2026-11-20T11:30:00Z"
}
```

Reglas de la respuesta:

- `status` refleja el valor persistido tras la operación.
- Todos los demás campos conservan sus valores persistidos.
- `imageUrl` es absoluta cuando existe archivo y cadena vacía cuando no.
- `createdAt` y `updatedAt` están normalizados a UTC.
- Repetir el estado vigente devuelve `200` y conserva `updatedAt`.

## Respuestas de error

### 400 Bad Request

Se produce cuando `id` está vacío, el cuerpo falta, `status` falta, está vacío o no es uno de los valores exactos admitidos.

Ejemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Status": [
      "El estado debe ser uno de: Available, Rented, Maintenance."
    ]
  }
}
```

El cuerpo exacto de `type` y `title` se rige por el formato estándar de ASP.NET Core; el contrato garantiza `status: 400` y el diccionario `errors`.

### 404 Not Found

Se produce cuando no existe una propiedad con el `id` indicado.

```json
{
  "status": 404,
  "title": "No existe la propiedad",
  "detail": "No existe una propiedad con el identificador indicado."
}
```

### 500 Internal Server Error

Solo para fallos inesperados. No incluye detalles técnicos.

```json
{
  "status": 500,
  "title": "Error interno"
}
```

No existe respuesta `409`: la concurrencia se resuelve con última escritura gana.

## Invariantes

- El cuerpo nunca acepta `title`, `description`, `address`, `price`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` ni `image`.
- Campos desconocidos en el cuerpo no alteran la propiedad.
- No se suben archivos.
- No se cambian las rutas ni la semántica de los endpoints existentes.
- La operación es idempotente para el mismo `id` y `status`.
