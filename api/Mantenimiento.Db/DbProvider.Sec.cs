 
using Cbsdg.Data.Helpers;
using Mantenimiento.Models;

namespace Mantenimiento.Db;

public partial class DbProvider 
{
    public async Task<OpResult<List<string>>> GetRolesUsuario(string id)
    {
        return await GetListFromDb("vw_user_role_select", (reader) => To(reader[0], string.Empty), id);
    }

    public async Task<OpResult<List<string>>> GetRoles()
    {
        return await GetListFromDb("vw_role_list", r => r.GetString(0));
    }
    public async Task<OpResult<Permiso>> GetPermisoObjeto(string usr, string objetoId)
    {
        return await GetRowFromDb<Permiso>("vwPermiso_selectPorObjetoId", usr, objetoId);
    }
}
