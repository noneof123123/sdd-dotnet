# Especificación de la característica: Actualización de estado de propiedades

**Rama de característica**: `009-update-status`

**Creado**: 2026-11-20

**Estado**: Implementada

**Entrada**: Descripción del usuario: "009-update-status"

## Aclaraciones

### Sesión 2026-11-20

- Q: ¿Qué operación debe ofrecer esta característica? → A: Una operación pública dedicada que cambia únicamente el estado de una propiedad existente identificada por su identificador, sin modificar otros datos ni la imagen.
- Q: ¿Qué estados debe aceptar y qué transiciones permitir? → A: `Available`, `Rented` y `Maintenance`; se permite cualquier transición, incluida repetir el estado ya vigente.
- Q: ¿Qué debe devolver la operación cuando el cambio se aplica? → A: HTTP 200 con la representación pública completa y actualizada de la propiedad; los errores de entrada o inexistencia devuelven respuestas estructuradas.
- Repetir el estado actual es una operación válida e idempotente: no altera identificador, datos, imagen ni fecha de modificación.
- Cuando dos solicitudes válidas se persisten concurrentemente sobre la misma propiedad, prevalece la última escritura confirmada y no se genera un error de conflicto.
- La representación devuelta sigue la convención vigente de la consulta por identificador: referencia pública de imagen absoluta cuando existe y cadena vacía cuando no existe.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Cambiar el estado sin reenviar el resto de datos (Prioridad: P1)

Como responsable del catálogo, quiero cambiar el estado de una propiedad existente indicando solo su identificador y el nuevo estado, para gestionar su disponibilidad sin repetir la carga de todos sus datos ni de su imagen.

**Por qué esta prioridad**: Es el flujo esencial y entrega valor de forma independiente; evita operaciones de reemplazo completo para una acción pequeña y frecuente.

**Prueba independiente**: Puede probarse enviando un identificador válido y un estado permitido distinto del actual, verificando que solo cambian el estado y la fecha de modificación, y que la respuesta contiene la representación pública completa.

**Escenarios de aceptación**:

1. **Dado** una propiedad existente con identificador válido y un estado permitido distinto del vigente, **Cuando** se solicita el cambio de estado, **Entonces** se persiste el nuevo estado, se registra una nueva fecha de modificación y se devuelve HTTP 200 con la representación pública actualizada.
2. **Dado** un cambio de estado correcto, **Cuando** se consulta posteriormente la propiedad por identificador, **Entonces** el estado y la fecha de modificación coinciden con los devueltos en la respuesta del cambio.
3. **Dado** una propiedad con datos e imagen asociada, **Cuando** cambia solo su estado, **Entonces** título, descripción, dirección, precio, dormitorios, baños, superficie, identificador, fecha de creación e imagen permanecen sin cambios.

---

### Historia de usuario 2 - Repetir el estado vigente sin provocar errores (Prioridad: P2)

Como responsable del catálogo, quiero que enviar el estado ya vigente sea aceptado, para que mis procesos repetidos o reintentos seguros no fallen ni generen cambios innecesarios.

**Por qué esta prioridad**: Reduce falsos errores en integraciones y hace predecible el comportamiento ante reintentos, sin afectar la información ya publicada.

**Prueba independiente**: Puede probarse enviando el mismo estado vigente sobre una propiedad existente y verificando que la operación termina satisfactoriamente sin modificar ningún dato, imagen ni marca temporal existente.

**Escenarios de aceptación**:

1. **Dado** una propiedad con un estado permitido, **Cuando** se envía ese mismo estado, **Entonces** la operación termina satisfactoriamente con HTTP 200 y devuelve la representación pública vigente.
2. **Dado** un estado repetido, **Cuando** se recibe la respuesta, **Entonces** la fecha de modificación permanece igual que antes de la solicitud.
3. **Dado** el mismo estado enviado dos veces, **Cuando** ambas solicitudes terminan, **Entonces** no se crea otra propiedad ni aparece ningún cambio observable distinto del estado ya existente.

---

### Historia de usuario 3 - Rechazar entradas inválidas sin efectos (Prioridad: P2)

Como consumidor del catálogo, quiero recibir errores claros y estructurados cuando el identificador o el estado no son válidos, para corregir la solicitud sin sospechar cambios parciales.

**Por qué esta prioridad**: Una validación temprana evita estados inconsistentes y hace que los errores sean accionables para quien integra o administra el catálogo.

**Prueba independiente**: Puede probarse con identificadores mal formados, identificadores inexistentes, estados desconocidos, estado vacío y solicitudes mal formadas, verificando en cada caso el tipo de error correspondiente y la ausencia de cambios.

**Escenarios de aceptación**:

1. **Dado** un identificador mal formado, **Cuando** se solicita el cambio, **Entonces** se devuelve HTTP 400 con detalles estructurados y no se accede ni modifica ninguna propiedad.
2. **Dado** un identificador válido sin propiedad asociada, **Cuando** se solicita el cambio, **Entonces** se devuelve HTTP 404 con detalles estructurados y no se crea ni modifica ningún registro ni archivo.
3. **Dado** un estado fuera de `Available`, `Rented` o `Maintenance`, vacío o ausente en una solicitud mal formada, **Cuando** se procesa, **Entonces** se devuelve HTTP 400 con detalles estructurados y la propiedad permanece sin cambios.

---

### Historia de usuario 4 - Mantener la coherencia del catálogo y su contrato público (Prioridad: P3)

Como responsable del sistema, quiero que el cambio de estado conviva con las capacidades ya aprobadas y quede descrito en el contrato público vigente, para evitar regresiones y contradicciones entre clientes.

**Por qué esta prioridad**: El estado es un dato compartido por listado, detalle y actualización; cualquier divergencia rompe la confianza de quienes consumen el catálogo.

**Prueba independiente**: Puede verificarse ejecutando las comprobaciones existentes de salud, listado, creación, actualización, consulta por identificador y contrato público tras incorporar el cambio de estado.

**Escenarios de aceptación**:

1. **Dado** el catálogo existente, **Cuando** cambia el estado de una propiedad, **Entonces** la creación, el listado paginado, la actualización general, la consulta por identificador y la salud del sistema conservan su comportamiento y contrato previo.
2. **Dado** la nueva capacidad, **Cuando** se publica el contrato del sistema, **Entonces** el cambio de estado queda descrito de forma coherente con las operaciones ya documentadas, sin contradecirlas ni duplicarlas.
3. **Dado** un consumidor del listado o del detalle, **Cuando** consulta una propiedad cuyo estado acaba de cambiar, **Entonces** interpreta el nuevo valor sin lógica adicional específica para esta operación.

---

## Casos límite

- Si el identificador está vacío o no cumple el formato vigente, la solicitud se rechaza como inválida antes de buscar la propiedad.
- Si el identificador es válido pero no existe, se devuelve no encontrado sin crear una propiedad vacía ni modificar archivos.
- Si el estado está vacío, es nulo, no corresponde a un valor permitido o la solicitud está mal formada, se rechaza como inválida sin efectos observables.
- Si el estado enviado es igual al vigente, la solicitud termina satisfactoriamente y no cambia la fecha de modificación.
- Si dos solicitudes válidas con estados distintos se persisten concurrentemente, prevalece el estado de la última escritura confirmada y no se devuelve un error de conflicto.
- Si la operación se cancela, se interrumpe sin crear, modificar ni eliminar propiedades, imágenes ni marcas temporales.
- Si falla la persistencia, se conserva íntegramente el estado anterior de la propiedad y se devuelve un error genérico sin detalles técnicos.
- La operación no elimina imágenes, no modifica la referencia pública de imagen y no cambia ningún campo distinto de estado y fecha de modificación efectiva.
- No se permite el cambio masivo de estado de varias propiedades en una sola solicitud.
- No se incorporan autenticación, autorización, creación, eliminación ni actualización de otros datos de la propiedad.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El sistema DEBE permitir cambiar el estado de una propiedad existente identificada de forma inequívoca mediante una operación pública dedicada.
- **FR-002**: La solicitud DEBE aceptar únicamente un valor de estado entre `Available`, `Rented` y `Maintenance`.
- **FR-003**: El sistema DEBE validar identificador y estado antes de realizar cualquier modificación.
- **FR-004**: Un identificador inválido DEBE producir HTTP 400 con detalles estructurados y sin efectos observables.
- **FR-005**: Un identificador válido sin propiedad asociada DEBE producir HTTP 404 con detalles estructurados y sin efectos observables.
- **FR-006**: Un estado ausente, vacío o fuera de los valores permitidos DEBE producir HTTP 400 con detalles estructurados y sin efectos observables.
- **FR-007**: Cuando el estado cambia efectivamente, la operación DEBE persistir el nuevo estado y registrar una nueva fecha de modificación en UTC.
- **FR-008**: La operación DEBE conservar identificador, fecha de creación, título, descripción, dirección, precio, dormitorios, baños, superficie e imagen asociada.
- **FR-009**: Cuando el estado enviado coincide con el vigente, la operación DEBE terminar satisfactoriamente sin modificar la fecha de modificación ni cualquier otro dato.
- **FR-010**: La respuesta satisfactoria DEBE devolver HTTP 200 con la representación pública completa y actualizada de la propiedad, siguiendo la convención vigente de la consulta por identificador para la referencia de imagen y sin exponer entidades persistentes, rutas físicas ni nombres originales inseguros.
- **FR-011**: Ante fallos de validación, inexistencia o solicitud mal formada, el sistema DEBE devolver un error estructurado sin efectos parciales.
- **FR-012**: Ante fallos internos de persistencia, el sistema DEBE devolver un error genérico sin detalles técnicos y conservar el estado anterior de la propiedad.
- **FR-013**: La operación DEBE propagar la cancelación y NO DEBE dejar cambios observables cuando se cancele.
- **FR-014**: Cuando dos solicitudes válidas se persistan concurrentemente sobre la misma propiedad, la operación DEBE aplicar la política de última escritura gana, sin rechazo por concurrencia ni control de versión.
- **FR-015**: La operación NO DEBE crear ni eliminar propiedades, modificar imágenes, alterar otros datos editables ni cambiar el comportamiento de las capacidades existentes.
- **FR-016**: La operación NO DEBE agregar cambio masivo de estado, autenticación, autorización, eliminación ni otros casos de uso no descritos en esta especificación.
- **FR-017**: La nueva capacidad DEBE integrarse con el mecanismo de descubrimiento existente sin registros manuales paralelos.
- **FR-018**: El contrato público vigente DEBE describir la nueva operación de forma coherente con las capacidades ya documentadas y NO DEBE introducir contradicciones con ellas.
- **FR-019**: La operación DEBE registrar información estructurada de la acción sin incluir datos sensibles ni detalles técnicos en las respuestas.
- **FR-020**: Las marcas temporales devueltas o registradas por esta capacidad DEBEN expresarse en UTC.

### Entidades clave *(incluir si la característica implica datos)*

- **Property**: Registro persistente existente que representa el inmueble y contiene sus datos comerciales, físicos, estado, imagen opcional y marcas temporales.
- **PropertyIdentifier**: Valor único que identifica de forma inequívoca la propiedad cuyo estado se cambiará.
- **PropertyStatus**: Estado permitido del catálogo vigente: `Available`, `Rented` o `Maintenance`.
- **PropertyStatusUpdate**: Solicitud que contiene el identificador de la propiedad y el nuevo estado deseado.
- **PublicPropertyRepresentation**: Representación pública completa devuelta tras aplicar o rechazar correctamente un cambio de estado, sin exponer el modelo persistente.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las solicitudes válidas con un estado distinto del vigente cambia solo estado y fecha de modificación, y devuelve HTTP 200 con la representación pública completa.
- **SC-002**: El 100 % de los estados permitidos, incluido el estado vigente, produce una respuesta satisfactoria; en el caso repetido, la fecha de modificación permanece sin cambios.
- **SC-003**: El 100 % de las solicitudes con identificador inválido, inexistente, estado no permitido o cuerpo mal formado devuelve el error estructurado correspondiente sin efectos observables.
- **SC-004**: El 100 % de los fallos internos probados conserva el estado anterior de la propiedad y devuelve un mensaje genérico sin detalles técnicos.
- **SC-005**: El 95 % de los cambios de estado válidos se completa en menos de 1 segundo en condiciones normales de uso.
- **SC-006**: El 100 % de las propiedades cambiadas conserva identificador, fecha de creación, datos comerciales, datos físicos e imagen exactamente igual que antes del cambio de estado.
- **SC-007**: El 100 % de las respuestas correctas omite entidades persistentes, rutas físicas y nombres originales inseguros.
- **SC-008**: Las comprobaciones existentes de salud, listado, creación, actualización, consulta por identificador y contrato público conservan su resultado satisfactorio tras incorporar el cambio de estado.
- **SC-009**: El 100 % de las solicitudes canceladas termina sin crear, modificar ni eliminar propiedades, imágenes ni marcas temporales.
- **SC-010**: Ante cambios concurrentes probados sobre la misma propiedad, el estado final observado corresponde al 100 % de los casos a la última escritura confirmada, sin errores de conflicto.

## Supuestos

- La iniciativa depende de los contratos y convenciones aprobados en `001-solution-foundation`, `002-foundation-backend`, `003-properties-persistence-seeding`, `004-properties-list-pagination`, `005-properties-create`, `006-properties-update`, `007-properties-get-by-id` y `008-open-api`.
- La operación es pública; la autenticación y la autorización quedan fuera del alcance.
- Los estados permitidos son los ya vigentes en el catálogo: `Available`, `Rented` y `Maintenance`.
- Se permite cualquier transición entre estados, incluida la repetición del estado actual.
- Repetir el estado vigente es idempotente y no actualiza la fecha de modificación.
- La concurrencia sigue la política de última escritura gana ya aprobada para la actualización general.
- La representación pública devuelta es la misma que la de la consulta por identificador, incluida su convención de imagen absoluta o cadena vacía.
- No se modifica el comportamiento ni el contrato de salud, listado paginado, creación, actualización general ni consulta por identificador.
- Las marcas temporales se expresan en UTC.
