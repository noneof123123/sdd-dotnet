---

description: "Lista de tareas para la implementación de la característica"
---

# Tareas: Listado paginado de propiedades en la página principal

**Entrada**: Documentos de diseño de `/specs/011-blazor-list-properties/`

**Prerrequisitos**: plan.md (obligatorio), spec.md (obligatorio para historias), research.md, data-model.md, contracts/

**Pruebas**: se incluyen tareas de prueba porque la spec declara la sección «Escenarios de usuario y pruebas» y criterios verificables. Solo son unit tests con xUnit y bUnit, sin infraestructura externa.

**Organización**: las tareas se agrupan por historia de usuario para permitir implementación, prueba y entrega independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: puede ejecutarse en paralelo (archivos distintos y sin dependencias)
- **[Historia]**: historia de usuario a la que pertenece (US1, US2, US3, US4)
- Incluir rutas de archivo exactas en las descripciones

## Convenciones de rutas

- **Aplicación web**: `app/frontend/src/RealtorWeb/`, `app/frontend/test/RealtorWeb/`
- **Backend existente**: `app/backend/src/RealtorApi/`
- No se crean proyectos nuevos; se usa la solución `app/Realtor.sln`.

---

## Fase 1: Configuración (infraestructura compartida)

**Propósito**: confirmar que el entorno y los proyectos existentes son suficientes; no se inicializa nada nuevo.

- [X] T001 Verificar la solución y los proyectos frontend y de pruebas existentes en `app/Realtor.sln`
- [X] T002 [P] Confirmar el registro de Refit y `ApiClients:RealtorApi:BaseAddress` en `app/frontend/src/RealtorWeb/Program.cs` y `app/frontend/src/RealtorWeb/appsettings.json`
- [X] T003 [P] Confirmar la configuración de xUnit y bUnit en `app/frontend/test/RealtorWeb/RealtorWebTests.csproj`

---

## Fase 2: Fundacional (prerrequisitos bloqueantes)

**Propósito**: texto visible, estilos y mapeo que todas las historias necesitan.

**⚠️ CRÍTICO**: ninguna historia puede comenzar hasta completar esta fase.

- [X] T004 Añadir `TotalItemsText` a `PropertyCatalogViewModel` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyViewModels.cs` según `data-model.md: PropertyCatalogViewModel.TotalItemsText`, con normalización desde `data-model.md: PropertyCatalogViewModel.TotalItems` y `data-model.md: PropertyCatalogViewModel.TotalPages`
- [X] T005 [P] Añadir las etiquetas de paginación en `app/frontend/src/RealtorWeb/Features/Properties/PropertyPresentation.cs` según `data-model.md: PropertyPresentation.PreviousControlText`, `data-model.md: PropertyPresentation.NextControlText`, `data-model.md: PropertyPresentation.PreviousDisabledLabel`, `data-model.md: PropertyPresentation.NextDisabledLabel` y `data-model.md: PropertyPresentation.PaginationNavLabel`
- [X] T006 [P] Añadir los estilos de control deshabilitado y resumen en `app/frontend/src/RealtorWeb/wwwroot/app.css` según `data-model.md: EstilosPaginacion.DisabledControlClass` y `data-model.md: EstilosPaginacion.SummaryClass`
- [X] T007 Actualizar el mapeo del contenedor paginado en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` según `data-model.md: ListPropertiesResponse.items`, `data-model.md: ListPropertiesResponse.page`, `data-model.md: ListPropertiesResponse.pageSize`, `data-model.md: ListPropertiesResponse.totalItems`, `data-model.md: ListPropertiesResponse.totalPages`, `data-model.md: ListPropertiesResponse.hasNext`, `data-model.md: ListPropertiesResponse.hasPrevious`, `data-model.md: PropertyCatalogViewModel.Items`, `data-model.md: PropertyCatalogViewModel.Page`, `data-model.md: PropertyCatalogViewModel.PageSize`, `data-model.md: PropertyCatalogViewModel.TotalItems`, `data-model.md: PropertyCatalogViewModel.TotalPages`, `data-model.md: PropertyCatalogViewModel.HasNext`, `data-model.md: PropertyCatalogViewModel.HasPrevious` y `data-model.md: PropertyCatalogViewModel.SummaryText`
- [X] T008 Verificar el mapeo de cada propiedad en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` según `data-model.md: ListPropertiesItemResponse.id`, `data-model.md: ListPropertiesItemResponse.title`, `data-model.md: ListPropertiesItemResponse.description`, `data-model.md: ListPropertiesItemResponse.address`, `data-model.md: ListPropertiesItemResponse.price`, `data-model.md: ListPropertiesItemResponse.status`, `data-model.md: ListPropertiesItemResponse.bedroomCount`, `data-model.md: ListPropertiesItemResponse.bathroomCount`, `data-model.md: ListPropertiesItemResponse.areaSquareMeters`, `data-model.md: ListPropertiesItemResponse.imageUrl`, `data-model.md: PropertyCardViewModel.Id`, `data-model.md: PropertyCardViewModel.Title`, `data-model.md: PropertyCardViewModel.Summary`, `data-model.md: PropertyCardViewModel.Address`, `data-model.md: PropertyCardViewModel.Price`, `data-model.md: PropertyCardViewModel.StatusLabel`, `data-model.md: PropertyCardViewModel.BedroomCount`, `data-model.md: PropertyCardViewModel.BathroomCount`, `data-model.md: PropertyCardViewModel.AreaSquareMeters`, `data-model.md: PropertyCardViewModel.ImageUrl`, `data-model.md: PropertyCardViewModel.ImageAlt` y `data-model.md: PropertyCardViewModel.DetailsUrl`

**Punto de control**: la capa de presentación, los estilos y el mapeo están listos; pueden comenzar las historias.

---

## Fase 3: Historia de usuario 1 - Explorar propiedades desde la entrada principal (Prioridad: P1) 🎯 MVP

**Objetivo**: abrir `/` y ver la primera página del catálogo con tarjetas, resumen de páginas y total de elementos.

**Prueba independiente**: abrir `/` con propiedades publicadas y comprobar que aparecen tarjetas con sus datos, «Página X de Y» y el total de propiedades.

### Pruebas de la historia 1

> Escribir primero las pruebas y comprobar que fallan antes de la implementación.

- [X] T009 [P] [US1] Añadir prueba xUnit de formato de total en `app/frontend/test/RealtorWeb/PropertyPresentationTests.cs` para `data-model.md: PropertyCatalogViewModel.TotalItemsText` y `data-model.md: PropertyCatalogViewModel.TotalItems`
- [X] T010 [P] [US1] Añadir prueba bUnit de render del catálogo en `app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs` para `data-model.md: PropertyCatalogViewModel.Items`, `data-model.md: PropertyCatalogViewModel.SummaryText` y `data-model.md: PropertyCatalogViewModel.TotalItemsText`

### Implementación de la historia 1

- [X] T011 [US1] Renderizar el resumen y el total en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` usando `data-model.md: PropertyCatalogViewModel.SummaryText`, `data-model.md: PropertyCatalogViewModel.TotalItemsText` y `data-model.md: EstilosPaginacion.SummaryClass`
- [X] T012 [US1] Verificar el render de tarjetas en `app/frontend/src/RealtorWeb/Components/Properties/PropertyCard.razor` sin introducir lógica de formato en el componente

**Punto de control**: la historia 1 es funcional y comprobable de forma independiente.

---

## Fase 4: Historia de usuario 2 - Navegar por las páginas del catálogo (Prioridad: P1)

**Objetivo**: avanzar y retroceder entre páginas con controles visibles, deshabilitados cuando no aplica, y URL coherente.

**Prueba independiente**: con varias páginas, seleccionar «Siguiente» y «Anterior» y verificar resultados, resumen, URL y estado de los controles; en una URL superior a la última, ver la última página efectiva.

### Pruebas de la historia 2

- [X] T013 [P] [US2] Añadir prueba bUnit de controles y navegación en `app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs` para `data-model.md: PropertyCatalogViewModel.HasPrevious`, `data-model.md: PropertyCatalogViewModel.HasNext`, `data-model.md: PropertyCatalogViewModel.PreviousUrl`, `data-model.md: PropertyCatalogViewModel.NextUrl`, `data-model.md: PropertyPresentation.PreviousDisabledLabel` y `data-model.md: PropertyPresentation.NextDisabledLabel`; comprobar que al seleccionar cada enlace cambian resultados, resumen y URL, y que la URL recargada conserva la página
- [X] T014 [P] [US2] Añadir prueba xUnit de saneamiento y página efectiva en `app/frontend/test/RealtorWeb/CatalogNavigationTests.cs` para `data-model.md: PropertyCatalogViewModel.Page` y `data-model.md: PropertyCatalogViewModel.SummaryText`, cubriendo ausente, vacío, no numérico, cero, negativo, desbordamiento y superior a la última página

### Implementación de la historia 2

- [X] T015 [US2] Renderizar «Anterior» y «Siguiente» en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` usando `data-model.md: PropertyPresentation.PreviousControlText`, `data-model.md: PropertyPresentation.NextControlText`, `data-model.md: PropertyPresentation.PreviousDisabledLabel`, `data-model.md: PropertyPresentation.NextDisabledLabel`, `data-model.md: PropertyPresentation.PaginationNavLabel` y `data-model.md: EstilosPaginacion.DisabledControlClass`
- [X] T016 [US2] Verificar los enlaces sobre la página efectiva en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` para `data-model.md: PropertyCatalogViewModel.Page`, `data-model.md: PropertyCatalogViewModel.PreviousUrl` y `data-model.md: PropertyCatalogViewModel.NextUrl`

**Punto de control**: las historias 1 y 2 funcionan de forma independiente y la navegación respeta la aclaración A.

---

## Fase 5: Historia de usuario 3 - Entender los estados del listado (Prioridad: P2)

**Objetivo**: mostrar estados claros de carga, ausencia de resultados y error, sin pantallas vacías ni paginación engañosa.

**Prueba independiente**: forzar carga lenta, catálogo vacío y fallo de obtención; verificar mensaje, reintento y ausencia de paginación.

### Pruebas de la historia 3

- [X] T017 [P] [US3] Añadir prueba bUnit de estados en `app/frontend/test/RealtorWeb/Components/PresentationStateTests.cs` para `data-model.md: PresentationState.Cargando`, `data-model.md: PresentationState.Contenido`, `data-model.md: PresentationState.Vacio`, `data-model.md: PresentationState.Error`, `data-model.md: PropertyPresentation.LoadingCatalogMessage`, `data-model.md: PropertyPresentation.EmptyMessage` y `data-model.md: PropertyPresentation.ErrorMessage`; confirmar que `NoEncontrado` no se renderiza en el listado
- [X] T018 [P] [US3] Añadir prueba xUnit de selección de estado en `app/frontend/test/RealtorWeb/PropertyPresentationTests.cs` para `data-model.md: PropertyCatalogViewModel.State`, `data-model.md: PropertyCatalogViewModel.Message` y `data-model.md: PropertyCatalogViewModel.RetryUrl`

### Implementación de la historia 3

- [X] T019 [US3] Verificar el componente de estados en `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor` para `data-model.md: PresentationState.Cargando`, `data-model.md: PresentationState.Contenido`, `data-model.md: PresentationState.Vacio`, `data-model.md: PresentationState.Error`, `data-model.md: PresentationState.NoEncontrado`, `data-model.md: PropertyPresentation.LoadingCatalogMessage`, `data-model.md: PropertyPresentation.EmptyMessage`, `data-model.md: PropertyPresentation.ErrorMessage`, `data-model.md: PropertyCatalogViewModel.Message` y `data-model.md: PropertyCatalogViewModel.RetryUrl`; `NoEncontrado` debe permanecer fuera del listado
- [X] T020 [US3] Verificar la traducción de fallos a estado en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` para `data-model.md: PropertyCatalogViewModel.State`, `data-model.md: PropertyCatalogViewModel.Message` y `data-model.md: PropertyCatalogViewModel.RetryUrl`

**Punto de control**: las historias 1, 2 y 3 son independientes y no muestran pantallas ambiguas.

---

## Fase 6: Historia de usuario 4 - Utilizar un listado claro, adaptable y accesible (Prioridad: P2)

**Objetivo**: paginación accesible con teclado, grilla adaptable y coherencia visual y lingüística.

**Prueba independiente**: recorrer controles con teclado y revisar a 320, 768 y 1280 px; verificar foco, etiquetas, columnas y ausencia de estilos prohibidos.

### Pruebas de la historia 4

- [X] T021 [P] [US4] Añadir prueba bUnit de accesibilidad de paginación en `app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs` para `data-model.md: PropertyPresentation.PreviousDisabledLabel`, `data-model.md: PropertyPresentation.NextDisabledLabel`, `data-model.md: PropertyPresentation.PaginationNavLabel` y `data-model.md: EstilosPaginacion.DisabledControlClass`
- [X] T022 [P] [US4] Añadir prueba bUnit de alternativas de tarjeta en `app/frontend/test/RealtorWeb/Components/PropertyCardTests.cs` para `data-model.md: PropertyCardViewModel.ImageAlt` y `data-model.md: PropertyCardViewModel.StatusLabel`, cubriendo las etiquetas exactas «Disponible», «Alquilada», «Mantenimiento» y «Estado no disponible» definidas en `app/frontend/src/RealtorWeb/Features/Properties/PropertyStatusLabels.cs`

### Implementación de la historia 4

- [X] T023 [US4] Ajustar contraste, foco y breakpoints de la grilla en `app/frontend/src/RealtorWeb/wwwroot/app.css` según `data-model.md: EstilosPaginacion.DisabledControlClass`
- [X] T024 [US4] Verificar `lang="es"`, textos en español y ausencia de estilos inline en `app/frontend/src/RealtorWeb/Components/App.razor` y `app/frontend/src/RealtorWeb/Components/Pages/Home.razor`

**Punto de control**: todas las historias son funcionales, accesibles y verificables de forma independiente.

---

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: validación final y comprobación de que no se introduce deuda fuera del alcance.

- [X] T025 Ejecutar las pruebas del proyecto frontend con `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj`
- [X] T026 Ejecutar la solución completa con `dotnet test app/Realtor.sln` y comprobar la red de seguridad `OpenApiDriftTests`
- [ ] T027 Ejecutar la verificación manual de `specs/011-blazor-list-properties/quickstart.md` y registrar evidencia real con fecha, comandos y resultados, incluyendo los protocolos SC-004 y SC-010 y una, dos y tres columnas a 320, 768 y 1280 px respectivamente
- [X] T028 Verificar que `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` permanece sin cambios y que no es necesario regenerarlo porque esta spec no toca el backend
- [X] T029 Comprobar que no hay estilos inline, frameworks CSS ni librerías de iconos nuevas en `app/frontend/src/RealtorWeb/`
- [X] T030 Ejecutar `git diff --check` desde la raíz del repositorio

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Fase 1 (Configuración)**: sin dependencias; puede comenzar de inmediato.
- **Fase 2 (Fundacional)**: depende de la Fase 1 y bloquea todas las historias.
- **Fases 3 a 6 (Historias)**: dependen de la Fase 2; pueden ejecutarse en paralelo si hay capacidad, o en orden P1 → P2.
- **Fase 7 (Pulido)**: depende de que estén completas las historias deseadas.

### Dependencias entre historias

- **US1 (P1)**: comienza tras la Fase 2; sin dependencias de otras historias.
- **US2 (P1)**: comienza tras la Fase 2; reutiliza el mapeo de US1 pero es comprobable por separado.
- **US3 (P2)**: comienza tras la Fase 2; no modifica la lógica de navegación de US2.
- **US4 (P2)**: comienza tras la Fase 2; ajusta presentación y accesibilidad sin cambiar el comportamiento de US1 a US3.

### Dentro de cada historia

- Las pruebas se escriben primero y deben fallar antes de la implementación.
- Los textos y view models de la Fase 2 preceden a los componentes.
- El componente se completa antes de la verificación de regresión de la fase final.

### Oportunidades de paralelismo

- T002 y T003 pueden ejecutarse en paralelo.
- T005 y T006 pueden ejecutarse en paralelo con T004; T007 y T008 son secuenciales por compartir archivo.
- T009 y T010 pueden ejecutarse en paralelo; T013 y T014 pueden ejecutarse en paralelo; T017 y T018 pueden ejecutarse en paralelo; T021 y T022 pueden ejecutarse en paralelo.
- Las cuatro historias pueden asignarse a personas distintas tras la Fase 2.

---

## Ejemplo de paralelismo: Historia 1

```bash
# Ejecutar juntas las pruebas de la Historia 1:
Tarea: "Añadir prueba xUnit de formato de total en app/frontend/test/RealtorWeb/PropertyPresentationTests.cs"
Tarea: "Añadir prueba bUnit de render del catálogo en app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs"

# Después, implementar el render del resumen y verificar la tarjeta:
Tarea: "Renderizar el resumen y el total en app/frontend/src/RealtorWeb/Components/Pages/Home.razor"
Tarea: "Verificar el render de tarjetas en app/frontend/src/RealtorWeb/Components/Properties/PropertyCard.razor"
```

---

## Estrategia de implementación

### MVP primero (solo Historia 1)

1. Completar Fase 1.
2. Completar Fase 2 (CRÍTICO: bloquea las historias).
3. Completar Fase 3 (US1).
4. **DETENERSE Y VALIDAR**: probar US1 de forma independiente.
5. Mostrar el resultado; las fases 4 a 6 pueden incorporarse después.

### Entrega incremental

1. Configuración + Fundacional → base lista.
2. US1 → catálogo con resumen y total → MVP.
3. US2 → navegación y controles deshabilitados → demo.
4. US3 → estados claros → demo.
5. US4 → accesibilidad y adaptación → cierre.

### Estrategia con equipo paralelo

1. El equipo completa las fases 1 y 2 juntos.
2. Tras la Fase 2: persona A en US1, persona B en US2, persona C en US3 y persona D en US4.
3. Cada historia se integra y se prueba por separado antes de la Fase 7.

---

## Notas

- `[P]` indica archivos distintos y sin dependencias dentro de la fase.
- `[USn]` traza cada tarea a su historia de usuario.
- Cada historia puede completarse y probarse de forma independiente.
- Verificar que las pruebas fallan antes de implementar.
- No confirmar una tarea como completada hasta verificarla.
- T001, T003 y T030 son tareas de infraestructura y calidad: se trazan al plan y a la constitución, no a un FR o SC concreto.
- No añadir backend, rutas, filtros, ordenación ni capacidades de escritura: están fuera del alcance de esta spec.
