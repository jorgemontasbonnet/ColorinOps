 
using Cbsdg.Data.Helpers;
using Mantenimiento.Models;

namespace Mantenimiento.Db;

public partial class DbProvider 
{
    public async Task<OpResult<List<FotoEspacio>>> GetFotosPorEspacioId(string espacioId)
    {
        return await GetListFromDb<FotoEspacio>("pubw_espacioFoto_selectAll", espacioId);
    }

    public async Task<OpResult<List<FotoEspacio>>> GetFotosEspacioPorOrdenId(Guid id)
    {
        return await GetListFromDb<FotoEspacio>("pubw_espacioFoto_selectByOrdenServicioIdAll", id);
    }

    public async Task<OpResult<int>> BorrarFotoEspacio(string espacioId, Guid id, string usuario)
    {
        return await ExecVoid("pubw_espacioFoto_delete", espacioId, id, usuario);
    }

    public async Task<OpResult<int>> GuardarFotoEspacio(FotoEspacio dto, string usuario)
    {
        return await ExecVoid("pubw_espacioFoto_post", dto.EspacioId,
            dto.Nombre, dto.Fecha, dto.Imagen, usuario);
    }

    public async Task<OpResult<FotoEspacio>> GetFotoEspacioPorId(string espacioId, Guid id)
    {
        return await GetRowFromDb<FotoEspacio>("pubw_espacioFoto_selectOne_noFoto", espacioId, id);
    }
    public async Task<OpResult<byte[]>> GetFotoEspacioBytes(string espacioId, Guid id)
    {
        var fotoRes = await GetRowFromDb<FotoEspacio>("pubw_espacioFoto_selectOne", espacioId, id);
        if (fotoRes.HasError)
        {
            return OpResult<byte[]>.Error(fotoRes.ErrorMessage, fotoRes.ErrorCode);
        }

        return new OpResult<byte[]>(fotoRes.Result.Imagen);
    }


    public async Task<OpResult<Impreso>> GetImpresoPorId(Guid id)
    {
        return await GetRowFromDb<Impreso>("pubw_impreso_selectOne", id);
    }

    public async Task<OpResult<byte[]>> GetImpresoBytes(Guid id)
    {
        var fotoRes = await GetScalarParamsAsync("pubw_impreso_select_imagen", id);
        if (fotoRes.HasError)
        {
            return OpResult<byte[]>.Error(fotoRes.ErrorMessage, fotoRes.ErrorCode);
        }

        return new OpResult<byte[]>(fotoRes.Result is null ? [] : (byte[]) fotoRes.Result);
    }
}
