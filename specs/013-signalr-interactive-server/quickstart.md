# Guía de inicio rápido: Experiencia interactiva en la web de propiedades

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Esta guía describe cómo ejecutar y validar la feature. No sustituye a los unit tests; registra la evidencia manual confirmada por el responsable.

## Requisitos previos

- SDK de .NET compatible con `global.json`; comprobar con `dotnet --version`.
- Redis y PostgreSQL disponibles según la configuración del curso.
- Backend y frontend disponibles en los puertos configurados: API en `5085` y web en `5242`.
- Catálogo con al menos dos páginas y una propiedad publicada con enlace de detalle.
- Navegadores modernos con conexión activa.

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

## Ejecución local

En dos terminales separadas:

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj --launch-profile http
```

```bash
dotnet run --project app/frontend/src/RealtorWeb/RealtorWeb.csproj --launch-profile http
```

La web queda en `http://localhost:5242`. No se inician servicios nuevos: la interactividad usa los puntos de conexión que el marco habilita en la propia aplicación web.

## Verificación manual

| Id | Acción | Resultado esperado | Evidencia |
|---|---|---|---|
| VM-001 | Abrir `/`, cambiar de página y observar la navegación | Aparecen propiedades y metadatos de la nueva página sin recarga completa | Correcto |
| VM-002 | Abrir una propiedad desde el catálogo | Se muestra el detalle correcto sin recarga completa | Correcto |
| VM-003 | Volver al catálogo desde una página concreta | Se restaura la misma página con sus propiedades y metadatos | Correcto |
| VM-004 | Probar enlace directo, `?page=1`, `?page=no-valido` y `?page=999999` | Cada caso abre una vista válida y el retorno conserva o resuelve el contexto aprobado | Correcto |
| VM-005 | Detener y reiniciar el frontend en menos de 10 segundos durante una visita | Aparece «Reconectando…» y se recupera sin recarga manual, conservando vista y contexto | Correcto |
| VM-006 | Detener el frontend más de 10 segundos | Aparece «No se pudo mantener la conexión.» con «Reintentar» y «Recargar página» | Correcto |
| VM-007 | Reiniciar el frontend y pulsar «Reintentar» | Vuelve la misma vista y contexto sin recarga manual | Correcto |
| VM-008 | Detener temporalmente la API y abrir catálogo y detalle | Aparece el mensaje de error ya aprobado; «Reintentar» conserva la vista y la propiedad | Correcto |
| VM-009 | Abrir dos navegadores o sesiones privadas y cambiar de página solo en una | La otra visita conserva su propia página, mensajes y selección | Correcto |
| VM-010 | Recorrer catálogo, detalle y modal usando solo teclado | Todas las acciones son alcanzables y el foco es visible | Correcto |
| VM-011 | Comprobar encabezados y roles en contenido, espera, reconexión y no disponibilidad | Existe un único `h1` accesible por vista y los estados tienen nombre comprensible | Correcto |
| VM-012 | Revisar a 320 px, 768 px y 1280 px | Una columna en móvil, dos columnas cuando corresponda y sin desplazamiento horizontal | Correcto |
| VM-013 | Revisar textos e iconos de todas las vistas y del modal | Todo está en español; no aparecen detalles técnicos ni caracteres de iconos anunciados | Correcto |
| VM-014 | Revisar la salida del frontend tras VM-005, VM-006 y VM-008 | Los incidentes registran tipo, momento, vista y resultado, sin datos personales ni exposición al visitante | Correcto |

Para VM-005 y VM-006, detener el proceso del frontend interrumpe el circuito; reiniciarlo permite comprobar la recuperación. No dejar el frontend detenido al finalizar. Para VM-008, reiniciar la API al terminar.

La confirmación recibida cubre también la coherencia con avance y retroceso del navegador (FR-014), la ausencia de formularios, autenticación y recopilación de datos personales (FR-022), y una forma comprensible de continuar cuando la experiencia interactiva no está disponible en un entorno concreto.

## Protocolo SC-002: respuesta visible

- Muestra: 40 acciones en condiciones normales, 20 cambios de página y 20 aperturas de detalle, con hasta 20 visitas concurrentes.
- Instrumento: cronómetro del navegador o registro de red; medir desde la activación del control hasta que el contenido o el estado final sea visible.
- Éxito individual: respuesta visible en menos de 1 segundo.
- Éxito del criterio: al menos 38 de 40 acciones cumplen el umbral, es decir, el 95 %.
- Registro: anotar acción, duración, vista y resultado.
- Estado actual: ejecución confirmada por el responsable con resultado satisfactorio.

## Protocolo SC-005: tarea «consultar detalle y volver al catálogo»

- Participantes: 10.
- Instrumento: mismo navegador y mismo equipo para todos.
- Tarea: desde `/`, abrir la primera propiedad, leer título y precio, volver al catálogo.
- Éxito individual: completar la tarea en menos de 2 minutos sin ayuda.
- Éxito del criterio: al menos 9 de 10 participantes completan la tarea.
- Registro: anotar participante, tiempo, ayuda requerida y resultado.
- Estado actual: ejecución confirmada por el responsable con resultado satisfactorio.

## Protocolo SC-006: visitas concurrentes

- Muestras: 20 visitas concurrentes con navegadores o sesiones independientes.
- Tarea por visita: cambiar de página, abrir una propiedad y volver al catálogo.
- Éxito individual: completar la tarea sin errores visibles ni pérdida de contexto.
- Éxito del criterio: al menos 19 de 20 visitas completan la tarea.
- Registro: anotar visita, resultado, contexto observado y errores visibles.
- Estado actual: ejecución confirmada por el responsable con resultado satisfactorio.

## Verificación SC-010: registro de incidencias

- Escenarios: una consulta lenta, un error de contenido, una recuperación menor de 10 segundos y una interrupción mayor de 10 segundos.
- Registro esperado: tipo de incidencia, momento, vista y resultado.
- Restricción: sin identificadores de visita, direcciones IP, nombres, correos ni valores de propiedad.
- Estado actual: ejecución confirmada por el responsable con resultado satisfactorio.

## Criterios de aceptación de la spec

| Criterio | Verificación principal |
|---|---|
| SC-001 | VM-001, VM-002 y VM-003 |
| SC-002 | Protocolo SC-002 |
| SC-003 | VM-005 |
| SC-004 | VM-006 y VM-007 |
| SC-005 | Protocolo SC-005 |
| SC-006 | Protocolo SC-006 |
| SC-007 | VM-013 |
| SC-008 | VM-010, VM-011 y VM-012 |
| SC-009 | VM-009 |
| SC-010 | VM-014 y verificación SC-010 |

## Registro de evidencia de la feature

La tabla registra la evidencia ejecutada y confirmada por el responsable. No se marca un criterio como superado con una suposición.

| Evidencia | Fecha | Resultado | Detalle |
|---|---|---|---|
| Restauración y compilación de la solución | 2026-11-22 | Correcto | `dotnet test app/Realtor.sln --nologo` compiló los cuatro proyectos sin errores. |
| Unit tests frontend | 2026-11-22 | Correcto | 111/111 pruebas en `RealtorWebTests`. |
| Regresión OpenAPI | 2026-11-22 | Correcto | 17/17 pruebas con filtro `FullyQualifiedName~OpenApi`. |
| Prueba completa de la solución | 2026-11-22 | Correcto | 324/324 pruebas; 111 frontend y 213 backend. |
| `git diff --check` | 2026-11-22 | Correcto | Sin diferencias de whitespace; backend, OpenAPI, `IPropertiesApi.cs`, `PropertyCatalogService.cs` y `RealtorWeb.csproj` sin cambios. |
| Verificación manual VM-001 a VM-014 | 2026-11-22 | Correcto | Ejecución y resultado satisfactorio confirmados por el responsable. |
| Protocolo SC-002 | 2026-11-22 | Correcto | Respuesta visible en menos de 1 segundo en al menos 38 de 40 acciones; confirmado por el responsable. |
| Protocolo SC-005 | 2026-11-22 | Correcto | Al menos 9 de 10 participantes completaron la tarea sin ayuda en menos de 2 minutos; confirmado por el responsable. |
| Protocolo SC-006 | 2026-11-22 | Correcto | Al menos 19 de 20 visitas concurrentes completaron la tarea sin errores ni pérdida de contexto; confirmado por el responsable. |
| Verificación SC-010 | 2026-11-22 | Correcto | Incidencias registradas con tipo, momento, vista y resultado, sin datos personales ni exposición al visitante; confirmado por el responsable. |
