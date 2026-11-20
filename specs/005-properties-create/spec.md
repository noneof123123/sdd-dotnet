# Especificación de la característica: Alta de propiedades

**Rama de característica**: `005-properties-create`

**Creado**: 2026-11-20

**Estado**: Implementada

**Entrada**: Descripción del usuario: "Crear la especificación 005-properties-create para definir un nuevo caso de uso de alta de propiedades mediante un slice Minimal API en backend. La especificación debe cubrir un endpoint público de creación que reciba los datos de la propiedad y permita de forma opcional la carga de imagen. Si no se envía imagen, la propiedad debe persistirse correctamente con imageUrl en null. Si se envía imagen válida, debe almacenarse en app/backend/src/RealtorApi/wwwroot/assets/properties y persistirse la URL pública/relativa correspondiente en la propiedad para su consumo posterior por clientes. Incluir criterios de validación funcional de la carga (formatos PNG/JPG y tamaño máximo 5MB), reglas de respuesta y errores esperados, escenarios independientes por prioridad (P1/P2/P3), requisitos funcionales testables y criterios de éxito medibles. Mantener alineación con la arquitectura existente de Vertical Slice (slice + handler + mapping + validator), auto-registro por IHandler e ISlice, y mapeos explícitos por slice sin lógica de negocio en mapping. Respetar gobernanza Spec-Driven del repositorio y dependencias con specs previas de foundation, persistencia y listado."

## Aclaraciones

### Sesión 2026-11-20

- Q: ¿Debe permitirse explícitamente `NULL` en la columna persistida de imagen? → A: Sí, la columna debe ser nullable para representar una propiedad creada sin fotografía.
- Los formatos admitidos son PNG y JPG/JPEG, el tamaño máximo efectivo es 5 MB y una propiedad creada sin imagen conserva `imageUrl` con valor nulo.
- El valor devuelto por el alta será una representación pública de la propiedad creada y conservará los datos comerciales y físicos registrados.
- Q: ¿Cuál debe ser la ubicación HTTP que indique el recurso recién creado? → A: HTTP 201 con `Location: /api/properties/{id}` y el cuerpo de la propiedad creada.
- Q: ¿Cómo deben enviarse los datos y la imagen opcional al endpoint? → A: Mediante campos individuales en una solicitud `multipart/form-data`.
- Q: ¿Qué respuesta debe recibir el cliente ante fallos internos de almacenamiento o persistencia? → A: HTTP 500 con `ProblemDetails`, sin exponer detalles técnicos ni resultados parciales.

## Escenarios de usuario y pruebas *(obligatorio)*

### Historia de usuario 1 - Crear una propiedad sin imagen (Prioridad: P1)

Como consumidor público de la API, quiero registrar una propiedad con sus datos obligatorios sin adjuntar una imagen, para poder crear el inmueble aunque todavía no exista una fotografía disponible.

**Por qué esta prioridad**: Es el flujo principal de creación y debe funcionar aunque la imagen sea opcional.

**Prueba independiente**: Puede probarse enviando una solicitud válida sin parte de imagen y verificando que se persiste una propiedad nueva y que la respuesta indica `imageUrl` como nulo.

**Escenarios de aceptación**:

1. **Dado** una propiedad con todos sus datos válidos y sin imagen, **Cuando** se solicita su creación, **Entonces** se persiste una propiedad nueva y se devuelve una respuesta exitosa con `imageUrl` igual a nulo.
2. **Dado** una creación sin imagen, **Cuando** se consulta posteriormente el listado existente, **Entonces** la propiedad aparece con su identificador y datos persistidos.
3. **Dado** una creación sin imagen, **Cuando** se recibe la respuesta, **Entonces** no se crea ni referencia ningún archivo de imagen.

---

### Historia de usuario 2 - Crear una propiedad con una imagen válida (Prioridad: P1)

Como consumidor público de la API, quiero adjuntar una imagen PNG o JPG válida durante la creación, para que la propiedad tenga una fotografía disponible inmediatamente.

**Por qué esta prioridad**: El alta con imagen aporta el contenido visual requerido por el catálogo y completa el contrato principal de creación.

**Prueba independiente**: Puede probarse enviando una propiedad válida con una imagen PNG o JPG de hasta 5 MB y verificando el archivo almacenado, la ruta persistida y la respuesta pública.

**Escenarios de aceptación**:

1. **Dado** una propiedad válida y una imagen PNG o JPG de tamaño menor o igual a 5 MB, **Cuando** se solicita la creación, **Entonces** se almacena el archivo en el destino público de imágenes y se persiste una URL relativa válida en la propiedad.
2. **Dado** una creación con imagen válida, **Cuando** se recibe la respuesta, **Entonces** `imageUrl` contiene la ruta relativa pública correspondiente al archivo almacenado.
3. **Dado** una propiedad creada con imagen válida, **Cuando** se solicita desde el listado, **Entonces** el cliente puede consumir la URL pública de la imagen sin conocer rutas físicas del servidor.

---

### Historia de usuario 3 - Rechazar datos o imágenes inválidos (Prioridad: P2)

Como consumidor de la API, quiero recibir errores claros y estructurados ante datos incompletos o archivos no admitidos, para corregir la solicitud antes de crear un registro.

**Por qué esta prioridad**: Una validación consistente evita propiedades incompletas y archivos incompatibles o excesivamente grandes.

**Prueba independiente**: Puede probarse enviando datos inválidos, formatos distintos de PNG/JPG, archivos mayores de 5 MB o solicitudes mal formadas y verificando HTTP 400 sin persistencia ni archivo huérfano.

**Escenarios de aceptación**:

1. **Dado** que faltan datos obligatorios o hay valores fuera de sus reglas, **Cuando** se solicita la creación, **Entonces** se devuelve HTTP 400 con detalles estructurados y no se crea la propiedad.
2. **Dado** una imagen cuyo formato no sea PNG ni JPG/JPEG, **Cuando** se solicita la creación, **Entonces** se devuelve HTTP 400 y no se persiste ni almacena la propiedad.
3. **Dado** una imagen superior a 5 MB, **Cuando** se solicita la creación, **Entonces** se devuelve HTTP 400 y no se persiste ni almacena la propiedad.
4. **Dado** una solicitud con múltiples archivos u otro archivo inválido en la parte de imagen, **Cuando** se procesa, **Entonces** se rechaza como solicitud inválida sin efectos parciales.

---

### Historia de usuario 4 - Mantener compatibilidad con el sistema existente (Prioridad: P3)

Como responsable de backend, quiero incorporar el alta sin alterar `/health`, el listado existente, el descubrimiento de casos de uso ni el comportamiento de persistencia aprobado por las iniciativas previas.

**Por qué esta prioridad**: La creación debe integrarse en la arquitectura existente sin introducir registros paralelos ni regresiones.

**Prueba independiente**: Puede verificarse ejecutando las pruebas existentes de salud, listado y persistencia después de incorporar el alta y comprobando que siguen pasando.

**Escenarios de aceptación**:

1. **Dado** la infraestructura de slices existente, **Cuando** se incorpora el endpoint de creación, **Entonces** `/health` conserva su respuesta y registro.
2. **Dado** las funcionalidades de persistencia y listado existentes, **Cuando** se crea una propiedad válida, **Entonces** el registro puede ser leído por el listado sin cambios incompatibles.
3. **Dado** la aplicación configurada con descubrimiento automático, **Cuando** arranca, **Entonces** el alta queda registrada junto con los demás casos de uso sin registro manual adicional.

---

## Casos límite

- Si se cancela la operación durante la creación o el almacenamiento, no debe quedar una propiedad persistida ni un archivo huérfano sin la asociación correspondiente.
- Si el almacenamiento falla después de validar datos e imagen, no debe existir una propiedad persistida nueva ni una URL registrada para un archivo inexistente.
- Si se produce un fallo al persistir después de almacenar el archivo, debe intentarse eliminar el archivo temporal o asociado para evitar archivos huérfanos.
- Si el nombre del archivo recibido es vacío, peligroso o contiene caracteres no seguros, debe reemplazarse por un identificador generado seguro sin exponer la ruta original.
- Si una imagen declara PNG/JPG pero su contenido no corresponde al formato declarado, debe considerarse inválida y rechazarse.
- Si no se adjunta imagen, el campo persistido `imageUrl` debe ser nulo, distinto de una cadena vacía.
- Si existen propiedades con imágenes del seed en `/images/properties` y propiedades creadas en `/assets/properties`, ambos contratos deben seguir siendo consumibles sin cambios incompatibles en el listado.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El backend DEBE proporcionar un endpoint público HTTP para crear una propiedad con sus datos comerciales y físicos.
- **FR-002**: La solicitud de creación DEBE aceptar título, descripción, dirección, precio, estado, cantidad de dormitorios, cantidad de baños y superficie en metros cuadrados mediante campos individuales en `multipart/form-data`.
- **FR-003**: La solicitud de creación DEBE aceptar opcionalmente una sola imagen; la ausencia de imagen DEBE ser válida.
- **FR-004**: Cuando no se envíe una imagen, la propiedad creada DEBE persistirse con `imageUrl` en nulo y su columna persistida DEBE ser nullable.
- **FR-005**: Cuando se envíe una imagen, el formato DEBE validarse como PNG o JPG/JPEG por tipo declarado y contenido compatible.
- **FR-006**: Una imagen DEBE rechazarse cuando exceda 5 MB como máximo inclusivo.
- **FR-007**: Una imagen válida DEBE almacenarse bajo el destino público `app/backend/src/RealtorApi/wwwroot/assets/properties` y en el destino publicado correspondiente.
- **FR-008**: Una propiedad creada con imagen válida DEBE persistir una URL relativa que siga el patrón público `/assets/properties/{nombreSeguro}`.
- **FR-009**: El nombre persistido y público DEBE ser seguro, único y NO DEBE exponer rutas físicas del servidor ni el nombre original peligroso.
- **FR-010**: Los datos de la propiedad creada DEBEN validar título, descripción, dirección, precio, estado y cantidades antes de iniciar cualquier persistencia o almacenamiento.
- **FR-011**: El título, la descripción y la dirección DEBEN cumplir las restricciones ya definidas por el modelo persistente vigente; el precio y las cantidades físicas NO DEBEN aceptar valores negativos.
- **FR-012**: El estado DEBE ser uno de los valores `Available`, `Rented` o `Maintenance`, conforme a la persistencia existente.
- **FR-013**: Ante validación fallida, inexistencia de un recurso requerido o solicitud mal formada, el endpoint DEBE devolver HTTP 400 o el código esperado por el tipo de error, con ProblemDetails o ValidationProblemDetails estructurados.
- **FR-014**: Ante creación exitosa sin imagen o con imagen válida, el endpoint DEBE devolver HTTP 201, incluir `Location: /api/properties/{id}` y una representación pública de la propiedad creada.
- **FR-015**: La representación de respuesta NO DEBE exponer entidades persistentes ni rutas físicas internas.
- **FR-016**: El flujo exitoso DEBE conservar consistencia entre el registro persistido y el archivo almacenado; no DEBE dejar el registro apuntando a un archivo inexistente.
- **FR-017**: Ante fallos internos de almacenamiento o persistencia, el endpoint DEBE devolver HTTP 500 con `ProblemDetails`, sin exponer detalles técnicos ni resultados parciales, y eliminar o impedir efectos parciales observables siempre que sea seguro hacerlo.
- **FR-018**: El caso de uso DEBE seguir la Vertical Slice Architecture existente: organizar endpoint, handler, mapping y validator dentro del slice de creación.
- **FR-019**: El handler DEBE contener la lógica del caso de uso y el endpoint y el mapping NO DEBEN contener lógica de negocio o decisiones de persistencia.
- **FR-020**: El mapping NO DEBE implementar validación, almacenar archivos ni modificar entidades persistentes.
- **FR-021**: El endpoint DEBE participar en el auto-registro mediante `ISlice` y el handler mediante `IHandler`, sin controllers ni registros manuales paralelos.
- **FR-022**: La operación DEBE propagar el token de cancelación desde la solicitud hasta las operaciones de almacenamiento y persistencia compatibles con cancelación.
- **FR-023**: Las pruebas requeridas DEBEN cubrir creación sin imagen, creación con PNG, creación con JPG/JPEG, datos inválidos, formato no permitido, tamaño excedido, fallos parciales y compatibilidad con `/health` y el listado.
- **FR-024**: Esta iniciativa NO DEBE agregar actualización, eliminación, autorización ni otros casos de uso no descritos en esta especificación.
- **FR-025**: Esta iniciativa NO DEBE modificar el comportamiento ni el contrato del endpoint de listado paginado aprobado en la especificación previa.
- **FR-026**: Esta iniciativa DEBE ser compatible con el modelo persistente y las convenciones establecidas por las especificaciones 001, 002, 003 y 004.

### Entidades clave *(incluir si la característica implica datos)*

- **Property**: Entidad persistente existente que representa el inmueble creado y contiene sus datos comerciales, físicos, estado, imagen opcional y marcas temporales.
- **CreatePropertyRequest**: Datos recibidos para crear una propiedad y la imagen opcional adjunta.
- **CreatePropertyResponse**: Representación pública explícita de la propiedad creada, sin exponer el modelo persistente.
- **PropertyImage**: Archivo opcional validado para asociar a una propiedad creada y publicar bajo una ruta pública segura.

## Criterios de éxito *(obligatorio)*

### Resultados medibles

- **SC-001**: El 100 % de las solicitudes válidas sin imagen crea una propiedad y devuelve HTTP 201 con `imageUrl` nulo.
- **SC-002**: El 100 % de las solicitudes válidas con PNG o JPG/JPEG de hasta 5 MB crea el registro y deja exactamente un archivo correspondiente en la ubicación pública segura.
- **SC-003**: El 100 % de las imágenes inválidas probadas, ya sea por formato, contenido declarado incompatible o tamaño superior a 5 MB, devuelve HTTP 400 sin crear propiedad ni archivo.
- **SC-004**: El 100 % de las solicitudes con datos obligatorios inválidos devuelve HTTP 400 estructurado sin persistir ni almacenar efectos parciales.
- **SC-005**: Las propiedades creadas con o sin imagen quedan visibles y compatibles con el listado existente en la siguiente consulta exitosa.
- **SC-006**: Las pruebas funcionales cubren independientemente P1, P2 y P3, y el conjunto requerido pasa sin fallos bloqueantes.
- **SC-007**: `/health` y el discovery existente mantienen su comportamiento después de incorporar el caso de uso de alta.
- **SC-008**: Ante errores de persistencia o almacenamiento probados, el porcentaje de registros o archivos huérfanos observables tras la respuesta es 0 %.
- **SC-009**: Ninguna respuesta ni ruta persistida expone rutas físicas del servidor ni nombres originales inseguros.
- **SC-010**: El 100 % de las respuestas exitosas contiene HTTP 201, el encabezado `Location: /api/properties/{id}` y el cuerpo de la propiedad creada.
- **SC-011**: El 100 % de los fallos internos probados de almacenamiento o persistencia devuelve HTTP 500 con `ProblemDetails` sin exponer detalles técnicos ni dejar efectos parciales observables.

## Supuestos

- La especificación utiliza PNG y JPG/JPEG como formatos admitidos; ambos deben validarse por contenido compatible además de metadatos declarados.
- 5 MB significa $5 \times 1024 \times 1024$ bytes como límite máximo inclusivo.
- Se considera una respuesta pública de creación con identificador, datos comerciales y físicos, estado e `imageUrl` nulo o relativo.
- La propiedad persistida puede utilizar `imageUrl` nulo aunque la implementación existente de persistencia haya requerido inicialmente una cadena no nula; la migración o configuración necesaria deberá alinearse sin cambiar el contrato aprobado del listado.
- El destino público `/assets/properties/` convive con el destino `/images/properties/` utilizado por seed y con las rutas ya expuestas por el listado.
- Esta especificación no define autenticación, permisos ni identidad del solicitante; el endpoint se considera público según el alcance indicado.
