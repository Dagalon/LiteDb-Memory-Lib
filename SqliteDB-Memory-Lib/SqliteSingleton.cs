using Microsoft.Data.Sqlite;
using System.Data;


namespace SqliteDB_Memory_Lib
{
    public sealed class SqLiteSingleton
    {
        private static SqLiteSingleton? _instance;
        private static readonly SqliteConnection _connection = SqLiteLiteTools.GetInstance(null);
        
        private SqLiteSingleton()
        {
        }

        public static SqLiteSingleton GetInstance(string? path)
        {
            return _instance ??= new SqLiteSingleton();
        }

        public  SqliteConnection GetConnection()
        {
            if (_connection.State == ConnectionState.Open)
            {
                return _connection;
            }

            Open();
            return _connection;

        }

        private static void Open()
        {
            _connection.Open();
        }

        public static void Close()
        {
            _connection.Dispose();
            _connection.Close();
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
            if (_mapIdDataBase.ContainsKey(path))
            {
                return true;
            }

            return false;
            
        }
        
        public static bool CheckIdDataBase(string idDb)
        {
            if (_mapIdDataBase.ContainsValue(idDb))
            {
                return true;
            }

            return false;
            
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