# Contrato de CLI: `support/scripts/generate-openapi-v1.sh`

Fecha: 2026-11-20  
Especificación: `specs/008-open-api/spec.md`

## Objetivo

Definir la interfaz del script que regenera, valida y reporta el documento OpenAPI v1. El script es la única secuencia soportada para reproducir el artefacto y sus comprobaciones.

## Invocación

```bash
support/scripts/generate-openapi-v1.sh
```

- Directorio de trabajo: raíz del repositorio.
- Permisos: ejecutable.
- Argumentos: ninguno obligatorio.
- Variables de entorno: ninguna obligatoria; se permite la configuración estándar de `DOTNET_CLI_TELEMETRY_OPTOUT` o cachés de herramientas.
- Resultado: código `0` solo si todas las fases pasan.

## Entradas

| Entrada | Ruta | Estado actual |
|---|---|---|
| Solución | `app/Realtor.sln` | existente |
| Proyecto API | `app/backend/src/RealtorApi/RealtorApi.csproj` | existente |
| Configuración Redocly | `.redocly.yaml` | creado |
| Configuración NSwag | `support/scripts/openapi-v1.nswag.json` | creado |
| Manifiesto de herramientas | `.config/dotnet-tools.json` | creado |
| Documento generado | `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` | generado por el build |

## Secuencia fail-fast

| Fase | Comando/acción | Criterio de éxito |
|---|---|---|
| 1. Generar | `dotnet build app/Realtor.sln` | build correcto y `v1.json` escrito/actualizado. |
| 2. Verificar salida | Comprobar existencia y tamaño > 0 de `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`. | Archivo presente y no vacío. |
| 3. Lint | `npx --yes @redocly/cli lint --config .redocly.yaml app/backend/src/RealtorApi/wwwroot/openapi/v1.json` | Sin errores; advertencias permitidas por `.redocly.yaml`. |
| 4. Herramientas | `dotnet tool restore` | NSwag.ConsoleCore 14.8.0 restaurado desde el manifiesto local. |
| 5. Cliente de humo | `dotnet tool run nswag run support/scripts/openapi-v1.nswag.json` | Cliente C# generado en `artifacts/openapi-client-smoke/` sin errores. |
| 6. Artefactos visuales | Verificar que `artifacts/openapi-client-smoke/` solo contiene el cliente generado. | No hay Swagger UI, ReDoc UI ni configuración visual. |
| 7. Reporte | Imprimir estado de cada fase y ruta del documento. | Todas las fases en estado correcto. |

Si una fase falla, el script debe detenerse, imprimir un mensaje `[ERROR]` identificando la fase y devolver código distinto de cero. No debe continuar generando artefactos posteriores.

## Salidas

| Salida | Ruta | Versionada |
|---|---|---|
| Documento OpenAPI | `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` | Sí |
| Cliente C# de humo | `artifacts/openapi-client-smoke/` | No; ignorada por `.gitignore:41` |
| Logs de consola | `stdout`/`stderr` | No |

El script no debe crear Swagger UI, ReDoc UI ni otros artefactos visuales.

## Contrato de `.redocly.yaml`

```yaml
extends:
  - recommended
rules:
  security-defined: off
  no-empty-servers: off
  operation-summary: off
  info-license: off
  tag-description: off
  operation-4xx-response: warn
```

**Restricciones:**
- No se permite elevar `security-defined` a `error`.
- No se permite eliminar la regla `operation-4xx-response`; solo degradarla a `warn`.
- No se permite configurar UI visual.

## Contrato de `.config/dotnet-tools.json`

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "nswag.consolecore": {
      "version": "14.8.0",
      "commands": ["nswag"]
    }
  }
}
```

**Restricciones:**
- Versión fijada exactamente; sin `*` ni rangos.
- Comando local `nswag`.
- No se requiere instalación global.

## Contrato de `support/scripts/openapi-v1.nswag.json`

| Propiedad | Valor esperado |
|---|---|
| Runtime | `Net110` |
| Documento de entrada | `../../app/backend/src/RealtorApi/wwwroot/openapi/v1.json`, relativo al archivo de configuración |
| Generador | `codeGenerators.openApiToCSharpClient` |
| Namespace del cliente de humo | `RealtorApi.OpenApi.SmokeClient` |
| Salida | `../../artifacts/openapi-client-smoke/RealtorApiClient.cs`, relativa al archivo de configuración |
| Clase generada | `RealtorApiClient` o equivalente estable |

El cliente generado solo se usa para demostrar que el documento es consumible; no se registra en DI, no se compila con la solución y no se usa en pruebas de negocio.

## Reporte esperado

```text
[OK] build y generación de app/backend/src/RealtorApi/wwwroot/openapi/v1.json
[OK] archivo generado: app/backend/src/RealtorApi/wwwroot/openapi/v1.json
[OK] redocly lint
[OK] dotnet tool restore
[OK] nswag smoke client: artifacts/openapi-client-smoke/RealtorApiClient.cs
[OK] sin artefactos visuales
OK: OpenAPI v1 regenerado y validado
Documento: app/backend/src/RealtorApi/wwwroot/openapi/v1.json
```

Cualquier línea debe poder cambiar `OK` por `ERROR` sin alterar el resto del contrato.
