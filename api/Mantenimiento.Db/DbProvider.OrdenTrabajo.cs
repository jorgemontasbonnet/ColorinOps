using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Mantenimiento.Models;
using Cbsdg.Data.Core;
using Cbsdg.Data.Helpers;

namespace Mantenimiento.Db;    

public partial class DbProvider : ProviderBase
{    
    public async Task<OpResult<List<OrdenServicio>>> GetOrdenesPorBrigadaAsync(string user, 
        DateTime desde,
        DateTime hasta,
        string numero,
        bool cerradas)
    {
        return await GetListFromDb<OrdenServicio>("pubw_ordenServicio_selectByBrigada", 
            desde,
            hasta,
            numero,
            cerradas,
            user);
    }    

    public async Task<OpResult<OrdenServicio>> GetOrdenServicio(Guid id, string user)
    {
        using var db = GetDb();
        var otRes = await GetRowFromDb<OrdenServicio>(db, "pubw_ordenServicio_selectOne", id, user);
        if (otRes.HasError) return otRes;

        var detRes = await GetListFromDb<OrdenServicioTrabajo>(db, "pubw_OrdenServTrabajo_select", id);
        if (detRes.HasError && detRes.ErrorCode != "404")
        {
            return OpResult<OrdenServicio>.Error(detRes.ErrorMessage, detRes.ErrorCode);
        }

        otRes.Result.Trabajos = detRes.Result;

        var fotosRes = await GetListFromDb<Foto>(db, "pubw_ordenServicioFoto_selectAll", id);
        if (fotosRes.HasError && detRes.ErrorCode != "404")
        {
            return OpResult<OrdenServicio>.Error(fotosRes.ErrorMessage, fotosRes.ErrorCode);
        }
        otRes.Result.Fotos = fotosRes.Result;

        //if (otRes.Result.Trabajos.Count > 0)
        //{
        //    var feRes = await GetListFromDb<FotoEspacio>(db, "pubw_fotoEspacio_selectByOrdenId", id);
        //    if (feRes.Success)
        //    {
        //        foreach (var t in otRes.Result.Trabajos)
        //        {
        //            t.Fotos = feRes.Result.Where(f => f.EspacioId == t.EspacioId).ToList();
        //        }
        //    }
        //}

        return otRes;
    }

    public async Task<OpResult<List<Foto>>> GetFotosPorOrdenId(Guid ordenId)
    {
        return await GetListFromDb<Foto>("pubw_ordenServicioFoto_selectAll", ordenId);
    }

    public async Task<OpResult<int>> MarcarOrdenLista(Guid ordenId, bool listo, DateTime inicio, DateTime fin, string obs, string usuario)
    {
        return await ExecVoid("pubw_ordenServicio_marcarLista", ordenId, listo, inicio, fin, obs, usuario);
    }

    public async Task<OpResult<int>> BorrarFoto(Guid id, string usuario)
    {
        return await ExecVoid("pubw_ordenServicioFoto_delete", id, usuario);
    }
    
    public async Task<OpResult<int>> GuardarFoto(Foto dto, string usuario)
    {
        return await ExecVoid("pubw_ordenServicioFoto_post", dto.Id, dto.OrdenServicioId,
            dto.Observaciones, dto.Imagen, usuario);
    }

    public async Task<OpResult<Foto>> GetFotoPorId(Guid id)
    {
        return await GetRowFromDb<Foto>("pubw_ordenServicioFoto_selectOne_noFoto", id);
    }
    public async Task<OpResult<byte[]>> GetImageBytes(Guid id)
    {
        var fotoRes = await GetRowFromDb<Foto>("pubw_ordenServicioFoto_selectOne", id);
        if (fotoRes.HasError)
        {
            return OpResult<byte[]>.Error(fotoRes.ErrorMessage, fotoRes.ErrorCode);
        }

        return new OpResult<byte[]>(fotoRes.Result.Imagen);
    }    

    public async Task<OpResult<OrdenServicioTrabajo>> GetTrabajoPorId(Guid id, string usuario)
    {
        using var db = GetDb();

        var res = await GetRowFromDb<OrdenServicioTrabajo>(db, "pubw_OrdenServTrabajo_selectOne", id, usuario);
        if (res.HasError)
        {
            return res;
        }

        var feRes = await GetListFromDb<FotoEspacio>(db, "pubw_espaciofoto_selectAll", res.Result.EspacioId);
        if (feRes.Success)
        {
              res.Result.Fotos = feRes.Result;               
            
        }

        return res;
    }

}

