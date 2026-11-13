using Cbsdg.Data.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cbsdg.Data.Core
{
    public class Database : IDisposable
    {
        public SqlConnection? Connection { get; set; }
        public string? ConnectionString { get; set; }
        private int _tranCount = 0;
        
        private readonly ConcurrentDictionary<string, SqlParameterCollection> ParamCache = new ();

        public Database() : this(default)
        {
        }

        public Database(string? conStr)
        {
            ConnectionString = conStr;
        }

        public SqlTransaction? CurrentTransaction { get; set; }

        public SqlTransaction? BeginTransaction()
        {
            _tranCount++;
            return null;
            //Open();

            //return CurrentTransaction ?? (CurrentTransaction = Connection.BeginTransaction());
        }

        public void RollbackTransaction()
        {
            if (_tranCount > 0)
            {
                _tranCount--;
            }

            if (CurrentTransaction == null) return;

            CurrentTransaction.Rollback();
            CurrentTransaction = null;
        }

        public void CommitTransaction()
        {
            if (_tranCount > 0)
            {
                _tranCount--;
            }

            if (CurrentTransaction == null) return;

            CurrentTransaction.Commit();
            CurrentTransaction = null;
        }
        public async Task OpenAsync()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new Exception("No a configurado la conexion a la base de datos!");
            }

            try
            {
                if (Connection is { State: ConnectionState.Open })
                {
                    return;
                }

                Connection = new SqlConnection(ConnectionString);
                await Connection.OpenAsync();

                int trs = 0;
                while ((Connection.State == ConnectionState.Connecting) && (trs < 10))
                {
                    await Task.Delay(50);
                    trs++;
                }
            }            
            catch (Exception ex)
            {
                throw new Exception("Error al abrir la conexion a la base de datos", ex);
            }
        }
        public void Open()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new Exception("No a configurado la conexion a la base de datos!");
            }

            if (Connection is { State: ConnectionState.Open })
            {
                return;
            }

            Connection = new SqlConnection(ConnectionString);
            Connection.Open();
        }

        public IDataReader GetStringCommand(string sql)
        {
            Open();
            var cmd = new SqlCommand(sql, Connection);
            return cmd.ExecuteReader();
        }

        public async Task<SqlCommand> GetCommandAsync(string sp)
        {
            await OpenAsync();
            var cmd = new SqlCommand(sp, Connection) { CommandType = CommandType.StoredProcedure };
            if (CurrentTransaction != null)
            {
                cmd.Transaction = CurrentTransaction;
            }
            DeriveParameters(cmd);

            return cmd;
        }

        public async Task<SqlCommand> GetCommandAsync(string sp, params object?[] args)
        {
            var cmd = await GetCommandAsync(sp);
            for (var i = 0; i < args.Length; i++)
            {
                var x = i + 1;
                if (x < cmd.Parameters.Count)
                {
                    cmd.Parameters[x].Value = (args[i] ?? DBNull.Value);
                }
            }            
            return cmd;
        }

        public SqlCommand GetCommand(string sp)
        {
            Open();
            var cmd = new SqlCommand(sp, Connection) { CommandType = CommandType.StoredProcedure };
            if (CurrentTransaction != null)
            {
                cmd.Transaction = CurrentTransaction;
            }
            DeriveParameters(cmd);

            return cmd;
        }

        public SqlCommand GetCommand(string sp, params object?[] args)
        {
            var cmd = GetCommand(sp);
            for (var i = 0; i < args.Length; i++)
            {
                var x = i + 1;
                if (x < cmd.Parameters.Count)
                {
                    cmd.Parameters[x].Value = (args[i] ?? DBNull.Value);
                }
            }
            return cmd;
        }

        public async Task<OpResult<int>> ExecuteNonQueryAsync(string sp, params object?[] args)
        {
            try
            {
                var cmd = await GetCommandAsync(sp, args);
                var i = await cmd.ExecuteNonQueryAsync();
                return new OpResult<int>(i);
            }
            catch (Exception ex)
            {
                return new OpResult<int>(ex);
            }
        }

        public int ExecuteNonQuery(string sp, params object?[] args)
        {
            var cmd = GetCommand(sp, args);
            return cmd.ExecuteNonQuery();
        }

        public async Task<OperationResult> ExecuteWithResultAsync(string sp, string outPar, params object[] args)
        {            
            //try 
            //{
            //    var cmd = await GetCommandAsync(sp, args);
            //    await cmd.ExecuteNonQueryAsync();
            //    return new OperationResult(cmd.Parameters[outPar].Value);
            //}
            //catch (Exception ex)
            //{
            //    return new OperationResult(ex);
            //}            

            return await RunTaskObj(async () =>
            {
                try
                {
                    var cmd = await GetCommandAsync(sp, args);
                    await cmd.ExecuteNonQueryAsync();
                    return cmd.Parameters[outPar].Value;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error ejecutando tarea", ex);
                }
            });
        }

        public async Task<IDataReader> ExecuteReaderAsync(string sp, params object[] args)
        {
            var cmd = await GetCommandAsync(sp, args);
            return await cmd.ExecuteReaderAsync();
        }

        public IDataReader ExecuteReader(string sp, params object[] args)
        {
            var cmd = GetCommand(sp, args);
            return cmd.ExecuteReader();
        }

        public async Task<OperationResult> ExecuteScalarAsync(string sp, params object[] args)
        {
            return await RunTaskObj(async () =>
            {
                var cmd = await GetCommandAsync(sp, args);
                return await cmd.ExecuteScalarAsync();
            });            
        }

        public async Task<OpResult<T>> ExecuteScalar<T>(string sp, params object[] args)
        {
            return await RunTask(async () =>
            {
                var cmd = await GetCommandAsync(sp, args);
                var obj = await cmd.ExecuteScalarAsync();
                if (obj == null || obj == DBNull.Value)
                {
                    return default!;
                }
                return (T)obj;
            });
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                Connection?.Dispose();
                Connection = null;
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        internal void DeriveParameters(SqlCommand cmd)
        {
            if (cmd.CommandType != CommandType.StoredProcedure)
            {
                throw new ArgumentException("Solo se puede usar DeriveParameters con sprocs");
            }

            var key = cmd.CommandText.ToLowerInvariant();
            if (ParamCache.ContainsKey(key))
            {
                cmd.Parameters.Clear();
                var paramCollection = ParamCache[key];
                cmd.Parameters.AddRange(
                    paramCollection.OfType<SqlParameter>()
                        .Select(x => new SqlParameter
                        {
                            ParameterName = x.ParameterName,
                            SqlDbType = x.SqlDbType,
                            Size = x.Size,
                            Direction = x.Direction
                        })
                        .ToArray()
                );
                return;
            }

            var paramsCmd = new SqlCommand("sp_procedure_params_100_managed", Connection, CurrentTransaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            paramsCmd.Parameters.Add(new SqlParameter("@procedure_name", SqlDbType.NVarChar, 255));
            paramsCmd.Parameters[0].Value = cmd.CommandText;

            var parameters = new List<SqlParameter>();

            using (var r = paramsCmd.ExecuteReader())
            {

                while (r.Read())
                {
                    // each row corresponds to a parameter of the stored proc.  Fill in all the info

                    var p = new SqlParameter
                    {
                        ParameterName = (string)r["PARAMETER_NAME"],
                        SqlDbType = (SqlDbType)(short)r["MANAGED_DATA_TYPE"]
                    };

                    switch (p.SqlDbType)
                    {
                        case SqlDbType.Image:
                        case SqlDbType.Timestamp:
                            p.SqlDbType = SqlDbType.VarBinary;
                            break;

                        case SqlDbType.NText:
                            p.SqlDbType = SqlDbType.NVarChar;
                            break;

                        case SqlDbType.Text:
                            p.SqlDbType = SqlDbType.VarChar;
                            break;
                    }
                    var a = r["CHARACTER_MAXIMUM_LENGTH"];
                    if (a is int size)
                    {
                        // Map MAX sizes correctly.  The Katmai server-side proc sends 0 for these instead of -1.
                        //  Should be fixed on the Katmai side, but would likely hold up the RI, and is safer to fix here.
                        //  If we can get the server-side fixed before shipping Katmai, we can remove this mapping.
                        if (0 == size &&
                            (p.SqlDbType == SqlDbType.NVarChar ||
                             p.SqlDbType == SqlDbType.VarBinary ||
                             p.SqlDbType == SqlDbType.VarChar))
                        {
                            size = -1;
                        }
                        p.Size = size;
                    }

                    // direction
                    p.Direction = ParameterDirectionFromOleDbDirection((short)r["PARAMETER_TYPE"]);

                    //if (p.SqlDbType == SqlDbType.Decimal)
                    // {
                    //     p.ScaleInternal = (byte)((short)r[colNames[(int)ProcParamsColIndex.NumericScale]] & 0xff);
                    //     p.PrecisionInternal = (byte)((short)r[colNames[(int)ProcParamsColIndex.NumericPrecision]] & 0xff);
                    // }

                    // type name for Structured types (same as for Udt's except assign p.TypeName instead of p.UdtTypeName
                    if (SqlDbType.Structured == p.SqlDbType)
                    {
                        //read the type name
                        p.TypeName = (string)r["TYPE_NAME"];
                    }

                    parameters.Add(p);
                }
            }
            cmd.Parameters.Clear();

            foreach (var temp in parameters)
            {
                cmd.Parameters.Add(temp);
            }

            ParamCache[key] = cmd.Parameters;
        }

        private static ParameterDirection ParameterDirectionFromOleDbDirection(short oledbDirection)
        {
            return oledbDirection switch
            {
                2 => ParameterDirection.InputOutput,
                3 => ParameterDirection.Output,
                4 => ParameterDirection.ReturnValue,
                _ => ParameterDirection.Input
            };
        }

        public static async Task<OperationResult> RunTaskObj(Func<Task<object?>> action)
        {
            try
            {
                var result = await action();
                return new OperationResult(result);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public static async Task<OpResult<T>> RunTask<T>(Func<Task<T>> action)
        {
            try
            {
                var result = await action();
                return new OpResult<T>(result);
            }
            catch (Exception ex)
            {
                return new OpResult<T>(ex);
            }
        }
        
    }
}
