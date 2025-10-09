using Microsoft.Data.Sqlite;
using System.Data;

namespace SqliteDB_Memory_Lib
{
    public sealed class ConnectionManager
    {
        private static readonly Lazy<ConnectionManager> LazyInstance =
            new(() => new ConnectionManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly object _syncRoot = new();
        private readonly Dictionary<string, SqliteConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

        private ConnectionManager()
        {
        }

        public static ConnectionManager GetInstance()
        {
            return LazyInstance.Value;
        }

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

        private static void EnsureOpen(SqliteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        private static string NormalizeAlias(string? alias)
        {
            return string.IsNullOrWhiteSpace(alias) ? "default" : alias.Trim();
        }

        public static void Close(string? alias = null)
        {
            GetInstance().CloseConnection(alias);
        }

        public static void CloseAll()
        {
            GetInstance().CloseAllConnections();
        }
    }

    public sealed class CounterExecutionFunctions
    {
        private static readonly Dictionary<string, int> _mapCounter = new();

        private CounterExecutionFunctions() { }


        public static int GetCounter(string idFunction)
        {
            if (_mapCounter.ContainsKey(idFunction))
            {
                _mapCounter[idFunction] += 1;
                return _mapCounter[idFunction];
            }

            return _mapCounter[idFunction] = 1;
        }

        public static int GetCurrentCounter(string idFunction)
        {
            if (_mapCounter.ContainsKey(idFunction))
            {
                return _mapCounter[idFunction];
            }

            return 0;
            
        }

    }
    
    public sealed class KeeperRegisterIdDataBase
    {
        private static readonly Dictionary<string, string> _mapIdDataBase = new Dictionary<string, string>();

        private KeeperRegisterIdDataBase() { }


        public static string GetIdDataBase(string path)
        {
            string idDataBase = "";
            if (_mapIdDataBase.ContainsKey(path))
            {
                idDataBase = _mapIdDataBase[path];
            }

            return idDataBase;
        }

        public static bool CheckPathDataBase(string path)
        {
            return _mapIdDataBase.ContainsKey(path);
        }
        
        public static bool CheckIdDataBase(string idDb)
        {
            return _mapIdDataBase.ContainsValue(idDb);
        }

        public static void register(string path, string idDataBase)
        {
            if (!CheckPathDataBase(path))
            {
                _mapIdDataBase[path] = idDataBase;
            }
        }

        public static void deleteRegister(string idDataBase)
        {
            var keepIdDataBases = _mapIdDataBase.Values.ToList();
            int indexValues = keepIdDataBases.IndexOf(idDataBase);
            string pathIdDataBase = _mapIdDataBase.Keys.ToList()[indexValues];

            _mapIdDataBase.Remove(pathIdDataBase);

        }

    }

}