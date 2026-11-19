# Validación rápida: Paginación y listado de propiedades

## Requisitos previos

- Backend configurado y PostgreSQL accesible.
- Migración de propiedades aplicada.
- Seed inicial disponible.
- API ejecutándose en un host conocido, por ejemplo `http://localhost:5023`.

## Compilar y probar

```text
dotnet build app/Realtor.sln
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

## Consultar el listado

```http
GET http://localhost:5023/api/properties
Accept: application/json
```

Resultado esperado:

- `page = 1`.
- `pageSize = 6`.
- Hasta 6 propiedades.
- Orden por `Title` y luego `Address` ascendente.
- Metadatos consistentes.
- `imageUrl` absoluta con el host efectivo.

## Consultar una página explícita

```http
GET http://localhost:5023/api/properties?page=2&pageSize=6
Accept: application/json
```

Resultado esperado:

- Segunda página si existe.
- Si `page` excede el rango, normalización a la última página disponible.

## Validar parámetros

```http
GET http://localhost:5023/api/properties?page=0&pageSize=6
```

Resultado esperado: HTTP 400 con `ValidationProblemDetails`.

```http
GET http://localhost:5023/api/properties?page=1&pageSize=100
```

Resultado esperado: `pageSize = 24` normalizado.

## Validar imágenes

Cada item válido debe producir una URL como:

```text
http://localhost:5023/assets/properties/1.png
```

Si no existe un nombre de imagen válido, `imageUrl` debe ser una cadena vacía.

## Evidencia ejecutada

**Fecha:** 2026-11-19

- `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` — 33 pruebas aprobadas, 0 fallidas.
- `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj --no-restore` — compilación exitosa.
- `GET http://localhost:5023/api/properties` — HTTP 200; página 1, tamaño 6 y metadatos consistentes.
- `GET http://localhost:5023/api/properties?page=2&pageSize=6` — HTTP 200; segunda página con `hasPrevious=true` y `hasNext=false`.
- `GET http://localhost:5023/api/properties?page=99&pageSize=2` — HTTP 200; normalización a la última página disponible.
- `GET http://localhost:5023/api/properties?page=1&pageSize=100` — HTTP 200; `pageSize` normalizado a 24.
- `GET http://localhost:5023/api/properties?page=0&pageSize=6` — HTTP 400 con `ValidationProblemDetails`.
- `GET http://localhost:5023/assets/properties/1.png` — HTTP 200; imagen disponible públicamente.
