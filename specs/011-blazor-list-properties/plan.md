# Plan de implementación: Listado paginado de propiedades en la página principal

**Rama**: `011-blazor-list-properties` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/011-blazor-list-properties/spec.md`

**Estado de la especificación**: `Borrador`. Este plan no modifica el estado ni sustituye la aprobación humana requerida para `speckit.implement`.

## Resumen

Cerrar las brechas entre la spec 011 y la experiencia de catálogo ya construida en la iniciativa 010, sin crear proyectos, capacidades de escritura ni cambios de backend. La página principal ya consume el listado paginado con Refit tipado, ya sanea la página y ya cubre los estados de carga, vacío y error. Este plan se limita a tres ajustes verificables: mostrar el total de elementos además del resumen de páginas, renderizar controles de paginación deshabilitados cuando no aplica «Anterior» o «Siguiente», y dejar evidencia de que una página fuera de rango se resuelve a la última página efectiva devuelta por la capacidad aprobada. Todo el diseño reutiliza `RealtorWeb`, `app.css`, Lucide Icons y el proyecto de pruebas frontend existente.

## Contexto técnico

- **Lenguaje y versión**: C# con .NET 11; `global.json` fija `11.0.100-rc.3.26522.148` y no se modifica.
- **Dependencias principales**: Blazor Web App con Razor Components; Refit `17.0.0`, `Refit.HttpClientFactory` `17.0.0` y `Refit.HttpClientFactory.Reflection` `17.0.0`; LucideBlazor `1.0.7`. No se añaden paquetes.
- **Almacenamiento**: no aplica. La feature es de solo lectura; no hay tablas, columnas, migraciones ni escritura.
- **Pruebas**: xUnit `2.9.3` y bUnit `2.14.0` en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`. Únicamente unit tests automatizados en proceso, sin Kestrel, navegador, PostgreSQL ni `HttpClient` real. La verificación manual de `quickstart.md` es una compuerta de validación, no un tipo de prueba automatizada.
- **Plataforma de destino**: servidor web con Static SSR y navegación mejorada; navegadores modernos; verificación visual a 320 px, 768 px y 1280 px.
- **Tipo de proyecto**: aplicación web, porción frontend de la solución compartida `app/Realtor.sln`.
- **Objetivos de rendimiento**: SC-010 exige que al menos 19 de 20 cargas frías de `/` muestren contenido útil en menos de 3 segundos; SC-004 exige que al menos 9 de 10 participantes completen «encontrar una propiedad y avanzar de página» en menos de 1 minuto. Ambos protocolos están definidos en `quickstart.md`.
- **Restricciones**: stack obligatorio de la constitución; CSS propio centralizado en `wwwroot/app.css`; sin estilos inline, colores hardcodeados, frameworks CSS ni librerías de iconos distintas de Lucide; textos visibles en español; objetivo interno WCAG 2.1 AA; sin cambios en backend, dominio, persistencia, OpenAPI ni `RealtorApi.http`.
- **Alcance y escala**: una ruta pública (`/`), 12 propiedades por página, las propiedades ya publicadas por el catálogo existente y los estados de presentación ya definidos. Sin nuevas pantallas, filtros, ordenación ni CRUD.
- **Incógnitas pendientes**: ninguna. Todas las decisiones de diseño quedan resueltas en `research.md`.

## Comprobación de la constitución previa al diseño

_PUERTA: todos los principios deben cumplirse antes de continuar con la Fase 0._

| Principio | Estado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | Cambios limitados a los proyectos existentes `RealtorWeb` y `RealtorWebTests` dentro de `app/Realtor.sln`. |
| II. Spec-Driven Development | CUMPLE | Existe `spec.md`, checklist 16/16 y una aclaración integrada; este plan no altera el estado `Borrador` ni salta fases. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Se conserva Blazor Web App con Razor Components y Static SSR; el backend y sus vertical slices no se tocan. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit con `IHttpClientFactory`, CSS centralizado y Lucide Icons ya están adoptados; no se introduce stack paralelo. |
| V. Calidad de Dominio y Contratos | CUMPLE | Los view models y el texto visible se calculan fuera de los componentes; solo unit tests con stubs en memoria. |
| VI. Idioma y Documentación | CUMPLE | Toda la documentación de planeación está en español; se conservan identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin edición silenciosa de estado, sin modificación de scripts o plantillas upstream y sin saltos de flujo. |

Resultado: **PUERTA CUMPLIDA**.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/011-blazor-list-properties/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── contrato-listado-propiedades.md
│   └── contrato-consumo-api-listado.md
├── checklists/
│   └── requirements.md
└── tasks.md             # No se crea en esta fase; lo genera /speckit.tasks
```

### Código fuente (raíz del repositorio)

```text
app/frontend/src/RealtorWeb/
├── Components/
│   ├── Pages/Home.razor                       # Ajustar resumen y controles de paginación
│   └── Properties/PropertyCard.razor          # Sin cambios previstos
├── Features/Properties/
│   ├── PropertyViewModels.cs                  # Añadir texto de total de elementos
│   ├── PropertyPresentation.cs                # Textos y etiquetas de controles deshabilitados
│   ├── PropertyCatalogService.cs              # Verificar y ajustar solo si falta normalización; fuente de la página efectiva
│   ├── CatalogNavigation.cs                   # Sin cambios funcionales
│   ├── IPropertiesApi.cs                      # Sin cambios
│   └── PropertyStatusLabels.cs                # Sin cambios
├── wwwroot/app.css                            # Añadir estilo de control deshabilitado y resumen
└── Program.cs                                 # Sin cambios

app/frontend/test/RealtorWeb/
├── CatalogNavigationTests.cs                  # Ampliar cobertura de saneamiento y página fuera de rango
├── PropertyPresentationTests.cs               # Ampliar cobertura de textos visibles y estados
└── Components/
    ├── CatalogPageTests.cs                    # Ampliar cobertura de resumen, navegación y controles deshabilitados
    ├── PresentationStateTests.cs              # Ampliar cobertura de estados y ausencia de paginación
    └── PropertyCardTests.cs                   # Ampliar cobertura de alternativas y etiquetas de estado
```

**Decisión estructural**: reutilizar los proyectos y la capa `Features/Properties` ya existentes. No se crea un proyecto nuevo ni una ruta nueva; la única página implicada es `/`. La spec 011 prevalece sobre la convención de la spec 010 únicamente en dos detalles de interfaz —total de elementos visible y controles deshabilitados— sin modificar los artefactos aprobados de la iniciativa 010.

## Fase 0: Investigación

Artefacto: `specs/011-blazor-list-properties/research.md`.

Decisiones registradas:

- R001: mejora incremental sobre la implementación existente, sin nuevos proyectos.
- R002: Static SSR con `@attribute [StreamRendering]`; sin modos interactivos.
- R003: Refit tipado registro existente; sin `HttpClient` directo ni `RestService.For`.
- R004: tamaño de página fijo en 12 y page transportado en `?page=N`.
- R005: página fuera de rango resuelta por la página efectiva devuelta por la API, según la aclaración A.
- R006: total de elementos calculado en el view model y mostrado en la página.
- R007: controles «Anterior» y «Siguiente» siempre presentes, con estado deshabilitado accesible cuando no aplica.
- R008: estilos nuevos solo en `app.css`; sin frameworks CSS ni estilos inline.
- R009: Lucide Icons y texto en español sin cambios de sistema visual.
- R010: xUnit y bUnit como única estrategia de prueba; sin infraestructura externa.
- R011: sin cambios en backend, OpenAPI, dominio ni persistencia.
- R012: precedencia de la spec 011 sobre la convención de ocultar enlaces de la spec 010, limitada a los controles deshabilitados.

## Fase 1: Diseño y contratos

### D001 — Superficie web

La ruta `/` sigue siendo la entrada principal y acepta `?page=N`. No se añaden rutas ni parámetros nuevos; el tamaño de página no viaja en la URL. La salida debe incluir tarjetas, resumen «Página X de Y», total de elementos, controles «Anterior» y «Siguiente» y los estados de carga, vacío y error.

### D002 — View model del catálogo

`PropertyCatalogViewModel` ya contiene `TotalItems`, `HasPrevious` y `HasNext`. Se añade una propiedad derivada `TotalItemsText`, calculada en `PropertyViewModels.cs` con formato `es-ES`, para que el componente solo renderice texto y no construya cadenas ni formatos.

### D003 — Controles de paginación deshabilitados

Cuando `HasPrevious` o `HasNext` sea `false`, se renderiza un elemento no interactivo con `aria-disabled="true"` y una etiqueta accesible en español, en lugar de omitirlo. Cuando sean `true`, se mantiene el enlace `<a href>` actual. Esto resuelve FR-008, FR-009 y FR-017 sin JavaScript ni modo interactivo. La convención contraria de la spec 010 queda superada solo para esta iniciativa.

### D004 — Página fuera de rango

El servicio continúa usando `Page` de la respuesta como página efectiva. La spec 004 ya normaliza una solicitud superior a la última página a la última página disponible. La verificación consistirá en comprobar que el resumen, los controles y los enlaces se construyen sobre esa página efectiva y no sobre el valor solicitado.

### D005 — Estados de presentación

No se redefine `PresentationState`. El estado `Cargando` se mantiene con `StreamRendering`; `Vacio` y `Error` conservan sus mensajes y acciones; la paginación solo se muestra en `Contenido`.

### D006 — Estilo y accesibilidad

Se añade en `wwwroot/app.css` una clase para el control deshabilitado, derivada de los tokens existentes, con contraste y foco acordes al sistema visual. La grilla conserva sus breakpoints de 48 rem y 80 rem para dos y tres columnas; a anchos menores permanece en una columna. Los controles deshabilitados no reciben foco de teclado y exponen su estado mediante `aria-disabled`.

### D007 — Pruebas

- xUnit para el formato de `TotalItemsText`, el saneamiento de página y la selección de estado.
- bUnit para comprobar que en contenido aparecen ambos controles, que el deshabilitado no es un enlace, que el resumen incluye total de elementos y que los estados vacío y error no muestran paginación.
- Regresión de la solución y del documento OpenAPI para confirmar que no cambia el contrato backend.
- Compuerta de validación manual de `quickstart.md` para anchos, navegación y protocolos SC-004/SC-010; no es un tipo de prueba automatizada.

### D008 — Contratos

Los contratos de esta feature describen la superficie web del listado y el consumo del endpoint ya publicado. No sustituyen al OpenAPI de la iniciativa 008 ni al contrato de navegación de la 010; solo fijan los puntos verificables de esta spec.

## Desglose previsto para `/speckit.tasks`

1. Ajustes de capa de presentación: `TotalItemsText` y textos de controles.
2. Ajustes de `Home.razor`: resumen con total y controles deshabilitados accesibles.
3. Ajuste puntual de `app.css` para el control deshabilitado.
4. Pruebas unitarias y de componente para los tres cambios funcionales.
5. Verificación de regresión de solución, OpenAPI y guía de inicio rápido.

## Seguimiento de complejidad

No existen desviaciones constitucionales que justificar. La precedencia de la spec 011 sobre dos detalles de la 010 es una decisión de alcance entre specs aprobadas, documentada en R012 y D003; no introduce un proyecto, tecnología ni patrón nuevo. La disponibilidad de PowerShell en el entorno no afecta al diseño: los scripts se ejecutan con la herramienta instalada y sin editar artefactos upstream.

## Comprobación de la constitución posterior al diseño

| Principio | Estado | Justificación post-diseño |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | El diseño usa exclusivamente los proyectos existentes de la solución. |
| II. Desarrollo Guiado por Especificaciones | CUMPLE | Todas las decisiones derivan de requisitos, aclaraciones y documentos de la spec vigente; el estado permanece `Borrador`. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Static SSR con Razor Components; sin controladores ni cambios backend. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit, CSS propio y Lucide Icons; sin frameworks CSS ni librerías de iconos nuevas. |
| V. Calidad de Dominio y Contratos | CUMPLE | Separación entre transporte, presentación y componentes; solo unit tests con stubs. |
| VI. Idioma y Documentación | CUMPLE | Artefactos en español con identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin cambios silenciosos de estado ni ediciones upstream; la precedencia entre specs queda registrada. |

Resultado final: **PUERTA CUMPLIDA**.
