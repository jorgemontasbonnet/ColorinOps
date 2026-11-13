using Cbsdg.Data.Helpers.Security;
using Mantenimiento.Api.Filters;
using Mantenimiento.Api.Models;
using Mantenimiento.Db;
using Mantenimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Mantenimiento.Api.Controllers;

[Produces("application/json")]
[Route("accounts")]
[ApiController]
[EnableCors]
public class AccountsController(IConfiguration configuration,
        IDbProviderFactory dbf) : CbsControllerBase
{

    private readonly AuthDbProvider authDb = dbf.GetAuthDbProvider();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> RequestToken([FromBody] TokenRequest request)
    {
        try
        {
            var usrRes = await authDb.ValidarUsuarioAsync(request.Username);
            if (usrRes.HasError)
            {
                return BadRequest(usrRes.ErrorMessage);
            }
            var dto = usrRes.Result;
            if (dto != null)
            {
                //var pwMatch = CypherHelper.PasswordsMatch(request.Password, dto.Password);
                var pwMatch = request.Password.Equals(dto.Password, StringComparison.InvariantCulture);
                //if (!string.IsNullOrWhiteSpace(request.Password) && (request.Password.Equals(_cypher.DecryptString(dto.Password), StringComparison.InvariantCulture)))
                if (pwMatch)
                {
                    var fixedTenant = GetFixedTenant();
                    if (fixedTenant != null)
                    {
                        dto.Empresas = [fixedTenant];
                    }
                    var tennants = string.Join(',', dto.Empresas.Select(x => x.Id));
                    var claims = new[]
                    {
                    new Claim(ClaimTypes.Name, dto.UserId),
                    new Claim(ClaimTypes.Email, dto.Email),
                    new Claim(ClaimTypes.GivenName, dto.FriendlyName),
                    new Claim("TENNANTS", tennants)
                };
                    var keyString = configuration["SecurityKey"] ?? string.Empty;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: configuration["SecIssuer"],
                        audience: configuration["SecAudience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(30),
                        signingCredentials: creds);

                    var payLoad = new LoginResponse
                    {
                        Username = dto.UserId,
                        Email = dto.Email,
                        FriendlyName = dto.FriendlyName,
                        Tenants = dto.Empresas,
                        Token = new JwtSecurityTokenHandler().WriteToken(token)
                    };

                    if (request.RememberMe)
                    {
                        var refreshToken = GenerateRefreshToken(payLoad, configuration);
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTime.UtcNow.AddDays(30),
                            SameSite = SameSiteMode.None,
                            Secure = true
                        };
                        Response.Cookies.Append("X-CBS-REFRESH-TOKEN", refreshToken, cookieOptions);
                    }

                    return Ok(payLoad);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error en RequestToken");
        }
        return Problem(detail: "Error en usuario y/o password!", statusCode: 400, title: "Error en usuario y/o password!!");
    }

    [HttpPost]
    [Authorize]
    [Route("regacceso/{empresaId}")]
    public async Task<IActionResult> RegAccesoEmpresa(Guid empresaId)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("No se pudo obtener el usuario de la sesión!");
        }

        var result = await authDb.RegistrarAcceso(userId, empresaId);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    [Route("roles")]
    [RequireTennantHeader]
    public async Task<IActionResult> GetRoles()
    {
        var dbResult = await dbf.GetProviderAsync(GetTennantId(), UsuarioActual);
        if (dbResult.HasError)
        {
            return BadRequest(dbResult.ErrorMessage);
        }
        var rolesRes = await dbResult.Result.GetRolesUsuario(UsuarioActual);
        if (rolesRes.HasError)
        {
            return BadRequest(rolesRes.ErrorMessage);
        }
        return Ok(rolesRes.Result);
    }

    [HttpPost]
    //[Authorize]
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var cookie = Request.Cookies.TryGetValue("X-CBS-REFRESH-TOKEN", out var refreshToken);
        if (!cookie || string.IsNullOrWhiteSpace(refreshToken))
        {
            //return BadRequest("No se pudo obtener el refresh token !");
            return Unauthorized("No se pudo obtener el refresh token !");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(refreshToken))
        {
            return BadRequest("El refresh token no es válido!");
        }

        var decoded = tokenHandler.ReadJwtToken(refreshToken);
        if (decoded.ValidTo < DateTime.UtcNow)
        {
            return Unauthorized("El refresh token ha expirado!");
        }

        var userId = decoded.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized("No se pudo obtener el usuario de la sesión!");
        }
        var usrRes = await authDb.ValidarUsuarioAsync(userId);
        if (usrRes.HasError)
        {
            return BadRequest(usrRes.ErrorMessage);
        }
        var dto = usrRes.Result;
        if (dto != null)
        {
            var fixedTenant = GetFixedTenant();
            if (fixedTenant != null)
            {
                dto.Empresas = [fixedTenant];
            }
            var tennants = string.Join(',', dto.Empresas.Select(x => x.Id));
            var claims = new[]
            {
                    new Claim(ClaimTypes.Name, dto.UserId),
                    new Claim(ClaimTypes.Email, dto.Email),
                    new Claim(ClaimTypes.GivenName, dto.FriendlyName),
                    new Claim("TENNANTS", tennants)
                };
            var keyString = configuration["SecurityKey"] ?? string.Empty;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["SecIssuer"],
                audience: configuration["SecAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return Ok(new
            {
                username = dto.UserId,
                dto.Email,
                dto.FriendlyName,
                tenants = dto.Empresas,
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
        return Problem(detail: "Error en usuario y/o password!", statusCode: 400, title: "Error en usuario y/o password!!");
    }

    [HttpPost]
    [Authorize]
    [Route("logout")]
    public IActionResult Logout()
    {
        if (Request.Cookies.ContainsKey("X-CBS-REFRESH-TOKEN"))
        {
            Response.Cookies.Delete("X-CBS-REFRESH-TOKEN", new CookieOptions
            {

                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1),
                SameSite = SameSiteMode.None,
                Secure = true

            });
        }
        return Ok();
    }

    private Empresa? GetFixedTenant()
    {
        var tenant = configuration["Tenant:id"];
        if (!string.IsNullOrWhiteSpace(tenant))
        {
            return new Empresa { Id = Guid.Parse(tenant), Nombre = configuration["Tenant:nombre"] ?? "Default" };
        }
        return null;
    }

    private string GenerateRefreshToken(LoginResponse payload, IConfiguration configuration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, payload.Username)
        };
        var keyString = configuration["SecurityKey"] ?? string.Empty;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["SecIssuer"],
            audience: configuration["SecAudience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}


