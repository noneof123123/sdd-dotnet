# Especificación de la característica: Detalle de propiedad en la experiencia web

**Rama de característica**: `012-blazor-property-detail`

**Creado**: 2026-11-21

**Estado**: En implementación

**Entrada**: Descripción del usuario: "012-blazor-property-detail"

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Consultar el detalle completo de una propiedad (Prioridad: P1)

Como visitante, quiero abrir una propiedad desde el catálogo y ver toda su información relevante en un solo lugar, para decidir si me interesa sin volver a buscar entre tarjetas resumidas.

**Por qué esta prioridad**: Es el valor central de la iniciativa; sin una vista de detalle completa y comprensible, el catálogo solo permite descubrir inmuebles pero no evaluarlos.

**Prueba independiente**: Puede probarse abriendo la vista de detalle de una propiedad publicada y verificando que presenta todos los datos principales, la imagen o su alternativa y la descripción completa, sin depender de otras vistas.

**Escenarios de aceptación**:

1. **Dado** un catálogo con propiedades publicadas, **Cuando** se selecciona una propiedad, **Entonces** se abre su detalle con título, precio, estado, dirección, dormitorios, baños, superficie, imagen o alternativa y descripción completa.
2. **Dado** un detalle disponible, **Cuando** se consulta su dirección directamente, **Entonces** se muestra la misma propiedad y la misma información que al llegar desde el catálogo.
3. **Dado** que una propiedad tiene fecha de publicación o actualización, **Cuando** se consulta su detalle, **Entonces** esas fechas se muestran con un formato comprensible en español.
4. **Dado** que una propiedad no tiene imagen válida, **Cuando** se consulta su detalle, **Entonces** se muestra una alternativa textual comprensible y el resto de la información permanece visible.
5. **Dado** que una propiedad tiene un estado sin etiqueta asignada, **Cuando** se consulta su detalle, **Entonces** se muestra «Estado no disponible» y se conserva el resto de la información.

---

### Historia de usuario 2 - Volver al catálogo conservando el contexto (Prioridad: P1)

Como visitante, quiero volver al catálogo desde el detalle y recuperar la página que estaba consultando, para continuar mi exploración donde la dejé.

**Por qué esta prioridad**: Evita que la persona pierda su lugar en una lista extensa y hace que la navegación entre resumen y detalle sea continua en lugar de empezar desde cero.

**Prueba independiente**: Puede probarse abriendo una propiedad desde una página concreta del catálogo, regresando mediante la acción de vuelta y comprobando que se vuelve a mostrar esa página con sus propiedades y metadatos.

**Escenarios de aceptación**:

1. **Dado** que se abre una propiedad desde la página dos del catálogo, **Cuando** se selecciona «Volver al catálogo», **Entonces** se muestra nuevamente la página dos con sus propiedades y metadatos.
2. **Dado** que se abre un detalle mediante una dirección compartida sin contexto de página, **Cuando** se selecciona «Volver al catálogo», **Entonces** se muestra la primera página del catálogo.
3. **Dado** un contexto de página no válido, **Cuando** se regresa al catálogo, **Entonces** se muestra la primera página sin mensajes técnicos ni una vista vacía.
4. **Dado** un contexto de página superior a la última disponible, **Cuando** se regresa al catálogo, **Entonces** se muestra la última página disponible con metadatos coherentes.

---

### Historia de usuario 3 - Entender los estados del detalle (Prioridad: P2)

Como visitante, quiero mensajes claros cuando el detalle está cargando, no existe o no puede obtenerse, para saber qué ocurre y poder continuar.

**Por qué esta prioridad**: Los fallos y las ausencias son parte del uso real; una vista ambigua o en blanco reduce la confianza y deja a la persona sin una acción siguiente.

**Prueba independiente**: Puede probarse forzando una carga lenta, una propiedad inexistente y un fallo de obtención, verificando en cada caso el mensaje, la acción ofrecida y la ausencia de datos parciales engañosos.

**Escenarios de aceptación**:

1. **Dado** una obtención en curso, **Cuando** se espera el detalle, **Entonces** se muestra una indicación de carga reconocible antes de mostrar datos parciales.
2. **Dado** un identificador que no corresponde a una propiedad publicada, **Cuando** se abre su detalle, **Entonces** se muestra un mensaje de no encontrado en español y una acción para volver al catálogo.
3. **Dado** un fallo al obtener el detalle, **Cuando** se abre la vista afectada, **Entonces** aparece un mensaje de error comprensible y una acción para reintentar.
4. **Dado** un mensaje de error, **Cuando** se selecciona la acción de reintento, **Entonces** se intenta mostrar el mismo detalle nuevamente sin exigir una recarga manual.
5. **Dado** cualquiera de estos estados, **Cuando** se lee el mensaje, **Entonces** no se exponen motivos técnicos, rutas internas ni valores técnicos.

---

### Historia de usuario 4 - Utilizar un detalle claro, adaptable y accesible (Prioridad: P2)

Como visitante, quiero leer y recorrer el detalle en distintos tamaños de pantalla y con teclado, para consultar la información sin esfuerzo adicional.

**Por qué esta prioridad**: Fija convenciones de presentación y accesibilidad coherentes con el catálogo antes de ampliar otras vistas, evitando correcciones posteriores.

**Prueba independiente**: Puede probarse recorriendo el detalle solo con teclado y consultándolo a 320, 768 y 1280 px, verificando foco visible, etiquetas comprensibles y ausencia de desplazamiento horizontal.

**Escenarios de aceptación**:

1. **Dado** un ancho de 1280 px, **Cuando** se visualiza el detalle, **Entonces** la información se organiza con el texto y la imagen sin recorte ni solapamiento.
2. **Dado** un ancho de 768 px, **Cuando** se visualiza el detalle, **Entonces** la información sigue siendo legible y las acciones permanecen visibles.
3. **Dado** un ancho de 320 px, **Cuando** se visualiza el detalle, **Entonces** el contenido se adapta a una sola columna y no aparece desplazamiento horizontal.
4. **Dado** un visitante que usa solo teclado, **Cuando** recorre las acciones del detalle, **Entonces** el foco es visible y las acciones pueden ejecutarse.
5. **Dado** el contenido visible del detalle, **Cuando** se lee, **Entonces** todos los textos dirigidos al visitante están en español, incluidas las etiquetas de estado y las alternativas de imagen.

---

## Casos límite

- Si el identificador de la propiedad está ausente o no tiene un formato reconocible, se muestra el estado de no encontrado sin exponer el valor técnico.
- Para esta iniciativa, una propiedad publicada es aquella presente en la respuesta pública del catálogo o del endpoint de detalle; una propiedad ausente de esa respuesta pública se trata como no encontrada para el visitante.
- Si la propiedad no tiene imagen válida, se presenta una alternativa textual accesible sin imagen rota ni espacio inexplicable.
- Si la descripción, el título o la dirección son extensos, la presentación conserva la legibilidad sin recortar información relevante.
- Si el precio o la superficie no pueden mostrarse con el formato habitual, se conserva el valor publicado y se evita una presentación rota.
- Si no hay fecha de actualización, se muestra una indicación comprensible de ausencia sin dejar un hueco sin explicación.
- Si el contexto de página es inválido, se aplica la primera página del catálogo; si supera la última página disponible, se aplica la última página efectiva.
- Si la obtención falla, se ofrece una acción de reintento que conserva la propiedad solicitada y el contexto de retorno.
- Si la carga tarda, se muestra una indicación de progreso antes de que la vista parezca vacía.
- Si se usa un ancho de 320 px, la imagen, el texto y las acciones principales siguen siendo alcanzables sin desplazamiento horizontal.
- Si una combinación de colores, tamaño o etiquetado no alcanza los criterios internos de accesibilidad, la vista no se considera terminada hasta corregirla.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: Cada propiedad publicada DEBE tener una vista de detalle accesible desde su representación en el catálogo.
- **FR-002**: La vista de detalle DEBE presentar título, descripción completa, dirección, precio, estado con etiqueta en español, dormitorios, baños, superficie, imagen o alternativa textual y fechas de publicación y actualización cuando estén disponibles.
- **FR-003**: Cada detalle DEBE tener una dirección estable y compartible que identifique de forma inequívoca la propiedad solicitada.
- **FR-004**: Cuando se abre un detalle desde el catálogo, la vista DEBE conservar la página de origen como contexto de retorno.
- **FR-005**: La vista de detalle DEBE ofrecer una acción «Volver al catálogo» que restaure la página de origen y vuelva a mostrar sus propiedades y metadatos.
- **FR-006**: Cuando no exista contexto de página, el retorno DEBE mostrar la primera página del catálogo.
- **FR-007**: Un contexto de página ausente, vacío, no numérico, cero o negativo DEBE resolverse a la primera página. Un contexto numérico positivo superior a la última página disponible DEBE resolverse a la última página, sin dejar una vista vacía.
- **FR-008**: Mientras se obtiene el contenido, la vista DEBE mostrar una indicación de carga reconocible.
- **FR-009**: Cuando el identificador solicitado no corresponda a una propiedad presente en la respuesta pública del catálogo o del endpoint de detalle, la vista DEBE mostrar un mensaje de no encontrado en español y una acción para volver al catálogo.
- **FR-010**: Cuando falle la obtención del detalle, la vista DEBE mostrar un mensaje de error comprensible en español y una acción de reintento para la misma propiedad.
- **FR-011**: La acción de reintento DEBE permitir volver a intentar la consulta sin exigir una recarga manual de la experiencia.
- **FR-012**: Los estados de carga, contenido, no encontrado y error DEBEN compartir una presentación visual coherente con el catálogo.
- **FR-013**: Todos los textos visibles dirigidos al visitante DEBEN estar en español, sin exponer valores técnicos, rutas internas ni detalles de fallos.
- **FR-014**: Las etiquetas de estado DEBEN ser exactamente «Disponible», «Alquilada» o «Mantenimiento» cuando corresponda; en otro caso DEBE mostrarse «Estado no disponible».
- **FR-015**: El detalle DEBE ofrecer alternativas textuales comprensibles para la imagen y permitir completar todas sus acciones usando solo teclado, con indicación visible de foco, conforme a WCAG 2.1 nivel AA como objetivo interno.
- **FR-016**: El contenido DEBE permanecer legible y sin desplazamiento horizontal a 320 px, 768 px y 1280 px, adaptándose a una sola columna en 320 px.
- **FR-017**: La iniciativa NO DEBE permitir crear, editar, eliminar ni cambiar el estado de propiedades, ni añadir formularios de contacto, filtros, búsqueda avanzada u ordenación adicional.
- **FR-018**: La iniciativa NO DEBE alterar la información publicada, las reglas ya aprobadas ni el comportamiento del catálogo existente.

### Entidades clave *(incluir si la característica implica datos)*

- **Propiedad publicada**: Inmueble presente en la respuesta pública de la API y disponible para su consulta pública, con título, descripción, dirección, precio, estado, dormitorios, baños, superficie, imagen opcional y fechas de publicación y actualización.
- **Detalle de propiedad**: Presentación completa de una propiedad publicada, diferenciada del resumen mostrado en el catálogo.
- **Contexto de retorno**: Página del catálogo desde la que se abrió el detalle, utilizada para restaurar la exploración al volver.
- **Estado de presentación**: Situación visible del detalle, entre carga, contenido, no encontrado y error.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las propiedades publicadas probadas muestra el detalle completo definido en FR-002 sin omisiones.
- **SC-002**: El 100 % de las direcciones de detalle probadas abre la propiedad correcta, tanto desde el catálogo como de forma directa.
- **SC-003**: El 100 % de los recorridos probados desde una página concreta del catálogo regresa a esa misma página con sus propiedades y metadatos.
- **SC-004**: Con 10 participantes en una prueba de uso local, al menos 9 localizan una propiedad desde el catálogo, consultan su detalle y vuelven a la página de origen sin ayuda en menos de 2 minutos, según el protocolo de `quickstart.md`.
- **SC-005**: El 100 % de los estados probados cumple FR-008 a FR-012 y nunca deja una vista vacía, ambigua o con datos parciales engañosos.
- **SC-006**: El 100 % de los fallos probados ofrece una acción de reintento que permite consultar de nuevo el mismo detalle sin recarga manual.
- **SC-007**: El 100 % de las acciones del detalle puede completarse usando solo teclado, con foco visible y alternativas textuales, conforme a las comprobaciones internas de WCAG 2.1 nivel AA.
- **SC-008**: El detalle se visualiza a 320 px, 768 px y 1280 px con una sola columna en 320 px, sin desplazamiento horizontal ni recorte.
- **SC-009**: El 100 % del contenido visible dirigido al visitante está en español y ningún estado expone valores técnicos ni detalles internos.
- **SC-010**: Con 20 cargas frías de una dirección de detalle repartidas entre 320, 768 y 1280 px, al menos 19 muestran el primer contenido útil en menos de 3 segundos. «Primer contenido útil» significa la información principal del detalle, el estado de no encontrado o el error; «condiciones normales de uso» significa la experiencia y sus datos de prueba disponibles localmente, según el protocolo de `quickstart.md`.
- **SC-011**: La creación, edición, eliminación, cambio de estado, formularios de contacto, filtros avanzados, búsqueda avanzada, ordenación adicional y alteraciones del catálogo existente permanecen fuera del alcance de esta iniciativa.

## Supuestos

- Dependencias: catálogo aprobado, información publicada de propiedades, convenciones visuales ya definidas y protocolos de verificación documentados en `quickstart.md`.
- Esta especificación detalla la consulta de detalle como una porción independiente del catálogo; no modifica el alcance aprobado de iniciativas anteriores y reutiliza sus convenciones visuales, de navegación y de etiquetado.
- La información de las propiedades publicadas y la existencia del catálogo son entradas de esta iniciativa.
- Cada propiedad publicada cuenta con una referencia estable que permite construir una dirección compartible para su detalle.
- El contexto de retorno puede transportarse junto con la dirección del detalle y se considera una ayuda de navegación, no información nueva sobre la propiedad.
- Los textos visibles del producto se entregan en español; los identificadores técnicos pueden conservarse cuando no sean visibles para el visitante.
- La identidad visual aprobada para el proyecto es una entrada de esta iniciativa, no una decisión nueva.
- La ausencia de imagen o de fecha de actualización se considera un caso normal y debe resolverse con una indicación comprensible.
- El objetivo interno de accesibilidad es WCAG 2.1 nivel AA; no se incluye certificación externa.
- No se incluyen autenticación, autorización, gestión de cuentas, analítica, internacionalización, modo sin conexión ni integraciones de terceros.
- La verificación se centra en la experiencia del visitante dentro del detalle y en la ausencia de regresiones en las capacidades ya aprobadas.
