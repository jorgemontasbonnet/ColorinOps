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
    public class FotosController(IDbProviderFactory dbf) : CbsDbControllerBase(dbf)
    {
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.BorrarFoto(id, UsuarioActual);
                return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
            });
        }

        [HttpGet]
        [Route("list/{id:guid}")]
        public async Task<IActionResult> GetList([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetFotosPorOrdenId(id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                return Ok(res.Result);
            });
        }


        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            return await RunDbTask(async db =>
            {
                var res = await db.GetFotoPorId(id);
                if (res.HasError)
                    return BadRequest(res.ErrorMessage);

                return Ok(res.Result);
            });
        }
        
        [HttpGet]
        [Route("jpg/{size}/{id:guid}")]
        [OutputCache (Duration = 1024)]
        public async Task<IActionResult> GetAsPng([FromRoute] string size, [FromRoute] Guid id)
        {
            var checkedSize = (size == "xl") ? "xl" : "sm";

            return await RunDbTask(async db =>
            {
                var res = await db.GetImageBytes(id);
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
                    var fotoDto = JsonSerializer.Deserialize<UploadFotoRequest>(fotoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (fotoDto == null)
                    {
                        return BadRequest("Datos de la imagen invalidos o no presentes");
                    }

                    var foto = new Foto
                    {
                        OrdenServicioId = fotoDto.OrdenId,
                        Observaciones = fotoDto.Observaciones,
                        Usuario = UsuarioActual,
                        Id = Guid.NewGuid()
                    };

                    // Read the image file into a byte array
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);
                    foto.Imagen = await ImageService.DelphiSafeImage(memoryStream.ToArray());

                    // Save the Foto with the image data
                    var res = await db.GuardarFoto(foto, UsuarioActual);
                    return res.HasError ? BadRequest(res.ErrorMessage) : Ok(res.Result);
                }
                catch (JsonException ex)
                {
                    return BadRequest($"Invalid Foto JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error processing upload: {ex.Message}");
                }
            });
        }
    }
}