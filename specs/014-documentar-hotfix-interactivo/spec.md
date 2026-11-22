# Especificación de la característica: Documentación retrospectiva del hotfix interactivo

**Rama de característica**: `hotfix/import-razor-utils`

**Creado**: 2026-11-22

**Estado**: Implementada

**Entrada**: Descripción del usuario: "Documenta retrospectivamente el hotfix que ya está implementado. Analiza el diff actual de Git, las pruebas relacionadas y el comportamiento observable de la aplicación. La especificación debe describir qué problema resolvemos, el comportamiento esperado, los escenarios principales, los casos límite y los criterios de aceptación. No propongas una nueva implementación y no modifiques el código. No copies detalles técnicos dentro de los requisitos funcionales."

## Clarificaciones

### Sesión 2026-11-22

- Q: ¿Qué alcance debe documentar la especificación retrospectiva? → A: El cambio interactivo completo ya implementado, incluida la navegación entre catálogo y detalle, la recuperación ante interrupciones, los estados de espera y error, la accesibilidad y el registro interno de incidencias.
- Q: ¿Dónde debe escribirse la especificación? → A: En una nueva especificación `014-documentar-hotfix-interactivo`, sin modificar la especificación 013 ni el código existente.
- Nota: Este documento es retrospectivo. Describe el comportamiento ya implementado y criterios verificables; no propone trabajo adicional ni cambios futuros.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Recorrer el catálogo y el detalle sin perder el contexto (Prioridad: P1)

Como visitante, quiero moverme entre el catálogo y el detalle de una propiedad con respuestas inmediatas, para continuar mi consulta sin perder mi lugar ni repetir pasos.

**Por qué esta prioridad**: Es el recorrido principal de la experiencia. Si una acción obliga a recargar la página o pierde la página visitada, el visitante pierde su lugar y puede abandonar la búsqueda.

**Prueba independiente**: Puede probarse abriendo el catálogo, cambiando de página, abriendo una propiedad y volviendo al catálogo, verificando que la respuesta es inmediata y que se conservan la página y la propiedad consultadas.

**Escenarios de aceptación**:

1. **Dado** el catálogo de propiedades, **Cuando** se selecciona una página distinta, **Entonces** se muestran las propiedades y los metadatos correspondientes sin una recarga completa de la página.
2. **Dado** una propiedad visible en el catálogo, **Cuando** se selecciona su enlace, **Entonces** se abre su detalle con la información correcta y sin una recarga completa de la página.
3. **Dado** un detalle abierto desde una página concreta del catálogo, **Cuando** se selecciona «Volver al catálogo», **Entonces** se restaura esa misma página con sus propiedades y metadatos.
4. **Dado** un enlace directo al detalle, **Cuando** se abre, **Entonces** se muestra la propiedad solicitada y la acción de retorno conduce a una página válida del catálogo.
5. **Dado** un contexto de página ausente o no válido, **Cuando** se regresa al catálogo, **Entonces** se muestra una página válida sin una vista vacía ni mensajes técnicos.

---

### Historia de usuario 2 - Recuperarse de interrupciones breves (Prioridad: P1)

Como visitante, quiero que una interrupción breve de la conexión se resuelva automáticamente, para seguir consultando propiedades sin recargar la página ni repetir la acción.

**Por qué esta prioridad**: Las redes reales tienen caídas cortas. Sin recuperación automática, una interrupción menor interrumpe la tarea y genera confusión.

**Prueba independiente**: Puede probarse iniciando una interrupción de hasta 10 segundos durante la navegación entre catálogo y detalle, restaurando después la conexión y verificando que se conservan la vista, la página, la propiedad y el retorno.

**Escenarios de aceptación**:

1. **Dado** una interrupción de conexión de hasta 10 segundos, **Cuando** se restaura la conexión, **Entonces** la experiencia se recupera automáticamente sin exigir una recarga manual.
2. **Dado** una interrupción durante la consulta de una página del catálogo, **Cuando** se recupera la conexión, **Entonces** se conserva la página consultada y se muestran sus propiedades y metadatos.
3. **Dado** una interrupción durante la consulta de un detalle, **Cuando** se recupera la conexión, **Entonces** se conserva la propiedad solicitada y su contexto de retorno.
4. **Dado** una recuperación satisfactoria, **Cuando** se observe la vista resultante, **Entonces** no aparecen mensajes técnicos, datos parciales engañosos ni acciones duplicadas.

---

### Historia de usuario 3 - Entender y reintentar cuando la interacción no está disponible (Prioridad: P2)

Como visitante, quiero recibir una indicación clara cuando la experiencia no puede responder o recuperarse, para saber qué ocurre y tener una acción sencilla para continuar.

**Por qué esta prioridad**: Una espera sin explicación o un error técnico reduce la confianza. Una acción de reintento comprensible permite recuperar la tarea sin ayuda.

**Prueba independiente**: Puede probarse forzando una respuesta lenta y una interrupción prolongada, verificando en cada caso el mensaje visible, las acciones ofrecidas y la ausencia de detalles técnicos.

**Escenarios de aceptación**:

1. **Dado** una respuesta que tarda en aparecer, **Cuando** se espera el resultado, **Entonces** se muestra una indicación de progreso reconocible antes de dejar una vista vacía.
2. **Dado** una interrupción superior a 10 segundos, **Cuando** no se logre la recuperación automática, **Entonces** se muestra un mensaje comprensible en español con una acción de reintento.
3. **Dado** un mensaje de no disponibilidad, **Cuando** se selecciona la acción de reintento, **Entonces** se intenta de nuevo la misma vista con la misma propiedad y el mismo contexto de retorno.
4. **Dado** una interrupción prolongada, **Cuando** se ofrezca continuar de otra forma, **Entonces** existe una acción comprensible para salir del estado actual y volver a una vista válida.
5. **Dado** cualquier estado de espera, recuperación o no disponibilidad, **Cuando** se lee el mensaje, **Entonces** no se exponen identificadores internos, rutas técnicas, detalles de conexión ni texto en otro idioma.

---

### Historia de usuario 4 - Mantener una experiencia coherente, aislada y accesible (Prioridad: P2)

Como visitante, quiero que la experiencia interactiva sea accesible, predecible y privada, para usarla con teclado, en distintos tamaños de pantalla y sin afectar a otras personas que la usan al mismo tiempo.

**Por qué esta prioridad**: La interactividad no debe degradar la accesibilidad ni mezclar estados entre visitantes. Estos requisitos evitan regresiones respecto del catálogo y el detalle existentes.

**Prueba independiente**: Puede probarse usando dos visitas independientes, recorriendo las acciones con teclado y revisando las vistas a 320, 768 y 1280 px, verificando aislamiento, foco visible y ausencia de desplazamiento horizontal.

**Escenarios de aceptación**:

1. **Dado** dos visitas independientes, **Cuando** una cambia de página o entra en un detalle, **Entonces** la otra conserva su propia vista y contexto sin recibir ese cambio.
2. **Dado** un visitante que usa solo teclado, **Cuando** recorre las acciones interactivas, **Entonces** el foco es visible y todas las acciones pueden ejecutarse.
3. **Dado** un ancho de 320 px, 768 px o 1280 px, **Cuando** se usa el catálogo o el detalle, **Entonces** el contenido permanece legible y no aparece desplazamiento horizontal.
4. **Dado** el contenido visible de la experiencia, **Cuando** se lee, **Entonces** todos los textos dirigidos al visitante están en español y las etiquetas son comprensibles.
5. **Dado** el cierre de una visita, **Cuando** se abre una nueva visita, **Entonces** no reaparece el contexto anterior de otra persona.

---

### Historia de usuario 5 - Registrar incidencias sin exponerlas al visitante (Prioridad: P3)

Como responsable de la experiencia, quiero que las incidencias de respuesta lenta o recuperación fallida queden registradas de forma interna, para detectar problemas operativos sin exponer información privada ni detalles técnicos al visitante.

**Por qué esta prioridad**: El registro permite observar fallos sin interrumpir la consulta ni añadir contenido técnico a las vistas públicas. Es importante para el soporte, pero no forma parte del recorrido principal del visitante.

**Prueba independiente**: Puede probarse provocando una respuesta lenta y una recuperación fallida, comprobando que queda un registro interno y que la vista del visitante solo muestra mensajes comprensibles.

**Escenarios de aceptación**:

1. **Dado** una interacción con respuesta lenta, **Cuando** finaliza, **Entonces** queda registrada internamente con el tipo de incidencia, el momento, la vista afectada y el resultado.
2. **Dado** una recuperación fallida, **Cuando** se muestra el mensaje al visitante, **Entonces** queda registrada internamente con el tipo de incidencia, el momento, la vista afectada y el resultado.
3. **Dado** un registro generado, **Cuando** se consulte, **Entonces** no contiene datos personales.
4. **Dado** un registro generado, **Cuando** se observe la vista del visitante, **Entonces** el registro no se muestra ni se menciona.

---

### Casos límite

- Si la conexión tarda en establecerse, se muestra una indicación de progreso en lugar de una vista vacía o engañosa.
- Si la interrupción dura 10 segundos o menos, la recuperación conserva automáticamente la vista, la página y la propiedad consultadas.
- Si la interrupción supera los 10 segundos, se ofrece una acción de reintento y una alternativa comprensible para continuar, sin exponer motivos técnicos.
- Si se repite rápidamente una acción, la vista resultante corresponde únicamente a la última intención válida del visitante.
- Si el visitante usa los controles de avance y retroceso del navegador, la vista mostrada corresponde a la dirección elegida y a un contexto válido.
- Si dos visitas están abiertas al mismo tiempo, sus estados de navegación y mensajes permanecen aislados.
- Si el contexto de página es inválido, se resuelve a una página válida sin dejar una lista vacía ni mostrar valores técnicos.
- Si la propiedad solicitada deja de estar disponible durante la interacción, se muestra el estado de no encontrado ya aprobado para el detalle, sin datos parciales.
- Si el visitante usa un ancho de 320 px, las acciones principales siguen siendo alcanzables y no aparece desplazamiento horizontal.
- Si la experiencia interactiva no está disponible en un entorno concreto, se mantiene una forma comprensible de continuar con la consulta sin mensajes técnicos.
- Si se cierra una visita y se abre otra, la nueva visita no hereda mensajes, selección ni contexto de la anterior.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El catálogo y el detalle de propiedades DEBEN responder a las acciones del visitante sin exigir una recarga completa de la página.
- **FR-002**: El cambio de página del catálogo DEBE conservar la paginación, las propiedades y los metadatos correspondientes a la página solicitada.
- **FR-003**: La apertura de un detalle DEBE conservar la propiedad seleccionada y la página de origen como contexto de retorno.
- **FR-004**: La acción «Volver al catálogo» DEBE restaurar la página de origen y volver a mostrar sus propiedades y metadatos.
- **FR-005**: Un enlace directo al detalle DEBE mostrar la propiedad solicitada y ofrecer un retorno a una página válida del catálogo.
- **FR-006**: Cuando una respuesta no aparezca de forma inmediata, la vista DEBE mostrar una indicación de progreso reconocible.
- **FR-007**: Tras una interrupción de hasta 10 segundos, la experiencia DEBE recuperarse automáticamente y conservar la vista, la página, la propiedad y el contexto de retorno vigentes.
- **FR-008**: Si la recuperación automática no tiene éxito, la vista DEBE mostrar un mensaje comprensible en español y una acción de reintento.
- **FR-009**: La acción de reintento DEBE conservar la misma vista, la misma propiedad cuando corresponda y el mismo contexto de retorno.
- **FR-010**: Si la recuperación automática no tiene éxito, la vista DEBE ofrecer una alternativa comprensible para salir del estado actual y volver a una vista válida.
- **FR-011**: Los estados de interacción DEBEN estar aislados entre visitas independientes y NO DEBEN compartirse entre visitantes.
- **FR-012**: Los estados de interacción NO DEBEN persistirse después de cerrar la visita ni filtrarse a una visita posterior.
- **FR-013**: Ante acciones rápidas repetidas, la vista DEBE reflejar únicamente la última intención válida del visitante y NO DEBE duplicar mensajes, acciones ni contenidos.
- **FR-014**: La navegación del navegador DEBE mostrar una vista coherente con la dirección seleccionada y con un contexto válido.
- **FR-015**: Si el contexto de página es ausente o inválido, la vista DEBE resolver a una página válida sin mostrar una lista vacía ni valores técnicos.
- **FR-016**: Si la propiedad solicitada deja de estar disponible, la vista DEBE mostrar el estado de no encontrado ya aprobado para el detalle y NO DEBE mostrar datos parciales.
- **FR-017**: Todos los textos visibles dirigidos al visitante DEBEN estar en español y NO DEBEN exponer identificadores internos, rutas técnicas, detalles de conexión ni mensajes del sistema.
- **FR-018**: El catálogo y el detalle DEBEN permanecer accesibles con teclado, con foco visible y sin desplazamiento horizontal a 320 px, 768 px y 1280 px.
- **FR-019**: Los estados de carga, contenido, interrupción, recuperación y no disponibilidad DEBEN ser visualmente coherentes con la experiencia existente.
- **FR-020**: Cuando una interacción no responda o no se recupere, DEBE registrarse internamente el tipo de incidencia, el momento, la vista afectada y el resultado, sin datos personales ni exposición de ese registro al visitante.
- **FR-021**: La iniciativa NO DEBE modificar propiedades, estados de propiedad, reglas de negocio, contenido publicado ni comportamientos ya aprobados para el catálogo y el detalle.
- **FR-022**: La iniciativa NO DEBE añadir formularios, autenticación, recopilación de datos personales ni funciones de creación, edición o eliminación.

### Entidades clave *(incluir si la característica implica datos)*

- **Visita interactiva**: Período de uso comprendido entre la apertura y el cierre de la experiencia por parte de un visitante; contiene el estado de navegación propio de esa visita.
- **Contexto de navegación**: Información necesaria para reconstruir la vista actual, incluida la página del catálogo, la propiedad consultada y la acción de retorno válida.
- **Estado de interacción**: Condición observable de la experiencia entre respuesta inmediata, espera, recuperación y no disponibilidad, utilizada para decidir mensajes y acciones visibles.
- **Incidencia operativa**: Registro interno de una interacción lenta o de una recuperación fallida; contiene tipo, momento, vista afectada y resultado, sin datos personales.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de los recorridos probados de cambio de página, apertura de detalle y retorno al catálogo se completa sin una recarga completa de la página.
- **SC-002**: Al menos el 95 % de las acciones probadas muestra una respuesta visible en menos de 1 segundo en condiciones normales de uso.
- **SC-003**: El 100 % de las interrupciones probadas de hasta 10 segundos se recupera sin recarga manual y conserva la vista, la página, la propiedad y el contexto de retorno.
- **SC-004**: El 100 % de las interrupciones probadas superiores a 10 segundos muestra un mensaje comprensible, una acción de reintento y una alternativa para continuar, sin exponer detalles técnicos.
- **SC-005**: Con 10 participantes en una prueba de uso local, al menos 9 consultan una propiedad desde el catálogo y vuelven a la página de origen sin ayuda en menos de 2 minutos.
- **SC-006**: Con 20 visitas concurrentes, al menos el 95 % de las tareas probadas se completa sin errores visibles ni pérdida de contexto.
- **SC-007**: El 100 % del contenido visible dirigido al visitante está en español y ningún estado muestra valores técnicos, rutas internas ni detalles de conexión.
- **SC-008**: El 100 % de las acciones probadas puede completarse usando solo teclado con foco visible, y las vistas no presentan desplazamiento horizontal a 320 px, 768 px ni 1280 px.
- **SC-009**: El 100 % de las pruebas con visitas independientes confirma que ninguna vista recibe el contexto, los mensajes ni la selección de otra visita.
- **SC-010**: El 100 % de las incidencias probadas de respuesta lenta o recuperación fallida genera un registro interno con tipo de incidencia, momento, vista afectada y resultado, sin datos personales ni exposición al visitante.

## Supuestos

- Los visitantes usan navegadores web modernos con conexión activa y capacidades interactivas disponibles.
- El catálogo y el detalle existentes son públicos; la iniciativa no introduce autenticación ni datos personales.
- Una interrupción breve se considera de hasta 10 segundos para efectos de recuperación automática.
- Las condiciones normales de uso para validación incluyen hasta 20 visitas concurrentes, mismo equipo y navegador, y la medición descrita en el protocolo SC-002 de la guía 013.
- El estado de interacción vive solo durante la visita actual; cerrar la visita descarta ese estado sin afectar a otras visitas.
- La información de propiedades, las reglas ya aprobadas y las rutas existentes se mantienen sin cambios.
- Los mensajes y comportamientos ya aprobados para carga, contenido, no encontrado y error en catálogo y detalle se conservan como base de esta experiencia.
- La documentación es retrospectiva: la evidencia real de las validaciones manuales quedó registrada en `quickstart.md`; por ello, los criterios de éxito se documentan como objetivos verificables y su cumplimiento se declara sobre esa evidencia.
