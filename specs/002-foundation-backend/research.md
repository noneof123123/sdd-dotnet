# Research: Fundación interna del backend

## Decisión: Registro automático de ISlice por assembly scanning

**Decisión**: Definir `ISlice` con `void AddEndpoint(IEndpointRouteBuilder app)`, registrar implementaciones públicas, concretas y no abstractas mediante reflexión y evitar nombres de clase o registries manuales. El lifetime recomendado será singleton si cada implementación es stateless.

**Rationale**: Cumple el descubrimiento solicitado y permite que `Program.cs` solo invoque una operación centralizada.

**Alternativas consideradas**:
- Registry manual de endpoints: descartado por exigir modificaciones en `Program.cs`.
- Convenciones por nombre de clase: descartadas por acoplar el descubrimiento a nombres arbitrarios.

## Decisión: Validación automática mediante FluentValidation

**Decisión**: Agregar `FluentValidation 12.2.0`, registrar validators con `AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Scoped)` y aplicar `ValidationFilterFactory.Create` de forma centralizada en el mapeo de slices.

**Rationale**: Es el mecanismo oficial para descubrimiento de validators y permite pass-through cuando no existe `IValidator<T>`.

**Alternativas consideradas**:
- Registro manual de validators: descartado por contradicción explícita.
- Validación invocada desde cada endpoint: descartada por duplicar el pipeline y romper la convención transversal.

## Decisión: Modelo Result propio mínimo

**Decisión**: Crear una representación compartida y mínima de `Result` y errores para las categorías Validation, NotFound y Conflict. La conversión HTTP será central y no dependerá de paquetes adicionales.

**Rationale**: El alcance exige una capacidad base sin introducir una librería adicional; un modelo pequeño reduce dependencia y permite controlar los payloads.

**Alternativas consideradas**:
- FluentResults: descartado para esta fundación porque añade una dependencia mayor y no es necesario para demostrar el contrato mínimo.
- Excepciones para control de flujo: descartadas por ser inconsistentes con el manejo de errores esperados.

## Decisión: Logging estructurado

**Decisión**: Usar `ILogger<T>` y eventos nombrados desde la infraestructura compartida. No se registrarán payloads completos, identificadores sensibles ni datos de negocio.

**Rationale**: Sigue el contrato obligatorio de logging y proporciona una base reutilizable sin instrumentar cada endpoint por separado.

**Alternativas consideradas**:
- Logging manual dentro de cada handler: descartado por duplicar convenciones antes de existir handlers reales.
- Solo logging de errores HTTP: descartado porque no cubre eventos base de infraestructura.

## Decisión: Pruebas unitarias sin host HTTP

**Decisión**: Mantener las pruebas de discovery, validation factory, mapping de Result y el slice /health en el proyecto xUnit de backend. Las clases auxiliares deben vivir en el proyecto de tests.

**Rationale**: Respeta la constitución que permite unit tests por defecto y evita ampliar el alcance a WebApplicationFactory, Testcontainers o integración HTTP no aprobada.

**Alternativas consideradas**:
- WebApplicationFactory: descartado porque la spec no exige pruebas de integración.
- Testcontainers: descartado por excluir explícitamente persistencia y dependencias externas.
