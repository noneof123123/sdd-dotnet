# Feature Specification: Paginación y listado de propiedades

**Feature Branch**: `004-properties-list-pagination`

**Created**: 2026-11-19

**Estado**: Implementada

**Input**: User description: "Crear la especificación 004-properties-list-pagination para definir e implementar un endpoint paginado de listado de propiedades con imágenes públicas absolutas."

## Clarifications

### Session 2026-11-19

- Q: ¿Cuál debe ser el máximo permitido para `pageSize`? → A: 24.
- Q: ¿Qué debe devolver el endpoint cuando `page` está por encima de la última página existente? → A: Normalizar a la última página disponible.
- Q: ¿Qué debe hacer el listado si una propiedad no tiene un archivo de imagen válido? → A: Devolver `imageUrl` vacío.
- Q: ¿Cómo debe ordenar propiedades que tengan exactamente el mismo título? → A: Por dirección ascendente.
- Q: ¿Qué ocurre si `pageSize` supera el máximo permitido de 24? → A: Limitar a 24.
- Los parámetros `page` y `pageSize` son opcionales, con `page = 1` y `pageSize = 6` por defecto.
- El listado DEBE usar título ascendente como orden principal y dirección ascendente como desempate estable.
- `imageUrl` de salida será absoluta y se construirá con el scheme y host del request como `http(s)://{host}/assets/properties/{fileName}`. Si falta un archivo válido, devolverá una cadena vacía.
- `pageSize` tiene un máximo efectivo de 24; valores mayores se normalizan a 24 y valores menores o iguales a cero son inválidos.
- Las URLs públicas se construyen sobre `/assets/properties/{fileName}`; si falta el archivo o el nombre no es válido, `imageUrl` será una cadena vacía.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consultar propiedades paginadas (Priority: P1)

Como consumidor de la API, quiero solicitar una página de propiedades con parámetros opcionales, para navegar por el catálogo sin recibir todos los registros a la vez.

**Why this priority**: Es la operación principal solicitada y permite presentar el catálogo con rendimiento y estabilidad previsibles.

**Independent Test**: Puede probarse llamando a `/api/properties` sin parámetros y con `page`/`pageSize`, comprobando cantidad, orden y metadatos.

**Acceptance Scenarios**:

1. **Given** existen propiedades seed, **When** se consulta `/api/properties` sin parámetros, **Then** se devuelve la página 1 con hasta 6 propiedades ordenadas por título ascendente.
2. **Given** existen suficientes propiedades, **When** se consulta `/api/properties?page=2&pageSize=6`, **Then** se devuelve la segunda página con metadatos consistentes.
3. **Given** una consulta válida, **When** se revisan los metadatos, **Then** se incluyen `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`.

---

### User Story 2 - Recibir imágenes públicas absolutas (Priority: P1)

Como consumidor del API o frontend, quiero que cada propiedad incluya una URL absoluta de imagen, para mostrarla sin convertir rutas internas ni depender del origen del servidor.

**Why this priority**: Las imágenes forman parte del contrato visible del listado y deben ser consumibles directamente.

**Independent Test**: Puede verificarse con una petición a `http://localhost:5023/api/properties` y comprobando que cada `imageUrl` comienza por el host actual y sigue `/assets/properties/{fileName}`.

**Acceptance Scenarios**:

1. **Given** una propiedad con imagen persistida, **When** se consulta el listado, **Then** el item devuelve una URL absoluta válida en `imageUrl`.
2. **Given** el host `http://localhost:5023`, **When** se construye el resultado, **Then** `imageUrl` sigue el patrón `http://localhost:5023/assets/properties/{fileName}`.
3. **Given** una propiedad, **When** se serializa el resultado, **Then** no se devuelve una ruta física ni una ruta relativa en `imageUrl`.

---

### User Story 3 - Rechazar parámetros inválidos (Priority: P2)

Como consumidor de la API, quiero recibir una respuesta estructurada ante parámetros inválidos, para corregir la solicitud sin ambigüedad.

**Why this priority**: Evita páginas negativas, tamaños inválidos y respuestas inconsistentes.

**Independent Test**: Puede probarse enviando `page=0`, `pageSize=0` o valores no enteros y verificando HTTP 400 con detalles de validación.

**Acceptance Scenarios**:

1. **Given** `page <= 0`, **When** se consulta el listado, **Then** se devuelve HTTP 400 con ValidationProblemDetails o ProblemDetails.
2. **Given** `pageSize <= 0`, **When** se consulta el listado, **Then** se devuelve HTTP 400 con detalle estructurado.
3. **Given** `page` o `pageSize` no representa un entero válido, **When** se consulta el listado, **Then** se devuelve HTTP 400 sin consultar ni modificar propiedades.

---

### Edge Cases

- Si no hay propiedades, se devuelve una página válida con `totalItems = 0`, `totalPages = 0`, `hasNext = false` y `hasPrevious = false`.
- Si `page` supera la última página válida, el endpoint DEBE normalizar `page` a la última página disponible y devolver los registros correspondientes con metadatos consistentes.
- Si una propiedad tiene un nombre de imagen vacío o inválido, el listado DEBE devolver `imageUrl` como cadena vacía y mantener el resto de la propiedad.
- Si el host del request contiene un puerto o cambia entre HTTPS y HTTP, la URL absoluta debe reflejar el scheme y host efectivos.
- La cancelación de la consulta debe propagarse hasta la operación de lectura y detener el procesamiento sin exponer datos parciales.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La API DEBE proporcionar `GET /api/properties` mediante un slice registrado bajo `Features/Properties/ListProperties` y descubierto mediante el patrón ISlice existente.
- **FR-002**: El endpoint DEBE aceptar los query params opcionales `page` y `pageSize`.
- **FR-003**: Cuando `page` no se proporciona, DEBE usar el valor 1.
- **FR-004**: Cuando `pageSize` no se proporciona, DEBE usar el valor 6.
- **FR-005**: El listado DEBE ordenarse por `title` ascendente y `address` ascendente como desempate antes de aplicar paginación.
- **FR-006**: La respuesta DEBE incluir una colección paginada de propiedades con `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`.
- **FR-007**: La respuesta DEBE incluir `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`.
- **FR-008**: Cuando exista una imagen válida, `imageUrl` DEBE ser una URL absoluta construida con el scheme y host del request actual y la ruta pública `/assets/properties/{fileName}`. Si no existe una imagen válida, el valor DEBE ser una cadena vacía.
- **FR-009**: La respuesta NO DEBE contener rutas físicas internas ni URLs relativas de imagen.
- **FR-010**: Los valores `page` y `pageSize` DEBEN ser enteros positivos; `pageSize` DEBE normalizarse a 24 cuando sea mayor que 24.
- **FR-011**: Los parámetros inválidos o no enteros DEBEN producir HTTP 400 con ProblemDetails o ValidationProblemDetails.
- **FR-012**: El flujo DEBE propagar `CancellationToken` desde el endpoint hasta la lectura de datos.
- **FR-013**: La lectura de datos DEBE evitar el seguimiento de entidades y NO DEBE devolver entidades EF en el contrato HTTP.
- **FR-014**: La lógica de caso de uso DEBE residir en un handler del slice y el endpoint DEBE permanecer delgado.
- **FR-015**: La construcción de URLs absolutas DEBE realizarse durante el mapping de respuesta a partir del request actual.
- **FR-016**: El endpoint NO DEBE usar controllers ni registrar endpoints individuales fuera del mecanismo ISlice.
- **FR-017**: Esta iniciativa NO DEBE agregar endpoints, entidades persistentes o funcionalidades distintas al listado paginado.
- **FR-018**: El proyecto DEBE incluir pruebas manuales HTTP para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6` en `app/backend/src/RealtorApi/RealtorApi.http`.
- **FR-019**: Las pruebas requeridas por la spec DEBEN validar defaults, orden, metadatos, páginas, validación, URLs absolutas y propagación de cancelación.

### Key Entities *(include if feature involves data)*

- **Property**: Entidad persistente existente utilizada como fuente de consulta; no se expone directamente en la respuesta.
- **ListPropertiesRequest**: Parámetros de consulta normalizados con `page` y `pageSize`.
- **ListPropertiesItemResponse**: Contrato explícito de cada propiedad devuelta.
- **ListPropertiesResponse**: Contenedor con colección y metadatos de paginación.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las consultas sin parámetros devuelve exactamente los defaults esperados y hasta 6 propiedades ordenadas por título ascendente.
- **SC-002**: El 100 % de las páginas válidas devuelve metadatos consistentes con el total de propiedades y el tamaño solicitado.
- **SC-003**: El 100 % de los parámetros inválidos probados, excluyendo `pageSize > 24` que se normaliza, produce HTTP 400 con un payload estructurado.
- **SC-004**: Las propiedades con imagen válida devuelven una `imageUrl` absoluta compatible con el scheme y host del request; las propiedades sin imagen válida devuelven cadena vacía.
- **SC-005**: Las consultas canceladas terminan sin exponer entidades EF ni resultados parciales.
- **SC-006**: `/health` y el descubrimiento existente de ISlice mantienen su comportamiento después de incorporar el slice.
- **SC-007**: Las pruebas manuales HTTP quedan disponibles para las dos URLs exigidas.

## Assumptions

- El catálogo actual mantiene al menos las propiedades seed y puede utilizarse como fuente real para la prueba funcional.
- El contrato HTTP serializa enum de estado de forma legible y consistente, sin exponer detalles de persistencia.
- `pageSize` tiene un máximo efectivo de 24; valores mayores se normalizan a 24 y valores menores o iguales a cero son inválidos.
- Las URLs públicas se construyen sobre `/assets/properties/{fileName}`; si falta el archivo o el nombre no es válido, `imageUrl` será una cadena vacía.
