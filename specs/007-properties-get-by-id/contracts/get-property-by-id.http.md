# Contrato HTTP: Consulta de propiedad por identificador

**Operación**: `GET /api/properties/{id}`

**Fecha**: 2026-11-20

**Estado**: Borrador de contrato derivado de [spec.md](../spec.md) y [data-model.md](../data-model.md).

## Solicitud

```http
GET /api/properties/{id} HTTP/1.1
Accept: application/json
```

El segmento `{id}` debe ser un identificador con formato válido y distinto de vacío. No requiere cuerpo, autenticación ni parámetros de consulta.

## Respuesta correcta

HTTP 200 con una representación pública explícita.

```json
{
  "id": "3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f",
  "title": "Casa con fotografía",
  "description": "Propiedad publicada con PNG válido.",
  "address": "Avenida Central 456",
  "price": 320000.00,
  "status": "Available",
  "bedroomCount": 4,
  "bathroomCount": 3,
  "areaSquareMeters": 180.00,
  "imageUrl": "http://localhost:5023/assets/properties/9f2c4a1b8d3e4f5a6b7c8d9e0f1a2b3c.png",
  "createdAt": "2026-11-20T10:00:00Z",
  "updatedAt": null
}
```

`imageUrl` se construye con el esquema y host de la solicitud. Si la propiedad no tiene imagen válida, el valor es `""`.

```json
{
  "id": "2a7c4d1e-9b3f-4a82-8c17-6d5e4f3a2b19",
  "title": "Casa sin fotografía",
  "description": "Propiedad publicada sin imagen.",
  "address": "Calle Central 123",
  "price": 250000.00,
  "status": "Rented",
  "bedroomCount": 3,
  "bathroomCount": 2,
  "areaSquareMeters": 140.50,
  "imageUrl": "",
  "createdAt": "2026-11-20T09:30:00Z",
  "updatedAt": "2026-11-20T11:45:00Z"
}
```

No se devuelven metadatos de paginación, rutas físicas, nombres originales de archivo, entidades persistentes ni detalles internos.

## Errores

### Identificador mal formado o vacío

HTTP 400 con errores de validación estructurados.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Id": ["El identificador de la propiedad es obligatorio."]
  }
}
```

Un path sin segmento `id` no alcanza este endpoint y queda resuelto por el routing general.

### Propiedad inexistente

HTTP 404 con `ProblemDetails`.

```json
{
  "type": "about:blank",
  "title": "Propiedad no encontrada",
  "status": 404,
  "detail": "No existe una propiedad con el identificador solicitado."
}
```

### Error interno

HTTP 500 con mensaje genérico y sin detalles técnicos.

```json
{
  "type": "about:blank",
  "title": "Error interno",
  "status": 500,
  "detail": "Ocurrió un error inesperado. Intente nuevamente."
}
```

## Compatibilidad

Los diez campos compartidos con `GET /api/properties` conservan nombre, tipo y significado. La operación de detalle añade `createdAt` y `updatedAt`, y no modifica los contratos de listado, creación ni actualización.
