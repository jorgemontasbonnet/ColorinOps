using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Cbsdg.Data.Helpers
{
    public interface IParamMapper
    {
        void Map<T>(T obj, SqlParameterCollection pars, Func<string, object, object> doMap);
    }
}