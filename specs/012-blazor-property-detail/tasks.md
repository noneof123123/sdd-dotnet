---
description: "Lista de tareas para la implementación de la iniciativa 012"
---

# Tareas: Detalle de propiedad en la experiencia web

**Entrada**: Documentos de diseño de `/specs/012-blazor-property-detail/`

**Prerrequisitos**: `plan.md` y `spec.md` requeridos; `research.md`, `data-model.md`, `contracts/` y `quickstart.md` disponibles.

**Pruebas**: Se incluyen tareas de prueba porque la spec exige pruebas independientes por historia y el plan define unit tests con xUnit y bUnit como verificación automatizada.

**Organización**: Las tareas se agrupan por historia de usuario para permitir implementación y validación independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo; usa archivos distintos y no depende de tareas incompletas.
- **[Historia]**: Etiqueta US1, US2, US3 o US4 solo en fases de historias.
- Incluir rutas exactas en cada descripción.

## Convenciones de rutas

- Solución: `app/Realtor.sln`.
- Frontend: `app/frontend/src/RealtorWeb/`.
- Pruebas frontend: `app/frontend/test/RealtorWeb/`.
- Backend: `app/backend/` solo para regresión; esta iniciativa no lo modifica.
- Documentación de la feature: `specs/012-blazor-property-detail/`.

## Phase 1: Configuración (infraestructura compartida)

**Propósito**: Confirmar que la implementación ocurre dentro de los proyectos existentes y sin nuevas dependencias.

- [X] T001 Verificar SDK, solución y proyectos existentes antes de tocar código en `global.json`, `app/Realtor.sln`, `app/frontend/src/RealtorWeb/RealtorWeb.csproj` y `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`
- [X] T002 [P] Confirmar alcance sin backend nuevo, sin paquetes nuevos y sin cambios de OpenAPI según `specs/012-blazor-property-detail/plan.md`, `specs/012-blazor-property-detail/research.md` y `specs/012-blazor-property-detail/contracts/contrato-consumo-api-detalle.md`; no modificar `app/backend/src/RealtorApi/RealtorApi.csproj`

---

## Phase 2: Fundamentos (prerrequisitos bloqueantes)

**Propósito**: Preparar la infraestructura de pruebas y verificar la integración existente antes de iniciar cualquier historia.

⚠️ **CRÍTICO**: Ninguna historia puede comenzar hasta completar esta fase.

- [X] T003 Crear el helper reutilizable de datos y respuestas de detalle en `app/frontend/test/RealtorWeb/DetailTestData.cs` para permitir pruebas deterministas de `GetPropertyByIdResponse`, `ApiResponse` y fallos esperados sin red, base de datos ni Kestrel
- [X] T004 [P] Verificar el registro actual de Refit y la configuración de `BaseAddress` en `app/frontend/src/RealtorWeb/Program.cs`, `app/frontend/src/RealtorWeb/Features/Properties/IPropertiesApi.cs` y `app/frontend/src/RealtorWeb/appsettings.json`; corregir solo si falta algún elemento requerido por `specs/012-blazor-property-detail/contracts/contrato-consumo-api-detalle.md`

**Checkpoint**: Fundación lista; pueden comenzar las historias de usuario.

---

## Phase 3: Historia de usuario 1 - Consultar el detalle completo de una propiedad (Prioridad: P1) 🎯 MVP

**Objetivo**: Abrir una propiedad desde el catálogo o por dirección directa y mostrar toda su información relevante sin depender de otras vistas.

**Prueba independiente**: Abrir la vista de detalle de una propiedad disponible y comprobar que presenta título, descripción completa, dirección, estado, precio, dormitorios, baños, superficie, imagen o alternativa y fechas legibles.

### Pruebas de la Historia de usuario 1

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T005 [P] [US1] Añadir pruebas bUnit del contrato web en `app/frontend/test/RealtorWeb/Components/PropertyDetailsTests.cs` para render completo, dirección directa, descripción sin truncar, placeholder sin imagen, fecha de actualización ausente y estado desconocido
- [X] T006 [P] [US1] Añadir pruebas unitarias del contrato de consumo en `app/frontend/test/RealtorWeb/PropertyDetailMappingTests.cs` para comprobar que una respuesta correcta se mapea a `PropertyDetailsViewModel` con precio, estado, imagen y fechas en español

### Implementación de la Historia de usuario 1

- [X] T007 [US1] Ajustar `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` para mapear la respuesta de detalle sin mover formato al componente, citando `data-model.md: PropertyApiResponse.id`, `data-model.md: PropertyApiResponse.title`, `data-model.md: PropertyApiResponse.description`, `data-model.md: PropertyApiResponse.address`, `data-model.md: PropertyApiResponse.price`, `data-model.md: PropertyApiResponse.status`, `data-model.md: PropertyApiResponse.bedroomCount`, `data-model.md: PropertyApiResponse.bathroomCount`, `data-model.md: PropertyApiResponse.areaSquareMeters`, `data-model.md: PropertyApiResponse.imageUrl`, `data-model.md: PropertyApiResponse.createdAt`, `data-model.md: PropertyApiResponse.updatedAt`, `data-model.md: PropertyDetailsViewModel.Id`, `data-model.md: PropertyDetailsViewModel.Price`, `data-model.md: PropertyDetailsViewModel.CreatedAt`, `data-model.md: PropertyDetailsViewModel.UpdatedAt`, `data-model.md: ValidationRule.VAL-005`, `data-model.md: ValidationRule.VAL-006` y `data-model.md: ValidationRule.VAL-009`
- [X] T008 [US1] Ajustar `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` para renderizar el contenido completo sin truncamientos ni formato duplicado, citando `data-model.md: PropertyDetailsViewModel.Title`, `data-model.md: PropertyDetailsViewModel.Description`, `data-model.md: PropertyDetailsViewModel.Address`, `data-model.md: PropertyDetailsViewModel.Price`, `data-model.md: PropertyDetailsViewModel.BedroomCount`, `data-model.md: PropertyDetailsViewModel.BathroomCount`, `data-model.md: PropertyDetailsViewModel.AreaSquareMeters`, `data-model.md: PropertyDetailsViewModel.ImageUrl`, `data-model.md: PropertyDetailsViewModel.ImageAlt`, `data-model.md: DetailVisual.Image` y `data-model.md: ValidationRule.VAL-005`
- [X] T009 [US1] Garantizar las etiquetas exactas de estado y el valor de contingencia en `app/frontend/src/RealtorWeb/Features/Properties/PropertyStatusLabels.cs` y `app/frontend/src/RealtorWeb/Features/Properties/PropertyPresentation.cs`, citando `data-model.md: StatusLabel.Available`, `data-model.md: StatusLabel.Rented`, `data-model.md: StatusLabel.Maintenance`, `data-model.md: StatusLabel.Unknown`, `data-model.md: PropertyDetailsViewModel.StatusLabel` y `data-model.md: ValidationRule.VAL-007`

**Checkpoint**: La Historia de usuario 1 es funcional y verificable de forma independiente.

---

## Phase 4: Historia de usuario 2 - Volver al catálogo conservando el contexto (Prioridad: P1)

**Objetivo**: Regresar al catálogo desde el detalle recuperando la página de origen y resolviendo contextos ausentes, inválidos o desbordados.

**Prueba independiente**: Abrir una propiedad desde una página concreta, pulsar «Volver al catálogo» y confirmar que se muestran sus propiedades y metadatos; repetir sin contexto, con contexto inválido y con contexto superior a la última página.

### Pruebas de la Historia de usuario 2

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T010 [P] [US2] Añadir pruebas unitarias de navegación en `app/frontend/test/RealtorWeb/PropertyDetailNavigationTests.cs` para saneamiento de página, dirección de retorno, dirección de detalle, ausencia de contexto y contexto superior a la última página
- [X] T011 [P] [US2] Añadir pruebas bUnit del enlace de retorno en `app/frontend/test/RealtorWeb/Components/PropertyDetailsBackLinkTests.cs` para comprobar los valores de `href` con página 1, página dos, contexto no numérico y contexto desbordado

### Implementación de la Historia de usuario 2

- [X] T012 [US2] Ajustar las utilidades de direcciones en `app/frontend/src/RealtorWeb/Features/Properties/CatalogNavigation.cs` para construir retorno y reintento sin recortar localmente el desbordamiento, citando `data-model.md: RequestContext.Id`, `data-model.md: RequestContext.SafePage`, `data-model.md: RequestContext.BackUrl`, `data-model.md: DetailVisual.BackLink`, `data-model.md: ValidationRule.VAL-001`, `data-model.md: ValidationRule.VAL-003` y `data-model.md: ValidationRule.VAL-004`
- [X] T013 [US2] Ajustar la parametrización de ruta y consulta en `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` para conservar el identificador y sanear la página solicitada antes de invocar el servicio, citando `data-model.md: RequestContext.Id`, `data-model.md: RequestContext.RequestedPage`, `data-model.md: ValidationRule.VAL-001` y `data-model.md: ValidationRule.VAL-003`
- [X] T014 [US2] Ajustar `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` para propagar la página saneada a retorno y reintento, citando `data-model.md: RequestContext.SafePage`, `data-model.md: PropertyDetailsViewModel.BackUrl`, `data-model.md: PropertyDetailsViewModel.RetryUrl`, `data-model.md: ValidationRule.VAL-003` y `data-model.md: ValidationRule.VAL-004`

**Checkpoint**: La Historia de usuario 2 es funcional y verificable de forma independiente; las Historias 1 y 2 funcionan juntas.

---

## Phase 5: Historia de usuario 3 - Entender los estados del detalle (Prioridad: P2)

**Objetivo**: Mostrar mensajes claros y acciones válidas durante la carga, cuando la propiedad no existe y cuando falla la obtención, sin datos parciales engañosos.

**Prueba independiente**: Forzar carga, propiedad inexistente y fallo de obtención; comprobar mensaje, acción y ausencia de datos parciales en cada caso, incluido el reintento de la misma propiedad.

### Pruebas de la Historia de usuario 3

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T015 [P] [US3] Añadir pruebas unitarias de estados en `app/frontend/test/RealtorWeb/PropertyDetailStateMappingTests.cs` para respuestas 404, respuestas de error, excepciones esperadas, cancelación y conservación del contexto de reintento
- [X] T016 [P] [US3] Añadir pruebas bUnit de estados en `app/frontend/test/RealtorWeb/Components/PropertyDetailsStateTests.cs` para carga, no encontrado, error, ausencia de datos parciales y acción de reintento sin recarga manual

### Implementación de la Historia de usuario 3

- [X] T017 [US3] Ajustar la selección de estado y direcciones en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` para mapear 404, errores y excepciones, citando `data-model.md: RequestContext.DetailsUrl`, `data-model.md: PropertyDetailsViewModel.State`, `data-model.md: PropertyDetailsViewModel.RetryUrl`, `data-model.md: PresentationState.NoEncontrado`, `data-model.md: PresentationState.Error`, `data-model.md: ValidationRule.VAL-002` y `data-model.md: ValidationRule.VAL-008`
- [X] T018 [US3] Ajustar la rama condicional en `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` para que solo `Contenido` renderice datos de propiedad, citando `data-model.md: PresentationState.Cargando`, `data-model.md: PresentationState.Contenido`, `data-model.md: PresentationState.NoEncontrado`, `data-model.md: PresentationState.Error` y `data-model.md: ValidationRule.VAL-009`
- [X] T019 [US3] Alinear mensajes y acciones en `app/frontend/src/RealtorWeb/Features/Properties/PropertyPresentation.cs` y `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor` sin exponer motivos técnicos, citando `data-model.md: PropertyDetailsViewModel.Message`, `data-model.md: PresentationState.Cargando`, `data-model.md: PresentationState.NoEncontrado`, `data-model.md: PresentationState.Error`, `data-model.md: DetailVisual.StatePanel` y `data-model.md: ValidationRule.VAL-008`

**Checkpoint**: La Historia de usuario 3 es funcional y verificable de forma independiente; las Historias 1, 2 y 3 funcionan juntas.

---

## Phase 6: Historia de usuario 4 - Utilizar un detalle claro, adaptable y accesible (Prioridad: P2)

**Objetivo**: Garantizar foco visible, encabezados accesibles, textos en español y adaptación a 320, 768 y 1280 px sin desplazamiento horizontal.

**Prueba independiente**: Recorrer el detalle solo con teclado y consultarlo a 320, 768 y 1280 px, verificando foco visible, etiquetas comprensibles, alternativas de imagen y ausencia de desplazamiento horizontal.

### Pruebas de la Historia de usuario 4

> **NOTA**: Escribir estas pruebas primero y confirmar que fallan antes de tocar la implementación.

- [X] T020 [P] [US4] Añadir pruebas bUnit de accesibilidad estructural en `app/frontend/test/RealtorWeb/Components/PropertyDetailsAccessibilityTests.cs` para un único encabezado `h1` por estado, sección con nombre accesible, encabezado oculto en estados no satisfactorios, texto alternativo de imagen e iconos decorativos

### Implementación de la Historia de usuario 4

- [X] T021 [US4] Ajustar `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` para mantener sección con nombre accesible, encabezado estable en todos los estados, enlace de retorno visible, panel de estado coherente, iconos decorativos y alternativa de imagen, citando `data-model.md: DetailVisual.DetailsSection`, `data-model.md: DetailVisual.Heading`, `data-model.md: DetailVisual.BackLink`, `data-model.md: DetailVisual.StatePanel`, `data-model.md: DetailVisual.Icons` y `data-model.md: DetailVisual.Image`
- [X] T022 [US4] Ajustar `app/frontend/src/RealtorWeb/wwwroot/app.css` para conservar foco visible, una columna en móvil, dos columnas para el cuerpo del detalle a 48 rem y ausencia de desplazamiento horizontal, citando `data-model.md: DetailVisual.Focus` y `data-model.md: DetailVisual.Breakpoints`
- [X] T023 [US4] Auditar textos visibles en `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`, `app/frontend/src/RealtorWeb/Features/Properties/PropertyPresentation.cs` y `app/frontend/src/RealtorWeb/Features/Properties/PropertyStatusLabels.cs` para que todo el contenido del visitante esté en español y no exponga valores técnicos, citando `data-model.md: StatusLabel.Available`, `data-model.md: StatusLabel.Rented`, `data-model.md: StatusLabel.Maintenance`, `data-model.md: StatusLabel.Unknown` y `data-model.md: DetailVisual.Icons`

**Checkpoint**: Las cuatro historias son funcionales y verificables de forma independiente.

---

## Phase 7: Pulido y preocupaciones transversales

**Propósito**: Verificar regresiones, ejecutar la guía de validación y dejar evidencia real sin modificar backend ni OpenAPI.

- [X] T024 Ejecutar pruebas unitarias frontend en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj` y corregir solo fallos causados por esta feature
- [X] T025 Ejecutar la suite completa de la solución en `app/Realtor.sln` y comprobar que no aparecen regresiones en catálogo, backend ni contratos
- [X] T026 Ejecutar la regresión OpenAPI con filtro `FullyQualifiedName~OpenApi` en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` y verificar que `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` permanece sin cambios
- [ ] T027 Ejecutar la verificación manual VM-001 a VM-014, el protocolo SC-004 y el protocolo SC-010 descritos en `specs/012-blazor-property-detail/quickstart.md`, registrando fecha, resultados y evidencia real
- [X] T028 Actualizar la tabla de evidencia en `specs/012-blazor-property-detail/quickstart.md` con los resultados ejecutados y verificar el estado final del árbol con `git status` y `git diff --check` desde `app/Realtor.sln`

---

## Dependencias y orden de ejecución

### Dependencias de fases

- **Phase 1 (Configuración)**: sin dependencias; puede iniciarse de inmediato.
- **Phase 2 (Fundamentos)**: depende de Phase 1; bloquea todas las historias.
- **Historias (Phase 3 a 6)**: dependen de Phase 2; están organizadas por prioridad y pueden validarse por separado.
- **Phase 7 (Pulido)**: depende de que todas las historias deseadas estén completas.

### Dependencias entre historias

- **Historia de usuario 1 (P1)**: puede iniciar tras Phase 2; es el MVP y no depende de otras historias.
- **Historia de usuario 2 (P1)**: puede iniciar tras Phase 2; depende conceptualmente del mapeo de US1 para verificar `BackUrl`, pero tiene pruebas y utilidades propias.
- **Historia de usuario 3 (P2)**: puede iniciar tras Phase 2; comparte archivos de implementación con US1 y US2, por lo que se recomienda ejecutarla después de ellas cuando haya un solo implementador.
- **Historia de usuario 4 (P2)**: puede iniciar tras Phase 2; sus pruebas son independientes, pero sus ajustes tocan los mismos componentes que US1 y US3.

### Grafo recomendado

```text
Phase 1 -> Phase 2 -> US1 (MVP) -> US2 -> US3 -> US4 -> Phase 7
```

Con varios implementadores, las tareas de prueba marcadas como `[P]` pueden avanzar en paralelo siempre que usen archivos distintos; las tareas de implementación que editan `PropertyCatalogService.cs`, `PropertyDetails.razor` o `app.css` deben coordinarse para evitar conflictos.

### Dentro de cada historia

1. Escribir primero las pruebas indicadas y confirmar que fallan.
2. Ajustar utilidades y mapeo antes del componente.
3. Ajustar el componente antes de la validación visual manual.
4. Verificar el checkpoint de la historia antes de avanzar.

### Oportunidades de paralelismo

- Phase 1: T002 puede ejecutarse en paralelo con T001.
- Phase 2: T004 puede ejecutarse en paralelo con T003.
- US1: T005 y T006 pueden ejecutarse en paralelo.
- US2: T010 y T011 pueden ejecutarse en paralelo.
- US3: T015 y T016 pueden ejecutarse en paralelo.
- US4: T020 puede ejecutarse en paralelo con la implementación de otra historia solo si otro agente posee los archivos de componente; de lo contrario, ejecutarla antes de T021.

---

## Ejemplo de paralelismo: Historia de usuario 1

```bash
# Ejecutar juntas las pruebas de US1:
Task: "Añadir pruebas bUnit del contrato web en app/frontend/test/RealtorWeb/Components/PropertyDetailsTests.cs"
Task: "Añadir pruebas unitarias del contrato de consumo en app/frontend/test/RealtorWeb/PropertyDetailMappingTests.cs"
```

## Ejemplo de paralelismo: Historia de usuario 2

```bash
# Ejecutar juntas las pruebas de US2:
Task: "Añadir pruebas unitarias de navegación en app/frontend/test/RealtorWeb/PropertyDetailNavigationTests.cs"
Task: "Añadir pruebas bUnit del enlace de retorno en app/frontend/test/RealtorWeb/Components/PropertyDetailsBackLinkTests.cs"
```

## Ejemplo de paralelismo: Historia de usuario 3

```bash
# Ejecutar juntas las pruebas de US3:
Task: "Añadir pruebas unitarias de estados en app/frontend/test/RealtorWeb/PropertyDetailStateMappingTests.cs"
Task: "Añadir pruebas bUnit de estados en app/frontend/test/RealtorWeb/Components/PropertyDetailsStateTests.cs"
```

---

## Estrategia de implementación

### MVP primero: solo Historia de usuario 1

1. Completar Phase 1 y Phase 2.
2. Completar Phase 3.
3. **DETENERSE Y VALIDAR** la Historia de usuario 1 de forma independiente.
4. Mostrar el detalle completo como incremento útil antes de continuar.

### Entrega incremental

1. Configuración y fundamentos listos.
2. US1: detalle completo; validar y mostrar.
3. US2: retorno con contexto; validar y mostrar.
4. US3: estados y reintento; validar y mostrar.
5. US4: accesibilidad y adaptación; validar y mostrar.
6. Pulido: regresiones y evidencia real.

### Estrategia con equipo

1. El equipo completa juntos Phase 1 y Phase 2.
2. Un implementador toma US1 y luego US2.
3. Otro implementador escribe pruebas de US3 y US4 en archivos nuevos.
4. Se integran cambios en `PropertyCatalogService.cs`, `PropertyDetails.razor` y `app.css` de uno en uno para evitar conflictos.

---

## Notas

- Las tareas marcadas con `[P]` usan archivos distintos y no dependen de resultados incompletos.
- La etiqueta `[USn]` permite trazar cada tarea hasta su historia en `spec.md`.
- Cada historia debe poder completarse y probarse por separado.
- Las pruebas deben fallar antes de la implementación.
- No se marcan tareas como completadas hasta verificar su resultado.
- No se crean proyectos, paquetes, backend, OpenAPI, migraciones ni capacidades de escritura.
- La spec está en estado `Aprobada`; `speckit.implement` puede iniciarse cuando se decida ejecutar la implementación.
- `specs/012-blazor-property-detail/quickstart.md` debe contener evidencia ejecutada antes de declarar la iniciativa terminada.
- Evitar descripciones vagas, rutas incompletas y dependencias entre historias que rompan su independencia.
