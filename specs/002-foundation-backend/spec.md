# Feature Specification: Fundación interna del backend

**Feature Branch**: `002-foundation-backend`

**Created**: 2026-11-19

**Estado**: Implementada

**Input**: User description: "Crear la especificación 002-foundation-backend para establecer la infraestructura transversal del backend sin features de producto."

## Clarifications

### Session 2026-11-19

- Q: ¿Qué forma debe tener la respuesta HTTP de `/health`? → A: HTTP 200 con `{ "status": "healthy" }`.
- Q: ¿Qué categorías de error esperado debe cubrir inicialmente el conversor centralizado de `Result` a `ProblemDetails`? → A: Validación 400, no encontrado 404 y conflicto 409.
- Q: ¿Qué contenido debe incluir `ProblemDetails` para los errores 404 y 409? → A: `title`, `detail` y `status`, sin detalles internos ni datos sensibles.
- Q: ¿Dónde debe iniciar el logging estructurado para los casos futuros? → A: En la infraestructura compartida, mediante `ILogger` y eventos con nombre estable.
- Q: ¿Qué debe validar la suite para el marker interface `IHandler`? → A: Contrato existente y descubrimiento mediante assembly scanning sin duplicados, sin handlers concretos de negocio.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Descubrir endpoints sin modificar el arranque (Priority: P1)

Como desarrollador backend, quiero que los endpoints Minimal API se descubran automáticamente mediante el contrato ISlice, para agregar sondas o funcionalidades futuras sin editar Program.cs.

**Why this priority**: El descubrimiento centralizado es la base arquitectónica que evita registries manuales y mantiene el arranque limitado a configuración.

**Independent Test**: Puede verificarse creando una clase ISlice de prueba en el proyecto de pruebas, registrando el assembly correspondiente y comprobando que el endpoint queda asociado una sola vez sin duplicados.

**Acceptance Scenarios**:

1. **Given** una clase de prueba que implementa ISlice en el assembly de pruebas, **When** se ejecuta el descubrimiento de slices, **Then** la clase se registra bajo el contrato ISlice una sola vez.
2. **Given** endpoints implementados como ISlice, **When** se invoca el mapeo centralizado, **Then** todos quedan disponibles sin cambios manuales en Program.cs.
3. **Given** que el assembly de producción contiene el slice de /health, **When** se inicia la aplicación, **Then** /health queda descubierto y responde correctamente sin registro manual.

---

### User Story 2 - Validar requests automáticamente (Priority: P1)

Como consumidor de la API, quiero que los requests con validadores registrados reciban errores estructurados y consistentes, para corregir entradas inválidas sin interpretar respuestas ad hoc.

**Why this priority**: La validación transversal debe estar disponible desde la fundación y aplicar la misma convención a todos los endpoints futuros.

**Independent Test**: Puede verificarse mediante validators y requests de prueba que ejerciten tanto la presencia como la ausencia de IValidator<T>.

**Acceptance Scenarios**:

1. **Given** un tipo de request con IValidator<T> registrado, **When** llega una solicitud válida, **Then** la validación termina sin errores y el endpoint continúa.
2. **Given** un tipo de request con IValidator<T> registrado, **When** llega una solicitud inválida, **Then** la respuesta utiliza ValidationProblemDetails con HTTP 400.
3. **Given** un tipo de request sin validator registrado, **When** llega una solicitud, **Then** el procesamiento continúa sin errores por ausencia de validación.

---

### User Story 3 - Comunicar errores esperados y registrar eventos base (Priority: P2)

Como equipo que desarrolla y opera Realtor, quiero una representación común de errores esperados y logging estructurado, para que las respuestas y diagnósticos futuros sean consistentes.

**Why this priority**: Un contrato común de error evita implementación divergente en futuras features y permite observabilidad desde el arranque.

**Independent Test**: Puede verificarse convirtiendo un resultado de error de prueba a ProblemDetails y comprobando el código HTTP y el payload esperado.

**Acceptance Scenarios**:

1. **Given** un Result que representa un error esperado, **When** se convierte a respuesta HTTP, **Then** el status code corresponde al tipo de error.
2. **Given** un error esperado convertido a ProblemDetails, **When** se inspecciona la respuesta, **Then** el payload incluye título, detalle y metadatos consistentes esperados por la API.
3. **Given** un evento base registrado por la infraestructura, **When** ocurre durante una operación, **Then** el sistema utiliza logging estructurado mediante ILogger sin exponer datos sensibles.

---

### User Story 4 - Registrar casos de uso sin enlazado manual (Priority: P2)

Como desarrollador backend, quiero que los handlers futuros se auto-registren mediante el marker IHandler y el assembly scanning, para incorporar casos de uso sin listas manuales de servicios.

**Why this priority**: Esta capacidad prepara la infraestructura para Vertical Slice Architecture sin crear handlers de negocio durante la fundación.

**Independent Test**: Puede verificarse en pruebas que el contrato IHandler y el mecanismo de descubrimiento existen y que no requiere clases concretas en producción.

**Acceptance Scenarios**:

1. **Given** el assembly de producción preparado para scanning, **When** se registra la infraestructura, **Then** no hay handlers concretos ni duplicados de registro introducidos por esta iniciativa.
2. **Given** que una futura clase implementa IHandler, **When** el assembly se vuelve a escanear, **Then** puede descubrirse sin modificar el registro manual de Program.cs.

---

### User Story 5 - Mantener /health como demostración end to end (Priority: P1)

Como responsable técnico, quiero migrar la sonda existente /health al patrón ISlice, para demostrar el descubrimiento, el mapeo y la respuesta HTTP sin tocar Program.cs para endpoints individuales.

**Why this priority**: /health es la única funcionalidad end to end necesaria para verificar la infraestructura sin introducir alcance de negocio.

**Independent Test**: Puede verificarse iniciando el servidor de pruebas y solicitando /health, o comprobando el slice y su respuesta en el harness correspondiente.

**Acceptance Scenarios**:

1. **Given** la aplicación iniciada, **When** se solicita /health, **Then** responde correctamente con el estado disponible de la sonda.
2. **Given** el slice de /health, **When** se revisa su integración, **Then** está registrado como ISlice y no requiere configuración manual en Program.cs.

### Edge Cases

- Si no existe un validator para el tipo recibido, el procesamiento debe continuar sin registrar validación fallida ni lanzar un error.
- Si el assembly no contiene clases ISlice, el descubrimiento debe finalizar sin registrar duplicados ni fallar el arranque.
- Si un resultado contiene varios errores esperados, la respuesta debe conservar una representación consistente y no exponer detalles internos.
- Si /health no puede determinar un estado determinado, debe conservar una respuesta estable y explícita sin añadir dependencias de persistencia.
- Si se detectan controllers, deben rechazarse porque esta iniciativa exige Minimal APIs.
- Si se crean entidades, persistencia o features de negocio, deben rechazarse por superar el alcance explícito.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El backend DEBE mantener la estructura interna de Vertical Slice Architecture y una organización transversal bajo Infrastructure para endpoints, validación y handlers.
- **FR-002**: El backend DEBE definir un contrato ISlice que permita mapear endpoints sobre IEndpointRouteBuilder.
- **FR-003**: El backend DEBE descubrir implementaciones de ISlice mediante assembly scanning y registrarlas bajo el contrato ISlice sin duplicados.
- **FR-004**: El backend DEBE proporcionar una operación centralizada que mapee todos los ISlice descubiertos sin que agregar un endpoint requiera modificar Program.cs.
- **FR-005**: El descubrimiento de endpoints NO DEBE depender de nombres de clases ni de registries manuales.
- **FR-006**: El backend DEBE definir un marker interface IHandler para el auto-registro de handlers futuros mediante assembly scanning.
- **FR-007**: El auto-registro de IHandler NO DEBE requerir ninguna clase IHandler concreta en el proyecto de producción ni registrar handlers individualmente en Program.cs.
- **FR-008**: El backend DEBE registrar validators disponibles mediante assembly scanning.
- **FR-009**: La infraestructura de validación DEBE detectar IValidator<T> por convención para el tipo request correspondiente.
- **FR-010**: Si no existe IValidator<T> para un tipo request, la validación DEBE hacer pass-through sin error.
- **FR-011**: Un fallo de validación DEBE producir ValidationProblemDetails con HTTP 400 y detalles consistentes de los campos inválidos.
- **FR-012**: Los validators, filtros y handlers futuros NO DEBEN registrarse de forma individual en Program.cs.
- **FR-013**: El backend DEBE disponer de una representación común de Result para errores esperados.
- **FR-014**: El backend DEBE convertir errores esperados representados por Result a ProblemDetails con el status code correcto y un payload consistente; las categorías iniciales serán validación con HTTP 400, no encontrado con HTTP 404 y conflicto con HTTP 409. Las respuestas 404 y 409 DEBEN incluir title, detail y status sin revelar detalles internos ni datos sensibles.
- **FR-015**: Las operaciones base DEBEN utilizar logging estructurado mediante ILogger desde la infraestructura compartida, con eventos de nombre estable y sin registrar datos sensibles.
- **FR-016**: El endpoint `/health` existente DEBE migrarse a una clase ISlice y descubrirse mediante el mecanismo centralizado; DEBE responder HTTP 200 con el payload `{ "status": "healthy" }`.
- **FR-017**: /health DEBE responder con el estado disponible sin depender de persistencia ni de entidades de dominio.
- **FR-018**: Program.cs DEBE limitarse a configurar servicios, middleware, registrar la infraestructura y invocar el mapeo centralizado de endpoints.
- **FR-019**: Las clases ISlice y los validators utilizados exclusivamente para pruebas DEBEN ubicarse en el proyecto de pruebas y NO en producción.
- **FR-020**: Los tests de backend DEBEN verificar el descubrimiento sin duplicados de ISlice, validación con y sin validator, respuesta HTTP 400 ante fallo, conversión de Result a ProblemDetails y /health.
- **FR-021**: Esta iniciativa NO DEBE crear entidades de dominio, AppDbContext, configuraciones EF Core, migraciones, seeders ni conexión a base de datos.
- **FR-022**: Esta iniciativa NO DEBE crear endpoints, handlers, validators ni mappings de negocio.
- **FR-023**: Esta iniciativa NO DEBE usar controllers ni modificar global.json.
- **FR-024**: El proyecto DEBE derivar su plataforma de global.json existente sin cambiarlo.
- **FR-025**: Las pruebas DEBEN validar que el contrato IHandler existe y que sus implementaciones de prueba se descubren mediante assembly scanning sin duplicados, sin crear handlers concretos de negocio en el proyecto de producción.

### Key Entities *(include if feature involves data)*

- **No aplica**: Esta iniciativa no introduce entidades de dominio ni modelos de persistencia.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El proyecto backend compila sin errores después de registrar toda la infraestructura transversal definida.
- **SC-002**: /health se descubre como ISlice y responde correctamente sin una llamada manual a su endpoint desde Program.cs.
- **SC-003**: El 100 % de los tipos request probados con IValidator<T> resultan validados; los tipos sin validator completan el pass-through sin error.
- **SC-004**: Las respuestas de validación fallida tienen HTTP 400 y ValidationProblemDetails con el contenido de errores esperado en el 100 % de los casos probados.
- **SC-005**: Los tests de conversión de Result producen el status code y payload correctos en el 100 % de los casos definidos.
- **SC-006**: Los tests requeridos de discovery, validación, errores y /health pasan.
- **SC-007**: Una revisión confirma que no existen entidades, persistencia, features ni casos de uso de negocio en el backend creado por esta iniciativa.

## Assumptions

- La aplicación base de la spec 001 ya existe y esta iniciativa se limita a ampliar su backend.
- La infraestructura compartida se implementará en carpetas transversales reales, sin convertirlas en un registry manual de features.
- FluentValidation, Microsoft ASP.NET Core y las herramientas de pruebas disponibles pueden satisfacerse mediante referencias del proyecto sin modificar global.json.
- Los tests requieren únicamente unit tests y no un servidor de integración ni Testcontainers, salvo que una planificación posterior amplíe explícitamente ese alcance.
- El estado de /health puede representarse como una sonda disponible de forma determinista, sin base de datos ni componentes externos.
