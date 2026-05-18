# Scaffold CRUD - CSBP Backend

Genera un CRUD completo en las 4 capas del backend siguiendo el patrón exacto de este proyecto.

**Argumento requerido:** `$ARGUMENTS` — descripción de la entidad, por ejemplo:
`Producto: Id(int), Nombre(string), Precio(decimal), Activo(bool) | tabla: inv.productos | sistema: ADMINISTRATIVO | base: LOCAL`

---

## Instrucciones

Analiza `$ARGUMENTS` para extraer:
- **Entidad**: nombre en PascalCase (ej. `Producto`)
- **Campos**: lista de propiedades con su tipo C#
- **Tabla SQL**: esquema + tabla (ej. `inv.productos`)
- **Sistema**: valor del enum `DB.Sistema.*` (ej. `ADMINISTRATIVO`)
- **BaseDeDatos**: valor del enum `DB.BaseDeDatos.*` (ej. `LOCAL`)
- **GestorDB**: inferir del proyecto actual. Por defecto usar `DB.GestorDB.POSTGRESQL`. Si el usuario indica SQL Server usar `DB.GestorDB.SQLSERVER` y en los DAL usar `Comun.Herramientas` en lugar de `Npgsql`.

Si algún dato no está en los argumentos, pregunta antes de generar código.

---

## Archivos a generar

### 1. Modelos — `4-ModeloDatos/Auth.ModeloDatos/`

**`{Entidad}Request.cs`** — DTO de entrada para crear/editar:
```csharp
namespace Auth.ModeloDatos;

public class {Entidad}Request
{
    // propiedades del request (sin Id para creación, con Id para edición)
}
```

**`{Entidad}Response.cs`** — DTO de salida:
```csharp
namespace Auth.ModeloDatos;

public class {Entidad}Response
{
    // propiedades que se devuelven al cliente
}
```

**`{Entidad}.cs`** — Modelo de entidad (mapea 1:1 con la tabla, snake_case → PascalCase vía Dapper):
```csharp
namespace Auth.ModeloDatos;

public class {Entidad}
{
    // todas las columnas de la tabla
}
```

---

### 2. DAL — `3-AccesoDatos/Auth.AccesoDatos/{Entidad}DAL.cs`

Patrón obligatorio:
- Clase `public static`
- `DapperDb` con los enums correctos de `Comun.BaseDatos.Enumeradores`
- `db.Open()` antes de queries, `db.Close()` en `finally`
- **Lecturas**: sin transacción, `QueryAsync` / `QueryFirstOrDefaultAsync`
- **Escrituras**: con `BeginTransaction()`, `Commit()` al final, `Rollback()` en el catch
- Manejo de excepciones en este orden exacto (para PostgreSQL):
  ```csharp
  catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
  catch (NpgsqlException ex)         { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
  catch (ExceptionControlado ex)     { throw new ExceptionControlado(ex.Message, ex); }
  catch (Exception ex)               { throw new Exception(ex.Message, ex); }
  ```
- Usings requeridos: `Comun.BaseDatos`, `Comun.Herramientas`, `Dapper`, `Npgsql`, `System.Net.Sockets`, `DB = Comun.BaseDatos.Enumeradores`

Métodos a generar:
```csharp
public static async Task<IEnumerable<{Entidad}>> Obtener{Entidad}s()
public static async Task<{Entidad}?> Obtener{Entidad}PorId(int id)
public static async Task<bool> Crear{Entidad}({Entidad}Request request)
public static async Task<bool> Actualizar{Entidad}({Entidad}Request request)
public static async Task<bool> Eliminar{Entidad}(int id)   // o desactivar si tiene campo estado/activo
```

Para `Crear` y `Actualizar`, usar transacción con el mismo patrón de `TotpDAL`:
```csharp
using var transaction = db.BeginTransaction();
try
{
    await db.ExecuteAsync(sql, params, transaction: transaction);
    transaction.Commit();
}
catch (ExceptionControlado ex) { transaction.Rollback(); throw new ExceptionControlado(ex.Message, ex); }
catch (Exception ex)           { transaction.Rollback(); throw new Exception(ex.Message, ex); }
```

---

### 3. BLL — `2-LogicaNegocios/Auth.LogicaNegocios/{Entidad}BLL.cs`

Patrón obligatorio:
- Clase `public static`
- Cada método retorna `Respuesta<T>` de `Comun.Herramientas`
- `response.ok = true` + `response.Datos = valor` en éxito
- `response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message)` en error
- Manejo de excepciones al final del try/catch:
  ```csharp
  catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
  catch (Exception e)            { response.SetMensaje(TiposMensaje.Error, "Atención", e.Message); }
  ```

```csharp
using Auth.AccesoDatos;
using Auth.ModeloDatos;
using Comun.Herramientas;

namespace Auth.LogicaNegocios;

public class {Entidad}BLL
{
    public static async Task<Respuesta<IEnumerable<{Entidad}Response>>> Obtener{Entidad}s() { ... }
    public static async Task<Respuesta<{Entidad}Response?>> Obtener{Entidad}PorId(int id) { ... }
    public static async Task<Respuesta<bool>> Crear{Entidad}({Entidad}Request request) { ... }
    public static async Task<Respuesta<bool>> Actualizar{Entidad}({Entidad}Request request) { ... }
    public static async Task<Respuesta<bool>> Eliminar{Entidad}(int id) { ... }
}
```

---

### 4. Controller — `1-Servicios/Auth.Api/Controllers/{Entidad}Controller.cs`

Patrón obligatorio (basado en `LoginController` y `TOTPController`):
- `[Route("api/[controller]")]`
- `[ApiController]`
- Por defecto: `[Authorize]` (requiere JWT con `TotpVerificado=true`)
- Si algún endpoint debe ser público: `[AllowAnonymous]` en ese método específico
- Si solo requiere JWT válido sin verificar TOTP: `[Authorize(Policy = "SoloAutenticado")]`
- Métodos delgados: solo llaman al BLL y retornan `Ok(resp)`
- Para obtener datos del token JWT usar `InfoToken.ObtenerDatos(HttpContext)` de `Comun.Herramientas.iHttpContext`

```csharp
using Auth.LogicaNegocios;
using Auth.ModeloDatos;
using Comun.Herramientas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class {Entidad}Controller : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<{Entidad}Response>>>> Obtener{Entidad}s()
    {
        var resp = await {Entidad}BLL.Obtener{Entidad}s();
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<{Entidad}Response?>>> Obtener{Entidad}PorId(int id)
    {
        var resp = await {Entidad}BLL.Obtener{Entidad}PorId(id);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> Crear{Entidad}([FromBody] {Entidad}Request request)
    {
        var resp = await {Entidad}BLL.Crear{Entidad}(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> Actualizar{Entidad}([FromBody] {Entidad}Request request)
    {
        var resp = await {Entidad}BLL.Actualizar{Entidad}(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> Eliminar{Entidad}(int id)
    {
        var resp = await {Entidad}BLL.Eliminar{Entidad}(id);
        return Ok(resp);
    }
}
```

---

## Reglas adicionales

- **Nunca** almacenar contraseñas en texto plano; usar `ASimetrica` para hashing y `Simetrica` para datos sensibles.
- El método `Eliminar` debe hacer **soft delete** (`UPDATE ... SET estado = FALSE`) si la tabla tiene columna `estado` o `activo`; hard delete solo si no existe ese campo.
- Los parámetros SQL siempre deben ser parametrizados (nunca concatenar strings).
- El mapeo snake_case → PascalCase lo maneja Dapper automáticamente (configurado en `Program.cs`).
- No agregar comentarios salvo que el `WHY` sea no obvio.
- Al terminar, listar los archivos creados con su ruta relativa desde `backend/`.
