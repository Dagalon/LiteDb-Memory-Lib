using System.Data;

namespace SqliteDB_Memory_Lib
{
    public static class NetTypeToSqLiteType
    {
        public static DbType GetDBType(Type type)
        {
            return type switch
            {
                _ when type == typeof(string) => DbType.String,
                _ when type == typeof(int) => DbType.Int32,
                _ when type == typeof(long) => DbType.Int64,
                _ when type == typeof(double) => DbType.Double,
                _ when type == typeof(float) => DbType.Single,
                _ when type == typeof(bool) => DbType.Boolean,
                _ when type == typeof(DateTime) => DbType.DateTime,
                _ when type == typeof(decimal) => DbType.Decimal,
                _ when type == typeof(byte[]) => DbType.Binary,
                _ => DbType.String // Valor por defecto
            };
        }

        public static (object, Type) StrTryParse(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return (null, typeof(string))!;

            if (int.TryParse(value, out int intValue))
                return (intValue, typeof(int));

            if (long.TryParse(value, out long longValue))
                return (longValue, typeof(long));

            if (double.TryParse(value, out double doubleValue))
                return (doubleValue, typeof(double));

            if (bool.TryParse(value, out bool boolValue))
                return (boolValue, typeof(bool));

            if (DateTime.TryParse(value, out DateTime dateValue))
                return (dateValue, typeof(DateTime));

            return (value, typeof(string));
        }
    }
}