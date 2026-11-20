# Investigación: Consulta de propiedad por identificador

**Fecha**: 2026-11-20

**Objetivo**: Resolver las incógnitas técnicas y de contrato necesarias para pasar del spec 007 al diseño e implementación, sin modificar las capacidades ya entregadas.

**Estado de clarificaciones**: no quedan incógnitas pendientes.

## 1. Operación y descubrimiento

- **Decisión**: Agregar `GET /api/properties/{id}` como un slice `GetPropertyById` dentro de `Features/Properties`, registrado por el descubrimiento automático de `ISlice`/`IHandler`.
- **Rationale**: Es el patrón consolidado en 004, 005 y 006; `Program.cs` solo configura infraestructura y `MapSliceEndpoints()`, por lo que no debe editarse para registrar la nueva operación.
- **Alternativas consideradas**: Controlador tradicional (rechazado por la constitución); registro manual de ruta en `Program.cs` (rechazado por duplicar el mecanismo existente); reutilizar el slice de listado con filtro (rechazado por mezclar contratos de colección y detalle).

## 2. Forma del request y validación del identificador

- **Decisión**: Usar un request explícito con `[AsParameters]` y `[FromRoute(Name = "id")] Guid Id`, con un validator que exija `Id != Guid.Empty`.
- **Rationale**: El enlace a `Guid` rechaza valores mal formados antes del handler; el validator añade la regla para el identificador vacío y se integra con `ValidationFilterFactory`, igual que en `UpdateProperty`.
- **Alternativas consideradas**: Validar manualmente dentro del slice (rechazado; rompe la validación centralizada del pipeline); aceptar `string` y convertir en el handler (rechazado; traslada reglas al handler y debilita el contrato); path sin segmento `id` (no entra en el endpoint y queda resuelto por el routing como 404, documentándolo como caso de routing, no como validación de la operación).

## 3. Respuesta pública y coherencia con el catálogo

- **Decisión**: Devolver `GetPropertyByIdResponse` con los diez campos públicos del listado más `createdAt` y `updatedAt`. Los diez compartidos conservan nombre, tipo y significado; `status` se expone como texto; `updatedAt` admite `null`.
- **Rationale**: La spec 007 FR-004 y su clarificación exigen marcas temporales; el prompt operativo de la iniciativa enumera los diez campos de 004, por lo que se resuelve manteniendo esos diez intactos y añadiendo solo las marcas exigidas por la spec, que es la fuente de verdad del flujo. El detalle no incluye metadatos de paginación.
- **Alternativas consideradas**: Copiar solo los diez campos del listado (rechazado; incumple FR-004); reutilizar `UpdatePropertyResponse` (rechazado; usa `imageUrl` relativa/null y omite marcas); devolver la entidad persistente (rechazado por la constitución); heredar DTO del listado (rechazado; se prefieren contratos explícitos por slice).

## 4. Representación de `imageUrl`

- **Decisión**: Aplicar exactamente el algoritmo del listado: extraer un nombre de archivo seguro, construir `{scheme}://{host}/assets/properties/{fileName}` y devolver cadena vacía si no existe imagen válida.
- **Rationale**: Es la convención fijada por la clarificación de la spec 007 y evita que el consumidor del detalle necesite conversiones distintas de las del listado. El valor persistido sigue siendo relativo o nulo; solo cambia la representación de salida.
- **Alternativas consideradas**: Copiar la representación relativa/null de create/update (rechazado; contradice la clarificación); unificar los tres endpoints a absoluta/cadena vacía (rechazado; modificaría specs implementadas y FR-014); persistir URLs absolutas o migrar datos (rechazado; acopla al host y sale del alcance); devolver `imagePath` adicional (rechazado; amplía el contrato sin beneficio).

## 5. Reutilización del resolvedor de URL

- **Decisión**: Replicar el resolvedor dentro de `GetPropertyByIdMapping` y cubrirlo con pruebas de equivalencia frente a `ListPropertiesMapping`; no extraer todavía un helper compartido.
- **Rationale**: El helper exigiría editar `ListPropertiesMapping.cs`, una feature ya aprobada e implementada. La duplicación es pequeña, está acotada a un slice y queda verificada por comparación directa de resultados, minimizando el riesgo de regresión en 004.
- **Alternativas consideradas**: Extraer `Shared/PublicImageUrl` y refactorizar listado y detalle (diferido; mayor superficie de cambio); llamar al handler de listado desde el detalle (rechazado; acoplamiento entre slices); configurar una base URL global (rechazado; ignora el host real de la solicitud).

## 6. Acceso a datos y solo lectura

- **Decisión**: El handler consulta `AppDbContext.Properties` con `AsNoTracking()` y `FirstOrDefaultAsync`, propagando `CancellationToken`. No publica ni retira archivos, no llama a `SaveChanges` y no muta entidades ni marcas temporales.
- **Rationale**: Cumple FR-008 y FR-013. Una sola lectura por identificador es suficiente para SC-003 (95 % en menos de 1 segundo) y evita sobrecarga de seguimiento de cambios.
- **Alternativas consideradas**: Cargar tracking y proyectar después (rechazado; innecesario y abre riesgo de escritura); repositorio genérico (rechazado; añade una capa trivial); caché (diferido; no necesario para el alcance y añadiría invalidación).

## 7. Errores y `ProblemDetails`

- **Decisión**: Identificador mal formado o `Guid.Empty` → 400 mediante el pipeline de validación; propiedad inexistente → `Error.NotFound` traducido a 404; fallo inesperado → `Error.Internal` traducido a 500 genérico. El slice devuelve `Results.Ok(result.Value)` en éxito porque `ProblemDetailsMapper.ToProblemDetails` solo produce 200 sin cuerpo.
- **Rationale**: Reutiliza `Result<T>`, `Error` y `ProblemDetailsMapper`, evitando lógica de error en el slice y manteniendo mensajes sin detalles técnicos.
- **Alternativas consideradas**: Lanzar excepciones (rechazado; rompe el pipeline); devolver 404 para identificadores mal formados (rechazado; contradice FR-007); códigos 409 o 422 (rechazado; no aplican a una lectura).

## 8. Logging estructurado

- **Decisión**: Inyectar `ILogger<GetPropertyByIdHandler>` y registrar un evento source-generated con el identificador de la propiedad como parámetro estructurado, sin datos de imagen, rutas ni información sensible.
- **Rationale**: FR-011 y la constitución exigen logging estructurado. `InfrastructureLog` ya concentra eventos con `[LoggerMessage]`; añadir un evento mantiene el patrón sin interpolación de mensajes.
- **Alternativas consideradas**: No registrar (rechazado; incumple FR-011); `Console.WriteLine` (rechazado; no estructurado); `LogInformation` con cadena interpolada directa (rechazado; pierde propiedades estructuradas y rompe el patrón del repositorio).

## 9. Estrategia de pruebas

- **Decisión**: Pruebas unitarias por capa (validator, mapping, handler) y pruebas del slice con `WebApplication` en memoria e invocación directa del `RequestDelegate`, sin Kestrel, PostgreSQL real ni Testcontainers. Se justifica explícitamente el uso de pruebas de componente en memoria como unit tests del slice, continuando el precedente de 006.
- **Rationale**: Cubre 200/400/404, cancelación, equivalencia de `imageUrl` con el listado, solo lectura y descubrimiento automático sin agregar dependencias ni infraestructura nueva.
- **Alternativas consideradas**: Solo pruebas de mapping (insuficiente para errores y descubrimiento); pruebas HTTP reales (rechazado; coste y dependencia de entorno); PostgreSQL real (rechazado; no requerido por la spec ni por la constitución); Moq/NSubstitute (rechazado; el repo usa stubs manuales).

## 10. Migraciones, dependencias y contrato abierto

- **Decisión**: No agregar migraciones, paquetes, proyectos ni cambios en `Program.cs`. Generar `contracts/backend-properties-get-by-id.openapi.yaml` y `contracts/get-property-by-id.http.md` como documentación de la operación.
- **Rationale**: El modelo persistente ya contiene todos los campos; el prompt operativo solicita contrato OpenAPI y el formato HTTP legible complementa la validación manual.
- **Alternativas consideradas**: Nueva migración (rechazado; sin cambios de esquema); nuevo proyecto de tests de integración (rechazado; viola el principio de solución única y añade complejidad); solo OpenAPI sin ejemplo HTTP (rechazado; el prompt también pide quickstart de validación manual).
