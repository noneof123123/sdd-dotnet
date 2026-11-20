# Validación rápida: Alta de propiedades

## Requisitos previos

- PostgreSQL accesible mediante `ConnectionStrings:DefaultConnection`.
- Migración que hace nullable `Property.ImageUrl` aplicada al arrancar.
- Directorio `app/backend/src/RealtorApi/wwwroot/assets/properties` disponible.
- API ejecutándose, por ejemplo en `http://localhost:5023`.

## Compilar y probar

```text
dotnet build app/Realtor.sln
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

## Crear sin imagen

Enviar `POST /api/properties` como `multipart/form-data` con todos los campos comerciales y físicos, sin parte `image`.

Resultado esperado:

- HTTP 201.
- Encabezado `Location: /api/properties/{id}`.
- Cuerpo con la propiedad creada.
- `imageUrl = null`.
- No se crea archivo nuevo.

## Crear con PNG válido

Enviar el mismo formulario y adjuntar una imagen PNG válida de hasta 5 × 1024 × 1024 bytes.

Resultado esperado:

- HTTP 201.
- Encabezado `Location: /api/properties/{id}`.
- Un archivo seguro bajo `wwwroot/assets/properties`.
- `imageUrl` con `/assets/properties/{nombreSeguro}.png`.

## Crear con JPG/JPEG válido

Repetir el flujo con una imagen JPG/JPEG válida.

Resultado esperado:

- HTTP 201.
- Un único archivo publicado.
- `imageUrl` relativa correspondiente.

## Validar rechazos

Probar separadamente:

- Campos obligatorios faltantes o negativos.
- Estado fuera de `Available`, `Rented` o `Maintenance`.
- Archivo GIF, PDF o contenido incompatible con la declaración.
- Archivo mayor de 5 × 1024 × 1024 bytes.
- Dos partes `image`.

Resultado esperado: HTTP 400 con `ValidationProblemDetails` o `ProblemDetails`, sin propiedad ni archivo creado.

## Validar consistencia interna

- Provocar un fallo controlado de persistencia después de almacenar una imagen válida.
- Provocar un fallo controlado de almacenamiento antes de persistir.

Resultado esperado:

- HTTP 500 con `ProblemDetails`.
- Sin propiedad persistida ni archivo huérfano observable.
- Sin detalles técnicos sensibles en la respuesta.

## Validar compatibilidad

- Consultar `GET /health`.
- Consultar `GET /api/properties`.
- Comprobar que una propiedad sin imagen aparece con el valor vacío definido por el listado o `imageUrl` compatible con el contrato vigente.
- Comprobar que una propiedad con imagen creada aparece con una URL consumible bajo `/assets/properties/{nombreSeguro}`.

## Evidencia de ejecución real

**Fecha**: 2026-11-20

### Comandos ejecutados desde la raíz del repositorio

```text
git diff --check
dotnet build app/Realtor.sln
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

### Resultados

- `git diff --check`: sin errores de whitespace.
- `dotnet build app/Realtor.sln`: compilación correcta, 0 advertencias y 0 errores.
- `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`: 71 pruebas ejecutadas, 71 correctas, 0 fallidas y 0 omitidas.

La ejecución anterior cubre pruebas unitarias, pruebas de componente en memoria y pruebas del delegado del endpoint con `DefaultHttpContext`. No se ejecutó una API real con PostgreSQL ni se enviaron solicitudes HTTP a un servidor en ejecución; por tanto, no se declara evidencia de esos escenarios.

### Revisión final de gobernanza

- No se agregaron controllers ni rutas de actualización, eliminación o autorización.
- `CreatePropertySlice.cs` solo enlaza, delega y traduce resultados; no contiene validación, persistencia ni escritura de archivos.
- `CreatePropertyMapping.cs` solo proyecta la entidad a `CreatePropertyResponse`; no valida, persiste ni escribe archivos.
- `CreatePropertyHandler.cs` es el único propietario de la coordinación del caso de uso y devuelve `Result<CreatePropertyResponse>`.
- Toda la validación de entrada permanece en `CreatePropertyValidator`, incluidos MIME, extensión, firma, tamaño, tipo de contenido y cantidad de partes `image`.
- Las respuestas y rutas persistidas son relativas bajo `/assets/properties/{nombreSeguro}`; no exponen rutas físicas ni nombres originales.
- El listado paginado conserva su contrato: la conversión a URL absoluta y el valor vacío para imágenes ausentes o inválidas no cambiaron.
- `/health`, `MapSliceEndpoints`, `RegisterSlices` y `RegisterHandlers` conservan su comportamiento y descubrimiento automático.
