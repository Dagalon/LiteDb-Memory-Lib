using LiteDB;
using System.Threading;

namespace LiteDb_Memory_Lib;

/// <summary>
/// Centralized manager responsible for orchestrating LiteDB in-memory connections.
/// </summary>
public sealed class ConnectionManager
{
    #region Attributes

    private static readonly Lazy<ConnectionManager> LazyInstance =
        new(() => new ConnectionManager(), LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly object _syncRoot = new();

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
    /// Retrieves a previously registered database by using its alias.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="createIfMissing">Indicates whether a new in-memory database should be created when the alias is missing.</param>
    /// <returns>An instance of <see cref="LiteDatabase"/> when the alias exists or is created.</returns>
    public LiteDatabase? GetDatabase(string alias, bool createIfMissing = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (_syncRoot)
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
    /// Creates a new database connection or replaces the existing one using the provided alias.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="path">Optional path to a LiteDB file. When omitted, an in-memory database is generated.</param>
    /// <param name="substituteIfExist">Indicates whether an already registered connection should be replaced.</param>
    /// <param name="isShared">When <c>true</c> the LiteDB file is opened in shared mode.</param>
    /// <returns>A value that reports whether the operation completed successfully.</returns>
    public EnumsLiteDbMemory.Output CreateDatabase(string alias, string? path = null, bool substituteIfExist = false,
        bool isShared = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (_syncRoot)
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
    /// Closes an existing database and optionally persists the in-memory contents to disk.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="pathToKeep">Optional path where the in-memory content will be saved.</param>
    /// <returns>A value that reports whether the closing operation succeeded.</returns>
    public EnumsLiteDbMemory.Output Close(string alias, string? pathToKeep = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        LiteDatabase? database;
        MemoryStream? memoryStream = null;

        lock (_syncRoot)
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

    /// <summary>
    /// Creates a new collection in the selected database and seeds it with optional documents.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collection">Name of the collection to create.</param>
    /// <param name="documents">Optional documents to insert into the collection.</param>
    /// <param name="useInsertBulk">When <c>true</c> the documents are inserted in bulk for better performance.</param>
    /// <returns>A value that reports whether the collection was created successfully.</returns>
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

    /// <summary>
    /// Creates a new collection and populates it with data from a JSON file.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collection">Name of the collection to create.</param>
    /// <param name="path">Path to a JSON file that contains the documents to insert.</param>
    /// <param name="useInsertBulk">When <c>true</c> the documents are inserted in bulk for better performance.</param>
    /// <returns>A value that reports whether the collection was created successfully.</returns>
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

    /// <summary>
    /// Retrieves a strongly typed collection from the managed database.
    /// </summary>
    /// <typeparam name="T">Type of the documents stored in the collection.</typeparam>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="collection">Name of the collection.</param>
    /// <returns>The requested collection or <c>null</c> when it does not exist.</returns>
    public ILiteCollection<T>? GetCollection<T>(string alias, string collection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);

        lock (_syncRoot)
        {
            return _databases.TryGetValue(alias, out var db) ? db.GetCollection<T>(collection) : null;
        }
    }

    /// <summary>
    /// Retrieves the names of all collections registered in the selected database.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <returns>A list with the available collection names.</returns>
    public List<string> GetCollectionNames(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);

        lock (_syncRoot)
        {
            return _databases.TryGetValue(alias, out var db)
                ? db.GetCollectionNames().ToList()
                : new List<string>();
        }
    }

    /// <summary>
    /// Provides access to the singleton instance of the connection manager.
    /// </summary>
    /// <returns>The shared <see cref="ConnectionManager"/> instance.</returns>
    public static ConnectionManager Instance()
    {
        return LazyInstance.Value;
    }

    /// <summary>
    /// Creates an in-memory database within a thread-safe context.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
    /// <param name="replaceExisting">Indicates whether an existing database should be replaced.</param>
    /// <returns>The created <see cref="LiteDatabase"/> instance.</returns>
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

    /// <summary>
    /// Releases the LiteDB connection and associated stream for the provided alias.
    /// </summary>
    /// <param name="alias">Alias of the database connection.</param>
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
