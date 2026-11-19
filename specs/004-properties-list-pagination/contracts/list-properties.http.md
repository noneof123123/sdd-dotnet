# Contrato HTTP: GET /api/properties

## Solicitud

```http
GET /api/properties?page=1&pageSize=6
Accept: application/json
```

Todos los parámetros son opcionales:

- `page`: entero positivo, default 1.
- `pageSize`: entero positivo, default 6, máximo efectivo 24.

## Respuesta exitosa

```json
{
  "items": [
    {
      "id": "a1f0e2d4-7c81-4d12-9e61-0b7f0b1a1001",
      "title": "Casa Coral Way con jardín tropical",
      "description": "Descripción",
      "address": "2450 SW 22nd St, Miami, FL 33145",
      "price": 4200.00,
      "status": "Available",
      "bedroomCount": 3,
      "bathroomCount": 2,
      "areaSquareMeters": 185.50,
      "imageUrl": "http://localhost:5023/assets/properties/1.png"
    }
  ],
  "page": 1,
  "pageSize": 6,
  "totalItems": 10,
  "totalPages": 2,
  "hasNext": true,
  "hasPrevious": false
}
```

## Validación inválida

HTTP 400 con `ValidationProblemDetails` cuando `page` o `pageSize` son menores o iguales a cero o no son enteros.

## Página fuera de rango

Si `page` supera `totalPages`, el handler debe responder con la última página disponible y metadatos consistentes. Cuando no existen propiedades, la página puede ser vacía con `totalPages = 0`.
