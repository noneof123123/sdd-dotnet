# Validación rápida: Actualización de propiedades

## Requisitos previos

- PostgreSQL accesible mediante `ConnectionStrings:DefaultConnection` para validación manual de la API.
- Esquema ya migrado por las iniciativas `001` a `005`; esta feature no agrega migraciones.
- Directorio `app/backend/src/RealtorApi/wwwroot/assets/properties` disponible.
- API ejecutándose, por ejemplo en `http://localhost:5023`.
- Una propiedad existente creada con `POST /api/properties` o presente en los datos semilla.

## Compilar y probar

```text
dotnet build app/Realtor.sln
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Para limitar la ejecución a esta característica:

```text
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~UpdateProperty"
```

Las definiciones de request, respuesta y errores están en `contracts/update-property.http.md`; las reglas de datos están en `data-model.md`.

## Actualizar sin imagen

Enviar `PUT /api/properties/{id}` con los ocho campos editables y sin parte `image`.

Resultado esperado:

- HTTP 200 con la representación pública actualizada.
- `id` sin cambios.
- `imageUrl` idéntico al valor previo, incluido `null` si no había imagen.
- No se escribe ni se elimina ningún archivo.
- La siguiente consulta a `GET /api/properties` muestra los datos vigentes.

## Actualizar con PNG o JPG/JPEG válido

Repetir la solicitud y adjuntar exactamente una parte `image` válida de hasta 5 × 1024 × 1024 bytes.

Resultado esperado:

- HTTP 200 con `imageUrl` relativo al nuevo archivo.
- Un único archivo activo asociado a la propiedad.
- El archivo anterior deja de estar asociado.
- El nombre original y las rutas físicas no aparecen en la respuesta.

## Validar rechazos

Probar separadamente:

- Identificador mal formado o vacío.
- Identificador válido que no corresponda a ninguna propiedad.
- Textos vacíos, longitudes excedidas, precio o cantidades negativas y estado desconocido.
- Archivo GIF, PDF o contenido incompatible con la declaración.
- Archivo mayor que 5 × 1024 × 1024 bytes.
- Dos partes `image` o una parte `image` que no sea archivo.

Resultado esperado:

- HTTP 400 con `ValidationProblemDetails` para solicitudes inválidas.
- HTTP 404 con `ProblemDetails` para identificador inexistente.
- Sin propiedad modificada, sin archivos nuevos y sin archivos anteriores retirados.

## Validar consistencia interna

Provocar fallos controlados en stubs de almacenamiento y persistencia:

- Fallo antes de publicar la imagen nueva.
- Fallo de persistencia después de publicar la imagen nueva.
- Fallo al retirar la imagen anterior después de persistir.
- Cancelación durante publicación, persistencia o retirada.

Resultado esperado:

- HTTP 500 genérico o propagación de cancelación según el caso.
- Estado anterior de la propiedad conservado cuando la operación no se confirma.
- Cero archivos nuevos huérfanos observables.
- Sin detalles técnicos en las respuestas de error.

## Validar compatibilidad

- Consultar `GET /health`.
- Consultar `GET /api/properties` antes y después de actualizar.
- Crear una propiedad nueva con `POST /api/properties` después de actualizar.
- Comprobar que los contratos de creación, listado, paginación y descubrimiento automático no cambiaron.

## Evidencia de ejecución

- **Fecha**: 2026-11-20
- **SDK**: `dotnet --version` → `11.0.100-rc.3.26522.148`
- **Compilación**: `dotnet build app/Realtor.sln` → `Build succeeded`, 0 advertencias, 0 errores.
- **Pruebas de la característica**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~UpdateProperty"` → 37 ejecutadas, 37 superadas, 0 fallidas, 0 omitidas.
- **Pruebas completas del backend**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build` → 108 ejecutadas, 108 superadas, 0 fallidas, 0 omitidas.

La validación manual contra PostgreSQL y una API en ejecución no forma parte de esta evidencia; los escenarios HTTP quedaron cubiertos por pruebas unitarias, de componente en memoria y ejemplos en `app/backend/src/RealtorApi/RealtorApi.http`.
