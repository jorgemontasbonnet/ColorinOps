using Mantenimiento.Api.Filters;
using Mantenimiento.Api.Models;
using Mantenimiento.Api.Services;
using Mantenimiento.Db;
using Mantenimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mantenimiento.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [RequireTennantHeader]
    public class ImpresosController(IDbProviderFactory dbf) : CbsDbControllerBase(dbf)
    {
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetImpresoPorId(id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                return Ok(res.Result);
            });
        }
        
        [HttpGet]
        [Route("jpg/{id:guid}/{size}")]
        [OutputCache (Duration = 1024)]
        public async Task<IActionResult> GetAsPng([FromRoute] string size, [FromRoute] Guid id)
        {
            var checkedSize = (size == "xl") ? "xl" : "sm";

            return await RunDbTask(async db =>
            {
                var res = await db.GetImpresoBytes(id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                var nuBytes = await ImageService.Resize(res.Result, checkedSize);
                return File(nuBytes, "image/jpeg");
            });
        }    
    }
}