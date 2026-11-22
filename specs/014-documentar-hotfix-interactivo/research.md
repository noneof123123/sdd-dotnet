# Investigación retrospectiva: Documentación retrospectiva del hotfix interactivo

**Fecha**: 2026-11-22

**Estado de clarificaciones**: Sin pendientes. El alcance y el destino se resolvieron en la sesión de especificación; los detalles técnicos se obtuvieron del commit `bdb714e`, del código actual y de las pruebas existentes.

## R1. Interactividad por página

- **Decisión**: Activar el modo interactivo únicamente en catálogo y detalle.
- **Justificación**: El recorrido problemático se concentra en esas dos vistas. El resto de páginas conserva su comportamiento estático y aprobado.
- **Alternativas consideradas**:
  - Activar interactividad global: descartado por ampliar innecesariamente el alcance y el consumo de recursos.
  - Mantener streaming estático con recargas parciales: descartado porque no conserva de forma natural la intención ni el contexto de retorno.
  - Añadir una aplicación separada: descartado por infringir el principio de solución única.

## R2. Contexto de interacción por visita

- **Decisión**: Mantener un contexto con ámbito de visita para vista, página, propiedad, retorno, estado y resultado.
- **Justificación**: Permite conservar el lugar del visitante sin persistir información entre visitas ni compartirla entre circuitos.
- **Alternativas consideradas**:
  - Estado global: descartado por riesgo de filtración entre visitantes.
  - Estado en el navegador: descartado porque añadiría persistencia no requerida y datos observables por el visitante.
  - Reconstruir el contexto solo desde la dirección: descartado porque no cubre espera, recuperación ni intención vigente.

## R3. Ciclo de conexión y recuperación

- **Decisión**: Registrar un gestor de ciclo de conexión que marca el inicio de la interrupción, clasifica la duración y cierra el contexto al finalizar la visita.
- **Justificación**: Centraliza la recuperación y evita que cada página interprete por separado los cambios de conexión.
- **Alternativas consideradas**:
  - Manejar eventos de conexión dentro de cada página: descartado por duplicación lógica y divergencia entre vistas.
  - Confiar solo en el modal del marco de trabajo: descartado porque no registra incidencias ni controla el estado de la visita.
  - Crear un hub propio: descartado por innecesario y por no formar parte del cambio implementado.

## R4. Registro de incidencias sin tocar el servicio base

- **Decisión**: Arbitrar el servicio de catálogo con un decorador que mide duración, detecta errores y emite un registro acotado.
- **Justificación**: Añade comportamiento operativo sin modificar la implementación original ni el contrato HTTP.
- **Alternativas consideradas**:
  - Modificar `PropertyCatalogService`: descartado por acoplar registro y consulta en una sola responsabilidad.
  - Cambiar el backend o el contrato OpenAPI: descartado por quedar fuera del alcance observable del frontend.
  - Registrar desde cada componente: descartado por duplicación y pérdida de uniformidad.

## R5. Umbrales y resultados controlados

- **Decisión**: Usar 1 segundo para respuesta lenta, 10 segundos para recuperación breve y cuatro resultados controlados.
- **Justificación**: Proporciona límites verificables y evita texto libre en registros y mensajes.
- **Alternativas consideradas**:
  - Umbrales configurables por entorno: no implementados; el cambio existente usa constantes compartidas.
  - Clasificación por excepción: descartada porque una respuesta lenta no es necesariamente un error.
  - Resultados libres: descartados por dificultar pruebas y lectura operativa.

## R6. Modal de reconexión y acciones

- **Decisión**: Usar los estados e identificadores reconocibles del marco de trabajo, con textos en español y acciones de reintento y recarga.
- **Justificación**: Mantiene compatibilidad con el mecanismo de reconexión existente y ofrece una salida comprensible cuando la recuperación automática falla.
- **Alternativas consideradas**:
  - Modal propio sin contrato de estados del marco: descartado por duplicar responsabilidad ya resuelta.
  - Mensaje técnico de conexión: descartado por incumplir los requisitos de contenido visible.
  - Solo recarga manual: descartado porque impide conservar la intención cuando es posible reintentar.

## R7. Reintento dentro de la vista

- **Decisión**: Añadir una acción de reintento interactiva en el componente de estado, priorizándola sobre el enlace alternativo cuando existe delegado.
- **Justificación**: Permite repetir la misma intención sin abandonar la vista, manteniendo el enlace como respaldo para contextos no interactivos.
- **Alternativas consideradas**:
  - Conservar solo enlaces: descartado porque cambia la dirección y pierde inmediatez.
  - Mostrar simultáneamente botón y enlace: descartado por duplicar acciones para el visitante.
  - Reintentar automáticamente sin intervención: descartado por riesgo de bucles y acciones repetidas.

## R8. Estilos y accesibilidad

- **Decisión**: Incorporar estilos propios centralizados para modal, progreso, foco, tamaños de pantalla y movimiento reducido.
- **Justificación**: Conserva el sistema visual existente y evita dependencias nuevas.
- **Alternativas consideradas**:
  - Frameworks CSS: prohibidos por la constitución.
  - Estilos en línea: descartados por dificultar mantenimiento y por las pruebas que los rechazan.
  - Iconos decorativos anunciados: descartados por ruido para tecnologías de asistencia.

## R9. Estrategia de pruebas

- **Decisión**: Cubrir comportamiento con pruebas unitarias de componentes, contexto, umbrales y registro; apoyar los criterios restantes con protocolos manuales documentados.
- **Justificación**: La constitución prioriza unit tests y las interacciones reales de conexión requieren observación humana.
- **Alternativas consideradas**:
  - Pruebas de integración con conexiones reales: no justificadas en el plan implementado.
  - Solo pruebas manuales: descartado por falta de regresión automática.
  - Pruebas de interfaz completas: descartado por añadir dependencias no presentes.

## R10. Archivo sin seguimiento

- **Decisión**: Documentar `app/frontend/src/RealtorWeb/Features/_Imports.razor` como observación del working tree, no como parte del hotfix confirmado.
- **Justificación**: `git status` lo muestra sin seguimiento y no está incluido en el commit `bdb714e`; el alcance confirmado por el usuario es el cambio interactivo completo.
- **Alternativas consideradas**:
  - Atribuirlo al commit principal: descartado por falta de evidencia en el historial.
  - Incluirlo como requisito funcional: descartado porque no produce comportamiento observable documentado.
  - Ignorarlo sin mención: descartado para conservar trazabilidad del diff actual.
