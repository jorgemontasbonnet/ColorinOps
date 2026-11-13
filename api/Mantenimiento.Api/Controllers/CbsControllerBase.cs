using Cbsdg.Data.Helpers;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Security.Claims;

namespace Mantenimiento.Api.Controllers;

[ApiController]
public class CbsControllerBase() : Controller
{
    protected string UsuarioActual => (User.Identity?.Name ?? string.Empty);
    protected Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();
    protected Guid TennantId => GetTennantId();

    protected Guid GetTennantId()
    {
        var id = Request.Headers["X-CBS-TENNANT"].ToString();
        return string.IsNullOrWhiteSpace(id) ? Guid.Empty : Guid.Parse(id);
    }
    protected void CheckTennantIdPresent()
    {
        if (TennantId == Guid.Empty)
        {
            throw new InvalidOperationException("Debe especificar una empresa válida");
        }
    }

    protected ObjectResult CreateResponse<T>(T obj)
    {
        return new ObjectResult(obj);
    }

    protected IActionResult ListMsg<T>(List<T> items)
    {
        return CreateResponse(new { data = items, isArray = true });
    }

    protected IActionResult ItemResult(object item)
    {
        if (item == null)
        {
            return NotFound();
        }

        return CreateResponse(new { data = item, isArray = false });
    }

    protected IActionResult Msg<T>(T item)
    {
        if (item == null)
        {
            return NotFound();
        }

        return CreateResponse(new { data = item, isArray = false });
    }

    protected IActionResult ReturnOpResult<T>(OpResult<T> res)
    {
        return res.HasError
            ? StatusCode(StatusCodes.Status500InternalServerError, res)
            : StatusCode(StatusCodes.Status200OK, res);
    }

    protected IEnumerable<Claim> GetClaims()
    {
        var claims = new List<Claim>();
        if (User.Identity?.IsAuthenticated == true)
        {
            foreach (var claim in User.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }
        }
        return claims;
    }
}
