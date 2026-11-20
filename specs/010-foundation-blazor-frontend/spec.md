# Especificación de la característica: Fundación del frontend web

**Rama de característica**: `010-foundation-blazor-frontend`

**Creado**: 2026-11-20

**Estado**: En implementación

**Entrada**: Descripción del usuario: "010-foundation-blazor-frontend"

## Aclaraciones

### Sesión 2026-11-20

- Q: ¿Qué alcance tiene esta iniciativa fundacional? → A: Establecer la experiencia web pública base para explorar el catálogo de propiedades y consultar su detalle, sin incorporar gestión de propiedades ni cuentas de usuario.
- Q: ¿Qué capacidades del catálogo debe exponer la fundación? → A: El listado paginado y la consulta de detalle completo ya aprobados en iniciativas anteriores.
- Q: ¿Cómo debe comportarse un visitante ante datos ausentes o fallos? → A: Ver mensajes claros en español, una acción de reintento cuando corresponda y nunca una pantalla vacía, ambigua o rota.
- Q: ¿Qué idioma y alcance de dispositivos se asume? → A: Contenido en español y uso correcto en tamaños de pantalla pequeños, medianos y grandes.
- La navegación desde el detalle debe devolver al visitante a la página del listado desde la que partió.
- La iniciativa no incluye creación, edición, eliminación, cambio de estado, autenticación, autorización ni formularios de contacto.
- Q: ¿Qué nivel de las Pautas de Accesibilidad para el Contenido Web (WCAG) debe cumplir la experiencia web para considerar completada la fundación? → A: WCAG 2.1 nivel AA como objetivo interno, sin auditoría externa.
- Q: ¿Cómo debe mostrarse al visitante el estado de una propiedad en el catálogo y en el detalle? → A: Con etiquetas en español: «Disponible», «Alquilada» o «Mantenimiento», sin exponer los valores técnicos publicados.
- Q: ¿Qué anchos de pantalla deben comprobarse para dar por válida la adaptación del catálogo y del detalle? → A: 320 px, 768 px y 1280 px.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Explorar el catálogo desde la entrada principal (Prioridad: P1)

Como visitante, quiero encontrar propiedades publicadas desde la primera pantalla, para explorar opciones sin conocer previamente el sistema.

**Por qué esta prioridad**: Es el flujo esencial y entrega valor de forma independiente; sin una entrada clara al catálogo, el resto de la experiencia web no tiene utilidad práctica.

**Prueba independiente**: Puede probarse abriendo la entrada principal con propiedades publicadas y verificando que aparece un listado paginado con la información principal de cada inmueble y controles para avanzar o retroceder.

**Escenarios de aceptación**:

1. **Dado** que existen propiedades publicadas, **Cuando** se abre la entrada principal, **Entonces** se muestra el catálogo con la primera página de resultados y sus datos de paginación.
2. **Dado** un catálogo con más de una página, **Cuando** se selecciona la página siguiente, **Entonces** cambia el conjunto de propiedades mostradas y permanece visible la navegación entre páginas.
3. **Dado** una propiedad con imagen disponible, **Cuando** se muestra en el listado, **Entonces** su tarjeta incluye título, precio, estado, dirección, dormitorios, baños, superficie e imagen principal.
4. **Dado** un catálogo con una sola página o sin resultados, **Cuando** se consulta, **Entonces** no se muestran controles de paginación engañosos.

---

### Historia de usuario 2 - Consultar el detalle completo de una propiedad (Prioridad: P1)

Como visitante, quiero abrir una propiedad desde el catálogo y ver toda su información publicada, para decidir si me interesa sin perder el contexto de mi búsqueda.

**Por qué esta prioridad**: El detalle convierte el catálogo en una herramienta útil de evaluación y permite que la fundación sirva como base para futuras experiencias de contenido.

**Prueba independiente**: Puede probarse seleccionando una tarjeta desde una página concreta del catálogo y verificando que se muestra la ficha completa y que la acción de volver restaura la página original.

**Escenarios de aceptación**:

1. **Dado** una propiedad visible en el catálogo, **Cuando** se selecciona su tarjeta, **Entonces** se abre una vista de detalle con título, descripción, dirección, precio, estado, dormitorios, baños, superficie, imagen y fechas publicadas.
2. **Dado** un detalle abierto desde la segunda página del catálogo, **Cuando** se vuelve al listado, **Entonces** se restaura la segunda página y el mismo orden de resultados.
3. **Dado** una propiedad sin imagen válida, **Cuando** se abre su detalle, **Entonces** se muestra una alternativa visual clara y el resto de la información permanece disponible.
4. **Dado** un identificador de propiedad inexistente, **Cuando** se intenta abrir su detalle, **Entonces** se muestra un mensaje comprensible en lugar de una vista vacía.

---

### Historia de usuario 3 - Comprender estados vacíos, de carga y de error (Prioridad: P2)

Como visitante, quiero recibir mensajes claros cuando no hay datos, están cargando o no pueden obtenerse, para saber qué ocurre y poder continuar.

**Por qué esta prioridad**: Los estados no satisfactorios son parte de la experiencia real y una fundación que los ignora produce pantallas confusas y pérdida de confianza.

**Prueba independiente**: Puede probarse forzando un catálogo vacío, una carga lenta y un fallo de obtención de datos, verificando en cada caso el mensaje, la acción disponible y la ausencia de pantallas vacías.

**Escenarios de aceptación**:

1. **Dado** un catálogo sin propiedades, **Cuando** se abre la entrada principal, **Entonces** se muestra un mensaje de ausencia de resultados y no se inventan tarjetas.
2. **Dado** una obtención de datos en curso, **Cuando** se espera la respuesta, **Entonces** se muestra una indicación de carga reconocible.
3. **Dado** un fallo al obtener datos, **Cuando** se muestra la vista afectada, **Entonces** aparece un mensaje de error en español con una acción para reintentar.
4. **Dado** un mensaje de error o de ausencia de resultados, **Cuando** se selecciona la acción disponible, **Entonces** la vista vuelve a intentar mostrar el contenido o regresa al catálogo sin quedar bloqueada.

---

### Historia de usuario 4 - Navegar con una experiencia coherente y accesible (Prioridad: P2)

Como visitante, quiero moverme entre el catálogo y el detalle con una presentación constante y controles utilizables con teclado, para completar mi exploración sin esfuerzo adicional.

**Por qué esta prioridad**: Una fundación web debe fijar convenciones de navegación, lenguaje y presentación antes de que aparezcan más pantallas; corregirlas después implicaría revisar todas las futuras vistas.

**Prueba independiente**: Puede probarse recorriendo catálogo, detalle y estados de error con teclado y en tres tamaños de pantalla, verificando navegación visible, foco perceptible y ausencia de desbordamiento horizontal.

**Escenarios de aceptación**:

1. **Dado** cualquiera de las vistas principales, **Cuando** se navega a otra vista, **Entonces** permanecen visibles los elementos de identificación y navegación comunes.
2. **Dado** un visitante que usa solo teclado, **Cuando** recorre los controles, **Entonces** el foco es visible y todas las acciones principales pueden ejecutarse.
3. **Dado** un ancho de 320 px, 768 px o 1280 px, **Cuando** se visualiza catálogo o detalle, **Entonces** el contenido se adapta sin recorte ni desplazamiento horizontal.
4. **Dado** un control o una imagen informativa, **Cuando** se presenta al visitante, **Entonces** cuenta con una etiqueta o texto alternativo comprensible.
5. **Dado** el contenido de cualquier vista, **Cuando** se lee, **Entonces** los textos visibles están en español.

---

## Casos límite

- Si no existen propiedades publicadas, el catálogo muestra un estado vacío y no presenta paginación ni tarjetas ficticias.
- Si una propiedad no tiene imagen válida, se muestra una alternativa visual accesible sin icono roto ni espacio en blanco inexplicable.
- Si el título, la dirección o la descripción son extensos, la presentación mantiene la legibilidad sin desbordar el contenedor.
- Si el precio o la superficie no pueden mostrarse con formato habitual, se conserva el valor publicado y se evita un aspecto roto.
- Si la obtención de datos falla, se muestra un mensaje accionable y no se deja una pantalla en blanco.
- Si la carga tarda, se muestra una indicación de progreso antes de que el visitante interprete la vista como vacía.
- Si una propiedad deja de estar disponible mientras se abre su detalle, se muestra un mensaje claro sin exponer detalles técnicos.
- Si se vuelve desde un detalle abierto desde una página concreta, se conserva esa página y el orden previo del catálogo.
- Si se usa un ancho de 320 px, la navegación y las acciones principales siguen siendo alcanzables sin desplazamiento horizontal.
- Si hay más propiedades que las caben en una página, los controles de paginación reflejan correctamente si existen páginas anterior y siguiente.
- Si el visitante navega a una vista inexistente, se muestra una respuesta comprensible y una forma de volver al catálogo.
- Si una combinación de colores, tamaño o etiquetado no alcanza los criterios internos de WCAG 2.1 nivel AA, la vista no se considera terminada hasta corregirla.
- Si un valor de estado publicado no tiene una etiqueta en español asignada, se muestra «Estado no disponible» y se conserva el resto de la información de la propiedad.
- La fundación no muestra información sensible, rutas internas ni detalles técnicos de fallos.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: La experiencia web DEBE ofrecer una entrada principal que presente el catálogo de propiedades publicadas.
- **FR-002**: El catálogo DEBE mostrarse en páginas con controles para avanzar y retroceder cuando corresponda.
- **FR-003**: El catálogo DEBE indicar la página actual, el total de páginas y la disponibilidad de páginas anterior y siguiente.
- **FR-004**: Cada propiedad del catálogo DEBE presentarse como una unidad visual independiente con título, precio, estado con etiqueta en español, dirección, dormitorios, baños, superficie e imagen o alternativa visual.
- **FR-005**: La selección de una propiedad del catálogo DEBE abrir su vista de detalle completa.
- **FR-006**: La vista de detalle DEBE mostrar título, descripción, dirección, precio, estado con etiqueta en español, dormitorios, baños, superficie, imagen o alternativa visual, fecha de creación y fecha de actualización.
- **FR-007**: La acción de volver desde el detalle DEBE restaurar la página y el orden que el visitante tenía en el catálogo.
- **FR-008**: La experiencia DEBE ofrecer navegación común para acceder al catálogo desde las vistas principales.
- **FR-009**: Mientras se obtiene contenido, la vista DEBE mostrar una indicación de carga reconocible.
- **FR-010**: Cuando no haya propiedades publicadas, el catálogo DEBE mostrar un estado vacío claro y NO DEBE mostrar datos ficticios.
- **FR-011**: Cuando falle la obtención de datos, la vista DEBE mostrar un mensaje de error comprensible y una acción de reintento.
- **FR-012**: La experiencia NO DEBE dejar pantallas vacías, controles inactivos sin explicación ni imágenes rotas como resultado de datos ausentes.
- **FR-013**: Todos los textos visibles y mensajes dirigidos al visitante DEBEN estar en español.
- **FR-014**: Las acciones principales DEBEN ser alcanzables y ejecutables mediante teclado, con indicación visible de foco, conforme a WCAG 2.1 nivel AA.
- **FR-015**: Las imágenes informativas y los controles sin texto propio DEBEN incluir alternativas comprensibles, conforme a WCAG 2.1 nivel AA.
- **FR-016**: El contenido DEBE adaptarse a 320 px, 768 px y 1280 px sin recorte ni desplazamiento horizontal.
- **FR-017**: El catálogo, el detalle y los estados vacío, de carga y de error DEBEN compartir una presentación visual coherente.
- **FR-018**: La experiencia DEBE seguir las convenciones visuales y de iconografía ya aprobadas para el proyecto sin introducir un lenguaje visual paralelo.
- **FR-019**: La fundación NO DEBE crear ni modificar propiedades, cambiar estados, gestionar cuentas ni incorporar formularios de contacto.
- **FR-020**: La fundación NO DEBE redefinir reglas de negocio del catálogo; DEBE presentar únicamente la información publicada por las capacidades existentes.
- **FR-021**: La fundación NO DEBE exponer rutas internas, mensajes técnicos ni datos sensibles al visitante.
- **FR-022**: Las vistas inexistentes DEBEN ofrecer un mensaje comprensible y una forma de regresar al catálogo.
- **FR-023**: El catálogo y el detalle DEBEN mostrar el estado con las etiquetas en español «Disponible», «Alquilada» o «Mantenimiento», sin exponer los valores técnicos publicados.

### Entidades clave *(incluir si la característica implica datos)*

- **Propiedad**: Información publicada de un inmueble, compuesta por identificador, título, descripción, dirección, precio, estado mostrado con etiqueta en español, dormitorios, baños, superficie, imagen opcional y fechas de creación y actualización.
- **Catálogo publicado**: Conjunto ordenado de propiedades disponibles para exploración, organizado en páginas con metadatos de navegación.
- **Tarjeta de propiedad**: Unidad visual del catálogo que resume los atributos principales de una propiedad y permite abrir su detalle.
- **Detalle de propiedad**: Vista que presenta toda la información publicada de una propiedad y permite regresar al catálogo anterior.
- **Estado de presentación**: Situación visible de una vista, entre carga, contenido, ausencia de resultados, error o ruta inexistente.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las propiedades publicadas en el catálogo tiene una tarjeta visible y una vista de detalle alcanzable desde ella.
- **SC-002**: El 100 % de las tarjetas muestra título, precio, estado, dirección, dormitorios, baños, superficie e imagen o alternativa visual.
- **SC-003**: El 90 % de los participantes en una prueba de uso completa sin ayuda la tarea «encontrar una propiedad y abrir su detalle» en menos de 2 minutos.
- **SC-004**: El 100 % de los detalles abiertos muestra todos los campos publicados por la propiedad correspondiente.
- **SC-005**: El 100 % de las propiedades sin imagen válida muestra una alternativa visual accesible y ninguna imagen rota.
- **SC-006**: El 100 % de los fallos de obtención de datos probados muestra un mensaje en español y una acción de reintento sin dejar una pantalla vacía.
- **SC-007**: El 100 % de los estados vacíos probados muestra un mensaje comprensible y evita tarjetas o controles engañosos.
- **SC-008**: El 100 % de las acciones principales puede completarse usando solo teclado y con foco visible.
- **SC-009**: Catálogo y detalle se visualizan sin desplazamiento horizontal ni recorte a 320 px, 768 px y 1280 px.
- **SC-010**: El 95 % de las cargas iniciales probadas muestra el primer contenido útil en menos de 3 segundos en condiciones normales de uso.
- **SC-011**: El 100 % del contenido visible dirigido al visitante está en español.
- **SC-012**: La creación, edición, eliminación, cambio de estado, autenticación, autorización y formularios de contacto permanecen fuera del alcance de esta iniciativa.
- **SC-013**: El 100 % de las vistas y estados principales supera las comprobaciones internas de WCAG 2.1 nivel AA aplicables a navegación por teclado, foco visible, contraste y alternativas textuales, sin auditoría externa.
- **SC-014**: El 100 % de las tarjetas y detalles muestra el estado usando exactamente «Disponible», «Alquilada» o «Mantenimiento», y ninguno de ellos expone los valores técnicos publicados.

## Supuestos

- La iniciativa depende de las capacidades aprobadas en `001-solution-foundation`, `004-properties-list-pagination`, `007-properties-get-by-id`, `008-open-api` y `009-update-status`.
- El catálogo y el detalle son públicos; la autenticación y la autorización quedan fuera del alcance.
- La información de propiedades ya existe y esta iniciativa solo la presenta al visitante.
- El visitante utiliza un navegador web moderno con conexión razonablemente estable; la verificación visual usa 320 px, 768 px y 1280 px.
- Los textos visibles del producto se entregarán en español; los identificadores técnicos pueden conservarse cuando no sean visibles para el visitante.
- La identidad visual y la iconografía aprobadas para el proyecto se consideran entradas de esta iniciativa, no decisiones nuevas.
- Los valores de estado publicados se presentan al visitante con las etiquetas en español «Disponible», «Alquilada» o «Mantenimiento», sin modificar la información publicada.
- La ausencia de imagen se considera un caso normal y debe resolverse con una alternativa visual.
- No se incluyen optimización avanzada para motores de búsqueda, analítica, internacionalización, modo sin conexión ni certificación externa de accesibilidad; el objetivo interno es WCAG 2.1 nivel AA.
- No se incluye gestión de propiedades, cuentas de usuario ni formularios de contacto.
- La verificación de esta fundación se centra en la experiencia del visitante y en la ausencia de regresiones en las capacidades ya aprobadas.
