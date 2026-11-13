using Mantenimiento.Api.Filters;
using Mantenimiento.Api.Models;
using Mantenimiento.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Mantenimiento.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [RequireTennantHeader]
    public class OrdenesController (IDbProviderFactory dbf): CbsDbControllerBase(dbf)
    {
        [HttpGet]
        [Route("list")]
        [OutputCache(Duration = 15)]
        public async Task<IActionResult> GetList([FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null, [FromQuery] string numero = "", [FromQuery] bool? cerradas = false)
        {
            return await RunDbTask(async db =>
            {
                DateTime checkedDesde = desde ?? new DateTime(2000, 1, 1);
                var checkedHasta = hasta ?? DateTime.Today;
                var checkedCerradas = cerradas ?? false;

                var listRes = await db.GetOrdenesPorBrigadaAsync(UsuarioActual, checkedDesde, checkedHasta, numero, checkedCerradas);
                return listRes.HasError ? BadRequest(listRes.ErrorMessage) : Ok(listRes.Result);
            });
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetOrdenServicio(id, UsuarioActual);
                return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
            });
        }

        [HttpGet]
        [Route("trabajo/{id:guid}")]
        public async Task<IActionResult> GetDetalleTrabajo([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetTrabajoPorId(id, UsuarioActual);
                return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
            });
        }

        [HttpPost]
        [Route("terminar")]
        public async Task<IActionResult> TerminarOrden([FromBody] MarcarOrdenListaOrdenRequest dto)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.MarcarOrdenLista(dto.OrdenId, true, dto.Inicio, dto.Fin, dto.Observaciones, UsuarioActual);
                return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
            });
        }

    }
}
