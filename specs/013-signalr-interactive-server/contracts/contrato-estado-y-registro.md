# Contrato de estado y registro interno

**Rama**: `013-signalr-interactive-server` | **Fecha**: 2026-11-21 | **Especificación**: [spec.md](../spec.md)

Este contrato define el comportamiento verificable del estado de interacción y del registro operativo. No describe tecnología, librerías ni mecanismos de transporte.

## Ámbito

- El estado existe solo durante la visita interactiva y se descarta al terminar.
- No se usa persistencia, cookies ni sesión HTTP para este estado.
- El registro operativo no forma parte de la respuesta visible y no cambia el contrato HTTP del backend.

## Estados

| Estado | Entrada típica | Salida típica |
|---|---|---|
| `Activa` | Inicio de la visita o finalización correcta | `Esperando` o `Recuperando` |
| `Esperando` | Acción del visitante o consulta en curso | `Activa` o `NoDisponible` |
| `Recuperando` | Interrupción de conexión | `Activa` o `NoDisponible` |
| `NoDisponible` | Superación del umbral o fallo de recuperación | `Recuperando` o `Activa` |

Solo un estado puede estar vigente por visita. La transición a `NoDisponible` requiere superar 10 segundos o un fallo confirmado.

## Contexto de la visita

| Campo | Valor permitido | Origen |
|---|---|---|
| `VistaActual` | `Catalogo`, `Detalle` | Vista que está usando el visitante. |
| `PaginaSaneada` | entero positivo | Saneamiento de la dirección visible. |
| `PropiedadId` | identificador o ausente | Solicitud de detalle, cuando aplica. |
| `UltimoResultado` | `Correcto`, `Lento`, `NoDisponible`, `Recuperado` | Última señal observada. |

El contexto se actualiza al cambiar de vista y se deriva de la dirección visible. No se comparte entre visitas.

## Registro de incidencias

| Campo | Valor permitido | Regla |
|---|---|---|
| `Tipo` | `Lento`, `NoDisponible`, `Recuperado`, `NoRecuperado` | Clasificación operativa de la incidencia. |
| `Momento` | fecha y hora del servidor | Se registra al detectar o cerrar la incidencia. |
| `Vista` | `Catalogo`, `Detalle` | Vista afectada en ese momento. |
| `Resultado` | `Consulta lenta finalizada`, `Contenido no disponible`, `Conexión restablecida` o `Conexión no restablecida` | Descripción breve, estable y no técnica; el valor depende de `Tipo`. |

| `Tipo` | `Resultado` permitido |
|---|---|
| `Lento` | `Consulta lenta finalizada` |
| `NoDisponible` | `Contenido no disponible` |
| `Recuperado` | `Conexión restablecida` |
| `NoRecuperado` | `Conexión no restablecida` |

Reglas de registro:

- Se registra `Lento` cuando una consulta tarda más de 1 segundo.
- Se registra `NoDisponible` cuando una consulta termina en error.
- Se registra `Recuperado` cuando la conexión vuelve antes del umbral.
- Se registra `NoRecuperado` cuando la visita termina sin recuperación.
- No se registran identificadores de visita, direcciones IP, nombres, correos, identificadores de propiedad ni valores de contenido.
- Los registros no se muestran al visitante.

## Criterios de verificación

- Toda transición de estado tiene un resultado observable en la superficie web o en el registro operativo.
- El umbral de 10 segundos se clasifica de la misma forma para la vista y para el registro.
- El contexto de una visita no aparece en otra visita independiente.
- Ningún registro contiene datos personales ni detalles técnicos visibles.
- Las pruebas en proceso pueden verificar clasificación, mensajes, render de estados y campos de registro sin red, navegador ni circuito real.
- La reconexión real, el cierre del circuito y la concurrencia de visitas requieren evidencia manual documentada.
