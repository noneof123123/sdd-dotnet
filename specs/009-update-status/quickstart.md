# Guía rápida: 009-update-status

Rama: `009-update-status`
Fecha: 2026-11-20
Especificación: `specs/009-update-status/spec.md`
Estado de la guía: ejecutada; registra la verificación realizada después de implementar la característica.

## Requisitos previos

- Ejecutar todos los comandos desde la raíz del repositorio.
- SDK de .NET conforme a `global.json`.
- Dependencias restauradas.
- Node.js disponible para Redocly.
- Para la prueba manual, API en ejecución con base de datos PostgreSQL accesible.

No se necesita PostgreSQL para las pruebas unitarias ni para regenerar el documento OpenAPI, salvo que el entorno local así lo requiera.

## Secuencia de validación

### 1. Restaurar y compilar

```bash
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
```

Resultado esperado: compilación sin errores.

### 2. Ejecutar pruebas de la característica

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~ChangePropertyStatus"
```

Resultado esperado: todas las pruebas de `ValidatorTests`, `HandlerTests`, `MappingTests` y `SliceTests` pasan.

### 3. Ejecutar pruebas OpenAPI

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~OpenApi"
```

Resultado esperado: documento, deriva, CLI y herramientas validan 4 rutas, 6 operaciones y `ChangePropertyStatus`.

### 4. Ejecutar toda la solución

```bash
dotnet test app/Realtor.sln
```

Resultado esperado: toda la suite verde; no se aceptan pruebas omitidas nuevas.

### 5. Regenerar el contrato público

```bash
sha256sum app/backend/src/RealtorApi/wwwroot/openapi/v1.json > /tmp/realtor-openapi-v1.before
./support/scripts/generate-openapi-v1.sh
sha256sum app/backend/src/RealtorApi/wwwroot/openapi/v1.json > /tmp/realtor-openapi-v1.after
diff -u /tmp/realtor-openapi-v1.before /tmp/realtor-openapi-v1.after
```

Resultado esperado: el script termina con `OK` y `diff` no muestra cambios en `v1.json`.

Si el script falla por un puerto ocupado, detener el proceso que usa el puerto temporal del script y repetir. No editar `v1.json` manualmente.

### 6. Validar con Redocly y NSwag

```bash
npx @redocly/cli@2.19.0 lint app/backend/src/RealtorApi/wwwroot/openapi/v1.json --extends=minimal
dotnet tool restore --tool-manifest .config/dotnet-tools.json
dotnet build app/backend/src/RealtorApi/RealtorApi.csproj /p:GenerateDocumentationFile=true /p:NoWarn=1591
dotnet tool run nswag run support/scripts/openapi-v1.nswag.json
```

Resultado esperado: Redocly sin errores, build correcto y generación NSwag local correcta.

## Prueba manual desde `.http`

La petición debe añadirse a `app/backend/src/RealtorApi/RealtorApi.http`:

```http
### Cambiar estado de propiedad a Rented
PATCH {{BaseUrl}}/api/properties/11111111-1111-1111-1111-111111111111/status
Content-Type: application/json

{
  "status": "Rented"
}
```

Comprobación:

- `200 OK` con la propiedad completa.
- `status` cambia a `Rented`.
- `title`, `description`, `address`, `price`, `bedroomCount`, `bathroomCount`, `areaSquareMeters`, `imageUrl` y `createdAt` conservan sus valores.
- Repetir la misma petición no cambia `updatedAt`.

## Casos negativos manuales

```http
### Estado ausente: 400
PATCH {{BaseUrl}}/api/properties/11111111-1111-1111-1111-111111111111/status
Content-Type: application/json

{}
```

```http
### Estado inválido: 400
PATCH {{BaseUrl}}/api/properties/11111111-1111-1111-1111-111111111111/status
Content-Type: application/json

{
  "status": "Sold"
}
```

```http
### Propiedad inexistente: 404
PATCH {{BaseUrl}}/api/properties/00000000-0000-0000-0000-000000000000/status
Content-Type: application/json

{
  "status": "Available"
}
```

## Criterios de cierre

- Compilación y pruebas verdes.
- Contrato HTTP y OpenAPI implementados según los archivos de `contracts/`.
- Documentación de 008 actualizada solo en inventario y ejemplos.
- Ejemplo manual presente en `RealtorApi.http`.
- Sin cambios aguas arriba, sin cambios de esquema y sin nuevos proyectos.
- Toda la documentación modificada permanece en español.
