using System;
using System.Data;

namespace Cbsdg.Data.Core
{
    public interface IDataRowMapper
    {
        T Map<T>(T obj, IDataRecord row, Func<string, IDataRecord, object?> doMap);
        T Map<T>(T obj, IDataRecord row);
    }
}