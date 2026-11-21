# Investigación: Listado paginado de propiedades en la página principal

Rama: `011-blazor-list-properties`  
Fecha: 2026-11-21  
Especificación: `specs/011-blazor-list-properties/spec.md`  
Objetivo: resolver las decisiones técnicas necesarias para ajustar el listado existente sin ambigüedades pendientes.

## Estado de las incógnitas

No quedan marcadores `NEEDS CLARIFICATION`. La única aclaración de la spec —mostrar la última página disponible cuando se solicita una página superior a la última— se incorpora en R005 y D004 del plan.

## R001 — Estrategia de cambio

**Decisión**: realizar una mejora incremental sobre la implementación ya existente en `RealtorWeb`, sin crear proyectos ni reconstruir el catálogo.

**Justificación**: la iniciativa 010 ya'entrega catálogo paginado, tarjetas, Refit, estados de presentación y grilla adaptable en la rama actual. La spec 011 solo añade precisión a ese flujo. Rehacerlo duplicaría código, aumentaría el riesgo de regresión y violaría la solución única.

**Alternativas consideradas**:

- Crear un proyecto frontend independiente: rechazada por violar el principio I.
- Reemplazar la página principal por una nueva implementación: rechazada por riesgo de regresión y por duplicar capacidades ya aprobadas.
- No hacer cambios por considerar que 010 ya cubre la feature: rechazada porque la spec 011 exige total de elementos y controles deshabilitados que hoy no están presentes.

## R002 — Modo de representación

**Decisión**: conservar Static SSR con `@attribute [StreamRendering]` en `Home.razor`. No añadir `InteractiveServer`, `InteractiveWebAssembly`, `InteractiveAuto` ni `@onclick`.

**Justificación**: la paginación se realiza mediante enlaces y la recarga de la URL saneada; el estado de carga ya se resuelve con streaming. Cumple FR-003, FR-010 y FR-011 sin circuitos ni JavaScript propio.

**Alternativas consideradas**:

- Componente interactivo con `@onclick`: rechazado porque exige un modo interactivo y rompe la navegación compartible.
- WebAssembly: rechazado por innecesario y por contradecir la decisión aprobada en 010.
- Esperar la respuesta sin streaming: rechazado porque elimina la indicación de carga visible.

## R003 — Cliente HTTP

**Decisión**: mantener la interfaz tipada `IPropertiesApi` y su registro con Refit mediante `IHttpClientFactory` bajo el nombre `RealtorApi`.

**Justificación**: cumple el principio IV y las instrucciones de frontend. `ApiResponse<T>` permite distinguir éxito, validación y fallo sin filtrar detalles técnicos al visitante. No se introduce `HttpClient` directo en componentes ni `RestService.For`.

**Alternativas consideradas**:

- `RestService.For<IPropertiesApi>()` en cada componente: rechazada por prohibida.
- `new HttpClient()` en `Home.razor`: rechazada por prohibida y difícil de probar.
- Crear un servicio nuevo por duplicado: rechazada porque `PropertyCatalogService` ya cumple la responsabilidad.

## R004 — Tamaño de página y transporte de la página

**Decisión**: fijar el tamaño de página en 12 propiedades y transportar solo el número de página en `?page=N`, reutilizando `CatalogNavigation`.

**Justificación**: coincide con el comportamiento ya desplegado, respeta el máximo del contrato backend y mantiene URLs compartibles. El tamaño no necesita viajar en la URL porque no es configurable por el visitante y la spec excluye filtros y ordenación.

**Alternativas consideradas**:

- Exponer `pageSize` en la URL: rechazada porque añadiría un parámetro manipulable sin beneficio para el visitante.
- Reducir a 6 propiedades por página: rechazada porque cambiaría la experiencia ya aprobada en 010.
- Transportar la página en sesión: rechazada por estado oculto y no compartible.

## R005 — Página superior a la última disponible

**Decisión**: mostrar la página efectiva devuelta por la API, que la capacidad aprobada en 004 normaliza a la última página disponible cuando se solicita una página superior a la última.

**Justificación**: es la respuesta a la aclaración A de la spec. Evita una vista vacía y mantiene metadatos, enlaces y contadores coherentes sin añadir lógica de negocio al frontend.

**Alternativas consideradas**:

- Volver a la primera página: rechazada porque contradice la aclaración aceptada y desorienta al visitante que venía de una página posterior.
- Mostrar un mensaje de página no disponible: rechazada por la misma razón y porque añadiría un cuarto estado de presentación sin requisito.
- Calcular la última página en el frontend: rechazada porque duplicaría una regla ya resuelta por el backend.

## R006 — Total de elementos visible

**Decisión**: añadir `TotalItemsText` a `PropertyCatalogViewModel`, calculado en la capa de presentación con formato `es-ES`, y renderizarlo junto al resumen «Página X de Y».

**Justificación**: FR-007 exige página actual, total de páginas y total de elementos. Los dos primeros ya están visibles; el total de elementos existe en el view model pero no se muestra. Calcular el texto fuera del componente mantiene la separación entre render y presentación y permite probarlo con xUnit.

**Alternativas consideradas**:

- Formatear directamente en `Home.razor`: rechazada porque introduce lógica de formato en el componente y dificulta la prueba unitaria.
- Sustituir el resumen existente por un único texto compuesto: rechazada porque haría menos legible la información y rompería pruebas actuales sin beneficio.
- Mostrar el número sin formato: rechazada porque no cumple el criterio de presentación en español.

## R007 — Controles deshabilitados

**Decisión**: renderizar siempre los controles «Anterior» y «Siguiente». Cuando `HasPrevious` o `HasNext` sea `false`, se usa un elemento no interactivo con `aria-disabled="true"`, texto visible y etiqueta accesible en español; cuando sean `true`, se conserva el enlace actual.

**Justificación**: FR-008 y FR-009 exigen controles visibles y deshabilitados cuando no aplica. La implementación actual omite el control, lo que cumple la convención de 010 pero no la spec 011. La opción elegida hace explícito el estado sin JavaScript y es comprobable con bUnit.

**Alternativas consideradas**:

- Omitir el control como en 010: rechazada por contradecir FR-009 de la spec más reciente.
- Usar `<button disabled>`: rechazada porque sugiere interactividad en Static SSR y no es un enlace navegable cuando está habilitado.
- Usar `<a aria-disabled="true">` sin cambiar el elemento: rechazada porque seguiría siendo activable por teclado y obligaría a evitar la navegación con lógica adicional.

## R008 — Estilos

**Decisión**: añadir una sola clase derivada de tokens en `wwwroot/app.css` para el control deshabilitado y, si hace falta, ajustar el contenedor del resumen. No crear hojas nuevas ni estilos inline.

**Justificación**: aplica el skill visual canónico y las instrucciones de frontend. La grilla ya responde con una, dos y tres columnas en los anchos exigidos, por lo que no se modifica.

**Alternativas consideradas**:

- Bootstrap o Tailwind: rechazados por prohibición constitucional.
- Archivos `.razor.css`: rechazados porque el sistema visual central es `app.css`.
- Colores o tamaños literales en Razor: rechazados por el skill visual.

## R009 — Iconografía e idioma

**Decisión**: conservar LucideBlazor `1.0.7` y todos los textos visibles en español. No se añaden iconos nuevos a la paginación; el estado deshabilitado se comunica con texto y atributos accesibles.

**Justificación**: evita introducir una librería paralela y mantiene la coherencia con el sistema visual aprobado. Los textos de estado ya están centralizados en `PropertyPresentation`.

**Alternativas consideradas**:

- Añadir iconos de flecha de otra librería: rechazada por prohibida y por no ser necesaria.
- Mostrar `Available`, `Rented` o `Maintenance`: rechazada porque la spec exige etiquetas en español.

## R010 — Estrategia de pruebas

**Decisión**: ampliar el proyecto de pruebas frontend existente con xUnit y bUnit. No añadir pruebas de integración con Kestrel, navegador real, PostgreSQL ni `HttpClient` de red.

**Justificación**: el principio V permite solo unit tests salvo justificación explícita. Los cambios son de presentación y render, por lo que bUnit cubre la interfaz y xUnit cubre las reglas sin infraestructura externa.

**Alternativas consideradas**:

- Pruebas end-to-end con navegador: rechazadas por no autorizadas en esta spec.
- Pruebas de integración con la API real: rechazadas por requerir base de datos y superar el alcance.
- Sin pruebas nuevas: rechazada porque los tres cambios necesitan verificación automática.

## R011 — Backend y contrato OpenAPI

**Decisión**: no modificar `ListPropertiesSlice.cs`, entidades, migraciones, `RealtorApi.http` ni `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`.

**Justificación**: el endpoint `GET /api/properties` ya expone `page`, `pageSize`, `items`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`, suficientes para la spec 011. Cualquier cambio de contrato exigiría otra spec y aprobación humana.

**Alternativas consideradas**:

- Añadir un campo de total calculado en backend: rechazada porque `totalItems` ya existe.
- Cambiar la normalización de página: rechazada porque ya está definida en 004 y confirmada por la aclaración A.
- Regenerar OpenAPI: rechazada porque no hay cambios de contrato.

## R012 — Precedencia entre specs 010 y 011

**Decisión**: aplicar la spec 011 solo a los puntos donde difiere de la 010 —total de elementos y controles deshabilitados— y mantener el resto de decisiones, rutas, estados y contratos de la 010.

**Justificación**: ambas specs describen el mismo catálogo, pero la 011 es más específica y posterior. Limitar la precedencia evita reabrir decisiones ya aprobadas y reduce el riesgo de regresión.

**Alternativas consideradas**:

- Editar los artefactos de 010: rechazada porque modificaría una iniciativa ya implementada sin una spec que lo autorice.
- Ignorar 010 por completo: rechazada porque reutilizar su diseño es la forma más segura de cumplir el principio I.
- Tratar la contradicción como bloqueo humano: rechazada porque la spec vigente resuelve explícitamente el comportamiento exigido.

## Resumen

Todas las decisiones están resueltas. El diseño resultante es un cambio acotado a la capa de presentación, la página principal, un estilo centralizado y pruebas unitarias; no afecta al backend, al esquema ni al contrato OpenAPI.
