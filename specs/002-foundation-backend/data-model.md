# Data Model: Fundación interna del backend

## Estado

Esta iniciativa no introduce entidades de dominio ni modelos persistentes.

## Contratos compartidos

### ISlice

Representa un endpoint Minimal API auto-descubierto.

- Operación: `AddEndpoint(IEndpointRouteBuilder app)`
- Relación: registrada como servicio DI bajo el contrato `ISlice`
- Validación: clase pública, concreta y no abstracta

### IHandler

Marker interface para handlers de casos de uso futuros.

- Implementaciones permitidas: clases concretas en assemblies aprobados
- Esta iniciativa no crea implementaciones de producción

### Result

Representación mínima de éxito o error esperado.

- Estados: éxito o error
- Categorías iniciales: Validation, NotFound, Conflict
- Relación: convertido centralmente a HTTP `ProblemDetails` o `ValidationProblemDetails`

### Error

Describe un error esperado compartido.

- Campos conceptuales: código estable, categoría, título y detalle seguro
- Restricción: no exponer excepciones internas ni datos sensibles

## Persistencia

No aplica. No se crean `AppDbContext`, entidades, configuraciones EF Core, migraciones ni seeders.
