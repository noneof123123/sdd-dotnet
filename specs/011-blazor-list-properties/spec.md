# Especificación de la característica: Listado paginado de propiedades en la página principal

**Rama de característica**: `011-blazor-list-properties`

**Creado**: 2026-11-21

**Estado**: En implementación

**Entrada**: Descripción del usuario: "011-blazor-list-properties"

## Aclaraciones

### Sesión 2026-11-21

- P: ¿Qué página debe mostrarse cuando un visitante solicita un número de página superior a la última página disponible? → R: Mostrar la última página disponible con sus propiedades y metadatos coherentes.
- P: ¿Qué significa «criterios internos de accesibilidad»? → R: foco visible, etiquetas accesibles en español, contraste WCAG 2.1 AA, alternativas textuales y ausencia de desplazamiento horizontal a 320 px.
- P: ¿Qué significa «presentación visual coherente»? → R: los estados usan los mismos tokens de `app.css`, el mismo componente de presentación y ningún estilo inline o framework CSS adicional.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Explorar propiedades desde la entrada principal (Prioridad: P1)

Como visitante, quiero abrir la página principal y ver de inmediato un listado de propiedades publicadas, para comenzar a explorar el catálogo sin realizar ninguna acción previa.

**Por qué esta prioridad**: Es el flujo esencial de la experiencia pública; sin un listado visible en la entrada principal, el resto de la navegación del catálogo carece de utilidad.

**Prueba independiente**: Puede probarse abriendo la página principal con propiedades publicadas y verificando que aparece una primera página de resultados con los datos principales de cada inmueble, sin depender de otras vistas.

**Escenarios de aceptación**:

1. **Dado** que existen propiedades publicadas, **Cuando** se abre la página principal, **Entonces** se muestra el listado de la primera página con los datos principales de cada propiedad.
2. **Dado** que la página principal está disponible, **Cuando** se consulta sin indicar una página concreta, **Entonces** se muestra la primera página del catálogo.
3. **Dado** un listado con propiedades, **Cuando** se revisa una propiedad mostrada, **Entonces** se pueden identificar su título, precio, estado, dirección, dormitorios, baños, superficie e imagen o alternativa visual.
4. **Dado** que una propiedad no tiene imagen válida, **Cuando** se muestra en el listado, **Entonces** aparece una alternativa visual comprensible y el resto de sus datos permanecen visibles.

---

### Historia de usuario 2 - Navegar por las páginas del catálogo (Prioridad: P1)

Como visitante, quiero avanzar y retroceder entre páginas y saber en cuál estoy, para recorrer todas las propiedades sin perder el contexto de mi exploración.

**Por qué esta prioridad**: Convierte una lista estática en un catálogo navegable y permite orientarse dentro de un conjunto de resultados extenso.

**Prueba independiente**: Puede probarse con un catálogo de varias páginas, seleccionando «Siguiente» y «Anterior» y comprobando que cambian los resultados, los metadatos y la página reflejada en la dirección.

**Escenarios de aceptación**:

1. **Dado** un catálogo con más propiedades que las caben en una página, **Cuando** se abre la primera página, **Entonces** se muestran los controles para avanzar y el control para retroceder permanece deshabilitado.
2. **Dado** un catálogo con páginas posteriores, **Cuando** se selecciona «Siguiente», **Entonces** se muestra la página siguiente con sus propiedades y metadatos actualizados.
3. **Dado** un catálogo con páginas anteriores, **Cuando** se selecciona «Anterior», **Entonces** se muestra la página anterior con sus propiedades y metadatos actualizados.
4. **Dado** un catálogo de una sola página, **Cuando** se consulta, **Entonces** ambos controles de navegación permanecen deshabilitados y no sugieren acciones disponibles.
5. **Dado** un cambio de página, **Cuando** se recarga la vista, **Entonces** se conserva la página seleccionada y se vuelve a mostrar el mismo conjunto de resultados.

---

### Historia de usuario 3 - Entender los estados del listado (Prioridad: P2)

Como visitante, quiero recibir mensajes claros cuando el catálogo está cargando, está vacío o no puede obtenerse, para saber qué ocurre y poder continuar.

**Por qué esta prioridad**: Los estados no satisfactorios forman parte del uso real; ignorarlos produce pantallas ambiguas y pérdida de confianza en el catálogo.

**Prueba independiente**: Puede probarse forzando una carga lenta, un catálogo sin resultados y un fallo de obtención de datos, verificando en cada caso el mensaje, la acción ofrecida y la ausencia de una pantalla vacía.

**Escenarios de aceptación**:

1. **Dado** una obtención de datos en curso, **Cuando** se espera la respuesta, **Entonces** se muestra una indicación de carga reconocible antes de mostrar resultados.
2. **Dado** un catálogo sin propiedades, **Cuando** se abre la página principal, **Entonces** se muestra un mensaje de ausencia de resultados y no se presentan propiedades ficticias ni paginación engañosa.
3. **Dado** un fallo al obtener el catálogo, **Cuando** se muestra la página afectada, **Entonces** aparece un mensaje de error comprensible y una acción para reintentar.
4. **Dado** un mensaje de error, **Cuando** se selecciona la acción de reintento, **Entonces** se intenta mostrar el catálogo nuevamente sin dejar la vista bloqueada.

---

### Historia de usuario 4 - Utilizar un listado claro, adaptable y accesible (Prioridad: P2)

Como visitante, quiero que el listado sea legible en distintos tamaños de pantalla y utilizable con teclado, para explorar las propiedades sin esfuerzo adicional.

**Por qué esta prioridad**: Fija convenciones de presentación y accesibilidad antes de que aparezcan más vistas, evitando correcciones posteriores en toda la experiencia.

**Prueba independiente**: Puede probarse recorriendo el listado y sus controles solo con teclado y en tres tamaños de pantalla, verificando foco visible, etiquetas comprensibles y ausencia de desbordamiento horizontal.

**Escenarios de aceptación**:

1. **Dado** un ancho de 1280 px, **Cuando** se visualiza el catálogo, **Entonces** las propiedades se organizan en tres columnas sin recorte.
2. **Dado** un ancho de 768 px, **Cuando** se visualiza el catálogo, **Entonces** las propiedades se organizan en dos columnas sin recorte.
3. **Dado** un ancho de 320 px, **Cuando** se visualiza el catálogo, **Entonces** las propiedades se organizan en una columna y los controles siguen siendo alcanzables sin desplazamiento horizontal.
4. **Dado** un visitante que usa solo teclado, **Cuando** recorre los controles de paginación y reintento, **Entonces** el foco es visible y las acciones pueden ejecutarse.
5. **Dado** el contenido visible del listado, **Cuando** se lee, **Entonces** todos los textos dirigidos al visitante están en español y los estados se muestran con etiquetas en español.

---

## Casos límite

- Si no existen propiedades publicadas, se muestra el estado vacío y no se inventan tarjetas ni controles de paginación utilizables.
- Si el número de página indicado no es válido, se muestra la primera página del catálogo; si supera la última página existente, se muestra la última página disponible con metadatos coherentes y sin una vista vacía.
- Si una propiedad no tiene imagen válida, se presenta una alternativa visual accesible sin imagen rota ni espacio inexplicable.
- Si el título, la dirección o la descripción son extensos, la presentación conserva la legibilidad sin desbordar la tarjeta.
- Si el precio o la superficie no pueden mostrarse con el formato habitual, se conserva el valor publicado y se evita una presentación rota.
- Si la obtención de datos falla, se ofrece un mensaje accionable y no se expone el motivo técnico del fallo.
- Si la carga tarda, se muestra una indicación de progreso antes de que la vista parezca vacía.
- Si el estado publicado de una propiedad no tiene una etiqueta asignada, se muestra «Estado no disponible» y se conserva el resto de la información.
- Si la última página tiene menos propiedades que el tamaño de página, se muestran solo las existentes y los metadatos reflejan el total real.
- Si se usa un ancho de 320 px, la navegación, el listado y las acciones principales siguen siendo alcanzables sin desplazamiento horizontal.
- Si una combinación de colores, tamaño o etiquetado no alcanza los criterios internos de accesibilidad, la vista no se considera terminada hasta corregirla.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: La página principal DEBE presentar el catálogo de propiedades publicadas como su contenido principal.
- **FR-002**: El catálogo DEBE mostrar como máximo 12 propiedades por página, ajustándose al total realmente disponible en la última página.
- **FR-003**: Cada página DEBE corresponder a un número de página concreto y ese número DEBE reflejarse en la dirección de la vista para poder recargarla o compartirla.
- **FR-004**: Cuando no se indique una página, el catálogo DEBE mostrar la primera página.
- **FR-005**: Un número de página ausente, vacío, no numérico, cero, negativo o con desbordamiento DEBE resolverse a la primera página; un número superior a la última página disponible DEBE resolverse a la última página, mostrando sus propiedades y metadatos coherentes y sin dejar una vista vacía.
- **FR-006**: Cada propiedad del catálogo DEBE presentarse como una unidad visual independiente con título, precio, estado con etiqueta en español, dirección, dormitorios, baños, superficie e imagen o alternativa visual.
- **FR-007**: El catálogo DEBE indicar la página actual, el total de páginas y el total de elementos disponibles.
- **FR-008**: El catálogo DEBE ofrecer controles para ir a la página anterior y a la siguiente.
- **FR-009**: El control «Anterior» DEBE permanecer deshabilitado cuando no exista una página anterior y el control «Siguiente» DEBE permanecer deshabilitado cuando no exista una página siguiente.
- **FR-010**: Al cambiar de página, DEBEN actualizarse las propiedades mostradas, los metadatos de paginación y la página reflejada en la dirección.
- **FR-011**: Mientras se obtiene el contenido, la vista DEBE mostrar una indicación de carga reconocible.
- **FR-012**: Cuando no haya propiedades publicadas, la vista DEBE mostrar un estado vacío claro y NO DEBE mostrar datos ficticios ni controles de paginación engañosos.
- **FR-013**: Cuando falle la obtención de datos, la vista DEBE mostrar un mensaje de error comprensible en español y una acción de reintento.
- **FR-014**: Los estados de carga, ausencia de resultados, error y contenido DEBEN compartir una presentación visual coherente con el resto de la experiencia.
- **FR-015**: Todos los textos visibles dirigidos al visitante DEBEN estar en español, sin exponer valores técnicos de estado, rutas internas ni detalles de fallos.
- **FR-016**: La grilla de propiedades DEBE mostrar una columna a 320 px, dos columnas a 768 px y tres columnas a 1280 px, sin recorte ni desplazamiento horizontal.
- **FR-017**: Las acciones principales DEBEN ser alcanzables y ejecutables mediante teclado, con indicación visible de foco y alternativas textuales comprensibles, conforme a WCAG 2.1 nivel AA como objetivo interno.
- **FR-018**: La iniciativa NO DEBE incluir creación, edición, eliminación ni cambio de estado de propiedades, filtros, búsqueda avanzada ni ordenación adicional.
- **FR-019**: La iniciativa NO DEBE modificar la consulta de detalle, la persistencia, el dominio ni las reglas de negocio ya aprobadas para el catálogo.
- **FR-020**: La iniciativa DEBE limitarse a presentar la información publicada por las capacidades existentes y a permitir la navegación entre sus páginas.

### Entidades clave *(incluir si la característica implica datos)*

- **Propiedad publicada**: Información de un inmueble disponible para su exploración, con título, descripción, dirección, precio, estado, dormitorios, baños, superficie, imagen opcional y fechas de publicación.
- **Página del catálogo**: Conjunto de propiedades correspondiente a un número de página y a un tamaño de página determinados.
- **Metadatos de paginación**: Información que permite situar al visitante dentro del catálogo: número de página actual, tamaño de página, total de elementos, total de páginas y existencia de páginas anterior y siguiente.
- **Tarjeta de propiedad**: Unidad visual que resume los datos principales de una propiedad dentro del listado.
- **Estado de presentación**: Situación visible del listado, entre carga, contenido, ausencia de resultados y error.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las propiedades de la página solicitada cumple FR-006 sin omisiones.
- **SC-002**: El 100 % de las páginas probadas cumple FR-007 con valores coherentes entre resumen, total y controles.
- **SC-003**: El 100 % de los controles «Anterior» y «Siguiente» refleja correctamente la existencia de páginas adicionales en los casos probados.
- **SC-004**: Con 10 participantes en una prueba de uso local, al menos 9 localizan una propiedad y avanzan a la página siguiente sin ayuda en menos de 1 minuto, según el protocolo de `quickstart.md`.
- **SC-005**: El 100 % de los estados probados cumple FR-011 a FR-014 y nunca deja una pantalla vacía o ambigua.
- **SC-006**: El 100 % de los fallos de obtención probados ofrece una acción de reintento que permite volver a intentar la consulta sin recargar manualmente la experiencia.
- **SC-007**: El 100 % de las acciones principales puede completarse usando solo teclado, con foco visible y alternativas textuales, conforme a las comprobaciones internas de WCAG 2.1 nivel AA.
- **SC-008**: El catálogo se visualiza a 320 px, 768 px y 1280 px con una, dos y tres columnas respectivamente, sin desplazamiento horizontal ni recorte.
- **SC-009**: El 100 % del contenido visible dirigido al visitante está en español y ningún estado expone valores técnicos ni detalles internos.
- **SC-010**: Con 20 cargas frías de `/` repartidas entre 320, 768 y 1280 px, al menos 19 muestran el primer contenido útil en menos de 3 segundos. «Primer contenido útil» significa la primera tarjeta, el estado vacío o el error; «condiciones normales de uso» significa backend y frontend locales activos, sin navegador real, según el protocolo de `quickstart.md`.
- **SC-011**: El 100 % de los estados mostrados utiliza las etiquetas en español «Disponible», «Alquilada» o «Mantenimiento» cuando corresponde.
- **SC-012**: La creación, edición, eliminación, cambio de estado, filtros avanzados, búsqueda avanzada, ordenación adicional y cambios en backend, persistencia o dominio permanecen fuera del alcance de esta iniciativa.

## Supuestos

- La iniciativa depende de las capacidades aprobadas en `004-properties-list-pagination`, `008-open-api` y `010-foundation-blazor-frontend`.
- Esta especificación detalla el listado paginado de la página principal como una porción independiente del catálogo; no modifica el alcance aprobado de iniciativas anteriores y reutiliza las convenciones visuales, de navegación y de etiquetado ya definidas.
- El tamaño de página se fija en 12 propiedades para la experiencia web, como continuación del comportamiento ya establecido para la página principal.
- El visitante utiliza un navegador web moderno con conexión razonablemente estable; la verificación visual se realiza a 320 px, 768 px y 1280 px.
- Los textos visibles del producto se entregan en español; los identificadores técnicos pueden conservarse cuando no sean visibles para el visitante.
- La identidad visual, el sistema de iconografía y el sistema de estilos centralizados aprobados para el proyecto son entradas de esta iniciativa, no decisiones nuevas; las decisiones de implementación se documentan en la fase de planificación.
- Los valores de estado publicados se presentan al visitante con las etiquetas en español «Disponible», «Alquilada» o «Mantenimiento», sin modificar la información publicada.
- La ausencia de imagen se considera un caso normal y debe resolverse con una alternativa visual accesible.
- No se incluyen autenticación, autorización, gestión de cuentas, formularios de contacto, analítica, internacionalización, modo sin conexión ni certificación externa de accesibilidad; el objetivo interno es WCAG 2.1 nivel AA.
- La verificación se centra en la experiencia del visitante dentro del listado y en la ausencia de regresiones en las capacidades ya aprobadas.
