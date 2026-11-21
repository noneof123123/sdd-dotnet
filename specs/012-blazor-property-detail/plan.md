# Plan de implementación: Detalle de propiedad en la experiencia web

**Rama**: `012-blazor-property-detail` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](spec.md)

**Entrada**: Especificación de la característica en `/specs/012-blazor-property-detail/spec.md`

**Estado de la especificación**: `Aprobada`. La aprobación humana ya fue registrada; este plan no altera el estado.

## Resumen

Cerrar las brechas entre la spec 012 y la vista de detalle ya existente en `RealtorWeb`, sin crear proyectos, capacidades de escritura ni cambios de backend. La ruta `/properties/{id}` ya muestra título, descripción, dirección, precio, estado, dormitorios, baños, superficie, imagen y fechas, ya conserva `?page=N` como contexto de retorno y ya reutiliza los estados compartidos. Este plan se limita a cuatro resultados verificables: completar la cobertura unitaria del mapeo de detalle, hacer que todos los estados tengan un encabezado accesible y un objetivo de foco, confirmar que el retorno y la página fuera de rango se resuelven con el comportamiento aprobado del catálogo, y dejar evidencia de accesibilidad, idioma y adaptación visual. Todo el diseño reutiliza `RealtorWeb`, `app.css`, Lucide Icons y el proyecto de pruebas frontend existente.

## Contexto técnico

- **Lenguaje y versión**: C# con .NET 11; `global.json` fija `11.0.100-rc.3.26522.148` y no se modifica.
- **Dependencias principales**: Blazor Web App con Razor Components; Refit `17.0.0`, `Refit.HttpClientFactory` `17.0.0` y `Refit.HttpClientFactory.Reflection` `17.0.0`; LucideBlazor `1.0.7`. No se añaden paquetes.
- **Almacenamiento**: no aplica. La feature es de solo lectura; no hay tablas, columnas, migraciones ni escritura.
- **Pruebas**: xUnit `2.9.3` y bUnit `2.14.0` en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`. Únicamente unit tests automatizados en proceso, sin Kestrel, navegador, PostgreSQL ni `HttpClient` real. La verificación manual de `quickstart.md` es una compuerta de evidencia requerida por SC-004 y SC-010, no un tipo de prueba automatizada.
- **Plataforma de destino**: servidor web con Static SSR y navegación mejorada; navegadores modernos; verificación visual a 320 px, 768 px y 1280 px.
- **Tipo de proyecto**: aplicación web, porción frontend de la solución compartida `app/Realtor.sln`.
- **Objetivos de rendimiento**: SC-010 exige que al menos 19 de 20 cargas frías de una dirección de detalle muestren contenido útil en menos de 3 segundos; SC-004 exige que al menos 9 de 10 participantes completen «consultar detalle y volver al catálogo» en menos de 2 minutos. Ambos protocolos están definidos en `quickstart.md`.
- **Restricciones**: stack obligatorio de la constitución; CSS propio centralizado en `wwwroot/app.css`; sin estilos inline, colores hardcodeados, frameworks CSS ni librerías de iconos distintas de Lucide; textos visibles en español; objetivo interno WCAG 2.1 AA; sin cambios en backend, dominio, persistencia, OpenAPI ni contratos HTTP de la iniciativa 007.
- **Alcance y escala**: una ruta pública de detalle con contexto opcional de página, un registro por solicitud y los cuatro estados de presentación ya definidos. Sin nuevas pantallas de formulario, filtros, ordenación ni CRUD.
- **Incógnitas pendientes**: ninguna. Todas las decisiones de diseño quedan resueltas en `research.md`.

## Comprobación de la constitución previa al diseño

_PUERTA: todos los principios deben cumplirse antes de continuar con la Fase 0._

| Principio | Estado | Justificación |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | Cambios limitados a los proyectos existentes `RealtorWeb` y `RealtorWebTests` dentro de `app/Realtor.sln`. |
| II. Spec-Driven Development | CUMPLE | Existe `spec.md` con checklist 16/16 y sin marcadores de aclaración; este plan no altera el estado `Aprobada` ni salta fases. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Se conserva Blazor Web App con Razor Components y Static SSR; el backend y sus vertical slices no se tocan. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit con `IHttpClientFactory`, CSS centralizado y Lucide Icons ya están adoptados; no se introduce stack paralelo. |
| V. Calidad de Dominio y Contratos | CUMPLE | Los view models, textos y formatos se calculan fuera del componente; solo unit tests con stubs en memoria. La validación manual es evidencia de SC-004/SC-010 aprobada en la spec, no una prueba automatizada adicional. |
| VI. Idioma y Documentación | CUMPLE | Toda la documentación de planeación está en español; se conservan identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin edición silenciosa de estado, sin modificación de scripts o plantillas upstream y sin saltos de flujo. |

Resultado: **PUERTA CUMPLIDA**.

## Estructura del proyecto

### Documentación (esta característica)

```text
specs/012-blazor-property-detail/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── contrato-detalle-propiedad.md
│   └── contrato-consumo-api-detalle.md
├── checklists/
│   └── requirements.md
└── tasks.md             # No se crea en esta fase; lo genera /speckit.tasks
```

### Código fuente (raíz del repositorio)

```text
app/frontend/src/RealtorWeb/
├── Components/
│   ├── Pages/PropertyDetails.razor            # Ajustar encabezado accesible de estados y verificar render
│   ├── Pages/NotFound.razor                   # Sin cambios; atiende identificadores mal formados
│   ├── Properties/PresentationState.razor     # Sin cambios previstos en el componente compartido
│   └── Properties/PropertyStatusBadge.razor   # Sin cambios previstos
├── Features/Properties/
│   ├── PropertyViewModels.cs                  # Verificar PropertyDetailsViewModel; sin campos nuevos previstos
│   ├── PropertyPresentation.cs                # Verificar textos y selección de estado de detalle
│   ├── PropertyCatalogService.cs              # Verificar y ampliar pruebas del mapeo de detalle
│   ├── CatalogNavigation.cs                   # Verificar BackUrl y RetryDetailsUrl
│   ├── IPropertiesApi.cs                      # Sin cambios
│   └── PropertyStatusLabels.cs                # Sin cambios
├── wwwroot/app.css                            # Verificar estados, foco y breakpoints; sin frameworks nuevos
└── Program.cs                                 # Sin cambios

app/frontend/test/RealtorWeb/
├── CatalogNavigationTests.cs                  # Ampliar cobertura de contexto de retorno y página fuera de rango
├── PropertyPresentationTests.cs               # Ampliar cobertura del servicio de detalle y estados
├── PropertyStatusLabelsTests.cs               # Sin cambios previstos
└── Components/
    ├── PropertyDetailsTests.cs                # Ampliar cobertura de render, estados y accesibilidad
    └── PresentationStateTests.cs              # Sin cambios previstos
```

**Decisión estructural**: reutilizar los proyectos y la capa `Features/Properties` ya existentes. No se crea un proyecto nuevo ni una ruta nueva; la única página implicada es `/properties/{id}`, con `?page=N` opcional. La spec 012 refina la accesibilidad de los estados del detalle sin modificar el comportamiento visible del catálogo existente.

## Fase 0: Investigación

Artefacto: `specs/012-blazor-property-detail/research.md`.

Decisiones registradas:

- R001: mejora incremental sobre la implementación existente, sin nuevos proyectos.
- R002: Static SSR con `@attribute [StreamRendering]`; sin modos interactivos.
- R003: Refit tipado registro existente; sin `HttpClient` directo ni `RestService.For`.
- R004: contexto de página transportado en `?page=N` y saneado a entero positivo; desbordamiento conservado para resolverse en el catálogo de destino.
- R005: una página superior a la última disponible se resuelve en el catálogo de destino mediante la página efectiva ya aprobada en la spec 011.
- R006: identificador mal formado o ausente atendido por la página global «Página no encontrada»; identificador válido inexistente atendido por el estado `NoEncontrado` del detalle.
- R007: precios, fechas, etiquetas de estado y alternativas de imagen calculados en la capa de presentación, no en el componente.
- R008: estados compartidos a través del componente de estados existente, con un encabezado accesible en la página de detalle para todos ellos.
- R009: reintento mediante enlace a la misma dirección de detalle; la navegación vuelve a obtener los datos sin exigir una recarga manual.
- R010: Lucide Icons, tokens de `app.css` y textos en español sin cambios de sistema visual.
- R011: xUnit y bUnit como única estrategia de prueba automatizada; sin infraestructura externa.
- R012: sin cambios en backend, OpenAPI, dominio ni persistencia.

## Fase 1: Diseño y contratos

### D001 — Superficie web

La ruta `/properties/{id}` sigue siendo la vista de detalle y acepta `?page=N` opcional. No se añaden rutas ni parámetros nuevos. La salida debe incluir el resumen completo, la acción «Volver al catálogo», la imagen o alternativa, los datos principales y los estados de carga, contenido, no encontrado y error.

### D002 — Contexto de retorno

`CatalogNavigation.BackUrl` continúa saneando valores no positivos a la página 1. Un valor superior a la última página se conserva en la dirección de retorno y el catálogo ya existente lo resuelve a la última página efectiva, conforme a la spec 011. No se consulta el catálogo desde el detalle ni se añade una dependencia nueva.

### D003 — Identificador no válido

La restricción de ruta `{id:guid}` se mantiene. Un identificador ausente o mal formado no llega al detalle y es atendido por la página global «Página no encontrada», que ya ofrece «Volver al catálogo» y no expone el valor técnico. Un identificador válido sin propiedad en la respuesta pública produce el estado `NoEncontrado` del detalle con «No encontramos esta propiedad.». Para esta iniciativa, una propiedad publicada equivale a una propiedad presente en la respuesta pública de la API; no se introduce un estado de publicación nuevo.

### D004 — Mapeo de detalle

`PropertyCatalogService.GetDetailsAsync` continúa siendo la única fuente de formato y estado. La respuesta de detalle se mapea a `PropertyDetailsViewModel` con precio en formato español, etiqueta de estado resuelta, fechas legibles, alternativa de imagen, dirección de retorno y estado de presentación. No se mueve lógica de formato a los componentes.

### D005 — Estados de presentación

No se redefine `PresentationState`. El estado `Cargando` se mantiene con `StreamRendering`; `NoEncontrado` y `Error` conservan sus mensajes y acciones; `Contenido` muestra la información completa. La página de detalle garantiza un encabezado de nivel uno accesible también en los estados no satisfactorios, para que el foco al navegar tenga un destino válido y la sección no quede sin etiqueta.

### D006 — Reintento

La acción «Reintentar» sigue siendo un enlace a la misma dirección de detalle con su contexto de página. Al activarla, la navegación vuelve a solicitar la propiedad y actualiza la vista sin que la persona tenga que recargar manualmente. No se introduce JavaScript ni modo interactivo.

### D007 — Accesibilidad e idioma

Se conservan el enlace de salto, `FocusOnNavigate`, el foco visible global, los iconos con `aria-hidden`, la alternativa textual de la imagen y los textos en español. Las etiquetas de estado siguen siendo exactamente «Disponible», «Alquilada», «Mantenimiento» y «Estado no disponible». La fecha ausente se indica como «No disponible».

### D008 — Estilo y adaptación

Los estilos existentes de detalle, panel de estado, enlace de retorno y foco se reutilizan sin frameworks CSS ni estilos inline. La composición base es de una columna; a 48 rem el contenido principal se organiza en dos columnas y a 80 rem se conserva el catálogo de tres columnas sin afectar al detalle.

### D009 — Pruebas

- xUnit para el mapeo de `GetDetailsAsync`: éxito, no encontrado, error, precio, estado, fechas, imagen, retorno y reintento.
- bUnit para comprobar que el detalle renderiza todos los campos, que el enlace de retorno conserva la página, que los estados no satisfactorios muestran mensaje y acción, que siempre existe un encabezado accesible y que el estado no encontrado no muestra datos parciales.
- Regresión de la solución y del contrato OpenAPI para confirmar que no cambia el backend.
- Compuerta de validación manual de `quickstart.md` para idioma, accesibilidad, direcciones concretas, anchos y protocolos SC-004/SC-010; no es un tipo de prueba automatizada.

### D010 — Contratos

Los contratos de esta feature describen la superficie web del detalle y el consumo del endpoint ya publicado. No sustituyen al contrato OpenAPI de la iniciativa 008 ni al contrato HTTP de la 007; solo fijan los puntos verificables de esta spec.

## Desglose previsto para `/speckit.tasks`

1. Pruebas de servicio y navegación para el mapeo de detalle, estados, retorno y reintento.
2. Pruebas de componente para render completo, encabezado accesible, estados y ausencia de datos parciales.
3. Ajuste puntual de `PropertyDetails.razor` para el encabezado accesible en estados no satisfactorios.
4. Verificación de estilos, foco, idioma y breakpoints existentes.
5. Verificación de regresión de solución, OpenAPI y guía de inicio rápido.

## Seguimiento de complejidad

No existen desviaciones constitucionales que justificar. La decisión de mantener el manejo global de identificadores mal formados es una decisión de alcance documentada en R003 y D003; no introduce un proyecto, tecnología ni patrón nuevo. La disponibilidad de PowerShell en el entorno no afecta al diseño: los scripts se ejecutan con la herramienta instalada y sin editar artefactos upstream.

## Comprobación de la constitución posterior al diseño

| Principio | Estado | Justificación post-diseño |
|---|---|---|
| I. Solución Única y Compartida | CUMPLE | El diseño usa exclusivamente los proyectos existentes de la solución. |
| II. Desarrollo Guiado por Especificaciones | CUMPLE | Todas las decisiones derivan de requisitos y documentos de la spec vigente; el estado es `Aprobada`. |
| III. Arquitectura Canónica de Backend y Frontend | CUMPLE | Static SSR con Razor Components; sin controladores ni cambios backend. |
| IV. Stack Tecnológico Obligatorio | CUMPLE | .NET 11, Refit, CSS propio y Lucide Icons; sin frameworks CSS ni librerías de iconos nuevas. |
| V. Calidad de Dominio y Contratos | CUMPLE | Separación entre transporte, presentación y componentes; solo unit tests con stubs. |
| VI. Idioma y Documentación | CUMPLE | Artefactos en español con identificadores técnicos permitidos. |
| VII. Gobernanza y Divergencias | CUMPLE | Sin cambios silenciosos de estado ni ediciones upstream. |

Resultado final: **PUERTA CUMPLIDA**.
