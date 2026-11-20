# Investigación: Actualización de propiedades

Estado: sin decisiones pendientes de aclaración. Todas las incógnitas técnicas se resolvieron a partir de la especificación vigente, la constitución y el código existente de `001` a `005`.

## Decisión 1: Contrato `PUT /api/properties/{id}` con reemplazo completo

- **Decisión**: Exponer un único endpoint `PUT /api/properties/{id}` que reciba los ocho campos editables como partes individuales de `multipart/form-data` y una parte opcional `image`.
- **Justificación**: La semántica de reemplazo completo de la especificación encaja con `PUT`, y el identificador de ruta permite actualizar una propiedad concreta sin añadir `id` al cuerpo. La omisión de `image` conserva la imagen asociada actual sin convertirla en eliminación explícita.
- **Alternativas consideradas**: `PATCH` con campos opcionales se rechazó porque contradice el supuesto de reemplazo completo; `POST /api/properties/{id}/update` se rechazó por duplicar convenciones REST ya establecidas; JSON con Base64 se rechazó por apartarse del patrón de creación.

## Decisión 2: Identificador de ruta enlazado dentro del request

- **Decisión**: Incluir `Id` como propiedad enlazada desde la ruta en `UpdatePropertyRequest`, con validación de `Guid` no vacío en `UpdatePropertyValidator`.
- **Justificación**: El filtro global de validación solo busca `IValidator<TRequest>` para los tipos de parámetro del endpoint. Si el `id` fuera un parámetro suelto, quedaría fuera de `ValidationProblemDetails`. Al incluirlo en el request, los identificadores mal formados y vacíos se rechazan con HTTP 400 antes de acceder a persistencia.
- **Alternativas consideradas**: Un parámetro `Guid id` separado se rechazó por no participar del pipeline de validación consistente; un `string id` convertido en el handler se rechazó por mover validación de entrada fuera del validator.

## Decisión 3: Reutilización de reglas de validación sin modificar la creación

- **Decisión**: Implementar `UpdatePropertyValidator` con las mismas reglas de `CreatePropertyValidator`: longitudes de texto, no negatividad, enum vigente, `multipart/form-data`, una sola parte `image`, MIME/extensión/firma PNG o JPG/JPEG y límite inclusivo de 5 × 1024 × 1024 bytes.
- **Justificación**: FR-020 exige concentrar la validación en un único componente de la nueva capacidad, y FR-017 prohíbe alterar la creación existente. Duplicar las reglas dentro del nuevo validator evita acoplar ambas features y mantiene trazable su comportamiento.
- **Alternativas consideradas**: Extraer un validator compartido y refactorizar `CreatePropertyValidator` se rechazó porque modificaría una feature ya cerrada; validar en el handler se rechazó por violar la arquitectura y el pipeline de `ProblemDetails`.

## Decisión 4: Códigos de error 400, 404 y 500

- **Decisión**: Devolver 400 con `ValidationProblemDetails` para datos, identificador o imagen inválidos; 404 con `ProblemDetails` cuando el identificador no exista; 500 genérico con `ProblemDetails` para fallos internos de almacenamiento o persistencia.
- **Justificación**: La especificación define “solicitud inválida”, “no encontrado” y “error genérico”, y la infraestructura ya mapea esos tres casos con `ValidationFilterFactory` y `ProblemDetailsMapper`.
- **Alternativas consideradas**: Introducir 413 para tamaño excedido y 415 para tipo no soportado se rechazó porque la spec vigente no los exige y obligaría a ampliar `ErrorType`, `ProblemDetailsMapper` y sus pruebas; devolver 409 por concurrencia se rechazó por la política acordada de última escritura gana.

## Decisión 5: Orden de publicación, persistencia y retirada de imágenes

- **Decisión**: Cargar la propiedad antes de escribir archivos; si hay imagen nueva, publicarla primero; aplicar cambios y persistir; solo después de confirmar la persistencia, retirar la imagen anterior. Si falla la publicación o la persistencia, retirar la imagen nueva. Si falla la retirada de la anterior, restaurar la instantánea de campos, `ImageUrl` y `UpdatedAt`, volver a persistir, retirar la imagen nueva y devolver error interno.
- **Justificación**: Evita publicar archivos para identificadores inexistentes, impide que una propiedad quede apuntando a un archivo no escrito y mantiene una sola imagen activa. La compensación cubre los fallos controlados exigidos por SC-005 sin dependencias transaccionales entre PostgreSQL y el sistema de archivos.
- **Alternativas consideradas**: Persistir antes de publicar se rechazó por dejar la propiedad sin imagen si falla el almacenamiento; retirar la imagen anterior antes de persistir se rechazó por destruir la imagen vigente ante un fallo de base de datos; ignorar fallos de retirada se rechazó por permitir huérfanos observables.

## Decisión 6: Marcas temporales y concurrencia

- **Decisión**: Conservar `Id` y `CreatedAt`, asignar `UpdatedAt = DateTime.UtcNow` en cada actualización persistida y no introducir token de concurrencia ni control de versión. Dos escrituras concurrentes válidas aplican la política de última escritura gana.
- **Justificación**: `Property` ya expone `UpdatedAt` como `DateTime?`, no existe auditoría automática y la spec aclara explícitamente que no se rechazan solicitudes por concurrencia.
- **Alternativas consideradas**: Añadir `rowVersion` o exigir `UpdatedAt` en la solicitud se rechazó por cambiar el contrato y el modelo persistente; actualizar automáticamente la marca en `SaveChanges` se rechazó por afectar a otras features.

## Decisión 7: Representación pública de la respuesta

- **Decisión**: Devolver HTTP 200 con `UpdatePropertyResponse`, que replica los diez campos públicos de `CreatePropertyResponse`: `id`, `title`, `description`, `address`, `price`, `status` como texto, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl` relativo o `null`.
- **Justificación**: Mantiene compatibilidad con el contrato de creación y listado, y evita exponer entidades persistentes, rutas físicas, nombres originales o marcas temporales.
- **Alternativas consideradas**: HTTP 204 se rechazó porque la spec exige devolver la representación actualizada; incluir `createdAt`/`updatedAt` se rechazó por ampliar el contrato público sin respaldo en la spec; devolver la entidad `Property` se rechazó por violar el principio de contratos explícitos.

## Decisión 8: Estrategia de pruebas

- **Decisión**: Usar xUnit con EF Core InMemory y `DefaultHttpContext`, siguiendo el patrón de `005`. Cubrir validator, mapping, handler con stubs de almacenamiento y slice con multipart real. No introducir PostgreSQL real, servidor HTTP, Testcontainers ni nuevos paquetes.
- **Justificación**: Es el patrón validado en la iniciativa anterior y permite comprobar 400/404/500, conservación de imagen, reemplazo, compensación, marcas temporales y descubrimiento automático sin infraestructura externa.
- **Alternativas consideradas**: Pruebas de integración con PostgreSQL o servidor real se rechazaron porque la constitución solo admite unit tests salvo justificación explícita, y la spec no exige evidencia de API real.
