using Cbsdg.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cbsdg.Data.Core
{
    public abstract class ProviderBase : IDisposable
    {
        public IDataRowMapper RowMapper { get; set; }

        //private static readonly Dictionary<string, Dictionary<string, int>> _fieldCache = [];

        public string ConnectionString { get; set; }
        //private Database? _database;
        public string UserParamName { get; set; } = "userId";

        protected ProviderBase(IDataRowMapper mapper, string connStr)
        {
            RowMapper = mapper;
            ConnectionString = connStr;
        }
        protected ProviderBase(string connStr)
        {
            RowMapper = new DataRowMapper();
            ConnectionString = connStr;
        }                

        protected virtual Database GetDb()
        {
            //return _database ??= new Database(ConnectionString);
            return new Database(ConnectionString);
        }

        public static T To<T>(object value, T defaultValue)
        {
            return (value != DBNull.Value) ? (T)value : defaultValue;
        }

        public static string ToStr(object value)
        {
            return (value != DBNull.Value) ? (string)value : string.Empty;
        }

        #region Execs
        protected async Task<OpResult<int>> ExecVoid(string sp, params object?[] args)
        {
           using var database = GetDb();
           return await ExecVoid(database, sp, args);
        }

        protected async static Task<OpResult<int>> ExecVoid(Database database, string sp, params object?[] args)
        {
            return await database.ExecuteNonQueryAsync(sp, args);
        }

        protected async Task<OperationResult> Exec(string sp, string outPar, params object[] args)
        {
            var database = GetDb();
            return await Exec(database, sp, outPar, args);
        }

        protected static async Task<OperationResult> Exec(Database database, string sp, string outPar, params object[] args)
        {
            return await database.ExecuteWithResultAsync(sp, outPar, args);
        }

        #endregion

        public async Task<OperationResult> GetScalarParamsAsync(string sp, params object[] args)
        {
            using var database = GetDb();
            return await database.ExecuteScalarAsync(sp, args);
        }

        public async Task<OpResult<T>> GetScalarParams<T>(string sp, params object[] args)
        {
            using var database = GetDb();
            return await database.ExecuteScalar<T>(sp, args);
        }

        protected static async Task<OpResult<IDataReader>> GetReaderAsync(Database database, string sp, params object[] args)
        {
            var rdr = await database.ExecuteReaderAsync(sp, args);
            return new OpResult<IDataReader>(rdr);
        }
                
        public async Task<OpResult<T>> GetRowFromDb<T>(Database database, string sp, params object[] args) where T : new()
        {
            try
            {
                var result = new T();
                var rdrResult = await GetReaderAsync(database, sp, args);
                if (!rdrResult.Success)
                {
                    return new OpResult<T> { Success = false, ErrorMessage = rdrResult.ErrorMessage };
                }
                using var reader = rdrResult.Result;
                return reader.Read() ? new OpResult<T>(RowMapper.Map(result, reader)) : new OpResult<T>() { ErrorMessage = "Record no existe", ErrorCode = "404", Success = false };
            }
            catch (Exception ex)
            {
                return OpResult<T>.FromException(ex);
            }
        }
        public async Task<OpResult<T>> GetRowFromDb<T>(string sp, params object[] args) where T : new()
        {
            using var database = GetDb();
            return await GetRowFromDb<T>(database, sp, args);
        }

        public async Task<OpResult<T>> GetRowFromDb<T>(string sp, Func<IDataRecord, T> mapper, params object[] args)
        {
            using var database = GetDb();
            return await GetRowFromDb(database, sp, mapper, args);
        }
        public async Task<OpResult<T>> GetRowFromDbMapAsync<T>(string sp, Func<IDataRecord, Task<T>> mapper, params object[] args) where T : new()
        {
            using var database = GetDb();
            return await GetRowFromDb(database, sp, mapper, args);
        }

        public static async Task<OpResult<T>> GetRowFromDb<T>(Database database, string sp, Func<IDataRecord, T> mapper, params object[] args)
        {
            var rdrResult = await GetReaderAsync(database, sp, args);
            if (!rdrResult.Success)
            {
                return OpResult<T>.Error(rdrResult.ErrorMessage);
            }
            using var reader = rdrResult.Result;
            if (reader.Read())
            {
                return new OpResult<T>(mapper(reader));
            }
            return new OpResult<T>() { ErrorMessage = "Record no existe", ErrorCode = "404", Success = false };
        }

        public static async Task<OpResult<T>> GetRowFromDb<T>(Database database, string sp, Func<IDataRecord, Task<T>> mapper, params object[] args) where T : new()
        {
            var rdrResult = await GetReaderAsync(database, sp, args);
            if (!rdrResult.Success)
            {
                return OpResult<T>.Error(rdrResult.ErrorMessage);
            }
            using var reader = rdrResult.Result;
            if (reader.Read())
            {
                return new OpResult<T>(await mapper(reader));
            }
            return new OpResult<T>() { ErrorMessage = "Record no existe", ErrorCode = "404", Success = false };
        }

        public async Task<OpResult<List<T>>> GetListFromDb<T>(Database database, string sp, params object[] args) where T : new()
        {
            return await GetListFromDb<T>(database, sp, (row) => RowMapper.Map(new T(), row), args);
        }

        public async Task<OpResult<List<T>>> GetListFromDb<T>(string sp, params object[] args) where T : new()
        {
            using var database = GetDb();
            return await GetListFromDb<T>(database, sp, (row) => RowMapper.Map(new T(), row), args);            
        }

        public static async Task<OpResult<List<T>>> GetListFromDb<T>(Database database, string sp, Func<IDataRecord, Task<T>> mapper, params object[] args)
        {
            var rdrResult = await GetReaderAsync(database, sp, args);
            if (!rdrResult.Success)
            {
                return new OpResult<List<T>> { Success = false, ErrorMessage = rdrResult.ErrorMessage };
            }

            using var reader = rdrResult.Result;
            return await GetListFromReaderAsync(reader, mapper);
        }

        public async Task<OpResult<List<T>>> GetListFromDb<T>(string sp, Func<IDataRecord, T> mapper, params object[] args)
        {
            using var database = GetDb();
            return await GetListFromDb(database, sp, mapper, args);
        }

        public static async Task<OpResult<List<T>>> GetListFromDb<T>(Database database, string sp, Func<IDataRecord, T> mapper, params object[] args) 
        {
            var rdrResult = await GetReaderAsync(database, sp, args);
            if (!rdrResult.Success)
            {
                return new OpResult<List<T>> { Success = false, ErrorMessage = rdrResult.ErrorMessage };
            }

            using var reader = rdrResult.Result;
            return GetListFromReader(reader, mapper);            
        }

        public static OpResult<List<T>> GetListFromReader<T>(IDataReader reader, Func<IDataRecord, T> mapper)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(mapper);

                var lista = new List<T>();
                while (reader.Read())
                {
                    lista.Add(mapper(reader));
                }
                return new OpResult<List<T>>(lista);
            }
            catch (Exception ex)
            {
                return new OpResult<List<T>>(ex);
            }
        }

        public static async Task<OpResult<List<T>>> GetListFromReaderAsync<T>(IDataReader reader, Func<IDataRecord, Task<T>> mapper)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(mapper);

                var lista = new List<T>();
                while (reader.Read())
                {
                    lista.Add(await mapper(reader));
                }
                return new OpResult<List<T>>(lista);
            }
            catch (Exception ex)
            {
                return new OpResult<List<T>>(ex);
            }
        }

        //static public IDictionary<string, int> LoadFieldMap(string key, IDataRecord row)
        //{
        //    if (_fieldCache.ContainsKey(key))
        //    {
        //        return _fieldCache[key];
        //    }
        //    var map = LoadFieldMap(row);
        //    _fieldCache.Add(key, map);
        //    return map;
        //}

        //static public Dictionary<string, int> LoadFieldMap(IDataRecord row)
        //{
        //    var res = new Dictionary<string, int>();
        //    for (int i = 0; i < row.FieldCount; i++)
        //    {
        //        res.Add(row.GetName(i).ToLower(), i);
        //    }

        //    return res;
        //}

        #region Cleanup
        public virtual void CleanUp()
        {
            //_database?.Dispose();
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
