using Cbsdg.Data.Helpers;

namespace Mantenimiento.Db;

public interface IDbProviderFactory
{
    Task<OpResult<DbProvider>> GetProviderAsync(Guid id, string usr);
    AuthDbProvider GetAuthDbProvider();

    Task<OpResult<string>> GetConnStringEmpresa(Guid id, string usr);
}
