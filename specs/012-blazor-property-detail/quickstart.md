# Guía de inicio rápido: Detalle de propiedad en la experiencia web

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

Esta guía describe cómo ejecutar y validar la feature después de implementarla. No sustituye a los unit tests y no registra evidencia que todavía no se haya ejecutado.

## Requisitos previos

- SDK de .NET compatible con `global.json`; comprobar con `dotnet --version`.
- Redis en ejecución según la configuración del entorno del curso. No se inicia desde esta feature.
- Backend y frontend disponibles en los puertos configurados: API en `5085` y web en `5242`.
- Datos de catálogo cargados y al menos una propiedad listada con enlace a detalle.

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

No se modifica `global.json` si el SDK no coincide; se usa el entorno previsto por el curso.

## Ejecución local

En dos terminales separadas:

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj --launch-profile http
```

```bash
dotnet run --project app/frontend/src/RealtorWeb/RealtorWeb.csproj --launch-profile http
```

La web queda en `http://localhost:5242`. La configuración `ApiClients:RealtorApi:BaseAddress` existente apunta a `http://localhost:5085/`.

## Obtención de una dirección válida

1. Abrir `http://localhost:5242/`.
2. Esperar a que aparezca una tarjeta de propiedad.
3. Copiar la dirección del enlace de la tarjeta; tendrá la forma `http://localhost:5242/properties/<guid>` o `http://localhost:5242/properties/<guid>?page=<n>`.
4. No teclear un GUID inventado para las pruebas de contenido.

## Verificación manual

| Id | Acción | Resultado esperado | Evidencia |
|---|---|---|---|
| VM-001 | Abrir la dirección de detalle válida | Se ve «Ficha de la propiedad», título, dirección, estado, precio, imagen o placeholder, descripción, dormitorios, baños, superficie, creación y última actualización | Pendiente |
| VM-002 | Abrir la misma dirección con `?page=1` | La página se ve igual y «Volver al catálogo» apunta a `/?page=1` | Pendiente |
| VM-003 | Abrir una tarjeta desde la segunda página del catálogo y pulsar «Volver al catálogo» | Se regresa a la segunda página del catálogo | Pendiente |
| VM-004 | Abrir una dirección con `?page=999999` y volver | El catálogo muestra la última página efectiva sin lista vacía | Pendiente |
| VM-005 | Abrir un GUID válido inexistente | Aparece «No encontramos esta propiedad.», sin datos parciales y con «Volver al catálogo» | Pendiente |
| VM-006 | Abrir `/properties/no-es-un-guid` | Aparece la página global «Página no encontrada» sin mostrar el texto técnico | Pendiente |
| VM-007 | Forzar un error de API y pulsar «Reintentar» | Aparece «No se pudo cargar la información. Inténtalo de nuevo.», la acción «Reintentar» vuelve a consultar y no exige recarga manual | Pendiente |
| VM-008 | Recorrer la página con Tab desde la barra de direcciones | El salto inicial y ambos enlaces tienen foco visible | Pendiente |
| VM-009 | Navegar a cada estado y comprobar encabezados | Existe un único `h1` accesible en carga, contenido, error y no encontrado | Pendiente |
| VM-010 | Revisar textos e iconos | Todo el texto visible está en español; los iconos no anuncian caracteres extraños | Pendiente |
| VM-011 | Revisar a 320 px, 768 px y 1280 px | Sin desbordamiento horizontal; una columna en móvil y dos columnas para el cuerpo a 768 px | Pendiente |
| VM-012 | Verificar imagen ausente | El placeholder muestra el texto alternativo «Imagen no disponible para …» | Pendiente |
| VM-013 | Verificar fecha de actualización ausente | Se muestra «No disponible» | Pendiente |
| VM-014 | Verificar estado desconocido | Se muestra «Estado no disponible» | Pendiente |

Para forzar el error de VM-007 se puede detener temporalmente la API y recargar la dirección de detalle; luego se reinicia y se pulsa «Reintentar». No se debe dejar la API detenida al finalizar.

## Protocolo SC-004: tarea «consultar detalle y volver al catálogo»

- Participantes: 10.
- Instrumento: mismo navegador y mismo equipo para todos.
- Tarea: desde `/`, abrir la primera propiedad, leer título y precio, volver al catálogo.
- Éxito individual: completar la tarea en menos de 2 minutos sin ayuda.
- Éxito del criterio: al menos 9 de 10 participantes completan la tarea.
- Registro: anotar participante, tiempo, ayuda requerida y resultado.
- Estado actual: pendiente de ejecución real.

## Protocolo SC-010: contenido útil en carga fría

- Muestras: 20 cargas frías de la misma dirección de detalle.
- Preparación: cerrar pestaña, limpiar caché del navegador o usar sesión privada, y repetir.
- Medición: desde que se confirma la navegación hasta que aparece el contenido principal o, en su caso, el estado terminal esperado.
- Éxito individual: menos de 3 segundos.
- Éxito del criterio: al menos 19 de 20 muestras.
- Registro: anotar muestra, milisegundos y resultado.
- Estado actual: pendiente de ejecución real.

## Criterios de aceptación de la spec

| Criterio | Verificación principal |
|---|---|
| SC-001 | VM-001 y unit tests de render completo |
| SC-002 | VM-003 y unit tests de `BackUrl` |
| SC-003 | VM-004 y regresión de la página efectiva del catálogo |
| SC-004 | Protocolo SC-004 |
| SC-005 | VM-005, VM-006 y unit tests de estados |
| SC-006 | VM-008, VM-009 y unit tests de encabezado/foco |
| SC-007 | VM-011 |
| SC-008 | VM-010, VM-012, VM-013 y VM-014 |
| SC-009 | VM-005 y ausencia de datos parciales en pruebas de estado |
| SC-010 | Protocolo SC-010 |
| SC-011 | VM-007 y unit tests de reintento |

## Registro de evidencia de la feature

La implementación debe sustituir `Pendiente` solo con evidencia ejecutada. No se marca un criterio como superado con una suposición.

| Evidencia | Fecha | Resultado | Detalle |
|---|---|---|---|
| Restauración y compilación de la solución | 2026-11-21 | Correcto | SDK `11.0.100-rc.3.26522.148`; restauración y compilación incluidas en `dotnet test app/Realtor.sln` |
| Unit tests frontend | 2026-11-21 | Correcto | `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj`: 84/84 |
| Regresión OpenAPI | 2026-11-21 | Correcto | `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~OpenApi"`: 17/17; `v1.json` sin cambios |
| Prueba completa de la solución | 2026-11-21 | Correcto | `dotnet test app/Realtor.sln`: 297/297 (84 frontend y 213 backend) |
| `git diff --check` | 2026-11-21 | Correcto | Sin salida; árbol sin problemas de whitespace |
| Verificación manual VM-001 a VM-014 | — | Pendiente | Requiere ejecución real en navegador |
| Protocolo SC-004 | — | Pendiente | Requiere 10 participantes reales |
| Protocolo SC-010 | — | Pendiente | Requiere 20 cargas frías reales |
