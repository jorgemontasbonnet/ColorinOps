using System;
using System.Data;
using System.Reflection;

namespace Cbsdg.Data.Core
{
    public class DataRowMapper : IDataRowMapper
    {

        protected virtual object? GetDefaultValue(Type t)
        {
            if (t == typeof(string))
            {
                return string.Empty;
            }
            if (t == typeof(bool))
            {
                return false;
            }

            if (t == typeof(DateTime))
            {
                return DateTime.MinValue;
            }

            if (t == typeof(DateTime?))
            {
                return null;
            }

            if (t == typeof(decimal))
            {
                return 0M;
            }

            if (t == typeof(int) || t == typeof(byte) || t == typeof(ushort))
            {
                return 0;
            }

            if (t == typeof(char))
            {
                return (char)0;
            }

            if (t == typeof(Guid))
            {
                return Guid.Empty;
            }

            if (t == typeof(double))
            {
                return 0d;
            }

            if (t == typeof(float))
            {
                return 0f;
            }

            if (t == typeof(long))
            {
                return 0;
            }

            return null;
        }
        protected virtual object? GetValueFromRow(string name, PropertyInfo info, IDataRecord row)
        {
            var stype = info.GetType();
            return row[name] == DBNull.Value ? GetDefaultValue(stype) : row[name];
        }
        public virtual T Map<T>(T obj, IDataRecord row)
        {
            if (obj != null)
            {
                for (var i = 0; i < row.FieldCount; i++)
                {
                    var name = row.GetName(i);
                    //var info = obj.GetType().GetProperty(name);
                    var info = obj.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                    if (info != null)
                    {
                        var value = (row[name] != DBNull.Value) ? row[name] : GetDefaultValue(info.GetType());

                        if (value != null)
                        {
                            info.SetValue(obj, value);
                        }
                    }
                }
            }
            return obj;
        }

        public virtual T Map<T>(T obj, IDataRecord row, Func<string, IDataRecord, object?>? doMap)
        {
            if (obj != null)
            {
                if (doMap is null)
                {
                    return Map(obj, row);
                }

                for (var i = 0; i < row.FieldCount; i++)
                {
                    var name = row.GetName(i);
                    var info = obj.GetType().GetProperty(name);
                    if (info != null)
                    {
                        var value = doMap(name, row);

                        if (value != null)
                        {
                            info.SetValue(obj, value);
                        }
                    }
                }
            }
            return obj;
        }
    }
}
