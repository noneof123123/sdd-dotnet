# Investigación: Persistencia y seed de propiedades

## Decisión: Persistir PropertyStatus como enum convertido a texto

**Contexto:** La especificación exige el enum `PropertyStatus` y aclara que no debe existir una tabla independiente.

**Decisión:** Definir `PropertyStatus` con `Available`, `Rented` y `Maintenance`, usarlo como propiedad `Status` de `Property` y configurarlo con conversión EF Core explícita a texto.

**Rationale:** Evita una tabla redundante, conserva una validación cerrada en el dominio y permite comparar e indexar el estado por nombre.

**Alternativas consideradas:**
- Entidad relacional independiente: descartada por la aclaración de especificación.
- Texto libre: descartado porque permite valores fuera del catálogo y reduce seguridad de tipos.

## Decisión: Usar una tabla properties con GUID como clave

**Contexto:** Los archivos JSON contienen Ids GUID y la entidad requiere una identidad persistente estable.

**Decisión:** Usar `Guid` como clave primaria y aplicar el Id estable del archivo JSON durante el seed.

**Rationale:** Permite una identidad reproducible e idempotente sin generar identificadores diferentes en cada ejecución.

**Alternativas consideradas:**
- Clave entera autogenerada: descartada porque no garantiza continuidad frente a los datos JSON.
- Clave compuesta por título y dirección: descartada por riesgo de colisiones y cambios posteriores.

## Decisión: Aplicar restricciones EF Core explícitas

**Contexto:** La especificación exige restricciones verificables de columnas pero no fija valores exactos.

**Decisión:** Adoptar límites razonables en el plan: título 200, descripción 4.000, dirección 500, URL 1.000; precio `decimal(18,2)`; superficie `decimal(12,2)`; recuentos enteros mayores o iguales a cero; estado requerido; CreatedAt requerido; UpdatedAt opcional.

**Rationale:** Estos límites cubren datos inmobiliarios normales sin cargas descontroladas y permiten precisión monetaria y métrica adecuada.

**Alternativas consideradas:**
- Sin límites: descartado por permitir datos desproporcionados y reducir calidad del esquema.
- Límites máximos excesivos: descartados para mantener un modelo compacto.

## Decisión: Usar MigrateAsync y delegados nativos de EF Core

**Contexto:** El arranque debe aplicar siempre migraciones y ejecutar el seed con/ sin migraciones pendientes.

**Decisión:** Registrar ambos delegados `UseSeeding` y `UseAsyncSeeding`, configurar Npgsql y llamar `await app.MigrateAsync()` antes de `app.Run()`.

**Rationale:** La operación de migración EF Core dispara el seeding asincrónico cuando finaliza, incluso si no existe trabajo pendiente.

**Alternativas consideradas:**
- Invocar `DatabaseSeeder` manualmente desde `MigrationExtensions`: descartado por invocar el seed fuera del delegado oficial.
- Condicionar `MigrateAsync` a migraciones pendientes: descartado por la especificación.

## Decisión: Resolver todos los activos desde seed-manifest.json

**Contexto:** JSON e imágenes provienen de `support/seed-data` y deben integrarse en build/publish.

**Decisión:** Crear un manifiesto que declare rutas relativas de JSON, directorio fuente de imágenes y destino público; el seeder leerá el manifiesto y resolverá todas las rutas relativas desde él. JSON y manifiesto serán incluido como contenido publicado junto a las imágenes sincronizadas.

**Rationale:** Centraliza resolución de activos y evita rutas hardcodeadas dispersas.

**Alternativas consideradas:**
- Rutas hardcodeadas en el seeder: descartadas por duplicación y dificultad de despliegue.
- Exponer directamente `support/`: descartado por mantener rutas físicas de desarrollo como API pública.

## Decisión: Publicar imágenes bajo /images/properties

**Contexto:** La especificación exige una ruta pública final para `ImageUrl`.

**Decisión:** Sincronizar `support/seed-data/images/properties/*.png` hacia `wwwroot/images/properties/*.png` durante build/publish y generar `/images/properties/{archivo}.png`.

**Rationale:** `wwwroot` permite entrega directa por ASP.NET Core y una ruta pública estable independiente del origen de soporte.

**Alternativas consideradas:**
- Servir desde `support/seed-data`: descartado por acoplar publicación y contenido de soporte.
- Guardar URL absoluta configurable: descartado por no ser necesario para el modelo inicial y aumentar configuración.
