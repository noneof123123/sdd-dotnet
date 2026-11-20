# Especificación de la característica: Consulta de propiedad por identificador

**Rama de característica**: `007-properties-get-by-id`

**Creado**: 2026-11-20

**Estado**: Implementada

**Entrada**: Descripción del usuario: "007-properties-get-by-id"

## Aclaraciones

### Decisiones iniciales adoptadas

- Q: ¿Qué operación debe ofrecer esta característica? → A: La consulta de una única propiedad existente a partir de su identificador, sin crear, modificar ni eliminar registros.
- Q: ¿Qué información debe devolverse? → A: La representación pública completa de la propiedad, con los mismos datos editables usados por el catálogo, su referencia pública de imagen cuando exista y sus marcas temporales.
- Q: ¿Cómo debe tratar un identificador inexistente? → A: Como un resultado de no encontrado, estructurado y sin efectos observables.
- Q: ¿Cómo debe tratar un identificador mal formado? → A: Como una solicitud inválida, estructurada y sin efectos observables.
- Q: ¿Debe incluirse autenticación, autorización o eliminación? → A: No; esta iniciativa es de solo lectura y pública, coherente con las capacidades vigentes del catálogo.

### Sesión 2026-11-20

- Q: ¿Debe devolver la consulta la misma representación de imagen que el listado, con URL absoluta y cadena vacía cuando no hay imagen? → A: Opción A: URL absoluta construida con el origen actual y cadena vacía si no hay imagen; esta convención prevalece sobre la representación relativa o nula usada por la actualización.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Obtener el detalle de una propiedad existente (Prioridad: P1)

Como consumidor del catálogo, quiero solicitar una propiedad concreta por su identificador y recibir todos sus datos públicos, para poder mostrar su ficha completa sin consultar el listado completo.

**Por qué esta prioridad**: Es el flujo esencial y entrega valor de forma independiente; permite pasar de un listado a una vista de detalle sin cambios en las capacidades ya aprobadas.

**Prueba independiente**: Puede probarse solicitando una propiedad existente con un identificador válido y verificando que la respuesta contiene todos los campos públicos esperados, la referencia de imagen vigente y las marcas temporales, sin alterar ningún dato.

**Escenarios de aceptación**:

1. **Dado** una propiedad existente, **Cuando** se consulta con su identificador válido, **Entonces** se devuelve su representación pública completa y la operación no modifica la propiedad ni sus archivos.
2. **Dado** una propiedad con imagen asociada, **Cuando** se consulta por identificador, **Entonces** `imageUrl` contiene una URL absoluta construida con el origen actual, compatible con la imagen usada por el listado, y no expone rutas internas.
3. **Dado** una propiedad sin imagen asociada, **Cuando** se consulta por identificador, **Entonces** `imageUrl` se devuelve como cadena vacía, sin inventar una referencia ni provocar errores.

---

### Historia de usuario 2 - Obtener un resultado coherente con el catálogo (Prioridad: P2)

Como cliente del catálogo, quiero que la consulta por identificador use la misma interpretación de datos, estados e imágenes que el listado existente, para reutilizar mis componentes de presentación sin conversiones especiales.

**Por qué esta prioridad**: Evita contradicciones entre el detalle y el listado, reduce errores de integración y preserva la compatibilidad de los contratos ya publicados.

**Prueba independiente**: Puede probarse consultando una propiedad creada o actualizada previamente y comparando los valores devueltos con los del listado existente, comprobando que estado, imagen y datos físicos coinciden.

**Escenarios de aceptación**:

1. **Dado** una propiedad visible en el listado, **Cuando** se consulta por identificador, **Entonces** los valores públicos devueltos coinciden con los mostrados por el listado para esa misma propiedad.
2. **Dado** una propiedad actualizada recientemente, **Cuando** se consulta por identificador, **Entonces** la respuesta refleja el estado vigente y la fecha de modificación registrada.
3. **Dado** un consumidor del listado, **Cuando** consulta el detalle, **Entonces** interpreta la referencia de imagen y el estado sin lógica adicional específica para esta operación.

---

### Historia de usuario 3 - Recibir errores claros y sin efectos (Prioridad: P3)

Como consumidor del sistema, quiero recibir un error comprensible cuando el identificador no existe o no es válido, para corregir la solicitud sin sospechar cambios parciales.

**Por qué esta prioridad**: Una operación de lectura debe ser predecible y segura; los errores ambiguos generan reintentos innecesarios y pueden interpretarse como fallos de datos.

**Prueba independiente**: Puede probarse con identificadores inexistentes y mal formados, verificando que se devuelve el tipo de error correspondiente y que no se crea, modifica ni elimina ninguna propiedad ni archivo.

**Escenarios de aceptación**:

1. **Dado** un identificador válido sin propiedad asociada, **Cuando** se consulta, **Entonces** se devuelve un resultado estructurado de no encontrado y no se produce ningún cambio observable.
2. **Dado** un identificador mal formado en la ruta, **Cuando** se consulta, **Entonces** se devuelve un resultado estructurado de solicitud inválida y no se produce ningún cambio observable; un path sin segmento `id` no alcanza el endpoint y queda resuelto por el enrutamiento general.
3. **Dado** un error interno durante la consulta, **Cuando** se devuelve la respuesta, **Entonces** el mensaje es genérico, no revela detalles técnicos y la operación sigue siendo de solo lectura.

---

## Casos límite

- Si el identificador está vacío o no cumple el formato vigente, la solicitud se rechaza como inválida antes de buscar la propiedad; un path sin segmento `id` no alcanza el endpoint y queda resuelto por el enrutamiento general.
- Si el identificador es válido pero no existe, se devuelve no encontrado sin crear una propiedad vacía ni una imagen.
- Si la propiedad existe sin imagen, `imageUrl` se devuelve como cadena vacía, equivalente a la usada por el listado, sin generar archivos ni rutas ficticias.
- Si la propiedad existe con imagen, `imageUrl` es una URL absoluta construida con el origen actual; nunca se expone su ruta física ni el nombre original del archivo.
- Si la consulta se cancela, se interrumpe sin efectos observables.
- Si dos consultas se realizan al mismo tiempo, cada una devuelve el estado visible en el momento de su lectura sin bloquear ni modificar la otra.
- Esta iniciativa no incluye creación, actualización, eliminación, publicación masiva, autorización ni eliminación explícita de imágenes.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El sistema DEBE permitir consultar una única propiedad existente mediante su identificador.
- **FR-002**: El identificador DEBE ser obligatorio y cumplir el formato vigente antes de realizarse la búsqueda.
- **FR-003**: Cuando el identificador corresponde a una propiedad existente, el sistema DEBE devolver su representación pública completa.
- **FR-004**: La representación pública DEBE incluir identificador, título, descripción, dirección, precio, estado, cantidad de dormitorios, cantidad de baños, superficie en metros cuadrados, referencia pública de imagen y marcas temporales.
- **FR-005**: `imageUrl` DEBE ser una URL absoluta construida con el origen de la solicitud cuando exista una imagen válida, y una cadena vacía cuando no exista; NO DEBE exponer rutas físicas ni nombres originales inseguros.
- **FR-006**: Cuando el identificador no corresponde a ninguna propiedad, el sistema DEBE devolver un resultado estructurado de no encontrado sin efectos observables.
- **FR-007**: Cuando el identificador es inválido, el sistema DEBE devolver un resultado estructurado de solicitud inválida sin efectos observables.
- **FR-008**: La operación DEBE ser de solo lectura; NO DEBE crear, actualizar ni eliminar propiedades, imágenes ni marcas temporales.
- **FR-009**: Los estados, formatos y significados devueltos DEBEN coincidir con los contratos vigentes de creación, actualización y listado de propiedades.
- **FR-010**: La nueva capacidad DEBE integrarse con el mecanismo de descubrimiento existente sin registros manuales paralelos.
- **FR-011**: La operación DEBE registrar información estructurada de la consulta sin incluir datos sensibles ni detalles técnicos en las respuestas.
- **FR-012**: Ante un error interno, el sistema DEBE devolver un mensaje genérico sin detalles técnicos y conservar el comportamiento de solo lectura.
- **FR-013**: La operación DEBE propagar la cancelación de la solicitud y NO DEBE dejar efectos observables cuando se cancele.
- **FR-014**: La consulta NO DEBE modificar el comportamiento ni el contrato de salud, listado paginado, creación ni actualización de propiedades.
- **FR-015**: La consulta NO DEBE agregar autenticación, autorización, eliminación ni otros casos de uso no descritos en esta especificación.

### Entidades clave *(incluir si la característica implica datos)*

- **Property**: Registro persistente existente que representa el inmueble y contiene sus datos comerciales, físicos, estado, imagen opcional y marcas temporales.
- **PropertyIdentifier**: Valor único que identifica de forma inequívoca la propiedad solicitada.
- **PropertyDetailResponse**: Representación pública explícita de la propiedad consultada, sin exponer el modelo persistente.
- **PublicImageReference**: `imageUrl` absoluta construida con el origen de la solicitud cuando existe imagen válida, o cadena vacía cuando no existe, compatible con el contrato vigente del listado.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las consultas con identificadores válidos y existentes devuelve la representación pública completa de la propiedad.
- **SC-002**: El 100 % de las consultas con identificadores inexistentes o inválidos devuelve el tipo de error correspondiente sin efectos observables.
- **SC-003**: El 95 % de las consultas válidas se resuelve en menos de 1 segundo en condiciones normales de uso.
- **SC-004**: El 100 % de las respuestas correctas utiliza los mismos nombres, formatos y significados de campo que el listado existente.
- **SC-005**: El 100 % de las respuestas correctas omite rutas físicas, nombres originales inseguros y detalles internos.
- **SC-006**: Las pruebas de salud, listado, creación, actualización y persistencia existentes conservan su resultado satisfactorio tras incorporar la consulta.
- **SC-007**: El 100 % de las consultas canceladas termina sin crear, modificar ni eliminar datos o archivos.
- **SC-008**: El 100 % de los errores internos probados devuelve un mensaje genérico sin detalles técnicos ni resultados parciales.
- **SC-009**: El 100 % de las respuestas con imagen válida devuelve `imageUrl` absoluta del origen actual y el 100 % de las respuestas sin imagen devuelve `imageUrl` como cadena vacía.

## Supuestos

- La iniciativa depende de los contratos y convenciones aprobados en 001-solution-foundation, 002-foundation-backend, 003-properties-persistence-seeding, 004-properties-list-pagination, 005-properties-create y 006-properties-update.
- El identificador sigue el formato ya utilizado por las capacidades de actualización y listado del catálogo.
- La operación es pública; la autenticación y la autorización quedan fuera del alcance.
- No se incorporan nuevos datos persistidos; se reutiliza la información ya descrita en el modelo de propiedades.
- La representación de imagen es la misma que consume actualmente el listado de propiedades: `imageUrl` absoluta del origen de la solicitud o cadena vacía. Esta convención prevalece sobre la ruta relativa o el valor nulo usados por la actualización.
- Las marcas temporales se expresan en UTC.
- No se modifica el comportamiento ni el contrato de salud, listado paginado, creación ni actualización de propiedades.
- La consulta por identificador no habilita edición, eliminación ni gestión de imágenes.
