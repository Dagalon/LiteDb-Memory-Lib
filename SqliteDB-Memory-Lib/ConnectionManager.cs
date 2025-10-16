using Microsoft.Data.Sqlite;
using System.Data;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Coordinates the lifecycle of SQLite in-memory connections and exposes helper utilities.
    /// </summary>
    public sealed class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> LazyInstance =
            new(() => new ConnectionManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly object _syncRoot = new();
        private readonly Dictionary<string, SqliteConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Prevents external instantiation to enforce the singleton pattern.
        /// </summary>
        private ConnectionManager()
        {
        }

        /// <summary>
        /// Retrieves the shared <see cref="ConnectionManager"/> instance.
        /// </summary>
        /// <returns>The singleton manager.</returns>
        public static ConnectionManager GetInstance()
        {
            return LazyInstance.Value;
        }

        /// <summary>
        /// Retrieves an existing connection identified by the supplied alias or creates a new one.
        /// </summary>
        /// <param name="alias">Alias used to register the connection.</param>
        /// <param name="path">Optional path to an on-disk SQLite database.</param>
        /// <returns>An open <see cref="SqliteConnection"/> ready for use.</returns>
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
        /// Closes and disposes the connection registered under the provided alias.
        /// </summary>
        /// <param name="alias">Alias used to register the connection.</param>
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
        /// Closes and disposes all managed connections.
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
        /// Opens the connection when it is not already available.
        /// </summary>
        /// <param name="connection">Connection to validate.</param>
        private static void EnsureOpen(SqliteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        /// <summary>
        /// Normalizes the connection alias, returning a default value when necessary.
        /// </summary>
        /// <param name="alias">Alias supplied by the caller.</param>
        /// <returns>The normalized alias.</returns>
        private static string NormalizeAlias(string? alias)
        {
            return string.IsNullOrWhiteSpace(alias) ? "default" : alias.Trim();
        }

        /// <summary>
        /// Convenience method that closes a single connection on the shared manager instance.
        /// </summary>
        /// <param name="alias">Alias used to register the connection.</param>
        public static void Close(string? alias = null)
        {
            GetInstance().CloseConnection(alias);
        }

        /// <summary>
        /// Convenience method that closes every connection on the shared manager instance.
        /// </summary>
        public static void CloseAll()
        {
            GetInstance().CloseAllConnections();
        }
    }

    /// <summary>
    /// Tracks the number of executions for functions identified by a string key.
    /// </summary>
    public sealed class CounterExecutionFunctions
    {
        private static readonly Dictionary<string, int> _mapCounter = new();

        private CounterExecutionFunctions() { }

        /// <summary>
        /// Increments and retrieves the number of times the specified function has been invoked.
        /// </summary>
        /// <param name="idFunction">Identifier of the function.</param>
        /// <returns>The updated execution count.</returns>
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
        /// Retrieves the current execution count for the specified function without incrementing it.
        /// </summary>
        /// <param name="idFunction">Identifier of the function.</param>
        /// <returns>The current execution count.</returns>
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
    /// Maintains a mapping between database paths and their generated identifiers.
    /// </summary>
    public sealed class KeeperRegisterIdDataBase
    {
        private static readonly Dictionary<string, string> _mapIdDataBase = new Dictionary<string, string>();

        private KeeperRegisterIdDataBase() { }

        /// <summary>
        /// Retrieves the identifier associated with the provided database path.
        /// </summary>
        /// <param name="path">Absolute or relative database path.</param>
        /// <returns>The stored identifier or an empty string when not registered.</returns>
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
        /// Checks whether the supplied path is already registered.
        /// </summary>
        /// <param name="path">Absolute or relative database path.</param>
        /// <returns><c>true</c> when the path is registered; otherwise, <c>false</c>.</returns>
        public static bool CheckPathDataBase(string path)
        {
            return _mapIdDataBase.ContainsKey(path);
        }

        /// <summary>
        /// Checks whether the supplied identifier is already registered.
        /// </summary>
        /// <param name="idDb">Identifier of the database.</param>
        /// <returns><c>true</c> when the identifier is registered; otherwise, <c>false</c>.</returns>
        public static bool CheckIdDataBase(string idDb)
        {
            return _mapIdDataBase.ContainsValue(idDb);
        }

        /// <summary>
        /// Registers a new mapping between a path and its identifier.
        /// </summary>
        /// <param name="path">Absolute or relative database path.</param>
        /// <param name="idDataBase">Identifier that represents the database.</param>
        public static void register(string path, string idDataBase)
        {
            if (!CheckPathDataBase(path))
            {
                _mapIdDataBase[path] = idDataBase;
            }
        }

        /// <summary>
        /// Removes the mapping that contains the supplied identifier.
        /// </summary>
        /// <param name="idDataBase">Identifier used during registration.</param>
        public static void deleteRegister(string idDataBase)
        {
            var keepIdDataBases = _mapIdDataBase.Values.ToList();
            int indexValues = keepIdDataBases.IndexOf(idDataBase);
            string pathIdDataBase = _mapIdDataBase.Keys.ToList()[indexValues];

            _mapIdDataBase.Remove(pathIdDataBase);

        }

    }

}