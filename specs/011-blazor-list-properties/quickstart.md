# Guía de inicio rápido: Listado paginado de propiedades en la página principal

Rama: `011-blazor-list-properties`  
Fecha: 2026-11-21  
Objetivo: describir cómo verificar esta feature después de `speckit.implement`, sin sustituir la spec, el plan ni los contratos.

## Prerrequisitos

- SDK de .NET fijado por `global.json`: `11.0.100-rc.3.26522.148`.
- Solución `app/Realtor.sln` en el estado actual de la rama.
- Proyectos existentes `RealtorWeb` y `RealtorWebTests`; no se crean proyectos nuevos.
- No se requiere PostgreSQL para las pruebas unitarias de esta feature.
- Para la comprobación manual se requiere el backend en ejecución y `ApiClients:RealtorApi:BaseAddress` apuntando a él.

## Comandos desde la raíz del repositorio

```bash
dotnet --version
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj --no-build
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build --filter "FullyQualifiedName~OpenApi"
dotnet test app/Realtor.sln --no-build
git diff --check
```

Resultado esperado:

- `dotnet --version` coincide con `global.json`.
- Restauración y compilación sin errores.
- Pruebas frontend en verde, incluidas las nuevas comprobaciones de total de elementos y controles deshabilitados.
- Pruebas OpenAPI siguen en verde: no cambian las `4` rutas ni las `6` operaciones.
- La solución completa sigue en verde.
- `git diff --check` sin errores de formato.

## Verificación manual local

1. Iniciar la API:

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj --launch-profile http
```

2. Iniciar el frontend en otra terminal:

```bash
dotnet run --project app/frontend/src/RealtorWeb/RealtorWeb.csproj --launch-profile http
```

3. Abrir `http://localhost:5242/` y comprobar:

- Aparece el catálogo con tarjetas reales o el estado vacío cuando no hay datos.
- Cada tarjeta muestra título, precio, estado en español, dirección, dormitorios, baños, superficie e imagen o alternativa.
- El resumen indica «Página X de Y» y, además, el total de propiedades con formato español.
- En la primera página, «Anterior» aparece deshabilitado, no es enlace y expone una etiqueta accesible en español.
- Si existen más páginas, «Siguiente» es un enlace; al seleccionarlo cambian los resultados, el resumen y la URL a `/?page=2`.
- En la última página, «Siguiente» aparece deshabilitado y no es enlace.
- Los controles deshabilitados no reciben foco de teclado; los habilitados sí muestran foco visible.
- Las etiquetas visibles son exactamente «Disponible», «Alquilada», «Mantenimiento» o «Estado no disponible»; no aparecen `Available`, `Rented` ni `Maintenance`.
- No se cargan hojas Bootstrap y no hay estilos inline en el HTML generado.

4. Probar URLs concretas:

| URL | Resultado esperado |
|---|---|
| `/` | Primera página, con «Anterior» deshabilitado. |
| `/?page=2` | Segunda página si existe; en otro caso, última página efectiva. |
| `/?page=999999` | Última página disponible con tarjetas y metadatos coherentes, nunca vista vacía. |
| `/?page=99999999999999999999` | Última página disponible sin desbordamiento ni vista vacía. |
| `/?page=no-valido` | Primera página. |
| `/?page=0` | Primera página. |
| `/?page=-1` | Primera página. |

5. Repetir la comprobación visual en anchos de 320 px, 768 px y 1280 px:

- 320 px: una columna, sin desplazamiento horizontal.
- 768 px: dos columnas.
- 1280 px: tres columnas.
- En los tres anchos, los controles de paginación siguen siendo visibles y distinguibles.

## Escenarios de error

- Con el backend detenido, la página muestra «No se pudo cargar la información. Inténtalo de nuevo.» y un reintento hacia la misma página saneada.
- El reintento vuelve a consultar sin recargar manualmente la experiencia.
- El estado vacío muestra «No hay propiedades disponibles en esta página.» sin tarjetas ficticias ni paginación.
- Ningún estado expone stack traces, rutas internas ni valores técnicos de estado.

## Medición de criterios

- **SC-004**: con 10 participantes, entregar sin ayuda la consigna «encuentra una propiedad y avanza a la página siguiente» y cronometrar. El criterio se cumple si al menos 9 de 10 lo logran en menos de 1 minuto.
- **SC-010**: con backend y frontend activos, ejecutar 20 cargas frías de `/` repartidas entre 320, 768 y 1280 px. Medir hasta que aparezca la primera tarjeta, el estado vacío o el error. El criterio se cumple si al menos 19 de 20 cargas terminan en menos de 3 segundos.
- Registrar fecha, comandos, resultados y mediciones reales antes de dar por terminada la implementación. No registrar valores estimados.

## Evidencia de validación automatizada ejecutada

Ejecutada el 2026-11-21 desde la raíz del repositorio con SDK `11.0.100-rc.3.26522.148`.

| Comando | Resultado real |
|---|---|
| `dotnet restore app/Realtor.sln` | `RESTORE_EXIT=0`; aviso `NU1902` de `Microsoft.Extensions.Validation` 11.0.0-rc.2.26504.105; sin errores. |
| `dotnet build app/Realtor.sln --no-restore` | `BUILD_EXIT=0`; 1 aviso `NU1902`; 0 errores. |
| `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj --no-build` | `FRONTEND_EXIT=0`; 58/58 pruebas superadas. |
| `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --no-build --filter "FullyQualifiedName~OpenApi"` | `OPENAPI_EXIT=0`; 17/17 pruebas superadas. |
| `dotnet test app/Realtor.sln --no-build` | `SOLUTION_EXIT=0`; 58/58 frontend y 213/213 backend; 271/271 en total. |
| `git diff --check` | `DIFF_CHECK_EXIT=0`. |

`git status --short` confirmó cambios únicamente en archivos frontend de esta feature y en los artefactos de especificación; `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` permanece sin cambios.

La evidencia manual con navegador real, participantes y mediciones SC-004/SC-010 sigue pendiente; no se registran resultados supuestos. Decisión del 2026-11-21: dejar T027 pendiente y mantener la spec en `En implementación` hasta aportar evidencia real.

## Referencias

- `specs/011-blazor-list-properties/spec.md`
- `specs/011-blazor-list-properties/plan.md`
- `specs/011-blazor-list-properties/data-model.md`
- `specs/011-blazor-list-properties/contracts/contrato-listado-propiedades.md`
- `specs/011-blazor-list-properties/contracts/contrato-consumo-api-listado.md`

## Límite de esta feature

No se ejecutan pruebas de integración con Kestrel, navegador real o PostgreSQL; no se editan migraciones ni el OpenAPI. Cualquier necesidad en ese sentido debe tratarse como una divergencia constitucional y requerir aprobación humana antes de cualquier cambio.
