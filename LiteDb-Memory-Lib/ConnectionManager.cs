using LiteDB;

namespace LiteDb_Memory_Lib;

public sealed class ConnectionManager
{
    #region Atrributes
    
    private static ConnectionManager? _instance;
    private static Dictionary<string, LiteDatabase> _databases = null!;
    private static Dictionary<string, MemoryStream> _memoryFiles = null!;
    
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

    public EnumsLiteDbMemory.Output Close(string alias, string? pathToKeep = null)
    {
        if (!_databases.TryGetValue(alias, out LiteDatabase? db)) 
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;

        if (!string.IsNullOrEmpty(pathToKeep))
        {
            _memoryFiles[alias].Position = 0;
            File.WriteAllBytes(pathToKeep, _memoryFiles[alias].ToArray());
        }

        db.Dispose();
        _databases.Remove(alias);
        _memoryFiles.Remove(alias);

        return EnumsLiteDbMemory.Output.SUCCESS;
    }

   

    public EnumsLiteDbMemory.Output CreateCollection<T>(string alias, string collection, List<T>? documents = null, 
        bool useInsertBulk=false)
    {
        if (!_databases.TryGetValue(alias, out LiteDatabase? db)) return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        
        if (useInsertBulk)
        {
            db.GetCollection<T>(collection).InsertBulk(documents);
        }
        else
        {
            db.GetCollection<T>(collection).Insert(documents);
        }
        
        db.Checkpoint();
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public ILiteCollection<T>? GetCollection<T>(string alias, string collection)
    {
        return _databases.TryGetValue(alias, out LiteDatabase? db) ? db.GetCollection<T>(collection) : null;
    }

    public List<string> GetCollectionNames(string alias)
    {
        return _databases.TryGetValue(alias, out LiteDatabase? db) ? db.GetCollectionNames().ToList() : [];
    }

    public EnumsLiteDbMemory.Output CreateCollection<T>(string alias, string collection, string path, bool useInsertBulk=false)
    {
        if (_databases.TryGetValue(alias, out LiteDatabase? db))
        {
            var documents = Tools.ReadJson<List<T>>(path);
            if (useInsertBulk)
            {
                db.GetCollection<T>(collection).InsertBulk(documents);
            }
            else
            {
                db.GetCollection<T>(collection).Insert(documents);
            }

            return EnumsLiteDbMemory.Output.SUCCESS;
        }

        return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
    }

    public EnumsLiteDbMemory.Output CreateDatabase(string alias, string? path=null, bool substituteIfExist = false, bool isShared = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (_databases.ContainsKey(alias))
            {
                return EnumsLiteDbMemory.Output.THERE_EXISTS_DATABASE;
            } 
            
            _memoryFiles.Add(alias, new MemoryStream());
            _databases.Add(alias, new LiteDatabase(_memoryFiles[alias]));
            
            return EnumsLiteDbMemory.Output.SUCCESS;
        }

        if (!File.Exists(path)) return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
        
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
                
        return EnumsLiteDbMemory.Output.SUCCESS;

    }
    public static ConnectionManager Instance()
    {
        return _instance ??= new ConnectionManager();
    }

    #endregion
}