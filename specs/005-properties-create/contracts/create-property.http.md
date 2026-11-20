# Contrato HTTP: Crear propiedad

## Endpoint

```text
POST /api/properties
Content-Type: multipart/form-data
```

## Campos requeridos

- `title`: texto de máximo 200 caracteres.
- `description`: texto de máximo 4000 caracteres.
- `address`: texto de máximo 500 caracteres.
- `price`: decimal no negativo.
- `status`: `Available`, `Rented` o `Maintenance`.
- `bedroomCount`: entero mayor o igual a cero.
- `bathroomCount`: entero mayor o igual a cero.
- `areaSquareMeters`: decimal mayor o igual a cero.
- `image`: archivo opcional PNG/JPG/JPEG de máximo 5 × 1024 × 1024 bytes.

## Respuesta exitosa sin imagen

```text
HTTP 201 Created
Location: /api/properties/{id}
Content-Type: application/json
```

El cuerpo contiene `imageUrl: null`.

## Respuesta exitosa con imagen

```text
HTTP 201 Created
Location: /api/properties/{id}
Content-Type: application/json
```

El cuerpo contiene `imageUrl: /assets/properties/{nombreSeguro}.{png|jpg}`.

## Solicitud sin imagen

```http
POST http://localhost:5023/api/properties
Content-Type: multipart/form-data; boundary=realtor-create

--realtor-create
Content-Disposition: form-data; name="title"

Casa de prueba
--realtor-create
Content-Disposition: form-data; name="description"

Propiedad creada sin fotografía.
--realtor-create
Content-Disposition: form-data; name="address"

Calle Central 123
--realtor-create
Content-Disposition: form-data; name="price"

250000.00
--realtor-create
Content-Disposition: form-data; name="status"

Available
--realtor-create
Content-Disposition: form-data; name="bedroomCount"

3
--realtor-create
Content-Disposition: form-data; name="bathroomCount"

2
--realtor-create
Content-Disposition: form-data; name="areaSquareMeters"

140.50
--realtor-create--
```

## Solicitud con imagen

El campo `image` debe adjuntar exactamente un único archivo. El nombre original no se conserva ni se utiliza para publicar el archivo.

## Errores

- Datos inválidos o solicitud mal formada: HTTP 400 con `ValidationProblemDetails` o `ProblemDetails`.
- MIME o contenido incompatible: HTTP 400.
- Archivo mayor de 5 × 1024 × 1024 bytes: HTTP 400.
- Más de una parte `image` o parte inválida: HTTP 400.
- Fallo interno de persistencia o almacenamiento: HTTP 500 con `ProblemDetails`, sin detalles técnicos.
