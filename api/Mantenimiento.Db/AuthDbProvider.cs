using Cbsdg.Data.Core;
using Cbsdg.Data.Helpers;
using System.Data;
using Mantenimiento.Models;
namespace Mantenimiento.Db;

public class AuthDbProvider(string conStr) : ProviderBase(conStr)
{
    //protected Usuario MapUser(IDataRecord reader)
    //{
    //    return new Usuario
    //    {
    //        Id = To(reader["Id"], Guid.Empty),
    //        UserId = To(reader["UserId"], string.Empty),
    //        Email = To(reader["Email"], string.Empty),
    //        FriendlyName = To(reader["FriendlyName"], string.Empty),
    //        Activo = To(reader["Activo"], false),
    //        Password = To(reader["Password"], string.Empty)
    //    };
    //}
    public async Task<OpResult<string>> GetUserPassword(string user)
    {        
        return await GetScalarParams<string>("vw_user_get_password", user);
    }

    public async Task<OpResult<Usuario>> GetUser(string usr)
    {
        return await GetRowFromDb<Usuario>("ocweb_user_find", usr);        
    }

    public async Task<OpResult<List<Empresa>>> GetEmpresasUsuario(string usuario)
    {
        return await GetListFromDb<Empresa>("vw_user_empresa_select", usuario);
    }

    public async Task<OpResult<ConeccionEmpresa>> GetConnEmpresa(Guid id, string usuario)
    {
        return await GetRowFromDb<ConeccionEmpresa>("vw_empresa_select_one", id, usuario);
    }

    public async Task<OpResult<int>> RegistrarAcceso(string usuario, Guid id)
    {
        return await ExecVoid("segRegAcceso", usuario, id);
    }

    public async Task<OpResult<Usuario>> ValidarUsuarioAsync(string usr)
    {
        var obj = await GetRowFromDb<Usuario>("ocweb_user_find", usr);
        if (!obj.HasError)
        {
            var empsResult = await GetEmpresasUsuario(usr);
            if (empsResult.HasError)
            {
                return OpResult<Usuario>.Error($"Error cargando empresas para el usuario: {usr}: {empsResult.ErrorMessage}", empsResult.ErrorCode);
            }
            obj.Result.Empresas = [.. empsResult.Result];
        }
        return obj;
    }

    public async Task<OpResult<int>> ChangePassword(string id, string newPwd)
    {
        return await ExecVoid("vw_user_change_password", id, newPwd);
    }       

    public async Task<OpResult<int>> CreateSession(Guid id, string userId, DateTime exp, string token, string refreshToken, string telemetry)
    {
        return await ExecVoid("segSession_create", id, userId, exp, token, refreshToken, telemetry);
    }

    public async Task<OpResult<int>> ClearAllSessions(string userId)
    {
        return await ExecVoid("segSession_clearAll", userId);
    }

    public async Task<OpResult<AuthSession>> GetSession(string userId)
    {
        return await GetRowFromDb<AuthSession>("segSession_select", userId);
    }

    public async Task<OpResult<AuthSession>> GetSessionById(Guid id)
    {
        return await GetRowFromDb<AuthSession>("segSession_selectById", id);
    }


}


