# Investigación: Paginación y listado de propiedades

## Decisión: Validar page y pageSize en un validator específico

**Contexto:** Ambos parámetros son opcionales, positivos y `pageSize` tiene un máximo efectivo.

**Decisión:** Modelar un request con `int Page` e `int PageSize`, asignar defaults al binding, validar `Page >= 1`, `PageSize >= 1` y normalizar `PageSize > 24` a 24 antes del handler.

**Rationale:** Rechazar cero y negativos evita paginación inconsistente; normalizar tamaños excesivos mantiene el contrato operativo sin convertir el límite en HTTP 400, según la aclaración humana.

**Alternativas consideradas:**
- HTTP 400 para `pageSize > 24`: descartado por la aclaración que exige normalización.
- Sin límite: descartado por permitir cargas excesivas en respuestas completas.

## Decisión: Normalizar páginas fuera de rango a la última página

**Contexto:** Una página puede exceder el número calculado de páginas.

**Decisión:** Calcular `totalItems` y `totalPages`, normalizar `page` al último valor cuando `totalPages > 0` y `page > totalPages`, y conservar una página vacía solo si no existen registros.

**Rationale:** Produce metadatos consistentes y evita un 404 o 400 para una consulta válida de paginación.

**Alternativas consideradas:**
- Página vacía sin normalizar: descartada por la aclaración.
- HTTP 404: descartado porque el listado sigue existiendo.

## Decisión: Usar proyección EF Core directa

**Contexto:** El endpoint solo necesita campos de respuesta y no debe exponer entidades.

**Decisión:** Contar con `CountAsync`, consultar `AsNoTracking()` y proyectar directamente a un DTO interno antes de `ToListAsync`, ordenando por `Title` y `Address`.

**Rationale:** Reduce tracking, memoria y datos cargados, y mantiene el resultado acotado a una página.

**Alternativas consideradas:**
- Cargar entidades y mapear: descartado por cargar datos innecesarios y contravenir el mapping preferido.
- Repository wrapper: descartado por ser una abstracción trivial para este caso.

## Decisión: Construir imágenes desde el request actual

**Contexto:** La respuesta debe proporcionar URLs absolutas y no rutas físicas ni relativas.

**Decisión:** Extraer `Request.Scheme` y `Request.Host`, derivar el nombre de archivo del campo persistido y mapear a `${scheme}://${host}/assets/properties/{fileName}`. Para nombres vacíos o inválidos, devolver cadena vacía.

**Rationale:** Evita configurar una base pública duplicada y refleja el host efectivo del request.

**Alternativas consideradas:**
- URL relativa: descartada explícitamente por la especificación.
- Ruta física persistida: descartada explícitamente por la especificación.
- URL absoluta desde configuración global: descartada porque no refleja correctamente hosts locales o pruebas diferentes.

## Decisión: Mantener el slice sin cambiar la infraestructura transversal

**Contexto:** Ya existen ISlice, IHandler, assembly scanning, FluentValidation y mapping central.

**Decisión:** Reutilizar esos mecanismos y agregar únicamente archivos específicos de `ListProperties`.

**Rationale:** Cumple la arquitectura existente y evita duplicar scanners, registries o filtros.

**Alternativas consideradas:**
- Registrar el endpoint manualmente en `Program.cs`: descartado por contradictorio con ISlice.
- Crear endpoints técnicos paralelos: descartado por la estructura obligatoria de Features.
