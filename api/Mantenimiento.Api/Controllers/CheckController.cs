using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mantenimiento.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class CheckController : CbsControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Msg(new { Estado = "Todo esta bien!" });
    }

    [HttpGet]
    [Route("secured")]   
    [Authorize]
    public IActionResult GetSecured()
    {
        var claims = GetClaims().ToArray();
        return Msg(new { Estado = "Todo esta bien! (seguro)", UsuarioActual, claims });
    }
}