# Especificación de la característica: Actualización de propiedades

**Rama de característica**: `006-properties-update`

**Creado**: 2026-11-20

**Estado**: Implementada

**Entrada**: Descripción del usuario: "006-properties-update"

## Aclaraciones

### Sesión 2026-11-20

- Q: ¿Qué operación debe ofrecer esta característica? → A: La modificación de una propiedad existente identificada por su identificador, sin crear ni eliminar registros.
- Q: ¿Cómo deben enviarse los datos editables? → A: Mediante campos individuales en una solicitud `multipart/form-data`, con el identificador de la propiedad indicado en la ruta.
- Q: ¿Qué ocurre si no se adjunta una imagen? → A: Se conserva la imagen asociada actual; esta característica no incluye la eliminación explícita de imágenes.
- Q: ¿Qué ocurre si se adjunta una imagen válida? → A: Se publica una nueva imagen con nombre seguro único, se actualiza la ruta relativa persistida y se retira la imagen anterior una vez confirmada la actualización.
- Los formatos admitidos son PNG y JPG/JPEG, con un tamaño máximo efectivo de 5 × 1024 × 1024 bytes de forma inclusiva.
- La actualización conserva el identificador y la fecha de creación originales, y registra una nueva fecha de modificación en UTC.
- La representación devuelta es pública, no expone la entidad persistente, rutas físicas ni nombres de archivo originales inseguros.
- Q: ¿Qué política debe aplicarse cuando dos solicitudes intentan actualizar la misma propiedad al mismo tiempo? → A: Opción A: última escritura gana; la solicitud que se persiste al final determina el estado final, sin rechazo por concurrencia ni control de versión.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Actualizar los datos de una propiedad sin cambiar la imagen (Prioridad: P1)

Como responsable del catálogo, quiero corregir los datos comerciales y físicos de una propiedad existente sin perder su fotografía actual, para mantener la información publicada actualizada sin repetir la carga de archivos.

**Por qué esta prioridad**: Es el flujo esencial de mantenimiento del catálogo y puede entregarse de forma independiente del reemplazo de imágenes.

**Prueba independiente**: Puede probarse enviando una actualización válida sin parte `image` sobre una propiedad existente y verificando que los datos cambiaron, el identificador y la fecha de creación se conservaron, la imagen anterior sigue asociada y se registró la fecha de modificación.

**Escenarios de aceptación**:

1. **Dado** una propiedad existente con o sin imagen, **Cuando** se envía una actualización válida sin parte `image`, **Entonces** se actualizan los datos editables, se conservan el identificador y la fecha de creación, se registra la fecha de modificación y se mantiene la imagen asociada.
2. **Dado** una actualización correcta sin imagen, **Cuando** se recibe la respuesta, **Entonces** se devuelve el estado de éxito correspondiente con la representación pública actualizada.
3. **Dado** una propiedad actualizada sin imagen, **Cuando** se consulta posteriormente el listado existente, **Entonces** aparece con los datos vigentes y una representación de imagen compatible con el contrato actual del listado.

---

### Historia de usuario 2 - Reemplazar la imagen de una propiedad (Prioridad: P1)

Como responsable del catálogo, quiero adjuntar una nueva imagen válida al actualizar una propiedad, para que la fotografía publicada refleje el estado actual del inmueble.

**Por qué esta prioridad**: La imagen es parte del catálogo visible y su reemplazo seguro evita huérfanos, rutas rotas y exposición de nombres originales.

**Prueba independiente**: Puede probarse actualizando una propiedad con PNG y con JPG/JPEG válidos de hasta el límite acordado, verificando que queda una sola imagen activa, que la ruta persistida es relativa y pública, y que el archivo anterior deja de estar asociado.

**Escenarios de aceptación**:

1. **Dado** una propiedad existente y una imagen PNG o JPG/JPEG válida, **Cuando** se envía la actualización con esa imagen, **Entonces** se publica un único archivo con nombre seguro, se persiste su ruta relativa y se retira la imagen anterior.
2. **Dado** una actualización con imagen válida, **Cuando** se recibe la respuesta, **Entonces** `imageUrl` contiene la ruta relativa pública del nuevo archivo y no expone rutas físicas ni el nombre original.
3. **Dado** una propiedad con imagen reemplazada, **Cuando** se consulta el listado, **Entonces** el cliente puede consumir la nueva imagen mediante la URL pública compatible sin conocer rutas internas.

---

### Historia de usuario 3 - Rechazar una actualización inválida sin efectos parciales (Prioridad: P2)

Como consumidor del sistema, quiero recibir errores claros y estructurados cuando la propiedad, el identificador o la imagen no cumplen las reglas vigentes, para corregir la solicitud sin dejar cambios parciales.

**Por qué esta prioridad**: Evita datos incompletos, imágenes incompatibles y estados inconsistentes entre el registro y el archivo publicado.

**Prueba independiente**: Puede probarse enviando identificadores inexistentes o mal formados, datos vacíos o negativos, estados desconocidos, imágenes no admitidas, formatos declarados de forma incompatible, archivos excesivos y partes `image` duplicadas; en todos los casos debe rechazarse la operación sin modificar la propiedad ni los archivos.

**Escenarios de aceptación**:

1. **Dado** un identificador inexistente, **Cuando** se solicita la actualización, **Entonces** se devuelve el estado de no encontrado y no se crea ni modifica ningún registro ni archivo.
2. **Dado** un dato obligatorio inválido o un estado fuera de los valores permitidos, **Cuando** se solicita la actualización, **Entonces** se devuelve el estado de solicitud inválida con detalles estructurados y la propiedad permanece sin cambios.
3. **Dado** un archivo con formato, contenido, extensión o tamaño no admitido, o dos partes `image`, **Cuando** se procesa la solicitud, **Entonces** se rechaza sin publicar archivos ni modificar la imagen asociada.

---

### Historia de usuario 4 - Mantener la compatibilidad del sistema existente (Prioridad: P3)

Como responsable del backend, quiero incorporar la actualización sin alterar la creación, el listado paginado, la salud del sistema ni los contratos ya aprobados, para preservar la estabilidad del catálogo.

**Por qué esta prioridad**: La actualización debe integrarse como una capacidad adicional sin regresiones en los casos de uso ya entregados.

**Prueba independiente**: Puede verificarse ejecutando las pruebas de salud, creación, listado, persistencia y descubrimiento después de incorporar la actualización y comprobando que siguen cumpliendo sus contratos.

**Escenarios de aceptación**:

1. **Dado** el catálogo existente, **Cuando** se actualiza una propiedad, **Entonces** la creación de nuevas propiedades y el listado paginado conservan su comportamiento y contrato previo.
2. **Dado** el descubrimiento automático de casos de uso, **Cuando** se incorpora la actualización, **Entonces** queda disponible junto a los casos de uso existentes sin registros manuales paralelos.
3. **Dado** un consumidor del listado, **Cuando** consulta propiedades creadas o actualizadas, **Entonces** interpreta las imágenes y los datos devueltos sin cambios incompatibles.

---

## Casos límite

- Si el identificador de la ruta no tiene un formato válido, la solicitud se rechaza como inválida sin acceder a la propiedad.
- Si la propiedad no existe, no se publica ninguna imagen ni se modifica ningún archivo existente.
- Si se cancela la operación durante la publicación o la persistencia, no debe quedar una nueva imagen huérfana ni un registro con una ruta inexistente.
- Si falla la publicación de la nueva imagen, la propiedad y su imagen anterior permanecen sin cambios.
- Si falla la persistencia después de publicar la nueva imagen, se retira la nueva imagen y se conserva la anterior.
- Si el nombre original del archivo es vacío, peligroso o contiene caracteres no seguros, se genera un nombre seguro sin exponer el original.
- Si una imagen declara PNG o JPG/JPEG pero su contenido no corresponde, se considera inválida.
- Si no se adjunta imagen, el estado persistido de imagen permanece exactamente igual; no se sustituye por una cadena vacía.
- Si la imagen anterior era nula, la actualización sin imagen conserva el valor nulo.
- La eliminación explícita de la imagen de una propiedad no forma parte de esta iniciativa.
- Si dos solicitudes válidas se persisten concurrentemente sobre la misma propiedad, prevalece el estado de la última escritura confirmada y no se genera un error de conflicto.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El sistema DEBE permitir actualizar una propiedad existente identificada de forma inequívoca.
- **FR-002**: La solicitud DEBE aceptar título, descripción, dirección, precio, estado, cantidad de dormitorios, cantidad de baños y superficie en metros cuadrados mediante campos individuales en `multipart/form-data`.
- **FR-003**: La solicitud DEBE aceptar opcionalmente una sola imagen; la ausencia de imagen DEBE conservar la imagen asociada actual.
- **FR-004**: La operación DEBE validar todos los campos editables antes de modificar la propiedad o publicar archivos.
- **FR-005**: El título, la descripción y la dirección DEBEN cumplir las restricciones vigentes del modelo persistente; el precio y las cantidades físicas NO DEBEN aceptar valores negativos.
- **FR-006**: El estado DEBE pertenecer a los valores permitidos por el catálogo vigente.
- **FR-007**: La imagen DEBE validarse por tipo declarado, extensión, firma de contenido y tamaño máximo inclusivo de 5 × 1024 × 1024 bytes.
- **FR-008**: Solo puede existir una parte `image` por solicitud.
- **FR-009**: La actualización DEBE conservar el identificador y la fecha de creación de la propiedad existente.
- **FR-010**: La actualización DEBE registrar una nueva fecha de modificación en UTC cuando los cambios se persistan correctamente.
- **FR-011**: La imagen válida publicada DEBE ubicarse en el destino público vigente y persistirse como una ruta relativa segura.
- **FR-012**: Cuando se reemplaza la imagen, la imagen anterior DEBE dejar de estar asociada a la propiedad una vez confirmada la actualización.
- **FR-013**: Ante fallos de validación, identificador inexistente o solicitud mal formada, el sistema DEBE devolver un estado de error estructurado sin efectos observables.
- **FR-014**: Ante fallos internos de publicación o persistencia, el sistema DEBE devolver un error genérico sin detalles técnicos, sin resultados parciales y conservando el estado anterior de la propiedad y sus archivos.
- **FR-015**: La operación DEBE propagar la cancelación hasta las operaciones compatibles y evitar cambios observables cuando se cancele.
- **FR-016**: La respuesta correcta DEBE devolver la representación pública actualizada de la propiedad, sin exponer entidades persistentes, rutas físicas ni nombres originales inseguros.
- **FR-017**: La actualización NO DEBE crear ni eliminar propiedades, modificar el contrato del listado paginado, alterar la creación existente ni cambiar el comportamiento de salud del sistema.
- **FR-018**: La actualización NO DEBE agregar autorización, eliminación, publicación masiva ni otros casos de uso no descritos en esta especificación.
- **FR-019**: El descubrimiento de la nueva capacidad DEBE integrarse con la infraestructura existente sin registros manuales paralelos.
- **FR-020**: Las reglas de validación de entrada DEBEN concentrarse en un único componente de la nueva capacidad, antes de cualquier persistencia o publicación de archivos.
- **FR-021**: Cuando dos solicitudes válidas se persistan concurrentemente sobre la misma propiedad, la operación DEBE aplicar la política de última escritura gana, sin rechazo por concurrencia ni control de versión.

### Entidades clave *(incluir si la característica implica datos)*

- **Property**: Registro persistente existente que representa el inmueble y contiene sus datos comerciales, físicos, estado, imagen opcional y marcas temporales.
- **UpdatePropertyRequest**: Datos recibidos para modificar una propiedad existente, incluido el identificador de la ruta y una imagen opcional.
- **UpdatePropertyResponse**: Representación pública explícita de la propiedad actualizada, sin exponer el modelo persistente.
- **PropertyImage**: Archivo opcional validado que puede sustituir a la imagen asociada de la propiedad.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las actualizaciones válidas sin imagen conserva el identificador, la fecha de creación y la imagen previa, y registra la nueva fecha de modificación.
- **SC-002**: El 100 % de las actualizaciones válidas con PNG o JPG/JPEG deja exactamente una imagen activa asociada a la propiedad y una ruta relativa pública válida.
- **SC-003**: El 100 % de las solicitudes con datos inválidos, imagen no admitida, tamaño excedido o partes `image` duplicadas se rechaza sin modificar la propiedad ni los archivos.
- **SC-004**: El 100 % de las solicitudes con identificador inexistente o mal formado se rechaza sin efectos observables.
- **SC-005**: El 100 % de los fallos internos probados conserva el estado anterior de la propiedad y deja 0 % de archivos huérfanos observables.
- **SC-006**: El 95 % de las actualizaciones válidas se completa en menos de 2 segundos en condiciones normales de uso.
- **SC-007**: El 100 % de las propiedades actualizadas queda visible y compatible con el listado existente en la siguiente consulta satisfactoria.
- **SC-008**: El 100 % de las respuestas correctas contiene la representación pública actualizada y ninguna expone rutas físicas ni nombres originales inseguros.
- **SC-009**: Las pruebas de salud, creación, listado, persistencia y descubrimiento existentes conservan su resultado satisfactorio tras incorporar la actualización.
- **SC-010**: El 100 % de los errores internos probados devuelve un mensaje genérico sin detalles técnicos ni resultados parciales.

## Supuestos

- La iniciativa depende de los contratos y convenciones aprobados en 001-solution-foundation, 002-foundation-backend, 003-properties-persistence-seeding, 004-properties-list-pagination y 005-properties-create.
- El endpoint de actualización es público; la autenticación y la autorización quedan fuera del alcance.
- La operación sigue una semántica de reemplazo completo de los campos editables; no se admite actualización parcial.
- La omisión de la imagen conserva la imagen actual; la eliminación explícita de imágenes se diferirá a otra iniciativa.
- El destino público vigente para imágenes es el mismo utilizado por la creación de propiedades y el listado.
- 5 MB significa 5 × 1024 × 1024 bytes como límite máximo inclusivo.
- Los formatos admitidos son PNG y JPG/JPEG, validados por metadatos y contenido compatible.
- Las marcas temporales se expresan en UTC.
- No se modifica el comportamiento ni el contrato de `/health`, `GET /api/properties` ni `POST /api/properties`.
