# Feature Specification: Base de la solución Realtor

**Feature Branch**: `001-solution-foundation`

**Created**: 2026-11-19

**Estado**: Borrador

**Input**: User description: "Crear la especificación para la iniciativa 001-solution-foundation"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Preparar el repositorio para desarrollar Realtor (Priority: P1)

Como responsable técnico del proyecto, quiero disponer de una solución compartida con backend, frontend y sus respectivos proyectos de pruebas, para poder comenzar futuras iniciativas sin reestructurar el repositorio.

**Why this priority**: Sin una estructura base compilable, ninguna iniciativa posterior puede implementarse ni verificarse de forma coherente.

**Independent Test**: Puede verificarse creando la solución, restaurando las dependencias y compilando todos los proyectos incluidos sin ejecutar lógica de negocio.

**Acceptance Scenarios**:

1. **Given** un repositorio que contiene el archivo global.json válido, **When** se prepara la iniciativa, **Then** la solución principal se encuentra en app/Realtor.sln y todos los proyectos definidos pertenecen a esa solución.
2. **Given** la solución preparada, **When** se ejecuta la restauración y la compilación, **Then** el backend, el frontend y los proyectos de pruebas se compilan correctamente.
3. **Given** la estructura preparada, **When** se revisa el backend, **Then** no existe lógica de negocio ni entidades de dominio implementadas.

---

### User Story 2 - Establecer los puntos de inicio base de backend y frontend (Priority: P2)

Como desarrollador, quiero contar con aplicaciones backend y frontend iniciales configuradas con sus servicios y estructura básica, para trabajar sobre convenciones arquitectónicas comunes desde el primer desarrollo.

**Why this priority**: Los puntos de entrada base permiten introducir de forma controlada servicios, middleware y endpoints futuros sin mezclar responsabilidades.

**Independent Test**: Puede verificarse revisando que las aplicaciones de inicio contienen únicamente configuración base y que el backend utiliza el modelo de endpoints apropiado.

**Acceptance Scenarios**:

1. **Given** el proyecto backend inicial, **When** se revisa su configuración de arranque, **Then** Program.cs se limita a servicios, middleware y mapeo inicial de endpoints.
2. **Given** el proyecto frontend inicial, **When** se revisa su punto de entrada, **Then** está preparado como una aplicación Blazor Web App basada en Razor Components.
3. **Given** cualquier proyecto .NET de esta iniciativa, **When** se determina su versión de SDK, **Then** se deriva del archivo global.json existente.

---

### User Story 3 - Incorporar pruebas de infraestructura desde el inicio (Priority: P3)

Como equipo de calidad, quiero disponer de proyectos de pruebas separados para backend y frontend, para que las futuras funcionalidades puedan validar cada capa de forma independiente.

**Why this priority**: Separar las pruebas desde la fundación evita mezclar tipos de proyectos y facilita la trazabilidad de cada capa.

**Independent Test**: Puede verificarse que ambos proyectos de pruebas existen dentro de la solución y pueden reconocerse y compilarse como parte del proyecto correspondiente.

**Acceptance Scenarios**:

1. **Given** la solución principal, **When** se revisan sus proyectos, **Then** existen las ubicaciones destinadas a las pruebas del backend y del frontend.
2. **Given** los proyectos de pruebas, **When** se restaura la solución, **Then** quedan asociados a sus respectivas aplicaciones sin introducir lógica de negocio.

### Edge Cases

- ¿Qué ocurre si global.json no existe al iniciar la iniciativa? La iniciativa debe detenerse sin crear ni modificar archivos de especificación ni proyectos, mostrando el error bloqueante definido.
- ¿Qué ocurre si se detectan controllers en el backend? Se considera una violación de la estructura fundacional y debe eliminarse la implementación correspondiente.
- ¿Qué ocurre si una iniciativa posterior añade entidades de dominio en esta fase? Debe rechazarse por exceder el alcance explícito de Foundation.
- ¿Qué ocurre si global.json contiene una versión distinta a la documentación del proyecto? global.json prevalece como fuente de verdad para los proyectos .NET.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: La solución principal DEBE estar ubicada en app/Realtor.sln.
- **FR-002**: La solución DEBE incluir un proyecto backend ubicado en app/backend/src/RealtorApi/.
- **FR-003**: El backend DEBE estar configurado como una aplicación ASP.NET Core basada en Minimal APIs.
- **FR-004**: El backend NO DEBE contener controllers.
- **FR-005**: La solución DEBE incluir un proyecto de pruebas backend ubicado en app/backend/tests/RealtorApiTests/.
- **FR-006**: La solución DEBE incluir un proyecto frontend ubicado en app/frontend/src/RealtorWeb/.
- **FR-007**: El frontend DEBE estar configurado como una aplicación Blazor Web App con Razor Components.
- **FR-008**: La solución DEBE incluir un proyecto de pruebas frontend ubicado en app/frontend/test/RealtorWeb/.
- **FR-009**: Program.cs del backend y del frontend DEBE contener únicamente configuración base aprobada para servicios, middleware y mapeo inicial de endpoints cuando aplique.
- **FR-010**: Esta iniciativa NO DEBE implementar lógica de negocio ni features.
- **FR-011**: Esta iniciativa NO DEBE crear entidades de dominio.
- **FR-012**: Esta iniciativa NO DEBE crear ni modificar global.json.
- **FR-013**: La versión de .NET utilizada por los proyectos futuros DEBE derivarse del contenido existente de global.json.
- **FR-014**: La solución resultante DEBE restaurar y compilar correctamente todos sus proyectos.
- **FR-015**: Todos los proyectos creados por esta iniciativa DEBEN pertenecer a app/Realtor.sln.

### Key Entities *(include if feature involves data)*

- **No aplica**: Esta iniciativa no introduce entidades de dominio ni modelos de persistencia.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de los proyectos definidos por esta iniciativa están presentes en sus rutas requeridas y forman parte de app/Realtor.sln.
- **SC-002**: Una restauración completa seguida de una compilación de la solución termina sin errores.
- **SC-003**: Una revisión arquitectónica confirma que el backend no contiene controllers y que no existe lógica de negocio ni entidades de dominio.
- **SC-004**: Una revisión de configuración confirma que todos los proyectos .NET utilizan la versión indicada por global.json sin haberlo modificado.
- **SC-005**: Los proyectos de pruebas backend y frontend están reconocidos por la solución y pueden compilarse junto al resto de proyectos.

## Assumptions

- El archivo global.json existe en la raíz del repositorio antes de iniciar esta iniciativa y no se modifica durante ella.
- La solución se crea en el repositorio compartido y no introduce estructuras paralelas.
- Los proyectos de pruebas pueden estar inicialmente vacíos o contener únicamente la estructura mínima necesaria para compilar.
- La implementación posterior se realizará mediante tareas trazables y respetará el orden de las fases Spec-Driven Development.
