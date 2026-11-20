# Contrato HTTP: Actualizar propiedad

## Endpoint

```text
PUT /api/properties/{id}
Content-Type: multipart/form-data
```

El valor de `{id}` debe ser un `Guid` válido. El endpoint es público; esta iniciativa no agrega autenticación ni autorización.

## Campos requeridos

Todos los campos siguientes son obligatorios y se envían como reemplazo completo:

- `title`: texto de máximo 200 caracteres.
- `description`: texto de máximo 4000 caracteres.
- `address`: texto de máximo 500 caracteres.
- `price`: decimal no negativo.
- `status`: `Available`, `Rented` o `Maintenance`.
- `bedroomCount`: entero mayor o igual a cero.
- `bathroomCount`: entero mayor o igual a cero.
- `areaSquareMeters`: decimal mayor o igual a cero.
- `image`: archivo opcional PNG/JPG/JPEG de máximo 5 × 1024 × 1024 bytes inclusive.

Solo puede existir una parte `image`. Si se omite, se conserva la imagen asociada actual. No existe un mecanismo para eliminar explícitamente la imagen.

## Respuesta exitosa sin imagen

```text
HTTP 200 OK
Content-Type: application/json
```

El cuerpo conserva `imageUrl` con la ruta relativa previa, o `null` si la propiedad no tenía imagen. `id` y el valor previo de creación no se exponen como campos separados, pero `id` se devuelve sin cambios.

## Respuesta exitosa con imagen

```text
HTTP 200 OK
Content-Type: application/json
```

El cuerpo contiene `imageUrl` con la nueva ruta relativa `/assets/properties/{nombreSeguro}.{png|jpg|jpeg}`. La imagen anterior deja de estar asociada una vez confirmada la actualización.

## Forma del cuerpo de respuesta

```json
{
  "id": "3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f",
  "title": "Casa actualizada",
  "description": "Descripción vigente",
  "address": "Calle Central 123",
  "price": 275000.00,
  "status": "Available",
  "bedroomCount": 3,
  "bathroomCount": 2,
  "areaSquareMeters": 145.50,
  "imageUrl": "/assets/properties/9f2c4a1b8d3e4f5a6b7c8d9e0f1a2b3c.png"
}
```

`status` se devuelve como texto. `imageUrl` puede ser `null`. Nunca se devuelven rutas físicas ni nombres originales de archivo.

## Solicitud sin imagen

```http
PUT http://localhost:5023/api/properties/3f8b1c2d-4e5a-4b6c-8d9e-0a1b2c3d4e5f
Content-Type: multipart/form-data; boundary=realtor-update

--realtor-update
Content-Disposition: form-data; name="title"

Casa actualizada
--realtor-update
Content-Disposition: form-data; name="description"

Descripción vigente
--realtor-update
Content-Disposition: form-data; name="address"

Calle Central 123
--realtor-update
Content-Disposition: form-data; name="price"

275000.00
--realtor-update
Content-Disposition: form-data; name="status"

Available
--realtor-update
Content-Disposition: form-data; name="bedroomCount"

3
--realtor-update
Content-Disposition: form-data; name="bathroomCount"

2
--realtor-update
Content-Disposition: form-data; name="areaSquareMeters"

145.50
--realtor-update--
```

## Solicitud con imagen

Se envía el mismo formulario y se añade una única parte `image` con `filename`, por ejemplo `photo.jpg` y `Content-Type: image/jpeg`. El nombre original no se conserva ni se publica.

## Errores

- Identificador mal formado, campos inválidos, imagen no admitida, tamaño excedido, más de una parte `image` o solicitud mal formada: HTTP 400 con `ValidationProblemDetails`.
- Identificador válido pero inexistente: HTTP 404 con `ProblemDetails`.
- Fallo interno de publicación, persistencia o compensación: HTTP 500 con `ProblemDetails` genérico, sin detalles técnicos ni resultados parciales observables.

No se devuelven 413 ni 415: la especificación vigente agrupa esos casos bajo solicitud inválida y la infraestructura actual ya los representa con HTTP 400.
