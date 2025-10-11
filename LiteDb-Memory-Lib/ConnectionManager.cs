using LiteDB;
using System.Threading;

namespace LiteDb_Memory_Lib;

/// <summary>
/// Administrador centralizado encargado de orquestar las conexiones en memoria de LiteDB.
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
    /// Obtiene una base de datos registrada previamente utilizando su alias.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="createIfMissing">Indica si se debe crear una nueva base de datos en memoria cuando el alias no existe.</param>
    /// <returns>Una instancia de <see cref="LiteDatabase"/> cuando el alias existe o se crea.</returns>
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
    /// Crea una nueva conexión de base de datos o reemplaza la existente usando el alias proporcionado.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="path">Ruta opcional a un archivo de LiteDB. Si se omite, se genera una base en memoria.</param>
    /// <param name="substituteIfExist">Indica si se debe sustituir una conexión ya registrada.</param>
    /// <param name="isShared">Cuando es <c>true</c> el archivo LiteDB se abre en modo compartido.</param>
    /// <returns>Un valor que indica si la operación se realizó correctamente.</returns>
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
    /// Cierra una base de datos existente y, opcionalmente, persiste el contenido en memoria a disco.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="pathToKeep">Ruta opcional donde se guardará el contenido en memoria.</param>
    /// <returns>Un valor que indica si el cierre se realizó correctamente.</returns>
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
    /// Crea una nueva colección en la base seleccionada y la inicializa con documentos opcionales.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collection">Nombre de la colección a crear.</param>
    /// <param name="documents">Documentos opcionales que se insertarán en la colección.</param>
    /// <param name="useInsertBulk">Cuando es <c>true</c> los documentos se insertan en bloque para mejorar el rendimiento.</param>
    /// <returns>Un valor que indica si la colección se creó correctamente.</returns>
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
    /// Crea una nueva colección y la rellena con los datos de un archivo JSON.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collection">Nombre de la colección a crear.</param>
    /// <param name="path">Ruta a un archivo JSON que contiene los documentos a insertar.</param>
    /// <param name="useInsertBulk">Cuando es <c>true</c> los documentos se insertan en bloque para mejorar el rendimiento.</param>
    /// <returns>Un valor que indica si la colección se creó correctamente.</returns>
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
    /// Obtiene una colección tipada desde la base administrada.
    /// </summary>
    /// <typeparam name="T">Tipo de los documentos almacenados en la colección.</typeparam>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="collection">Nombre de la colección.</param>
    /// <returns>La colección solicitada o <c>null</c> si no existe.</returns>
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
    /// Obtiene los nombres de todas las colecciones registradas en la base seleccionada.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <returns>Una lista con los nombres disponibles.</returns>
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
    /// Proporciona acceso a la instancia única del administrador de conexiones.
    /// </summary>
    /// <returns>La instancia compartida de <see cref="ConnectionManager"/>.</returns>
    public static ConnectionManager Instance()
    {
        return LazyInstance.Value;
    }

    /// <summary>
    /// Genera una base de datos en memoria dentro de un contexto seguro para hilos.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
    /// <param name="replaceExisting">Indica si debe reemplazarse una base existente.</param>
    /// <returns>La instancia de <see cref="LiteDatabase"/> creada.</returns>
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
    /// Libera la conexión de LiteDB y el flujo asociado para el alias indicado.
    /// </summary>
    /// <param name="alias">Alias de la conexión de base de datos.</param>
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
