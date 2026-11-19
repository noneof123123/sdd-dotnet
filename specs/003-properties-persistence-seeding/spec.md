# Feature Specification: Persistencia y seed de propiedades

**Feature Branch**: `003-properties-persistence-seeding`

**Created**: 2026-11-19

**Estado**: Implementada

**Input**: User description: "Crear la especificación 003-properties-persistence-seeding para definir el modelo persistente inicial de propiedades y estados de propiedad, migración EF Core, seeding idempotente automático y manejo de archivos externos."

## Clarifications

### Session 2026-11-19

- Q: ¿Cómo debe identificar el seeder una propiedad inicial para evitar duplicados? → A: Por Id estable del archivo JSON.
- Q: ¿Qué debe hacer el seed cuando una propiedad con el mismo Id ya existe? → A: Actualizar sus datos iniciales.
- Q: ¿Qué formato debe usar la URL pública de cada imagen de propiedad? → A: `/images/properties/{archivo}`.
- Q: ¿Cómo debe identificar el seeder cada estado inicial de propiedad? → A: Por nombre único del estado.
- Q: ¿PropertyStatus debe ser un enum persistido como texto o una entidad relacional independiente? → A: Enum persistido como texto.

- `PropertyStatus` DEBE existir como enum persistido mediante conversión EF Core; no DEBE crear una tabla independiente de estados.
- `Property.Status` DEBE almacenar únicamente uno de los nombres `Available`, `Rented` o `Maintenance`.
- La URL pública persistida en `Property.ImageUrl` DEBE seguir el formato `/images/properties/{archivo}` y corresponder a una imagen disponible en el destino publicado por la API.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Preparar el modelo persistente de propiedades (Priority: P1)

Como equipo de desarrollo backend, quiero disponer de un modelo persistente completo para propiedades y sus estados, para que futuros casos de uso puedan crear, consultar, actualizar y cambiar el estado de inmuebles sin modificar la estructura base.

**Why this priority**: El modelo y su esquema son la base de todas las funcionalidades de negocio de propiedades.

**Independent Test**: Puede verificarse inspeccionando el modelo, configurando el contexto en una base de datos de pruebas y comprobando que el esquema y las restricciones esperadas existen después de la migración.

**Acceptance Scenarios**:

1. **Given** una aplicación que posee el modelo persistente de propiedades, **When** se crea o actualiza una propiedad válida, **Then** se conservan título, descripción, dirección, precio, estado, habitaciones, baños, superficie, imagen y marcas temporales.
2. **Given** una propiedad con un estado persistido, **When** EF Core carga la entidad, **Then** el estado se convierte entre su representación de texto y el valor de negocio correspondiente sin pérdida.
3. **Given** el catálogo inicial de estados, **When** se valida el modelo, **Then** existen Available, Rented y Maintenance como estados admitidos.

---

### User Story 2 - Inicializar el esquema automáticamente (Priority: P1)

Como operador de Realtor, quiero que la aplicación aplique su migración única al arrancar y complete los datos iniciales, para desplegar la base actualizada sin pasos manuales separados.

**Why this priority**: El arranque automático evita que el esquema y el seed queden desalineados entre entornos.

**Independent Test**: Puede verificarse iniciando la aplicación con una base nueva y con una base ya migrada, y comprobando que el esquema queda disponible y el seed termina sin duplicados.

**Acceptance Scenarios**:

1. **Given** una base de datos sin el esquema de propiedades, **When** arranca la aplicación, **Then** la aplicación ejecuta MigrateAsync y crea el esquema esperado.
2. **Given** una base de datos ya migrada, **When** arranca nuevamente la aplicación, **Then** la aplicación continúa el arranque y ejecuta el seeding asincrónico aunque no haya migraciones pendientes.
3. **Given** el proceso de arranque, **When** EF Core aplica la migración y ejecuta el seeding, **Then** la migración termina antes de que el seeding asincrónico acceda a los datos.

---

### User Story 3 - Cargar datos iniciales e imágenes de forma idempotente (Priority: P1)

Como responsable de datos, quiero que los estados y las propiedades iniciales se carguen desde archivos externos versionados y que sus imágenes queden consumibles por la API, para disponer de datos iniciales consistentes y reutilizables.

**Why this priority**: Los datos y activos iniciales deben ser reproducibles y no deben introducir rutas físicas específicas del entorno de desarrollo.

**Independent Test**: Puede verificarse leyendo el manifiesto, ejecutando el seed varias veces y confirmando que no aparecen duplicados y que las rutas públicas de imagen corresponden a archivos disponibles.

**Acceptance Scenarios**:

1. **Given** los JSON y el manifiesto incluidos en el proyecto, **When** inicia el seeding, **Then** la aplicación obtiene las rutas de origen desde el manifiesto y lee los datos sin rutas dispersas codificadas.
2. **Given** un seed ejecutado una o varias veces, **When** vuelven a procesarse los mismos datos, **Then** se conservan los registros existentes y no se crean duplicados ni se pierden cambios válidos posteriores.
3. **Given** una imagen de la carpeta de seed, **When** finaliza la sincronización, **Then** la imagen queda en el destino público final de la API y Property.ImageUrl contiene una ruta pública válida, nunca una ruta física de soporte.

---

### User Story 4 - Mantener compatibilidad con las capacidades existentes (Priority: P2)

Como desarrollador backend, quiero incorporar persistencia sin alterar el comportamiento de /health ni el patrón existente de descubrimiento de slices, para reducir riesgos de regresión.

**Why this priority**: Las mejoras de persistencia deben integrar la arquitectura existente sin cambios innecesarios en infraestructura ya aprobada.

**Independent Test**: Puede verificarse ejecutando las pruebas existentes de /health y verificando que el arranque, el descubrimiento de ISlice y el mapeo mantienen su comportamiento previo.

**Acceptance Scenarios**:

1. **Given** la infraestructura de slices existente, **When** se añade persistencia y seeding, **Then** /health conserva su respuesta y registro sin modificación funcional.
2. **Given** las carpetas Features, Domain e Infrastructure existentes, **When** se incorpora esta iniciativa, **Then** los casos de negocio de propiedades se organizan por slice y no se crean endpoints paralelos técnicos fuera de Features.

---

### Edge Cases

- Si el mismo registro inicial ya existe, el seeding debe actualizar únicamente los datos necesarios sin insertar duplicados.
- Si los archivos JSON son inválidos o faltan, el arranque debe finalizar sin asumir datos incompletos y debe dejar evidencia diagnóstica estructurada.
- Si una imagen referenciada por el seed no existe, el proceso no debe persistir una ruta física de soporte ni una URL pública que apunte a un archivo inexistente.
- Si no hay migraciones pendientes, MigrateAsync debe seguir retornando el control al proceso y EF Core debe ejecutar UseAsyncSeeding de todos modos.
- Si el destino público ya contiene una imagen con el mismo nombre, la sincronización debe mantener una copia consistente sin duplicar registros ni referencias.
- Si EF Core aplica varias versiones de modelo, esta iniciativa debe mantenerse cubierta por una única migración coherente.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El backend DEBE representar una propiedad persistente con identificador, título, descripción, dirección, precio, estado, cantidad de habitaciones, cantidad de baños, superficie en metros cuadrados, URL de imagen y marcas temporales de creación y actualización.
- **FR-002**: El backend DEBE representar los estados disponibles como Available, Rented y Maintenance.
- **FR-003**: El estado de una propiedad DEBE persistirse como texto y convertirse hacia y desde el valor de estado de negocio durante el acceso por EF Core.
- **FR-004**: El modelo persistente DEBE aplicar restricciones de longitud, precisión numérica, obligatoriedad y valores por defecto definidos por la configuración de persistencia de esta feature.
- **FR-005**: Las entidades persistentes DEBEN tener configuraciones separadas mediante IEntityTypeConfiguration y el contexto DEBE descubrirlas automáticamente sin configuración inline.
- **FR-006**: El contexto DEBE registrar los DbSet necesarios para el modelo persistente de propiedades.
- **FR-007**: Esta feature DEBE incluir una única migración coherente que contenga el esquema completo del modelo de propiedades y estados.
- **FR-008**: La migración DEBE poder revisarse mediante sus métodos Up y Down y su snapshot, y debe estar alineada con el modelo persistente final.
- **FR-009**: El arranque DEBE ejecutar MigrateAsync siempre, sin condicionar la llamada por la existencia previa de migraciones pendientes.
- **FR-010**: El arranque DEBE configurar y ejecutar UseSeeding y UseAsyncSeeding con una implementación sincronizada en su comportamiento.
- **FR-011**: El seeding DEBE ser idempotente y evitar duplicados ante ejecuciones sucesivas o reinicios de la aplicación.
- **FR-012**: El seeding DEBE leer properties.json, properties-statuses.json y un seed-manifest.json central mediante rutas resueltas desde el manifiesto.
- **FR-013**: El manifiesto DEBE ser la única fuente declarativa para relacionar archivos JSON e imágenes con sus destinos y rutas públicas.
- **FR-014**: Los JSON y el manifiesto DEBEN quedar disponibles en tiempo de ejecución en build y publish.
- **FR-015**: Las imágenes DEBEN sincronizarse desde soporte a la ubicación final consumible por el proyecto API y quedar disponibles en build y publish.
- **FR-016**: ImageUrl DEBE almacenar una ruta pública final servida por la API y nunca una ruta física dentro de support o del repositorio.
- **FR-017**: El seed DEBE reproducir la información inicial de estados y propiedades sin usar HasData.
- **FR-018**: El seeder manual NO DEBE invocarse desde MigrationExtensions ni como fallback manual fuera del flujo de EF Core.
- **FR-019**: Los casos de uso de propiedades DEBEN organizarse dentro de Features/Properties y no como endpoints técnicos paralelos fuera de esa estructura.
- **FR-020**: Esta feature NO DEBE alterar el comportamiento funcional existente de /health ni requerir cambios manuales en Program.cs para mapear futuros slices de propiedades.
- **FR-021**: La feature DEBE incluir pruebas unitarias de configuración EF, conversión enum-string, esquema de migración, idempotencia, ejecución asincrónica, lectura del manifiesto y sincronización de imágenes.
- **FR-022**: Esta feature NO DEBE usar controllers ni una migración separada por cada entidad.
- **FR-023**: Los archivos externos de seed DEBEN mantenerse separados del código fuente de la API y mantenerse integrados mediante las referencias de contenido del proyecto API.

### Key Entities *(include if feature involves data)*

- **Property**: Inmueble persistido con información comercial, estado, características físicas, imagen pública y marcas temporales.
- **PropertyStatus**: Estado de disponibilidad del inmueble con los valores Available, Rented y Maintenance.
- **SeedManifest**: Declaración central de rutas de datos y activos utilizada por el proceso de seed para resolver JSON e imágenes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El modelo y todas sus configuraciones compilan y permiten crear el esquema esperado sin errores de persistencia.
- **SC-002**: La revisión confirma exactamente una migración funcional para el conjunto completo de entidades de esta iniciativa y una snapshot coherente.
- **SC-003**: El 100 % de las ejecuciones repetidas del seeding no produce registros duplicados ni modifica incorrectamente registros existentes válidos.
- **SC-004**: Al arrancar con migraciones pendientes o sin migraciones pendientes, el proceso aplica el flujo de migración y ejecuta UseAsyncSeeding antes de aceptar operaciones normales.
- **SC-005**: El 100 % de las rutas JSON, manifest e imágenes probadas están disponibles en build y publish desde las ubicaciones configuradas.
- **SC-006**: Todas las propiedades sembradas utilizan URL públicas que corresponden a imágenes sincronizadas en el destino final del API.
- **SC-007**: Las pruebas requeridas pasan y cubren configuración EF, conversión enum-string, migración, idempotencia, seed asincrónico, manifiesto y imágenes.
- **SC-008**: /health y el discovery existente mantienen su comportamiento previo después de incorporar persistencia.
