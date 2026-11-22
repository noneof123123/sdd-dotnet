# Guía de validación retrospectiva: Documentación retrospectiva del hotfix interactivo

**Fecha**: 2026-11-22

**Finalidad**: Verificar que la documentación retrospectiva corresponde al comportamiento ya implementado. No describe trabajo nuevo ni sustituye la guía original de la feature 013.

## Requisitos previos

- SDK .NET 11 y solución `app/Realtor.sln` disponible.
- Dependencias restauradas.
- Para scripts `.specify` en este entorno: `DOTNET_ROLL_FORWARD=Major /tmp/opencode/pwsh-old/pwsh`.
- Para validación manual, navegadores con conexión y sesiones independientes.

## Comandos de validación automatizada

Desde la raíz del repositorio:

```bash
dotnet restore app/Realtor.sln
dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj --nologo
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --nologo --filter "FullyQualifiedName~OpenApi"
dotnet test app/Realtor.sln --nologo
git diff --check
```

### Resultados esperados

| Comando | Resultado esperado |
|---|---|
| Restauración | Sin errores en los cuatro proyectos de la solución |
| Pruebas frontend | 111/111 pruebas superadas |
| Regresión OpenAPI | 17/17 pruebas superadas |
| Pruebas completas | 324/324 pruebas superadas; 111 frontend y 213 backend |
| `git diff --check` | Sin salida |

## Verificación de contratos

- Comportamiento visible: [contrato-experiencia-interactiva.md](contracts/contrato-experiencia-interactiva.md).
- Contexto, umbrales y registro: [contrato-estado-y-registro.md](contracts/contrato-estado-y-registro.md).
- Modelo de estados e incidencias: [data-model.md](data-model.md).
- Decisiones retrospectivas: [research.md](research.md).

## Validación manual

Los protocolos completos permanecen en `specs/013-signalr-interactive-server/quickstart.md`:

- VM-001 a VM-014: recorridos de catálogo, detalle, espera, reintento, interrupciones, accesibilidad, aislamiento y contenido visible.
- SC-002: respuesta visible en menos de 1 segundo para al menos 38 de 40 acciones.
- SC-005: al menos 9 de 10 participantes completan la tarea principal sin ayuda en menos de 2 minutos.
- SC-006: al menos 19 de 20 visitas concurrentes completan la tarea sin errores ni pérdida de contexto.
- SC-010: incidencias registradas con tipo, momento, vista y resultado, sin datos personales.

Estos protocolos fueron ejecutados y confirmados por el responsable con resultado satisfactorio. La confirmación cubre también la coherencia con avance y retroceso del navegador (FR-014), la ausencia de formularios, autenticación y recopilación de datos personales (FR-022), y una forma comprensible de continuar cuando la experiencia interactiva no está disponible en un entorno concreto.

## Registro de evidencia

| Evidencia | Fecha | Resultado | Detalle |
|---|---|---|---|
| Restauración y compilación de la solución | 2026-11-22 | Correcto | `dotnet test app/Realtor.sln --nologo` compiló los cuatro proyectos sin errores. |
| Unit tests frontend | 2026-11-22 | Correcto | 111/111 pruebas en `RealtorWebTests`. |
| Regresión OpenAPI | 2026-11-22 | Correcto | 17/17 pruebas con filtro `FullyQualifiedName~OpenApi`. |
| Prueba completa de la solución | 2026-11-22 | Correcto | 324/324 pruebas; 111 frontend y 213 backend. |
| `git diff --check` | 2026-11-22 | Correcto | Sin diferencias de whitespace. |
| Verificación manual VM-001 a VM-014 | 2026-11-22 | Correcto | Ejecución y resultado satisfactorio confirmados por el responsable; cubre FR-014, FR-022 y el caso límite de entorno sin interactividad. |
| Protocolos SC-002, SC-005, SC-006 y SC-010 | 2026-11-22 | Correcto | Umbral de respuesta, tarea con participantes, concurrencia y registro de incidencias confirmados por el responsable. |

## Nota retrospectiva

Los resultados automatizados citados corresponden a la evidencia registrada durante la implementación 013 y a las pruebas unitarias del ciclo añadidas durante la remediación documental. No se modificó código de producción.
