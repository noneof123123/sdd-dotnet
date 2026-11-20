# Investigación: Alta de propiedades

## Decisión 1: Solicitud multipart con campos individuales

- **Decisión**: Usar `multipart/form-data` con campos individuales para los datos y una parte opcional `image` de tipo `IFormFile`.
- **Justificación**: Permite adjuntar binarios sin codificar Base64 y coincide con el formulario HTTP requerido por la especificación.
- **Alternativas consideradas**: JSON con Base64 y un DTO JSON dentro de multipart; se rechazaron por duplicar conversión y aumentar complejidad sin beneficio funcional.

## Decisión 2: Validación de formato por metadatos y contenido

- **Decisión**: Centralizar en `CreatePropertyValidator` la validación de extensión, MIME declarado, firmas binarias PNG/JPG/JPEG, tamaño, tipo de contenido y cantidad de partes `image`, antes de escribir el archivo; el almacenamiento solo escribe bytes ya validados.
- **Justificación**: Evita aceptar contenido incompatible con su declaración y reduce riesgos de nombres o tipos maliciosos.
- **Alternativas consideradas**: Validación solo por extensión o solo por MIME; ambas fueron rechazadas por ser insuficientes.

## Decisión 3: Límite de tamaño

- **Decisión**: Rechazar imágenes mayores de 5 × 1024 × 1024 bytes.
- **Justificación**: El límite máximo debe ser exacto, binario y inclusivo.
- **Alternativas consideradas**: 5.000.000 bytes; se rechazó por no corresponder al significado binario acordado.

## Decisión 4: Consistencia entre archivo y registro

- **Decisión**: Validar antes de escribir, guardar el archivo con nombre generado seguro y persistir el registro con su ruta relativa. Si persiste falla, eliminar el archivo creado.
- **Justificación**: La operación debe evitar registros apuntando a archivos inexistentes y archivos huérfanos observables.
- **Alternativas consideradas**: Persistir primero y escribir después; se rechazó porque una propiedad podría quedar sin imagen por un fallo de almacenamiento.

## Decisión 5: Respuesta HTTP

- **Decisión**: Devolver HTTP 201, encabezado `Location: /api/properties/{id}` y `CreatePropertyResponse`; usar HTTP 400 para validación y HTTP 500 con `ProblemDetails` para fallos internos.
- **Justificación**: Sigue semántica REST y separa errores del cliente de fallos internos.
- **Alternativas consideradas**: HTTP 200, HTTP 422 y HTTP 503; se rechazaron por la decisión aclarada en la especificación.

## Decisión 6: Nulabilidad de ImageUrl

- **Decisión**: Cambiar `Property.ImageUrl` a `string?` y aplicar `IsRequired(false)` en su configuración EF.
- **Justificación**: Permite representar sin ambigüedad una propiedad creada sin imagen y coincide con la aclaración aprobada.
- **Alternativas consideradas**: cadena vacía o marcador interno; se rechazaron por no representar `null` y por confundir ausencia con error.
