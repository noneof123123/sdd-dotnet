# Contrato de estado y registro de interacciones

**Fecha**: 2026-11-22

**Finalidad**: Describir el contrato interno ya implementado para contexto, estados, umbrales y registro operativo.

## Contexto por visita

- El contexto se crea con una vista por defecto de catálogo, estado `Activa` y resultado `Correcto`.
- El contexto se conserva mientras la visita está abierta y se cierra al finalizar.
- Cada visita tiene su propio contexto; no existe un contexto compartido entre visitantes.
- Al registrar catálogo se fijan vista, página solicitada, página saneada, dirección de retorno y se limpia la propiedad.
- Al registrar detalle se fijan vista, propiedad, página saneada y dirección de retorno.
- La dirección de retorno se normaliza a una ruta local segura; una dirección vacía, relativa sin barra inicial o que empiece por `//` resuelve a `/`.

## Estados y resultados

- Los estados posibles son `Activa`, `Esperando`, `Recuperando` y `NoDisponible`.
- Los resultados posibles son `Correcto`, `Lento`, `NoDisponible` y `Recuperado`.
- Comenzar una espera cambia el estado a `Esperando`.
- Completar correctamente vuelve a `Activa` y marca `Correcto` o `Lento`.
- Completar sin disponibilidad cambia el estado a `NoDisponible` y marca `NoDisponible`.
- Comenzar recuperación cambia a `Recuperando` solo si el estado no era `NoDisponible`.
- Finalizar recuperación cambia a `Activa` con resultado `Recuperado`, o a `NoDisponible` con resultado `NoDisponible`.

## Umbrales

| Situación | Regla |
|---|---|
| Consulta lenta | Duración estrictamente mayor que 1 segundo |
| Consulta no lenta | Duración de 1 segundo o menos |
| Recuperación breve | Duración de 10 segundos o menos |
| Recuperación fallida | Duración mayor que 10 segundos o cierre con interrupción pendiente |

## Tipos de incidencia y resultados controlados

| Tipo | Resultado |
|---|---|
| `Lento` | Consulta lenta finalizada |
| `NoDisponible` | Contenido no disponible |
| `Recuperado` | Conexión restablecida |
| `NoRecuperado` | Conexión no restablecida |

## Registro operativo

- Cada incidencia contiene únicamente tipo, momento, vista afectada y resultado.
- El formato observado es: `Incidente de interacción. Tipo: {Tipo}, Momento: {Momento}, Vista: {Vista}, Resultado: {Resultado}`.
- El momento se registra con fecha y hora absolutas.
- La vista se registra como `Catalogo` o `Detalle`.
- El registro no incluye datos personales, identificadores de visita, direcciones, nombres, correos, valores de propiedad ni mensajes de excepción.
- El registro no se muestra en la interfaz dirigida al visitante.
- Las excepciones de cancelación solicitadas por el visitante se propagan sin generar incidencia.
- Otras excepciones generan una incidencia de contenido no disponible y se propagan.

## Ciclo de conexión

- Al caer la conexión se marca el inicio de la interrupción y se comienza la recuperación.
- Al restablecerse se calcula la duración, se clasifica la incidencia, se actualiza el contexto y se limpia el inicio de interrupción.
- Si el circuito se cierra con una interrupción pendiente, se registra `NoRecuperado` y se cierra la visita.
- Si el circuito se cierra sin interrupción pendiente, solo se cierra la visita.

## Aislamiento y ciclo de vida

- Dos contextos distintos pueden registrar vistas, páginas y propiedades diferentes sin interferencia.
- Cerrar un contexto fija su momento de fin sin alterar otros contextos.
- El contexto se elimina al finalizar la visita; no hay persistencia ni reutilización entre visitas.

## Verificación

| Contrato | Evidencia |
|---|---|
| Estados y contexto | Pruebas de interacción de catálogo y detalle; `InteractionDiagnosticsTests` |
| Umbrales | `InteractionDiagnosticsTests` |
| Registro | `InteractionLoggingCatalogServiceTests` |
| Reconexión | Pruebas del modal y protocolos VM-005/VM-006/SC-010 |
| Aislamiento | Pruebas de contextos independientes y protocolo SC-006 |
