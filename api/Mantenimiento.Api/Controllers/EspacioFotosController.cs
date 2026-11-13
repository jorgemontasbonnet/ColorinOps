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
    public class EspacioFotosController(IDbProviderFactory dbf) : CbsDbControllerBase(dbf)
    {
        private static readonly JsonSerializerOptions jsonOps = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        [HttpDelete]
        [Route("{espacioId}/{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] string espacioId, [FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.BorrarFotoEspacio(espacioId, id, UsuarioActual);
                return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
            });
        }

        [HttpGet]
        [Route("list/{espacioId}")]
        public async Task<IActionResult> GetList([FromRoute] string espacioId)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetFotosPorEspacioId(espacioId);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                return Ok(res.Result);
            });
        }


        [HttpGet]
        [Route("{espacioId}/{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] string espacioId, [FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetFotoEspacioPorId(espacioId, id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                return Ok(res.Result);
            });
        }
        
        [HttpGet]
        [Route("jpg/{espacioId}/{id:guid}/{size}")]
        [OutputCache (Duration = 4096)]
        public async Task<IActionResult> GetAsJpg([FromRoute] string espacioId, [FromRoute] Guid id, [FromRoute] string size)
        {
            var checkedSize = (size == "xl") ? "xl" : "sm";

            return await RunDbTask(async db =>
            {
                var res = await db.GetFotoEspacioBytes(espacioId, id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                var nuBytes = await ImageService.Resize(res.Result, checkedSize);
                return File(nuBytes, "image/jpeg");
            });
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFotoWithImage([FromForm] string fotoJson, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            return await RunDbTask(async db =>
            {
                try
                {
                    // Deserialize the Foto object from the JSON string
                    var fotoDto = JsonSerializer.Deserialize<UploadFotoEspacioRequest>(fotoJson, jsonOps);
                    if (fotoDto == null)
                    {
                        return BadRequest("Datos de la imagen invalidos o no presentes");
                    }

                    var foto = new FotoEspacio
                    {
                        EspacioId = fotoDto.EspacioId,
                        Nombre = fotoDto.Observaciones,
                        Fecha = fotoDto.Fecha,                          
                        Id = Guid.NewGuid()
                    };

                    // Read the image file into a byte array
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);
                    foto.Imagen = await ImageService.DelphiSafeImage(memoryStream.ToArray());

                    // Save the Foto with the image data
                    var res = await db.GuardarFotoEspacio(foto, UsuarioActual);
                    return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
                }
                catch (JsonException ex)
                {
                    return BadRequest($"Datos de la imagen invalidos: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error procesando imagen: {ex.Message}");
                }
            });
        }
    }
}