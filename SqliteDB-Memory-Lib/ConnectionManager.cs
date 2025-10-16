using Microsoft.Data.Sqlite;
using System.Data;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Centralized manager that controls the lifecycle of SQLite connections identified by an alias.
    /// </summary>
    public sealed class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> LazyInstance =
            new(() => new ConnectionManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly object _syncRoot = new();
        private readonly Dictionary<string, SqliteConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

        private ConnectionManager()
        {
        }

        /// <summary>
        /// Provides access to the singleton instance of the connection manager.
        /// </summary>
        /// <returns>The shared <see cref="ConnectionManager"/> instance.</returns>
        public static ConnectionManager GetInstance()
        {
            return LazyInstance.Value;
        }

        /// <summary>
        /// Retrieves an open SQLite connection identified by the provided alias, creating it when necessary.
        /// </summary>
        /// <param name="alias">Alias used to differentiate connections.</param>
        /// <param name="path">Optional file path to attach to the connection.</param>
        /// <returns>An open <see cref="SqliteConnection"/> ready to use.</returns>
        public SqliteConnection GetConnection(string? alias = null, string? path = null)
        {
            var normalizedAlias = NormalizeAlias(alias);

            lock (_syncRoot)
            {
                if (_connections.TryGetValue(normalizedAlias, out var existingConnection))
                {
                    EnsureOpen(existingConnection);
                    return existingConnection;
                }

                var newConnection = SqLiteLiteTools.GetInstance(path);
                EnsureOpen(newConnection);
                _connections[normalizedAlias] = newConnection;

                return newConnection;
            }
        }

        /// <summary>
        /// Closes and disposes the connection associated with the provided alias.
        /// </summary>
        /// <param name="alias">Alias of the connection to close.</param>
        public void CloseConnection(string? alias = null)
        {
            var normalizedAlias = NormalizeAlias(alias);

            lock (_syncRoot)
            {
                if (!_connections.TryGetValue(normalizedAlias, out var connection))
                {
                    return;
                }

                try
                {
                    connection.Close();
                }
                finally
                {
                    connection.Dispose();
                    _connections.Remove(normalizedAlias);
                }
            }
        }

        /// <summary>
        /// Closes and disposes all active SQLite connections managed by this instance.
        /// </summary>
        public void CloseAllConnections()
        {
            lock (_syncRoot)
            {
                foreach (var connection in _connections.Values)
                {
                    try
                    {
                        connection.Close();
                    }
                    finally
                    {
                        connection.Dispose();
                    }
                }

                _connections.Clear();
            }
        }

        /// <summary>
        /// Opens the provided SQLite connection when it is not already open.
        /// </summary>
        /// <param name="connection">Connection that must be open.</param>
        private static void EnsureOpen(SqliteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        /// <summary>
        /// Normalizes aliases to ensure consistent lookups inside the connection dictionary.
        /// </summary>
        /// <param name="alias">Alias provided by the caller.</param>
        /// <returns>A normalized alias value.</returns>
        private static string NormalizeAlias(string? alias)
        {
            return string.IsNullOrWhiteSpace(alias) ? "default" : alias.Trim();
        }

        /// <summary>
        /// Static helper that delegates to <see cref="CloseConnection(string?)"/>.
        /// </summary>
        /// <param name="alias">Alias of the connection to close.</param>
        public static void Close(string? alias = null)
        {
            GetInstance().CloseConnection(alias);
        }

        /// <summary>
        /// Static helper that disposes every managed connection.
        /// </summary>
        public static void CloseAll()
        {
            GetInstance().CloseAllConnections();
        }
    }

    /// <summary>
    /// Tracks how many times a specific function has been executed during runtime.
    /// </summary>
    public sealed class CounterExecutionFunctions
    {
        private static readonly Dictionary<string, int> _mapCounter = new();

        private CounterExecutionFunctions() { }

        /// <summary>
        /// Increments the execution counter for the supplied identifier and returns its new value.
        /// </summary>
        /// <param name="idFunction">Identifier of the tracked function.</param>
        /// <returns>The updated counter value.</returns>
        public static int GetCounter(string idFunction)
        {
            if (_mapCounter.ContainsKey(idFunction))
            {
                _mapCounter[idFunction] += 1;
                return _mapCounter[idFunction];
            }

            return _mapCounter[idFunction] = 1;
        }

        /// <summary>
        /// Retrieves the current counter for the supplied identifier without increasing it.
        /// </summary>
        /// <param name="idFunction">Identifier of the tracked function.</param>
        /// <returns>The stored counter value or zero when no data exists.</returns>
        public static int GetCurrentCounter(string idFunction)
        {
            if (_mapCounter.ContainsKey(idFunction))
            {
                return _mapCounter[idFunction];
            }

            return 0;
            
        }

    }
    
    /// <summary>
    /// Keeps track of database identifiers and their associated file paths.
    /// </summary>
    public sealed class KeeperRegisterIdDataBase
    {
        private static readonly Dictionary<string, string> _mapIdDataBase = new Dictionary<string, string>();

        private KeeperRegisterIdDataBase() { }

        /// <summary>
        /// Retrieves the database identifier registered for the provided file path.
        /// </summary>
        /// <param name="path">File system path of the database file.</param>
        /// <returns>The registered identifier or an empty string when not found.</returns>
        public static string GetIdDataBase(string path)
        {
            string idDataBase = "";
            if (_mapIdDataBase.ContainsKey(path))
            {
                idDataBase = _mapIdDataBase[path];
            }

            return idDataBase;
        }

        /// <summary>
        /// Verifies whether the supplied path is already registered.
        /// </summary>
        /// <param name="path">File system path of the database file.</param>
        /// <returns><c>true</c> when the path is registered; otherwise, <c>false</c>.</returns>
        public static bool CheckPathDataBase(string path)
        {
            return _mapIdDataBase.ContainsKey(path);
        }

        /// <summary>
        /// Verifies whether the supplied identifier is already associated with a path.
        /// </summary>
        /// <param name="idDb">Identifier of the database.</param>
        /// <returns><c>true</c> when the identifier exists; otherwise, <c>false</c>.</returns>
        public static bool CheckIdDataBase(string idDb)
        {
            return _mapIdDataBase.ContainsValue(idDb);
        }

        /// <summary>
        /// Registers the relationship between a database file path and its identifier.
        /// </summary>
        /// <param name="path">File system path of the database file.</param>
        /// <param name="idDataBase">Identifier assigned to the database.</param>
        public static void register(string path, string idDataBase)
        {
            if (!CheckPathDataBase(path))
            {
                _mapIdDataBase[path] = idDataBase;
            }
        }

        /// <summary>
        /// Removes the record for the supplied database identifier.
        /// </summary>
        /// <param name="idDataBase">Identifier of the database to forget.</param>
        public static void deleteRegister(string idDataBase)
        {
            var keepIdDataBases = _mapIdDataBase.Values.ToList();
            int indexValues = keepIdDataBases.IndexOf(idDataBase);
            string pathIdDataBase = _mapIdDataBase.Keys.ToList()[indexValues];

            _mapIdDataBase.Remove(pathIdDataBase);

        }

    }

}