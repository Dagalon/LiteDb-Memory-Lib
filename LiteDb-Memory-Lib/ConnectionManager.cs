using LiteDB;

namespace LiteDb_Memory_Lib;

public sealed class ConnectionManager
{
    #region Atrributes
    private static ConnectionManager? _instance;
    private static Dictionary<string, LiteDatabase> _databases = new();
    #endregion
    
    #region Contructors

    private ConnectionManager()
    {
        _databases = new Dictionary<string, LiteDatabase>();
    }

    #endregion
    
    #region Methods

    public static LiteDatabase? GetDatabase(string alias, bool createIfNotExists = false)
    {
        if (_databases.TryGetValue(alias, out LiteDatabase? value))
        {
            return value;
        }

        if (!createIfNotExists) return null;
       
        _databases.Add(alias, new LiteDatabase(":memory:"));
        return _databases[alias];

    }

    #endregion
}