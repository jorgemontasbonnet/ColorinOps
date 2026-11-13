using Cbsdg.Data.Helpers;
using System.Net.NetworkInformation;

namespace Mantenimiento.Db;

public class DbProviderFactory : IDbProviderFactory
{
    private readonly Dictionary<Guid, DbProvider> providers = [];
    private readonly AuthDbProvider authDbProvider;

    public DbProviderFactory(string conx)
    {
        if (string.IsNullOrWhiteSpace(conx))
        {
            throw new Exception("No se ha definido la cadena de conexión a la base de datos!");
        }

        authDbProvider = new AuthDbProvider(conx);
    }

    public AuthDbProvider GetAuthDbProvider()
    {
        return authDbProvider;
    }

    public async Task<OpResult<string>> GetConnStringEmpresa(Guid id, string usr)
    {
        var empConnRes = await authDbProvider.GetConnEmpresa(id, usr);
        if ( empConnRes.HasError)
        {
            return OpResult<string>.Error($"Error en conexión a la empresa: {empConnRes.ErrorMessage}");
        }
        var empConn = empConnRes.Result;
        
        return new OpResult<string>(BuildConxString(empConn.Servidor, empConn.BaseDatos, empConn.Trusted, empConn.UserId, empConn.UserPwd, empConn.Encriptada));        
    }

    public async Task<OpResult<DbProvider>> GetProviderAsync(Guid id, string usr)
    {
        var res = await authDbProvider.GetConnEmpresa(id, usr);
        if (res.HasError)
        {
            return OpResult<DbProvider>.Error(res.ErrorMessage); 
        }
        var conx = BuildConxString(res.Result.Servidor, res.Result.BaseDatos, res.Result.Trusted, res.Result.UserId, res.Result.UserPwd, res.Result.Encriptada);
        return new OpResult<DbProvider>(AddProvider(id, conx));
    }

    private DbProvider AddProvider(Guid id, string conxStr)
    {
        if (providers.TryGetValue(id, out DbProvider? value))
        {
            return value;
        }

        var prov = new DbProvider(conxStr);
        providers.Add(id, prov);
        return prov;
    }

    private static string BuildConxString(string servidor, string dbname, bool trusted, string user, string password, bool encriptada)
    {
        string userInfo;
        if (trusted)
        {
            userInfo = "Integrated Security=True";
        }
        else
        {
            if (encriptada)
            {
                userInfo = $"User ID={CypherHelper.DeEncryptString(user)};Password={CypherHelper.DeEncryptString(password)}";
            }
            else
            {
                userInfo = $"User ID={user};Password={password}";

            }
        }
        return $"Data Source={servidor};Initial Catalog={dbname};{userInfo};MultipleActiveResultSets=True;Encrypt=False";
    }
}