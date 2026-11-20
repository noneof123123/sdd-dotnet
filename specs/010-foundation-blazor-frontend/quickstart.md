# Guía de inicio rápido: Fundación del frontend web

Rama: `010-foundation-blazor-frontend`
Fecha: 2026-11-20
Objetivo: describir cómo verificar la fundación web después de `speckit.implement`, sin sustituir la spec ni el plan.

## Prerrequisitos

- SDK de .NET fijado por `global.json`: `11.0.100-rc.3.26522.148`.
- Solución `app/Realtor.sln` disponible en el estado actual de la rama.
- Backend y frontend comparten la misma solución; no se crean proyectos nuevos.
- No se requiere PostgreSQL para las pruebas unitarias de esta feature.
- No se requiere `pwsh`: las plantillas se resuelven con los scripts `sh` existentes.

## Comandos desde la raíz del repositorio

```bash
dotnet --version
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj --no-build
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build --filter "FullyQualifiedName~OpenApi"
dotnet test app/Realtor.sln --no-build
```

Resultado esperado:

- `dotnet --version` coincide con `global.json`.
- Restauración y compilación sin errores.
- Pruebas frontend en verde, incluyendo render de catálogo, tarjeta, detalle y estados.
- Pruebas OpenAPI siguen en verde: `4` rutas y `6` operaciones.
- La solución completa sigue en verde.

Si el número de rutas u operaciones OpenAPI cambia, se detiene la verificación y se reporta la divergencia; esta feature no autoriza modificar el contrato del backend.

## Evidencia de validación automatizada ejecutada

- **Fecha**: 2026-11-20.
- **Entorno**: SDK `11.0.100-rc.3.26522.148`, coincidente con `global.json`; Linux sin `pwsh`.
- **Comandos ejecutados desde la raíz del repositorio** y resultados reales:

| Comando | Resultado |
|---|---|
| `dotnet restore app/Realtor.sln` | Correcto; 4 proyectos restaurados. Advertencia NU1902 del paquete transitivo `Microsoft.Extensions.Validation` 11.0.0-rc.2.26504.105, no bloqueante. |
| `dotnet build app/Realtor.sln --no-restore` | Compilación correcta; 0 errores y 1 advertencia NU1902. |
| `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj --no-build` | 45 pruebas superadas, 0 fallidas. |
| `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build --filter "FullyQualifiedName~OpenApi"` | 17 pruebas superadas, 0 fallidas. |
| `dotnet test app/Realtor.sln --no-build` | 45 pruebas frontend y 213 pruebas backend superadas, 0 fallidas. |

- **Regresión OpenAPI**: `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` sin cambios en `git`; se conservan `4` rutas y `6` operaciones.
- **Diff**: `git diff --check` sin errores.
- **Límite de esta evidencia**: no sustituye la comprobación manual en navegador a 320, 768 y 1280 px (T040) ni las mediciones reales de SC-003 y SC-010 (T042), que siguen pendientes.

## Paquetes y configuración esperados

```bash
dotnet add app/frontend/src/RealtorWeb/RealtorWeb.csproj package Refit --version 17.0.0
dotnet add app/frontend/src/RealtorWeb/RealtorWeb.csproj package Refit.HttpClientFactory --version 17.0.0
dotnet add app/frontend/src/RealtorWeb/RealtorWeb.csproj package Refit.HttpClientFactory.Reflection --version 17.0.0
dotnet add app/frontend/src/RealtorWeb/RealtorWeb.csproj package LucideBlazor --version 1.0.7
dotnet add app/frontend/test/RealtorWeb/RealtorWebTests.csproj package bunit --version 2.14.0
```

Estos comandos son la referencia de versión para `tasks.md` y la implementación. No se cambia `global.json`.

Configuración esperada en `app/frontend/src/RealtorWeb/appsettings.json` o `appsettings.Development.json`:

```json
{
  "ApiClients": {
    "RealtorApi": {
      "BaseAddress": "http://localhost:5085/"
    }
  }
}
```

No se almacenan secretos en esta configuración. En otros entornos, el valor puede sobreescribirse con configuración estándar de ASP.NET Core.

## Verificación manual local

1. Iniciar la API en una terminal:

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj --launch-profile http
```

2. Iniciar el frontend en otra terminal:

```bash
dotnet run --project app/frontend/src/RealtorWeb/RealtorWeb.csproj --launch-profile http
```

3. Abrir `http://localhost:5242/`.

4. Comprobar:

- El documento declara `lang="es"`.
- El catálogo muestra datos reales del backend o el estado vacío cuando no hay datos.
- Cada tarjeta muestra imagen o marcador de posición, etiqueta de estado en español, título, precio, dirección y datos numéricos.
- No aparecen `Available`, `Rented` ni `Maintenance` como texto visible.
- La paginación respeta `hasPrevious` y `hasNext`; el resumen indica «Página X de Y».
- Al abrir una tarjeta, la URL conserva `?page=N`.
- «Volver al catálogo» regresa a `/?page=N` sin parámetros de orden.
- Una URL `/properties/00000000-0000-0000-0000-000000000000?page=2` muestra «No encontramos esta propiedad.»
- Una ruta inexistente muestra la vista de ruta inexistente en español.
- Los iconos son SVG de Lucide y no se cargan hojas Bootstrap.
- El foco de teclado es visible en todos los enlaces.

5. Repetir la comprobación visual en anchos de 320, 768 y 1280 px.

## Escenarios de error

- Si el backend está caído, el catálogo y el detalle muestran «No se pudo cargar la información. Inténtalo de nuevo.» sin stack traces.
- Si `page=no-valido`, `page=0`, `page=-1` o un valor desbordado, la vista sanea a la página `1`.
- Si `imageUrl` está ausente, se renderiza el marcador de posición con texto alternativo útil.
- Si el estado de la API no está en el mapa conocido, se muestra «Estado no disponible».

## Medición de SC-003 y SC-010

- **SC-003**: seleccionar 10 participantes, entregar sin ayuda la consigna «encuentra una propiedad y abre su detalle» y cronometrar cada intento. El criterio se cumple si al menos 9 de 10 participantes completan la tarea en menos de 2 minutos. Registrar el tiempo por participante y los intentos no completados.
- **SC-010**: con backend y frontend activos, ejecutar 20 cargas frías de `http://localhost:5242/` repartidas entre los anchos de 320, 768 y 1280 px. Medir desde la petición inicial hasta que aparezca la primera tarjeta de propiedad, el estado vacío o el mensaje de error. El criterio se cumple si al menos 19 de 20 cargas terminan en menos de 3 segundos. Registrar ancho, tiempo y resultado por carga.
- Anexar las mediciones a esta guía. Si alguno de los criterios no se cumple, la feature no se considera terminada.
- Resultado actual: pendiente de ejecutar con participantes y mediciones reales; esta guía no registra valores estimados.

## Verificación de contratos

Los detalles verificables están en:

- `specs/010-foundation-blazor-frontend/contracts/contrato-navegacion-catalogo.md`
- `specs/010-foundation-blazor-frontend/contracts/contrato-vistas-propiedad.md`
- `specs/010-foundation-blazor-frontend/contracts/contrato-consumo-api.md`

La spec y el checklist siguen siendo la referencia de criterios de aceptación; esta guía no añade requisitos nuevos.

## Límite de esta feature

No se ejecutan pruebas de integración con Kestrel, navegador o PostgreSQL, no se editan migraciones y no se modifica el OpenAPI. Cualquier necesidad en ese sentido debe tratarse como una divergencia constitucional y requeriría aprobación humana antes de cualquier cambio.
