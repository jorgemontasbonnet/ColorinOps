using Cbsdg.Data.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Cbsdg.Data.core
{
    public class ObjectParamMapper : IParamMapper
    {
        private Type? _type;
        private object? _obj;

        protected object? MyGetPropertyValue(string vname)
        {
            if (string.IsNullOrWhiteSpace(vname) || _type is null) return null;

            var vn = vname.Replace("@", string.Empty);
            foreach (var p in _type.GetProperties())
            {
                if (p.Name.Equals(vn, StringComparison.InvariantCultureIgnoreCase))
                {
                    return p.GetValue(_obj);
                }
            }

            return null;
        }

        private void ClearParams(SqlParameterCollection pars)
        {
            for (var i = 0; i < pars.Count; i++)
            {
                pars[i].Value = DBNull.Value;
            }
        }

        public void Map<T>(T? obj, SqlParameterCollection pars)
        {
            if (obj is null)
            {
                ClearParams(pars);
                return;
            }

            _obj = obj;
            _type = _obj.GetType();

            for (var i = 0; i < pars.Count; i++)
            {
                var par = pars[i];
                object? value = null;

                if (par.Direction is ParameterDirection.Input or ParameterDirection.InputOutput)
                {
                    var name = par.ParameterName;
                    value = MyGetPropertyValue(name);
                }

                par.Value = value ?? DBNull.Value;
            }
        }

        public void Map<T>(T? obj, SqlParameterCollection pars, Func<string, object, object>? doMap)
        {
            if (obj is null)
            {
                ClearParams(pars);
                return;
            }

            if (doMap is null)
            {
                Map(obj, pars);
                return;
            }

            _obj = obj;
            _type = _obj.GetType();

            for (var i = 0; i < pars.Count; i++)
            {
                var par = pars[i];
                object? value = null;

                if (par.Direction is ParameterDirection.Input or ParameterDirection.InputOutput)
                {
                    var name = par.ParameterName;
                    value = doMap(name, _obj);
                }

                par.Value = value ?? DBNull.Value;
            }
        }
    }
}
