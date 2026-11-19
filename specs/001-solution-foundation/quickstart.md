# Quickstart: Validación de la base de Realtor

## Prerequisitos

- Tener el SDK `.NET` seleccionado por `global.json` disponible en el entorno.
- Ejecutar los comandos desde la raíz del repositorio.
- Verificar que `global.json` exista y no haya sido modificado por esta iniciativa.

## Restauración y compilación

```bash
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
```

## Resultado esperado

- `app/Realtor.sln` existe.
- `RealtorApi`, `RealtorApiTests`, `RealtorWeb` y `RealtorWeb` de pruebas están incluidos en la solución.
- Los cuatro proyectos restauran y compilan sin errores.
- El backend no contiene controllers.
- No existen entidades de dominio ni lógica de negocio creada por esta iniciativa.
- `global.json` permanece sin cambios.

## Validación de estructura

Confirmar mediante exploración del repositorio que existen:

- `app/backend/src/RealtorApi/`
- `app/backend/tests/RealtorApiTests/`
- `app/frontend/src/RealtorWeb/`
- `app/frontend/test/RealtorWeb/`

## Alcance fuera de la fundación

No ejecutar ni esperar validaciones de endpoints de negocio, migraciones, persistencia o interfaces externas, ya que no forman parte de esta iniciativa.
