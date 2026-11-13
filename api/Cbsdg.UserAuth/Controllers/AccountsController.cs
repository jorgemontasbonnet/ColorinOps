using Cbsdg.Data.Helpers;
using Cbsdg.Data.Helpers.Security;
using Cbsdg.UserAuth.Models;
using Mantenimiento.Db;
using Mantenimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Cbsdg.UserAuth.Controllers;


[Produces("application/json")]
[Route("accounts")]
[ApiController]
[EnableCors]
public class AccountsController(IConfiguration configuration, 
        IDbProviderFactory dbf) : CbsControllerBase
{

    const int REFRESH_TOKEN_TIME = 30; // minutos

    private readonly AuthDbProvider authDb = dbf.GetAuthDbProvider();
        
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
                var pwMatch = request.Password.Equals(dto.Password, StringComparison.InvariantCulture);                
                if (pwMatch)
                {
                    var fixedTenant = GetFixedTenant();
                    if (fixedTenant != null)
                    {
                        dto.Empresas = [ fixedTenant ];
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
                        expires: DateTime.Now.AddMinutes(REFRESH_TOKEN_TIME),
                        signingCredentials: creds);

                    var refreshToken = GenerateRefreshToken(dto.UserId, configuration);
                    var sesionId = Guid.NewGuid();

                    var payLoad = new LoginResponse
                    {
                        SessionId = sesionId, 
                        Username = dto.UserId,
                        Email = dto.Email,
                        FriendlyName = dto.FriendlyName,
                        Tenants = dto.Empresas,
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        RefreshToken = refreshToken
                    };   
                    
                    await authDb.CreateSession(sesionId, dto.UserId, DateTime.Now.AddDays(30), payLoad.Token, refreshToken, string.Empty);

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
    [Route("regacceso/{tenantId}/{userId}")]
    public async Task<IActionResult> RegAccesoEmpresa(Guid tenantId, string userId)
    {        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("No se pudo obtener el usuario de la sesión!");
        }

        var result = await authDb.RegistrarAcceso(userId, tenantId);
        return Ok(result);
    }

    [HttpGet]    
    [Route("roles/{tenantId}/{userId}")]
    public async Task<IActionResult> GetRoles(Guid tenantId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || tenantId == Guid.Empty)
        {
            return BadRequest("Parámetros inválidos!");
        }

        var dbResult = await dbf.GetProviderAsync(tenantId, userId);
        if (dbResult.HasError)
        {
            return BadRequest(dbResult.ErrorMessage);
        }
        var rolesRes = await dbResult.Result.GetRolesUsuario(userId);
        if (rolesRes.HasError)
        {
            return BadRequest(rolesRes.ErrorMessage);
        }
        return Ok(rolesRes.Result);
    }

    [HttpPost]    
    [Route("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req)
    {   
        var sessionRes = await authDb.GetSessionById(req.SessionId);
        if (sessionRes.HasError)
        {
            return BadRequest(sessionRes.ErrorMessage);
        }

        var refreshToken = sessionRes.Result.RefreshToken;

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(refreshToken))
        {
            return BadRequest("El refresh token no es válido!");
        }

        var decoded = tokenHandler.ReadJwtToken(refreshToken);
        if (decoded.ValidTo < DateTime.UtcNow)
        {
            return BadRequest("El refresh token ha expirado!");
        }

        var userId = decoded.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("No se pudo obtener el usuario de la sesión!");
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
                expires: DateTime.Now.AddMinutes(REFRESH_TOKEN_TIME),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            
            return Ok(new
            {
                req.SessionId,
                Username = dto.UserId,
                dto.Email,
                dto.FriendlyName,
                tenants = dto.Empresas,
                token = accessToken,
                refreshToken = sessionRes.Result.RefreshToken
            });
        }
        return Problem(detail: "Error en usuario y/o password!", statusCode: 400, title: "Error en usuario y/o password!!");
    }

    [HttpPost]    
    [Route("logout")]
    public async Task<IActionResult> Logout(string usuario)
    {
        await authDb.ClearAllSessions(usuario);
        return Ok();
    }

    [HttpGet]
    [Route("session/{sessionId}")]
    public async Task<IActionResult> GetSession([FromRoute] Guid sessionId)
    {
        var result = await authDb.GetSessionById(sessionId);
        if (result.HasError)
        {            
            return BadRequest(result.ErrorMessage);
        }
        return Ok(new AuthSession
        {
            Id = sessionId,
            Cancelada = false,
            Inicio = result.Result.Inicio,
            Expiracion = result.Result.Expiracion
        });
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


    private string GenerateRefreshToken(string username, IConfiguration configuration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
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


