# Plan de implementación: Fundación del frontend web

**Rama**: `010-foundation-blazor-frontend` | **Fecha**: 2026-11-20 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/010-foundation-blazor-frontend/spec.md`

**Estado de la especificación**: `En implementación`. La aprobación humana ocurrió antes de `speckit.implement` y el cierre depende de las validaciones documentadas.

## Resumen

Construir la experiencia web pública base de Realtor dentro del proyecto Blazor existente: catálogo paginado, detalle de propiedad, estados de carga/vacío/error/ruta inexistente, navegación común y accesibilidad interna WCAG 2.1 AA. La decisión estructural confirmada con el usuario es Static SSR con navegación mejorada, sin circuitos SignalR, sin WebAssembly y sin `@onclick`. El frontend consumirá el backend por Refit registrado con `IHttpClientFactory`, mantendrá la página del catálogo en la query string, mostrará los estados con etiquetas en español y sustituirá la plantilla Bootstrap por `wwwroot/app.css` con Lucide Icons. No se crean proyectos, tablas, migraciones ni capacidades de escritura.

## Contexto técnico

- **Lenguaje y SDK**: C# con .NET 11; `global.json` fija `11.0.100-rc.3.26522.148` y no se modifica.
- **Proyectos existentes**: `app/frontend/src/RealtorWeb/RealtorWeb.csproj` y `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`, ya incluidos en `app/Realtor.sln`. No se crean proyectos nuevos.
- **Modo de representación**: Static SSR puro. Se conservan `AddRazorComponents()`, `MapRazorComponents<App>()` y `blazor.web.js`; no se añaden `AddInteractiveServerComponents`, `AddInteractiveWebAssemblyComponents`, `@rendermode`, `InteractiveServer`, `InteractiveWebAssembly` ni `InteractiveAuto`.
- **Carga diferida**: `@attribute [StreamRendering]` en las páginas con ruta para mostrar el estado de carga sin interactividad de componente.
- **Navegación**: rutas canónicas `/?page=N` y `/properties/{id:guid}?page=N`; la página se enlaza como texto y se sanea a un entero positivo. El orden no se transporta: lo define el backend y se reproduce al repetir la consulta.
- **Comunicación con backend**: Refit `17.0.0` con `Refit.HttpClientFactory` `17.0.0`; interfaz tipada por feature `IPropertiesApi`, registro mediante `IHttpClientFactory`, `BaseAddress` en `ApiClients:RealtorApi:BaseAddress` y cancelación desde `HttpContext.RequestAborted`.
- **Contrato consumido**: `GET /api/properties?page=&pageSize=` y `GET /api/properties/{id}` del documento `v1` de 008; no se modifica el backend ni el OpenAPI.
- **Presentación**: view models propios del frontend, sin mezclar DTO de transporte con entidades de dominio. Estado visible traducido a «Disponible», «Alquilada» o «Mantenimiento», con fallback «Estado no disponible».
- **Estilos**: CSS propio centralizado en `wwwroot/app.css`. Se eliminan la referencia y los archivos de Bootstrap y las hojas `.razor.css` de layout; `App.razor` usa `lang="es"`.
- **Iconografía**: `LucideBlazor` `1.0.7`, namespace `LucideBlazor`, componente `LucideIcon` con nombres Lucide en minúsculas/kebab-case y clases `.icon` reutilizables.
- **Accesibilidad**: objetivo interno WCAG 2.1 AA para foco visible, contraste, etiquetas y alternativas textuales; sin auditoría externa. Verificación a 320 px, 768 px y 1280 px.
- **Pruebas**: xUnit `2.9.3` y bUnit `2.14.0` en el proyecto de pruebas frontend existente; únicamente unit tests en proceso, sin Kestrel, navegador, PostgreSQL ni `HttpClient` real. El proyecto de pruebas cambia su SDK a `Microsoft.NET.Sdk.Razor` para renderizar componentes; bUnit `2.14.0` es compatible con el destino `net11.0` de este SDK RC3 sin alterar `global.json`.
- **Entorno**: Linux sin `pwsh`. La resolución de plantillas usa `.specify/scripts/sh/resolve-template.sh` y la resolución de rutas se emula desde `.specify/feature.json`, según instrucción humana previa registrada en 008. No se editan scripts ni plantillas upstream.
- **Incógnitas pendientes**: ninguna; todas quedan resueltas en `research.md`.

## Comprobación de la constitución previa al diseño

_PUERTA: todos los principios deben cumplirse antes de continuar con Fase 1._

| Principio | Estado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | Todo el cambio vive en los proyectos ya existentes de la solución compartida; no se crea un frontend paralelo. |
| II. Spec-Driven Development | CUMPLE | Existe `spec.md`, checklist 16/16 y la aclaración sin ambigüedades críticas. Este plan no edita el estado `Borrador` ni sustituye la aprobación humana. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Se mantiene Blazor Web App con Razor Components; el backend sigue en Vertical Slice Architecture y no se toca. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Blazor Web App, Refit, CSS propio y Lucide Icons; no se incorpora Bootstrap ni otro framework CSS. |
| V. Calidad de Dominio y Contratos | CUMPLE | DTO de transporte, view models y lógica de presentación separados de componentes; solo unit tests con stubs en memoria. |
| VI. Idioma y Documentación | CUMPLE | Toda la documentación de planeación está en español; se conservan identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin edición silenciosa de estado, sin saltos de fase y sin modificar scripts aguas arriba; la emulación de `pwsh` queda documentada. |

Resultado: **PUERTA CUMPLIDA**.

## Fase 0: Investigación

Artefacto: `specs/010-foundation-blazor-frontend/research.md`.

Decisiones registradas:

- R001: Static SSR con navegación mejorada, decidida por el usuario.
- R002: `@attribute [StreamRendering]` para el estado de carga.
- R003: navegación con query string saneada como único transporte de la página.
- R004: Refit tipado y registrado con `IHttpClientFactory`.
- R005: view models y mapeadores separados de los componentes Razor.
- R006: etiquetas de estado en español con fallback controlado.
- R007: CSS propio centralizado y eliminación de Bootstrap.
- R008: LucideBlazor `1.0.7` como sistema de iconos.
- R009: xUnit + bUnit como unit tests, sin infraestructura externa.
- R010: errores y regresión sin exponer detalles técnicos.
- R011: sin cambios de backend, esquema u OpenAPI.

## Fase 1: Diseño y contratos

### D001 — Superficie web

- `/` muestra el catálogo paginado; acepta `?page=N`.
- `/properties/{id:guid}` muestra el detalle; acepta `?page=N` solo como contexto de retorno.
- `/not-found` y `/Error` se adaptan al lenguaje y al layout comunes.
- Todos los enlaces internos se generan en servidor y conservan `?page=N` cuando corresponde.

### D002 — Capa de aplicación frontend

`Features/Properties` contendrá la interfaz Refit, el servicio de catálogo, el saneamiento de página, el constructor de URL, el mapeador de estados a etiquetas, los view models y el selector de estado de presentación. Los componentes Razor solo renderizan y enlazan enlaces/formularios.

### D003 — Consumo del backend

`IPropertiesApi` expondrá `ListAsync(page, pageSize, cancellationToken)` y `GetByIdAsync(id, cancellationToken)`. El servicio convertirá respuestas y fallos en view models o estados de presentación. `ApiResponse` permite distinguir `200` de `400`, `404` y `500` sin filtrar mensajes técnicos.

### D004 — Estados de presentación

El selector de vista producirá `Cargando`, `Contenido`, `Vacio`, `Error` o `NoEncontrado`. Cada estado tiene texto visible en español, una acción válida y ninguna tarjeta ficticia. La reintentiva será un enlace a la misma ruta con la página saneada.

### D005 — Estilo y accesibilidad

`wwwroot/app.css` definirá tokens de color, tipografía, espaciado, foco y tamaños de icono. Se usarán grid/flexbox y media queries propias para 320, 768 y 1280 px. Los iconos decorativos llevarán `aria-hidden="true"`; los controles sin texto tendrán `aria-label`; el foco será visible con contorno y anillo contrastados.

### D006 — Pruebas

- xUnit puro para saneamiento de página, construcción de URL, etiquetas de estado y formatos.
- bUnit para render de catálogo, tarjeta, detalle, estados y navegación por enlaces.
- Stubs manuales del servicio de catálogo para datos, vacío, error y carga pendiente.
- Regresión de solución y OpenAPI paraconfirmar que el frontend no altera contratos previos.

## Estructura del proyecto

### Documentación creada por este plan

```text
specs/010-foundation-blazor-frontend/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── contrato-navegacion-catalogo.md
│   ├── contrato-vistas-propiedad.md
│   └── contrato-consumo-api.md
└── checklists/
    └── requirements.md
```

### Código y pruebas previstos para `/speckit.tasks`

```text
app/frontend/src/RealtorWeb/
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── PropertyDetails.razor
│   │   ├── NotFound.razor
│   │   └── Error.razor
│   └── Properties/
│       ├── PropertyCard.razor
│       ├── PropertyStatusBadge.razor
│       └── PresentationState.razor
├── Features/Properties/
│   ├── IPropertiesApi.cs
│   ├── PropertyCatalogService.cs
│   ├── CatalogNavigation.cs
│   ├── PropertyStatusLabels.cs
│   ├── PropertyViewModels.cs
│   └── PropertyPresentation.cs
├── wwwroot/app.css
├── appsettings.json
└── Program.cs

app/frontend/test/RealtorWeb/
├── CatalogNavigationTests.cs
├── PropertyStatusLabelsTests.cs
├── PropertyPresentationTests.cs
└── Components/
    ├── CatalogPageTests.cs
    ├── PropertyCardTests.cs
    ├── PropertyDetailsTests.cs
    ├── PresentationStateTests.cs
    └── Layout/
        └── LayoutTests.cs
```

Se eliminan `app/frontend/src/RealtorWeb/wwwroot/lib/bootstrap/`, la referencia Bootstrap en `App.razor`, `Components/Layout/MainLayout.razor.css`, `Components/Layout/NavMenu.razor.css`, `Components/Layout/NavMenu.razor.js` y la referencia `RealtorWeb.styles.css` cuando dejen de existir esas hojas.

**Decisión estructural**: reutilizar los proyectos `RealtorWeb` y `RealtorWebTests` existentes, mantener la solución compartida y crear una capa de aplicación frontend pequeña bajo `Features/Properties`. No se modifica el backend, `global.json`, `RealtorApi.http` ni el OpenAPI generado.

## Fase 2: Descomposición

`tasks.md` no se crea en esta fase. El siguiente comando debe ser `/speckit.tasks`, usando este plan y `research.md`, `data-model.md`, `contracts/` y `quickstart.md` como entradas.

Paquetes de trabajo esperados, en orden: dependencias y registro de servicios, capa de aplicación y view models, componentes y CSS, estados de presentación y navegación, pruebas unitarias, limpieza de Bootstrap, verificación completa.

## Seguimiento de complejidad

No existen desviaciones constitucionales que requieran justificación. La ausencia de `pwsh` es una condición operativa del entorno, resuelta mediante el resolver shell y la emulación de prerrequisitos; no cambia el diseño ni la arquitectura. La adopción de bUnit no es una excepción al principio V porque sigue siendo unit test dentro de xUnit y sin infraestructura externa.

## Comprobación de la constitución posterior al diseño

| Principio | Estado | Justificación post-diseño |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | El diseño usa exclusivamente los proyectos existentes de la solución. |
| II. Desarrollo Guiado por Especificaciones | CUMPLE | Todas las decisiones derivan de requisitos y clarificaciones; el estado permanece `Borrador` hasta la aprobación humana. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Static SSR con Razor Components; sin controladores, carpetas técnicas globales ni cambios backend. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit, CSS propio y Lucide Icons; Bootstrap queda eliminado. |
| V. Calidad de Dominio y Contratos | CUMPLE | Separación entre transporte, presentación y componentes; solo unit tests con stubs. |
| VI. Idioma y Documentación | CUMPLE | Artefactos en español con identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin cambios silenciosos de estado ni ediciones upstream; decisiones estructurales registradas y aprobadas. |

Resultado final: **PUERTA CUMPLIDA**.
