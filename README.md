# API de días hábiles

Esta es la API que desarrollé para la prueba técnica de Power Platform ProCode. Es una función HTTP en Azure Functions (.NET 10, modelo aislado, C#) que calcula cuántos días hábiles hay entre dos fechas. La uso desde la Canvas App de solicitudes de vacaciones, a través de un Custom Connector.

## Cómo calcula los días hábiles

- Cuento la fecha de inicio y la de fin, ambas incluidas.
- Tomo de lunes a sábado como días hábiles. Me basé en el ejemplo del enunciado (del 2 al 7 de noviembre de 2026, con el lunes festivo, da 5 días).
- No cuento los domingos.
- No cuento las fechas que lleguen en la lista de festivos.

Decidí que los festivos lleguen en cada petición y no escribirlos en el código. En la Power App los leo de la lista `Festivos` de SharePoint y se los envío a la API. Así, si cambia un festivo, solo edito la lista y no tengo que volver a desplegar.

## Endpoint

`POST /api/CalcularDiasHabiles`

La función tiene nivel de autorización `Function`, así que hay que enviar su clave (parámetro `code` o encabezado `x-functions-key`). Lo hice así para que la API no quede abierta a cualquiera en internet.

### Petición

```json
{
  "fechaInicio": "2026-11-02",
  "fechaFin": "2026-11-07",
  "festivos": ["2026-11-02"]
}
```

Las fechas van en formato `yyyy-MM-dd`. El campo `festivos` es opcional.

### Respuesta (200)

```json
{
  "fechaInicio": "2026-11-02",
  "fechaFin": "2026-11-07",
  "diasHabiles": 5
}
```

### Errores (400)

Agregué validaciones para que la API responda un mensaje claro en vez de fallar. Devuelve error 400 cuando:

- El cuerpo no es un JSON válido o viene vacío.
- `fechaInicio` o `fechaFin` faltan o no tienen el formato `yyyy-MM-dd`.
- `fechaFin` es anterior a `fechaInicio`.
- Algún festivo no tiene el formato `yyyy-MM-dd`.

```json
{ "error": "fechaFin no puede ser anterior a fechaInicio." }
```

## Estructura del proyecto

| Archivo | Para qué sirve |
| --- | --- |
| `CalcularDiasHabiles.cs` | La función HTTP: lee la petición, valida los datos y responde. |
| `CalculadoraDiasHabiles.cs` | La lógica del cálculo. La separé de la función para que no dependa de HTTP y sea fácil de explicar y probar. |
| `Models.cs` | Los modelos de entrada y salida, que definen el JSON. |
| `Program.cs` | El arranque del host de Azure Functions. |

## Cómo ejecutarla en local

Necesito el SDK de .NET 10 y Azure Functions Core Tools 4.

```
dotnet build
func start
```

Para probarla:

```
$body = '{"fechaInicio":"2026-11-02","fechaFin":"2026-11-07","festivos":["2026-11-02"]}'
Invoke-RestMethod -Method Post -Uri http://localhost:7071/api/CalcularDiasHabiles -ContentType "application/json" -Body $body
```

En local no pide clave. Los avisos sobre `AzureWebJobsStorage` los ignoro, porque la función solo usa HTTP y no necesita almacenamiento.

## Despliegue

La desplegué en una Function App (Flex Consumption, Linux) dentro del grupo de recursos `GR_Sebastian_Orrego`, usando la extensión de Azure Functions de VS Code (`Azure Functions: Deploy to Function App`).

La clave de la función no está en este repositorio. En el Custom Connector de Power Platform la configuro como parámetro de autenticación.

## Problemas que encontré

- **Azure CLI bloqueada.** Al iniciar sesión con `az login` con el usuario del reto me salió el error 530035, una política del tenant que bloquea ese acceso. No lo pude cambiar yo, así que desplegué desde la extensión de VS Code, que no tiene ese bloqueo.
- **Error 502 después del primer despliegue.** La Function App estaba creada con .NET 10 y mi proyecto apuntaba a .NET 8, así que Azure no podía arrancar el código. Cambié el proyecto a `net10.0`, actualicé la ruta de publicación en `.vscode/settings.json` y volví a desplegar. El documento pide .NET 8 o superior, por lo que cumple.

## Decisiones y supuestos

- El sábado cuenta como día hábil, según el ejemplo del enunciado.
- Los festivos llegan en la petición para no dejarlos fijos en el código.
- Dejé la lógica del cálculo en una clase aparte para mantener el código claro y mantenible.