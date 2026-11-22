# Modelo de datos retrospectivo: Documentación retrospectiva del hotfix interactivo

**Fecha**: 2026-11-22

**Alcance**: Modelo de estado de interacción y registro operativo implementado en el frontend. No define entidades de dominio ni persistencia nueva.

## Entidades y tipos

### Visita interactiva

- **Descripción**: Período de uso comprendido entre la apertura y el cierre de una experiencia interactiva.
- **Atributos**:
  - `Inicio`: momento de creación del contexto.
  - `Fin`: momento de cierre; permanece vacío mientras la visita sigue activa.
- **Regla**: El contexto se crea por visita y se descarta al finalizar. No se comparte con otras visitas.

### Contexto de navegación

- **Descripción**: Estado que permite reconstruir la vista vigente y el retorno válido.
- **Atributos**:
  - `VistaActual`: catálogo o detalle.
  - `PaginaSolicitada`: valor original solicitado; puede ser inválido.
  - `PaginaSaneada`: página efectiva usada para la consulta.
  - `PropiedadId`: propiedad abierta cuando la vista es detalle.
  - `DireccionRetorno`: dirección local segura para volver al catálogo.
- **Reglas**:
  - La página se sanea antes de consultar; valores ausentes, no numéricos, cero o negativos resuelven a la página 1.
  - La dirección de retorno debe ser una ruta local que empiece por `/` y no por `//`; en caso contrario resuelve a `/`.
  - Registrar catálogo limpia la propiedad activa.
  - Registrar detalle conserva la página saneada y la dirección de retorno.

### Estado de interacción

- **Descripción**: Condición observable de la vista durante una operación.
- **Valores**:
  - `Activa`: la vista está disponible para seguir interactuando.
  - `Esperando`: hay una consulta en curso y se muestra indicación de progreso.
  - `Recuperando`: la conexión está caída y se intenta restablecer.
  - `NoDisponible`: la consulta falló o la recuperación no tuvo éxito.
- **Resultados asociados**:
  - `Correcto`.
  - `Lento`.
  - `NoDisponible`.
  - `Recuperado`.

### Intención vigente

- **Descripción**: Última acción válida del visitante dentro de una vista.
- **Atributos**:
  - `ClaveCargada`: identifica la combinación de vista, propiedad y página ya cargada.
  - `ContadorDeIntencion`: correlativo que permite descartar respuestas de acciones superadas.
- **Regla**: Solo se aplica el resultado cuyo contador coincide con la intención más reciente.

### Incidencia operativa

- **Descripción**: Registro interno de una interacción lenta, contenido no disponible o cambio de conexión.
- **Atributos**:
  - `Tipo`: `Lento`, `NoDisponible`, `Recuperado` o `NoRecuperado`.
  - `Momento`: fecha y hora de la incidencia.
  - `Vista`: `Catalogo` o `Detalle`.
  - `Resultado`: texto controlado según el tipo.
- **Reglas**:
  - El registro no contiene datos personales, identificadores de visita, direcciones ni valores de propiedad.
  - El registro no se muestra al visitante.
  - Los resultados controlados son: «Consulta lenta finalizada», «Contenido no disponible», «Conexión restablecida» y «Conexión no restablecida».

### Estados de presentación

- **Descripción**: Estados visuales ya aprobados para catálogo y detalle.
- **Valores**: `Cargando`, `Contenido`, `Vacio`, `Error`, `NoEncontrado`.
- **Regla**: El estado de presentación decide mensaje e icono; el botón de reintento aparece en `Error` cuando la vista ofrece una acción interactiva.

## Reglas de umbral

- Una consulta es lenta cuando su duración supera estrictamente 1 segundo.
- Una interrupción se considera breve cuando dura 10 segundos o menos.
- Una interrupción mayor a 10 segundos se clasifica como no recuperada para efectos de registro.
- Los límites exactos están cubiertos por pruebas: 999 ms y 1 s no son lentos; 1001 ms sí; 10 s es recuperable; 10,001 s y 11 s no.

## Transiciones de estado

### Carga de vista

1. `Activa` → `Esperando` al registrar la vista y comenzar la consulta.
2. `Esperando` → `Activa` si la consulta termina correctamente.
3. `Esperando` → `Activa` con resultado `Lento` si supera el umbral.
4. `Esperando` → `NoDisponible` si la consulta devuelve error o lanza una excepción.
5. Las respuestas tardías de intenciones superadas no cambian el estado.

### Conexión

1. `Activa` o `NoDisponible` → `Recuperando` al caer la conexión.
2. `Recuperando` → `Activa` con resultado `Recuperado` si la conexión vuelve dentro del umbral.
3. `Recuperando` → `NoDisponible` con resultado `NoDisponible` si la conexión vuelve después del umbral.
4. Si el circuito se cierra con una interrupción pendiente, se registra `NoRecuperado` y se cierra la visita.

### Cierre

- Al disponer el contexto o cerrar el circuito se fija `Fin`.
- El cierre de una visita no altera el contexto de otra visita.

## Relaciones

- Una **visita interactiva** contiene un **contexto de navegación**, un **estado de interacción** y una **intención vigente**.
- Una **consulta** produce un **estado de presentación** y, cuando aplica, una **incidencia operativa**.
- Una **interrupción de conexión** actualiza el **estado de interacción** y puede producir una **incidencia operativa**.
- Las **incidencias operativas** no se persisten en el modelo de dominio y no se asocian a datos personales.

## Persistencia

- No hay tablas, migraciones ni almacenamiento nuevo.
- No se modifica la información de propiedades ni su historial de estados.
- El contexto solo existe durante la visita y se elimina al cerrarse.
