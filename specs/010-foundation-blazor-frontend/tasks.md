---

description: "Lista de tareas para la fundación del frontend web"
---

# Tareas: Fundación del frontend web

**Entrada**: Documentos de diseño en `/specs/010-foundation-blazor-frontend/`

**Prerrequisitos**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/` y `quickstart.md`

**Pruebas**: se incluyen pruebas unitarias porque la especificación declara las pruebas como obligatorias y define criterios de prueba independiente por historia.

**Organización**: las tareas se agrupan por historia de usuario para permitir implementación y verificación independientes.

## Formato: `[ID] [P?] [Historia] Descripción con ruta de archivo`

- **[P]**: puede ejecutarse en paralelo; usa archivos distintos y no depende de tareas incompletas.
- **[Historia]**: etiqueta `US1`, `US2`, `US3` o `US4` en las fases de historia de usuario.
- Todas las tareas incluyen rutas de archivo exactas.

## Convenciones de rutas

- Proyecto web: `app/frontend/src/RealtorWeb/`.
- Pruebas web: `app/frontend/test/RealtorWeb/`.
- Backend: `app/backend/src/RealtorApi/` y `app/backend/tests/RealtorApiTests/`.
- Solución: `app/Realtor.sln`.
- No se crean proyectos nuevos.

---

## Fase 1: Configuración (infraestructura compartida)

**Propósito**: preparar dependencias, configuración y namespace de la fundación.

- [X] T001 [P] Añadir los paquetes `Refit`, `Refit.HttpClientFactory` y `Refit.HttpClientFactory.Reflection` 17.0.0, y `LucideBlazor` 1.0.7, en `app/frontend/src/RealtorWeb/RealtorWeb.csproj`.
- [X] T002 [P] Cambiar el SDK de `app/frontend/test/RealtorWeb/RealtorWebTests.csproj` a `Microsoft.NET.Sdk.Razor` y añadir `bunit` 2.14.0.
- [X] T003 [P] Configurar `ApiClients:RealtorApi:BaseAddress` con `http://localhost:5085/` en `app/frontend/src/RealtorWeb/appsettings.json` y `app/frontend/src/RealtorWeb/appsettings.Development.json`.
- [X] T004 [P] Añadir `@using LucideBlazor` y `@using RealtorWeb.Features.Properties` en `app/frontend/src/RealtorWeb/Components/_Imports.razor`.

---

## Fase 2: Fundacional (prerrequisitos bloqueantes)

**Propósito**: crear la infraestructura que debe existir antes de cualquier historia de usuario.

**CRÍTICO**: ninguna historia de usuario puede comenzar hasta completar esta fase.

- [X] T005 Crear la interfaz Refit y los tipos de transporte en `app/frontend/src/RealtorWeb/Features/Properties/IPropertiesApi.cs` con `ListAsync(page, pageSize, cancellationToken)` y `GetByIdAsync(id, cancellationToken)`, resolviendo data-model.md `ListPropertiesResponse.items`, data-model.md `ListPropertiesResponse.page`, data-model.md `ListPropertiesResponse.pageSize`, data-model.md `ListPropertiesResponse.totalItems`, data-model.md `ListPropertiesResponse.totalPages`, data-model.md `ListPropertiesResponse.hasNext`, data-model.md `ListPropertiesResponse.hasPrevious`, data-model.md `ListPropertiesItemResponse.id`, data-model.md `ListPropertiesItemResponse.title`, data-model.md `ListPropertiesItemResponse.description`, data-model.md `ListPropertiesItemResponse.address`, data-model.md `ListPropertiesItemResponse.price`, data-model.md `ListPropertiesItemResponse.status`, data-model.md `ListPropertiesItemResponse.bedroomCount`, data-model.md `ListPropertiesItemResponse.bathroomCount`, data-model.md `ListPropertiesItemResponse.areaSquareMeters`, data-model.md `ListPropertiesItemResponse.imageUrl`, data-model.md `GetPropertyByIdResponse.id`, data-model.md `GetPropertyByIdResponse.title`, data-model.md `GetPropertyByIdResponse.description`, data-model.md `GetPropertyByIdResponse.address`, data-model.md `GetPropertyByIdResponse.price`, data-model.md `GetPropertyByIdResponse.status`, data-model.md `GetPropertyByIdResponse.bedroomCount`, data-model.md `GetPropertyByIdResponse.bathroomCount`, data-model.md `GetPropertyByIdResponse.areaSquareMeters`, data-model.md `GetPropertyByIdResponse.imageUrl`, data-model.md `GetPropertyByIdResponse.createdAt` y data-model.md `GetPropertyByIdResponse.updatedAt`, según `contracts/contrato-consumo-api.md`, sin `RestService.For<T>()`.
- [X] T006 [P] Registrar `IPropertiesApi` con `IHttpClientFactory`, nombre de cliente `RealtorApi` y `BaseAddress` desde configuración en `app/frontend/src/RealtorWeb/Program.cs`, sin `RestService.For<T>()`.
- [X] T007 [P] Crear `PropertyCardViewModel` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyViewModels.cs`, resolviendo data-model.md `PropertyCardViewModel.Id`, data-model.md `PropertyCardViewModel.Title`, data-model.md `PropertyCardViewModel.Summary`, data-model.md `PropertyCardViewModel.Address`, data-model.md `PropertyCardViewModel.Price`, data-model.md `PropertyCardViewModel.StatusLabel`, data-model.md `PropertyCardViewModel.BedroomCount`, data-model.md `PropertyCardViewModel.BathroomCount`, data-model.md `PropertyCardViewModel.AreaSquareMeters`, data-model.md `PropertyCardViewModel.ImageUrl`, data-model.md `PropertyCardViewModel.ImageAlt` y data-model.md `PropertyCardViewModel.DetailsUrl`.
- [X] T008 [P] Crear `PropertyCatalogViewModel` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyViewModels.cs`, resolviendo data-model.md `PropertyCatalogViewModel.Items`, data-model.md `PropertyCatalogViewModel.Page`, data-model.md `PropertyCatalogViewModel.PageSize`, data-model.md `PropertyCatalogViewModel.TotalItems`, data-model.md `PropertyCatalogViewModel.TotalPages`, data-model.md `PropertyCatalogViewModel.HasNext`, data-model.md `PropertyCatalogViewModel.HasPrevious`, data-model.md `PropertyCatalogViewModel.PreviousUrl`, data-model.md `PropertyCatalogViewModel.NextUrl`, data-model.md `PropertyCatalogViewModel.SummaryText`, data-model.md `PropertyCatalogViewModel.State`, data-model.md `PropertyCatalogViewModel.Message` y data-model.md `PropertyCatalogViewModel.RetryUrl`.
- [X] T009 [P] Crear `PropertyDetailsViewModel` y `PresentationState` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyViewModels.cs`, resolviendo data-model.md `PropertyDetailsViewModel.Id`, data-model.md `PropertyDetailsViewModel.Title`, data-model.md `PropertyDetailsViewModel.Description`, data-model.md `PropertyDetailsViewModel.Address`, data-model.md `PropertyDetailsViewModel.Price`, data-model.md `PropertyDetailsViewModel.StatusLabel`, data-model.md `PropertyDetailsViewModel.BedroomCount`, data-model.md `PropertyDetailsViewModel.BathroomCount`, data-model.md `PropertyDetailsViewModel.AreaSquareMeters`, data-model.md `PropertyDetailsViewModel.ImageUrl`, data-model.md `PropertyDetailsViewModel.ImageAlt`, data-model.md `PropertyDetailsViewModel.CreatedAt`, data-model.md `PropertyDetailsViewModel.UpdatedAt`, data-model.md `PropertyDetailsViewModel.BackUrl`, data-model.md `PropertyDetailsViewModel.State`, data-model.md `PropertyDetailsViewModel.Message` y data-model.md `PropertyDetailsViewModel.RetryUrl`.
- [X] T010 Crear `PropertyStatusLabels` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyStatusLabels.cs`, resolviendo data-model.md `ListPropertiesItemResponse.status`, data-model.md `GetPropertyByIdResponse.status`, data-model.md `PropertyCardViewModel.StatusLabel` y data-model.md `PropertyDetailsViewModel.StatusLabel`, sin mostrar los valores técnicos publicados.
- [X] T011 Crear `CatalogNavigation` en `app/frontend/src/RealtorWeb/Features/Properties/CatalogNavigation.cs`, resolviendo data-model.md `ListPropertiesResponse.page`, data-model.md `ListPropertiesResponse.pageSize`, data-model.md `ListPropertiesResponse.totalItems`, data-model.md `ListPropertiesResponse.totalPages`, data-model.md `ListPropertiesResponse.hasNext`, data-model.md `ListPropertiesResponse.hasPrevious`, data-model.md `PropertyCatalogViewModel.Page`, data-model.md `PropertyCatalogViewModel.PageSize`, data-model.md `PropertyCatalogViewModel.TotalPages`, data-model.md `PropertyCatalogViewModel.HasNext`, data-model.md `PropertyCatalogViewModel.HasPrevious`, data-model.md `PropertyCatalogViewModel.PreviousUrl`, data-model.md `PropertyCatalogViewModel.NextUrl`, data-model.md `PropertyCatalogViewModel.RetryUrl`, data-model.md `PropertyCardViewModel.DetailsUrl`, data-model.md `PropertyDetailsViewModel.BackUrl` y data-model.md `PropertyDetailsViewModel.RetryUrl`.
- [X] T012 [P] Adaptar `app/frontend/src/RealtorWeb/Components/App.razor` a `lang="es"`, eliminando la hoja Bootstrap y la referencia `RealtorWeb.styles.css`, y conservando `app.css` y `blazor.web.js`.
- [X] T013 [P] Definir tokens de color, tipografía, espaciado, foco e iconos, y estilos base en `app/frontend/src/RealtorWeb/wwwroot/app.css`.
- [X] T014 [P] Eliminar los archivos de plantilla `app/frontend/src/RealtorWeb/wwwroot/lib/bootstrap/`, `app/frontend/src/RealtorWeb/Components/Layout/MainLayout.razor.css`, `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor.css` y `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor.js`.
- [X] T015 Crear el componente compartido de estados en `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`, resolviendo data-model.md `PropertyCatalogViewModel.State`, data-model.md `PropertyCatalogViewModel.Message`, data-model.md `PropertyCatalogViewModel.RetryUrl`, data-model.md `PropertyDetailsViewModel.State`, data-model.md `PropertyDetailsViewModel.Message` y data-model.md `PropertyDetailsViewModel.RetryUrl`.
- [X] T016 Crear el shell base en `app/frontend/src/RealtorWeb/Components/Layout/MainLayout.razor` y `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor` con identificación y enlace común al catálogo, sin clases Bootstrap.

**Punto de control**: la base está lista; las historias de usuario pueden comenzar.

---

## Fase 3: Historia de usuario 1 - Explorar el catálogo desde la entrada principal (Prioridad: P1) - MVP

**Objetivo**: mostrar un catálogo paginado con la información principal de cada propiedad desde la entrada principal.

**Prueba independiente**: abrir la entrada principal con propiedades publicadas y verificar un listado paginado con datos de cada inmueble y controles para avanzar o retroceder.

**Nota de pruebas**: escribir primero las pruebas de esta historia y confirmar que fallan antes de la implementación.

### Pruebas de la historia de usuario 1

- [X] T017 [P] [US1] Crear pruebas unitarias de saneamiento de página y construcción de URLs en `app/frontend/test/RealtorWeb/CatalogNavigationTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.
- [X] T018 [P] [US1] Crear pruebas bUnit de render del catálogo, resumen de paginación y enlaces anterior/siguiente en `app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.
- [X] T019 [P] [US1] Crear pruebas bUnit de tarjeta con imagen, sin imagen y con cada etiqueta de estado en `app/frontend/test/RealtorWeb/Components/PropertyCardTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.
- [X] T020 [P] [US1] Crear pruebas unitarias de `PropertyStatusLabels` en `app/frontend/test/RealtorWeb/PropertyStatusLabelsTests.cs` para `Available`, `Rented`, `Maintenance` y valor no asignado, verificando «Disponible», «Alquilada», «Mantenimiento» y «Estado no disponible»; ejecutarlas antes de la implementación y confirmar que fallan.

### Implementación de la historia de usuario 1

- [X] T021 [US1] Implementar el listado en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs`, resolviendo data-model.md `ListPropertiesResponse.items`, data-model.md `ListPropertiesResponse.page`, data-model.md `ListPropertiesResponse.pageSize`, data-model.md `ListPropertiesResponse.totalItems`, data-model.md `ListPropertiesResponse.totalPages`, data-model.md `ListPropertiesResponse.hasNext`, data-model.md `ListPropertiesResponse.hasPrevious`, data-model.md `ListPropertiesItemResponse.id`, data-model.md `ListPropertiesItemResponse.title`, data-model.md `ListPropertiesItemResponse.description`, data-model.md `ListPropertiesItemResponse.address`, data-model.md `ListPropertiesItemResponse.price`, data-model.md `ListPropertiesItemResponse.status`, data-model.md `ListPropertiesItemResponse.bedroomCount`, data-model.md `ListPropertiesItemResponse.bathroomCount`, data-model.md `ListPropertiesItemResponse.areaSquareMeters`, data-model.md `ListPropertiesItemResponse.imageUrl`, data-model.md `PropertyCardViewModel.Id`, data-model.md `PropertyCardViewModel.Title`, data-model.md `PropertyCardViewModel.Summary`, data-model.md `PropertyCardViewModel.Address`, data-model.md `PropertyCardViewModel.Price`, data-model.md `PropertyCardViewModel.BedroomCount`, data-model.md `PropertyCardViewModel.BathroomCount`, data-model.md `PropertyCardViewModel.AreaSquareMeters`, data-model.md `PropertyCardViewModel.ImageUrl`, data-model.md `PropertyCardViewModel.ImageAlt` y data-model.md `PropertyCardViewModel.DetailsUrl`.
- [X] T022 [US1] Implementar `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` con `@attribute [StreamRendering]`, catálogo y paginación, resolviendo data-model.md `PropertyCatalogViewModel.Items`, data-model.md `PropertyCatalogViewModel.Page`, data-model.md `PropertyCatalogViewModel.TotalPages`, data-model.md `PropertyCatalogViewModel.HasNext`, data-model.md `PropertyCatalogViewModel.HasPrevious`, data-model.md `PropertyCatalogViewModel.PreviousUrl`, data-model.md `PropertyCatalogViewModel.NextUrl`, data-model.md `PropertyCatalogViewModel.SummaryText`, data-model.md `PropertyCatalogViewModel.State`, data-model.md `PropertyCatalogViewModel.Message` y data-model.md `PropertyCatalogViewModel.RetryUrl`.
- [X] T023 [US1] Implementar `app/frontend/src/RealtorWeb/Components/Properties/PropertyCard.razor`, resolviendo data-model.md `PropertyCardViewModel.Id`, data-model.md `PropertyCardViewModel.Title`, data-model.md `PropertyCardViewModel.Summary`, data-model.md `PropertyCardViewModel.Address`, data-model.md `PropertyCardViewModel.Price`, data-model.md `PropertyCardViewModel.StatusLabel`, data-model.md `PropertyCardViewModel.BedroomCount`, data-model.md `PropertyCardViewModel.BathroomCount`, data-model.md `PropertyCardViewModel.AreaSquareMeters`, data-model.md `PropertyCardViewModel.ImageUrl`, data-model.md `PropertyCardViewModel.ImageAlt` y data-model.md `PropertyCardViewModel.DetailsUrl`.
- [X] T024 [US1] Implementar `app/frontend/src/RealtorWeb/Components/Properties/PropertyStatusBadge.razor`, resolviendo data-model.md `PropertyCardViewModel.StatusLabel` y data-model.md `PropertyDetailsViewModel.StatusLabel`.

**Punto de control**: la historia 1 es funcional y verificable de forma independiente.

---

## Fase 4: Historia de usuario 2 - Consultar el detalle completo de una propiedad (Prioridad: P1)

**Objetivo**: abrir una propiedad desde el catálogo y mostrar toda su información publicada conservando la página de origen.

**Prueba independiente**: seleccionar una tarjeta desde una página concreta del catálogo y verificar la ficha completa y que la acción de volver restaura esa página y el orden del catálogo mediante `/?page=N`, sin parámetros de orden.

**Nota de pruebas**: escribir primero las pruebas de esta historia y confirmar que fallan antes de la implementación.

### Pruebas de la historia de usuario 2

- [X] T025 [P] [US2] Crear pruebas bUnit de detalle completo, imagen ausente, identificador inexistente y retorno a la página de origen preservando el orden del catálogo mediante `/?page=N` sin parámetros de orden en `app/frontend/test/RealtorWeb/Components/PropertyDetailsTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.

### Implementación de la historia de usuario 2

- [X] T026 [US2] Ampliar `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs` con la obtención y mapeo de detalle, resolviendo data-model.md `GetPropertyByIdResponse.id`, data-model.md `GetPropertyByIdResponse.title`, data-model.md `GetPropertyByIdResponse.description`, data-model.md `GetPropertyByIdResponse.address`, data-model.md `GetPropertyByIdResponse.price`, data-model.md `GetPropertyByIdResponse.status`, data-model.md `GetPropertyByIdResponse.bedroomCount`, data-model.md `GetPropertyByIdResponse.bathroomCount`, data-model.md `GetPropertyByIdResponse.areaSquareMeters`, data-model.md `GetPropertyByIdResponse.imageUrl`, data-model.md `GetPropertyByIdResponse.createdAt`, data-model.md `GetPropertyByIdResponse.updatedAt`, data-model.md `PropertyDetailsViewModel.Id`, data-model.md `PropertyDetailsViewModel.Title`, data-model.md `PropertyDetailsViewModel.Description`, data-model.md `PropertyDetailsViewModel.Address`, data-model.md `PropertyDetailsViewModel.Price`, data-model.md `PropertyDetailsViewModel.BedroomCount`, data-model.md `PropertyDetailsViewModel.BathroomCount`, data-model.md `PropertyDetailsViewModel.AreaSquareMeters`, data-model.md `PropertyDetailsViewModel.ImageUrl`, data-model.md `PropertyDetailsViewModel.ImageAlt`, data-model.md `PropertyDetailsViewModel.CreatedAt` y data-model.md `PropertyDetailsViewModel.UpdatedAt`.
- [X] T027 [US2] Implementar `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor` con la ruta `/properties/{id:guid}`, `@attribute [StreamRendering]` y enlace de retorno a `/?page=N` sin parámetros de orden, resolviendo data-model.md `PropertyDetailsViewModel.Title`, data-model.md `PropertyDetailsViewModel.Description`, data-model.md `PropertyDetailsViewModel.Address`, data-model.md `PropertyDetailsViewModel.Price`, data-model.md `PropertyDetailsViewModel.StatusLabel`, data-model.md `PropertyDetailsViewModel.BedroomCount`, data-model.md `PropertyDetailsViewModel.BathroomCount`, data-model.md `PropertyDetailsViewModel.AreaSquareMeters`, data-model.md `PropertyDetailsViewModel.ImageUrl`, data-model.md `PropertyDetailsViewModel.ImageAlt`, data-model.md `PropertyDetailsViewModel.CreatedAt`, data-model.md `PropertyDetailsViewModel.UpdatedAt`, data-model.md `PropertyDetailsViewModel.BackUrl`, data-model.md `PropertyDetailsViewModel.State`, data-model.md `PropertyDetailsViewModel.Message` y data-model.md `PropertyDetailsViewModel.RetryUrl`.

**Punto de control**: las historias 1 y 2 son funcionales y verificables de forma independiente.

---

## Fase 5: Historia de usuario 3 - Comprender estados vacíos, de carga y de error (Prioridad: P2)

**Objetivo**: ofrecer mensajes claros y acciones válidas cuando no hay datos, están cargando o no pueden obtenerse.

**Prueba independiente**: forzar catálogo vacío, carga lenta y fallo de obtención, verificando mensaje, acción y ausencia de pantallas vacías.

**Nota de pruebas**: escribir primero las pruebas de esta historia y confirmar que fallan antes de la implementación.

### Pruebas de la historia de usuario 3

- [X] T028 [P] [US3] Crear pruebas unitarias del selector de estados en `app/frontend/test/RealtorWeb/PropertyPresentationTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.
- [X] T029 [P] [US3] Crear pruebas bUnit de mensajes y acciones para carga, vacío, error y no encontrado en `app/frontend/test/RealtorWeb/Components/PresentationStateTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.

### Implementación de la historia de usuario 3

- [X] T030 [US3] Implementar `PropertyPresentation` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyPresentation.cs`, resolviendo data-model.md `PropertyCatalogViewModel.State`, data-model.md `PropertyCatalogViewModel.Message`, data-model.md `PropertyCatalogViewModel.RetryUrl`, data-model.md `PropertyDetailsViewModel.State`, data-model.md `PropertyDetailsViewModel.Message` y data-model.md `PropertyDetailsViewModel.RetryUrl`.
- [X] T031 [US3] Integrar los estados en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, resolviendo data-model.md `PropertyCatalogViewModel.State`, data-model.md `PropertyCatalogViewModel.Message`, data-model.md `PropertyCatalogViewModel.RetryUrl`, data-model.md `PropertyCatalogViewModel.HasNext`, data-model.md `PropertyCatalogViewModel.HasPrevious`, data-model.md `PropertyDetailsViewModel.State`, data-model.md `PropertyDetailsViewModel.Message` y data-model.md `PropertyDetailsViewModel.RetryUrl`.
- [X] T032 [US3] Adaptar `app/frontend/src/RealtorWeb/Components/Pages/NotFound.razor` y `app/frontend/src/RealtorWeb/Components/Pages/Error.razor` con mensajes en español y enlace de regreso al catálogo, sin detalles técnicos.

**Punto de control**: las historias 1, 2 y 3 son funcionales y verificables de forma independiente.

---

## Fase 6: Historia de usuario 4 - Navegar con una experiencia coherente y accesible (Prioridad: P2)

**Objetivo**: fijar navegación común, foco visible, alternativas textuales y adaptación a 320, 768 y 1280 px.

**Prueba independiente**: recorrer catálogo, detalle y estados de error con teclado y en tres tamaños de pantalla, verificando navegación visible, foco perceptible y ausencia de desplazamiento horizontal.

**Nota de pruebas**: escribir primero las pruebas de esta historia y confirmar que fallan antes de la implementación.

### Pruebas de la historia de usuario 4

- [X] T033 [P] [US4] Crear pruebas bUnit del shell común, enlaces, `aria-label`, texto alternativo y foco en `app/frontend/test/RealtorWeb/Components/Layout/LayoutTests.cs`; ejecutarlas antes de la implementación y confirmar que fallan.

### Implementación de la historia de usuario 4

- [X] T034 [US4] Refinar `app/frontend/src/RealtorWeb/Components/Layout/MainLayout.razor` y `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor` con identificación y navegación coherente, textos en español y enlaces accesibles por teclado.
- [X] T035 [US4] Implementar reglas responsive, contraste y foco visible para 320 px, 768 px y 1280 px en `app/frontend/src/RealtorWeb/wwwroot/app.css`.
- [X] T036 [US4] Integrar iconos Lucide con `aria-hidden="true"` o `aria-label` según corresponda en `app/frontend/src/RealtorWeb/Components/Properties/PropertyCard.razor`, `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor` y `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`.
- [X] T037 [US4] Revisar que todos los textos visibles estén en español en `app/frontend/src/RealtorWeb/Components/Pages/Home.razor`, `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`, `app/frontend/src/RealtorWeb/Components/Pages/NotFound.razor`, `app/frontend/src/RealtorWeb/Components/Pages/Error.razor`, `app/frontend/src/RealtorWeb/Components/Properties/PropertyCard.razor`, `app/frontend/src/RealtorWeb/Components/Properties/PresentationState.razor`, `app/frontend/src/RealtorWeb/Components/Layout/MainLayout.razor` y `app/frontend/src/RealtorWeb/Components/Layout/NavMenu.razor`.

**Punto de control**: todas las historias son funcionales y verificables de forma independiente.

---

## Fase 7: Pulido y preocupaciones transversales

**Propósito**: verificar límites arquitectónicos, regresiones y evidencia de cierre.

- [X] T038 [P] Verificar que no aparezcan `AddInteractiveServerComponents`, `AddInteractiveWebAssemblyComponents`, `@rendermode`, `InteractiveServer`, `InteractiveWebAssembly` ni `InteractiveAuto` en `app/frontend/src/RealtorWeb/Program.cs`, `app/frontend/src/RealtorWeb/Components/Routes.razor`, `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`.
- [X] T039 [P] Verificar que no aparezcan `HttpClient` directo ni `RestService.For<T>()` en `app/frontend/src/RealtorWeb/Features/Properties/PropertyCatalogService.cs`, `app/frontend/src/RealtorWeb/Components/Pages/Home.razor` y `app/frontend/src/RealtorWeb/Components/Pages/PropertyDetails.razor`.
- [ ] T040 Ejecutar la validación documentada en `specs/010-foundation-blazor-frontend/quickstart.md`, incluyendo restauración, compilación, pruebas frontend, regresión OpenAPI y comprobación manual de rutas y anchos. Nota: restauración, compilación, pruebas frontend, regresión OpenAPI y solución completa ejecutados en verde; comprobación manual en navegador y anchos pendiente.
- [X] T041 Ejecutar la solución completa con `app/Realtor.sln` y la regresión OpenAPI en `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`, confirmando que no cambian las `4` rutas ni las `6` operaciones.
- [ ] T042 Ejecutar y anexar en `specs/010-foundation-blazor-frontend/quickstart.md` el protocolo de medición de SC-003 y SC-010, confirmando al menos 9 de 10 participantes en menos de 2 minutos y al menos 19 de 20 cargas con primer contenido útil en menos de 3 segundos.
- [ ] T043 Verificar en `specs/010-foundation-blazor-frontend/spec.md` que el estado es `En implementación` mientras falte evidencia de cierre, que no existen cambios en `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` y que `git diff --check` no reporta errores; transicionar a `Implementada` solo cuando todas las tareas y la evidencia estén completas. Nota: estado verificado como `En implementación`, OpenAPI sin cambios y `git diff --check` limpio; cierre pendiente de T040 y T042.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Fase 1 (Configuración)**: sin dependencias; puede comenzar de inmediato.
- **Fase 2 (Fundacional)**: depende de la Fase 1 y bloquea todas las historias.
- **Fases 3 a 6 (Historias)**: dependen de la Fase 2; pueden ejecutarse en paralelo si hay capacidad, o en orden P1 → P2.
- **Fase 7 (Pulido)**: depende de todas las historias incluidas en el alcance.

### Dependencias entre historias

- **US1 (P1)**: comienza tras la Fase 2; no depende de otras historias.
- **US2 (P1)**: comienza tras la Fase 2; reutiliza el servicio de la Fase 2 y el catálogo de US1 para la navegación de entrada, pero es verificable de forma independiente con una URL directa.
- **US3 (P2)**: comienza tras la Fase 2; refine estados usados por US1 y US2, por lo que conviene ejecutarla después de ellas.
- **US4 (P2)**: comienza tras la Fase 2; refina shell, iconos y CSS usados por todas las vistas, por lo que conviene ejecutarla después de US1 y US2.

### Dentro de cada historia

- Las pruebas se escriben primero y deben fallar antes de la implementación.
- Los modelos y view models de la Fase 2 preceden a los servicios.
- Los servicios preceden a los componentes Razor.
- La integración y los puntos de control se verifican antes de avanzar.

### Oportunidades de paralelismo

- Las cuatro tareas de la Fase 1 pueden ejecutarse en paralelo.
- En la Fase 2, las tareas marcadas `[P]` pueden ejecutarse en paralelo; T006 depende de T005, y T010, T011, T015 y T016 esperan a los view models o al shell base.
- Las cuatro pruebas de US1 y las pruebas de US2, US3 y US4 pueden ejecutarse en paralelo entre sí.
- US1 y US2 pueden implementarse en paralelo tras la Fase 2.
- US3 y US4 pueden preparar pruebas en paralelo con US1 y US2.
- T038 y T039 pueden ejecutarse en paralelo.

---

## Ejemplos de ejecución paralela

### Ejemplo paralelo: historia de usuario 1

```text
T017 Crear pruebas unitarias de saneamiento y URLs en app/frontend/test/RealtorWeb/CatalogNavigationTests.cs
T018 Crear pruebas bUnit de catálogo y paginación en app/frontend/test/RealtorWeb/Components/CatalogPageTests.cs
T019 Crear pruebas bUnit de tarjeta en app/frontend/test/RealtorWeb/Components/PropertyCardTests.cs
T020 Crear pruebas unitarias de etiquetas de estado en app/frontend/test/RealtorWeb/PropertyStatusLabelsTests.cs
```

### Ejemplo paralelo: historia de usuario 2

```text
T025 Crear pruebas bUnit de detalle en app/frontend/test/RealtorWeb/Components/PropertyDetailsTests.cs
```

### Ejemplo paralelo: historia de usuario 3

```text
T028 Crear pruebas unitarias del selector en app/frontend/test/RealtorWeb/PropertyPresentationTests.cs
T029 Crear pruebas bUnit de estados en app/frontend/test/RealtorWeb/Components/PresentationStateTests.cs
```

### Ejemplo paralelo: historia de usuario 4

```text
T033 Crear pruebas bUnit del shell y accesibilidad en app/frontend/test/RealtorWeb/Components/Layout/LayoutTests.cs
```

---

## Estrategia de implementación

### MVP primero: solo historia de usuario 1

1. Completar la Fase 1.
2. Completar la Fase 2 de forma crítica.
3. Completar la Fase 3.
4. Detenerse y validar US1 de forma independiente.
5. Mostrar el catálogo como incremento de valor antes de continuar.

### Entrega incremental

1. Configuración + fundacional → base lista.
2. US1 → catálogo paginado → validar y mostrar MVP.
3. US2 → detalle con retorno → validar y mostrar.
4. US3 → estados accionables → validar y mostrar.
5. US4 → navegación coherente y accesible → validar y mostrar.
6. Pulido → verificación completa y evidencia.

### Estrategia con equipo paralelo

1. El equipo completa juntas las Fases 1 y 2.
2. Después de la Fase 2:
   - Desarrollador A: US1.
   - Desarrollador B: US2.
   - Desarrollador C: pruebas de US3 y US4.
3. Integrar US3 y US4 después de que US1 y US2 estén estables.

---

## Notas

- `[P]` significa archivos distintos y sin dependencias de tareas incompletas.
- La etiqueta `[USn]` solo aparece en tareas de historias de usuario.
- Cada historia puede detenerse y validarse en su propio punto de control.
- No se regenera el OpenAPI porque la especificación no crea ni modifica endpoints.
- El estado de `spec.md` se actualiza automáticamente: `Aprobada` tras la aprobación humana, `En implementación` al iniciar este comando e `Implementada` solo cuando todas las tareas y la evidencia estén completas.
- Todas las tareas usan el formato obligatorio: checkbox, identificador secuencial, marcadores aplicables y ruta de archivo.
