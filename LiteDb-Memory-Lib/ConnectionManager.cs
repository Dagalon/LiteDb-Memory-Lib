using LiteDB;

namespace LiteDb_Memory_Lib;

public sealed class ConnectionManager
{
    #region Atrributes
    
    private static ConnectionManager? _instance;
    private static Dictionary<string, LiteDatabase> _databases;
    private static Dictionary<string, MemoryStream> _memoryFiles;
    
    #endregion
    
    #region Contructors

    private ConnectionManager()
    {
        _databases = new Dictionary<string, LiteDatabase>();
        _memoryFiles = new Dictionary<string, MemoryStream>();
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


    public void Close(string alias)
    {
        if (!_databases.TryGetValue(alias, out LiteDatabase? value)) return;
        value.Dispose();
        _databases.Remove(alias);
        _memoryFiles.Remove(alias);

    }

    public void CreateDatabase(string alias, string path=null, bool substituteIfExist = false, bool isShared = false)
    {

        if (string.IsNullOrWhiteSpace(path))
        {
            if (_databases.ContainsKey(alias))
            {
                throw new Exception($"There is a data base in memory with alias {alias}");
            } 
            
            _memoryFiles.Add(alias, new MemoryStream());
            _databases.Add(alias, new LiteDatabase(new MemoryStream()));
        }
        else
        {
            if (File.Exists(path))
            {
                var connectionString = isShared 
                    ? @$"filename={path};connection=shared"  
                    : @$"filename={path};connection=direct";
                
                if (_databases.ContainsKey(alias))
                {
                    if (substituteIfExist)
                    {
                        _databases.Remove(alias);
                        _databases.Add(alias, new LiteDatabase(new ConnectionString(connectionString)));
                    }
                    else
                    {
                        throw new Exception($"The database already {alias} exists.");
                    }
                }
                else
                {
                    _databases.Add(alias, new LiteDatabase(new ConnectionString(connectionString)));
                }
            }
            else
            {
                throw new Exception($"The path {path} does not exist.");
            }

        }
    }
    public static ConnectionManager Instance()
    {
        return _instance ??= new ConnectionManager();
    }

    
    #endregion
}