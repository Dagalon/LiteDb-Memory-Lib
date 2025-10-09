using LiteDB;

namespace MemoryDb_Lib;

/// <summary>
/// Centralised manager responsible for orchestrating LiteDB in-memory connections.
/// </summary>
public sealed class ConnectionManager
{
    #region Attributes

    private static ConnectionManager? _instance;

    private static readonly object SyncRoot = new();

    private readonly Dictionary<string, LiteDatabase> _databases;

    private readonly Dictionary<string, MemoryStream> _memoryFiles;

    #endregion

    #region Constructors

    private ConnectionManager()
    {
        _databases = new Dictionary<string, LiteDatabase>(StringComparer.OrdinalIgnoreCase);
        _memoryFiles = new Dictionary<string, MemoryStream>(StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves a previously registered database using its alias.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="createIfMissing">Indicates whether a new in-memory database should be created when the alias is unknown.</param>
    /// <returns>An instance of <see cref="LiteDatabase"/> when the alias exists or is created.</returns>
    public LiteDatabase? GetDatabase(string alias, bool createIfMissing = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (SyncRoot)
        {
            if (_databases.TryGetValue(alias, out var database))
            {
                return database;
            }

            if (!createIfMissing)
            {
                return null;
            }

            return CreateInMemoryDatabaseLocked(alias, replaceExisting: false);
        }
    }

    /// <summary>
    /// Creates or replaces a database using the provided alias.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="path">Optional path to a LiteDB file. When omitted an in-memory database is created.</param>
    /// <param name="substituteIfExist">Indicates whether an existing connection should be replaced.</param>
    /// <param name="isShared">When <c>true</c> the LiteDB file is opened in shared mode.</param>
    public EnumsLiteDbMemory.Output CreateDatabase(string alias, string? path = null, bool substituteIfExist = false,
        bool isShared = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (SyncRoot)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (!substituteIfExist && _databases.ContainsKey(alias))
                {
                    return EnumsLiteDbMemory.Output.THERE_EXISTS_DATABASE;
                }

                CreateInMemoryDatabaseLocked(alias, substituteIfExist);
                return EnumsLiteDbMemory.Output.SUCCESS;
            }

            if (!File.Exists(path))
            {
                return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
            }

            if (_databases.ContainsKey(alias))
            {
                if (!substituteIfExist)
                {
                    return EnumsLiteDbMemory.Output.THERE_EXISTS_DATABASE;
                }

                DisposeDatabaseLocked(alias);
            }

            var connectionString = isShared
                ? @$"filename={path};connection=shared"
                : @$"filename={path};connection=direct";

            _databases[alias] = new LiteDatabase(new ConnectionString(connectionString));
            if (_memoryFiles.Remove(alias, out var memoryStream))
            {
                memoryStream.Dispose();
            }

            return EnumsLiteDbMemory.Output.SUCCESS;
        }
    }

    /// <summary>
    /// Closes a registered database and optionally persists the in-memory content to disk.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="pathToKeep">Optional destination path where the in-memory content should be written.</param>
    public EnumsLiteDbMemory.Output Close(string alias, string? pathToKeep = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        LiteDatabase? database;
        MemoryStream? memoryStream = null;

        lock (SyncRoot)
        {
            if (!_databases.TryGetValue(alias, out database))
            {
                return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
            }

            _databases.Remove(alias);

            if (_memoryFiles.TryGetValue(alias, out memoryStream))
            {
                _memoryFiles.Remove(alias);
            }
        }

        try
        {
            if (!string.IsNullOrEmpty(pathToKeep) && memoryStream is not null)
            {
                memoryStream.Position = 0;

                var directory = Path.GetDirectoryName(pathToKeep);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var fileStream = File.Create(pathToKeep);
                memoryStream.CopyTo(fileStream);
            }

            return EnumsLiteDbMemory.Output.SUCCESS;
        }
        finally
        {
            memoryStream?.Dispose();
            database?.Dispose();
        }
    }

    public EnumsLiteDbMemory.Output CreateCollection<T>(string alias, string collection, List<T>? documents = null,
        bool useInsertBulk = false) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);

        var db = GetDatabase(alias, createIfMissing: false);
        if (db is null)
        {
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        }

        var dbCollection = db.GetCollection<T>(collection);

        if (documents is not null && documents.Count > 0)
        {
            if (useInsertBulk)
            {
                dbCollection.InsertBulk(documents);
            }
            else
            {
                dbCollection.Insert(documents);
            }
        }
        else
        {
            dbCollection.Insert(new T());
        }

        db.Checkpoint();
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public EnumsLiteDbMemory.Output CreateCollection<T>(string alias, string collection, string path,
        bool useInsertBulk = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var db = GetDatabase(alias, createIfMissing: false);
        if (db is null)
        {
            return EnumsLiteDbMemory.Output.DB_NOT_FOUND;
        }

        if (!Tools.TryReadJson<List<T>>(path, out var documents))
        {
            return EnumsLiteDbMemory.Output.PATH_NOT_FOUND;
        }

        if (documents is null || documents.Count == 0)
        {
            return EnumsLiteDbMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        var dbCollection = db.GetCollection<T>(collection);

        if (useInsertBulk)
        {
            dbCollection.InsertBulk(documents);
        }
        else
        {
            dbCollection.Insert(documents);
        }

        db.Checkpoint();
        return EnumsLiteDbMemory.Output.SUCCESS;
    }

    public ILiteCollection<T>? GetCollection<T>(string alias, string collection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);

        lock (SyncRoot)
        {
            return _databases.TryGetValue(alias, out var db) ? db.GetCollection<T>(collection) : null;
        }
    }

    public List<string> GetCollectionNames(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (SyncRoot)
        {
            return _databases.TryGetValue(alias, out var db)
                ? db.GetCollectionNames().ToList()
                : new List<string>();
        }
    }

    public static ConnectionManager Instance()
    {
        return _instance ??= new ConnectionManager();
    }

    private LiteDatabase CreateInMemoryDatabaseLocked(string alias, bool replaceExisting)
    {
        if (replaceExisting)
        {
            DisposeDatabaseLocked(alias);
        }
        else if (_databases.ContainsKey(alias))
        {
            return _databases[alias];
        }

        var memoryStream = new MemoryStream();

        try
        {
            var database = new LiteDatabase(memoryStream);
            _databases[alias] = database;
            _memoryFiles[alias] = memoryStream;
            return database;
        }
        catch
        {
            memoryStream.Dispose();
            throw;
        }
    }

    private void DisposeDatabaseLocked(string alias)
    {
        if (_databases.Remove(alias, out var existingDb))
        {
            existingDb.Dispose();
        }

        if (_memoryFiles.Remove(alias, out var existingStream))
        {
            existingStream.Dispose();
        }
    }

    #endregion
}
