using Mantenimiento.Db;
using Microsoft.AspNetCore.Mvc;

namespace Mantenimiento.Api.Controllers;

[ApiController]
public class CbsDbControllerBase(IDbProviderFactory dbf): CbsControllerBase 
{
    protected async Task<IActionResult> RunDbTask(Func<DbProvider, Task<IActionResult>> dbAction)
    {
        try
        {
            var dbRes = await dbf.GetProviderAsync(TennantId, UsuarioActual);
            if (dbRes.HasError)
            {
                Logger.Error("Error obteniendo proveedor de base de datos: {0}", dbRes.ErrorMessage);
                return BadRequest(dbRes.ErrorMessage);
            }

            return await dbAction(dbRes.Result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error en {mod}", GetType().Name);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error interno en el servidor");
        }
    }
}