using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace SqliteDB_Memory_Lib
{
    public static partial class SqLiteLiteTools
    {
        public static string CreateDatabase(string? idDataBase, string path, bool walMode = false, object? dependency = null)
        {
            SqliteConnection connection = SqLiteSingleton.GetInstance(null).GetConnection();
            List<string>? listDataBase = GetListDataBase(connection);

            if (listDataBase != null && idDataBase != null && listDataBase.Contains(idDataBase))
            {
                return "sqliteCreateDB" + "-" + idDataBase;
            }

            if (string.IsNullOrEmpty(idDataBase))
            {

                if (string.IsNullOrEmpty(path))
                {
                    throw new Exception("Invalid name, id and path are empty or null.");
                }
                   
                idDataBase = Path.GetFileName(path)?.Split('.')[0];

            }
               
            if (KeeperRegisterIdDataBase.CheckPathDataBase(path))
            {
                string idRegistered = KeeperRegisterIdDataBase.GetIdDataBase(path);
                throw new Exception($"The path '{path}' has associated the data base '{idRegistered}'");
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (idDataBase != null) KeeperRegisterIdDataBase.register(path, idDataBase);
            }

            AttachedDataBase(connection, path, idDataBase, false);
           
            if (walMode)
            {
                ActivateWalMode(connection);
            }

            return "sqliteCreateDB" + "-" + idDataBase;
        }

        public static void CreateTable(SqliteConnection db, string idDataBase, string idTable, List<string> headers, object[,]? values)
        {
            if (string.IsNullOrEmpty(idDataBase))
            {
                idDataBase = "main";
            }

            DropTable(db,idDataBase,idTable);
            QueryExecutor.CreateTable(db, idDataBase, idTable, headers);
            if (values != null)
            {
                QueryExecutor.Insert(db, idDataBase, idTable, headers, values, "");
            }
        }

        public static void CreateTable(SqliteConnection db, string idDataBase, string idTable, string pathCsvValues)
        {
            if (string.IsNullOrEmpty(idDataBase))
            {
                idDataBase = "main";
            }

            var reader = new StreamReader(Path.GetFullPath(pathCsvValues));
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";"
            };
            var csv = new CsvReader(reader, config);
           
            csv.Read();
            csv.ReadHeader();
            if (csv.HeaderRecord != null)
            {
                var fields = csv.HeaderRecord.ToList();
                csv.Read();
           
                var values = new List<List<object>>();
                var firstRow = new List<object>();
           
                foreach (string field in fields)
                {  
                    var fieldValue = NetTypeToSqLiteType.StrTryParse(csv.GetField(field));
                    firstRow.Add(fieldValue.Item1);
                }

                values.Add(firstRow);
           
                while (csv.Read())
                {
                    var row = fields.Select(field => NetTypeToSqLiteType.StrTryParse(csv.GetField(field))).Select(fieldValue => fieldValue.Item1).ToList();

                    values.Add(row);
                }
           
                reader.Close();
         
           
                var noRows = values.Count;
                var noColumns = values[0].Count;
                var arrayValues = new object[noRows, noColumns];
                for (var i = 0; i < noRows; i++)
                {
                    for (var j = 0; j < noColumns; j++)
                    {
                        arrayValues[i, j] = values[i][j];
                    }
                }

                DropTable(db, idDataBase, idTable);
                CreateTable(db, idDataBase, idTable, fields.Select((field, i)=>field.ToDbString("[","]")).ToList(), arrayValues);
            }
        }

        public static void Insert(SqliteConnection db, string idDataBase, string idTable, string pathCsvValues, string extraEnd)
        {
            if (File.Exists(Path.GetFullPath(pathCsvValues)))
            {
                var reader = new StreamReader(Path.GetFullPath(pathCsvValues));
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";"
                };
                var csv = new CsvReader(reader, config);

                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord != null)
                {
                    var fieldsToInsert = csv.HeaderRecord.ToList();
               
                    var values = new List<List<object>>();
               
                    while (csv.Read())
                    {
                        var row = fieldsToInsert.Select(field => NetTypeToSqLiteType.StrTryParse(csv.GetField(field))).Select(fieldValue => fieldValue.Item1).ToList();

                        values.Add(row);
                    }
                    reader.Close();
               
                    int noRows = values.Count;
                    int noColumns = values[0].Count;
                    object[,] arrayValues = new object[noRows, noColumns];
                    for (int i = 0; i < noRows; i++)
                    {
                        for (int j = 0; j < noColumns; j++)
                        {
                            arrayValues[i, j] = values[i][j];
                        }
                    }

                    Insert(db, idDataBase, idTable, fieldsToInsert, arrayValues, extraEnd);
                }
            }
            else
            {
                throw new FileNotFoundException("File not found -" + pathCsvValues);
            }
        }
       
        public static void Insert(SqliteConnection db, string idDataBase, string idTable, List<String> fields,
            object[,] values, string extraEnd)
        {

            if (string.IsNullOrEmpty(idDataBase))
            {
                idDataBase = "main";
            }

            QueryExecutor.Insert(db, idDataBase, idTable, fields, values, extraEnd);
        }

        public static List<Dictionary<string, object>> Select(SqliteConnection db, string qry)
        {
            return QueryExecutor.ExecuteQryReader(db, qry);
        }

        public static SqliteConnection GetInstance(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new SqliteConnection("Data Source=:memory:;Cache=Shared");
            }

            return File.Exists(path)
                ? new SqliteConnection($"Data Source={path};Mode=Memory")
                : new SqliteConnection("Data Source=:memory:");
        }

        public static void AttachedDataBase(SqliteConnection db, string path, string? aliasDataBase, bool removeIfExist)
            {
               
                if (!string.IsNullOrEmpty(path)){
                    if (File.Exists(path))
                    {
                        if (removeIfExist)
                        {
                            File.Delete(path);
                        }

                    }
                       
                    var directory = Path.GetDirectoryName(path);
                    if (Directory.Exists(directory))
                    {
                        if (!File.Exists(path))
                        {
                            File.Create(path).Close();
                        }

                    }
                    else
                    {
                        if (directory != null) Directory.CreateDirectory(directory);
                        File.Create(path).Close();
                    }

                }

                var strConnection = path;
                string  attachedQry;
                if (String.IsNullOrEmpty(aliasDataBase))
                {
                    attachedQry = $"ATTACH '{strConnection}'";
                }
                {
                    attachedQry = $"ATTACH '{strConnection}' AS '{aliasDataBase}' ";
                }

                var cmd = new SqliteCommand(attachedQry, db);
                cmd.ExecuteNonQuery();
            }

        public static void ActivateWalMode(SqliteConnection db)
        {
            var cmd = new SqliteCommand("PRAGMA journal_mode = 'wal'", db);
            cmd.ExecuteNonQuery();
        }

        public static List<string>? GetListDataBase(SqliteConnection db)
            {

                var cmd = new SqliteCommand("PRAGMA database_list", db);
                var dataBases = cmd.ExecuteReader();
                List<string> idList = [];

                while (dataBases.Read())
                {
                    idList.Add(dataBases[1].ToString()!);
                }

                dataBases.Close();
                return idList;
            }

        public static List<string>? GetListTables(SqliteConnection db, string idDataBase)
        {
                var dataBases = GetListDataBase(db);

                if (dataBases != null && dataBases.Contains(idDataBase))
                {
                    List<string>? tables = [];
                    var qry = $"SELECT name FROM {idDataBase + "." + "sqlite_master"}   WHERE type = 'table'";
                    var cmd = new SqliteCommand(qry, db);
                    var qryReader = cmd.ExecuteReader();

                    while (qryReader.Read())
                    {
                        tables.Add(qryReader[0].ToString()!);
                    }

                    qryReader.Close();
                    if (tables.Count > 0)
                    {
                        return tables;
                    }

                    tables.Add($"There is not tables in {idDataBase}");
                    return tables;
                }

                throw new ArgumentException("The data base " + idDataBase +
                                            " is not attached to the current connection.");
            }

        public static void ExecuteQryNotReader(SqliteConnection db, string qryFilePath, Dictionary<string, string>? parameters)
            {
                if (File.Exists(Path.GetFullPath(qryFilePath)))
                {
                    var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
                    StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                    var qryToExecute = reader.ReadToEnd();
                   
                    reader.Close();
                    fs.Close();
                   
                    if (parameters == null)
                    {
                        QueryExecutor.ExecuteQryNotReader(db, qryToExecute);
                    }
                    else
                    {
                        QueryExecutor.ExecuteQryNotReader(db, qryToExecute, parameters);
                    }
                }
                else
                {
                    throw new FileNotFoundException("File not found -" + qryFilePath);

                }
            }

        public static string SustituteParameters(string qryFilePath, Dictionary<string, string> parameters)
        {
            if (File.Exists(Path.GetFullPath(qryFilePath)))
            {
                var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                var qry = reader.ReadToEnd();
               
                reader.Close();
                fs.Close();

                foreach (var param in parameters.Keys)
                {
                    qry = qry.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase);
                    // qry = Regex.Replace(qry, param, parameters[param], RegexOptions.IgnoreCase);
                }

                return qry;
            }
            else
            {
                throw new FileNotFoundException("File not found -" + qryFilePath);

            }
           
        }
        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qryFilePath, Dictionary<string, string> parameters)
            {
                if (File.Exists(Path.GetFullPath(qryFilePath)))
                {
                    var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
                    StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                    var qryToExecute = reader.ReadToEnd();
                   
                    reader.Close();
                    fs.Close();
                   
                    if (parameters==null)
                    {
                        return QueryExecutor.ExecuteQryReader(db, qryToExecute);
                    }
                    else
                    {
                        return QueryExecutor.ExecuteQryReader(db, qryToExecute, parameters);
                    }
                }
                else
                {
                    throw new FileNotFoundException("File not found -" + qryFilePath);

                }
            }

        public static void SaveDataBase(SqliteConnection db, string idDataBase, string idPathFile)
            {
                if (string.IsNullOrEmpty(idDataBase))
                {
                    idDataBase = "main";
                }

                using SqliteConnection destination = new SqliteConnection($"Data Source={idPathFile}; Version=3;");
                destination.Open();
                db.BackupDatabase(destination, "main", idDataBase);
                destination.Close();
            }

        public static object[] GetListParameters(string qryFilePath, string qry)
        {
            string qryToExecute = "";  
            if (string.IsNullOrEmpty(qry))
            {
                if (File.Exists(qryFilePath))
                {
                    var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
                    StreamReader reader = new StreamReader(fs, Encoding.UTF8);
                    qryToExecute = reader.ReadToEnd();
                   
                    reader.Close();
                    fs.Close();
                   
                }
                else
                {
                    throw new FileNotFoundException("File not found -" + qryFilePath);
                }

            }
            else
            {
                qryToExecute = qry;
            }

            if (qryToExecute.Contains('@'))
            {
                var matchExpression = MyRegex().Matches(qryToExecute);
                return matchExpression.OfType<Match>().ToList().Select(object (m) => m.Value).ToArray().Distinct().ToArray();
            }
           
            return [string.Empty];
           
        }

        public static string DropTable(SqliteConnection db, string idDataBase, string idTable)
        {
            if (string.IsNullOrEmpty(idDataBase))
            {
                idDataBase = "main";
            }

            var listTables = GetListTables(db, idDataBase);

            if (listTables != null && listTables.Contains(idTable))
            {
                string qry = $"DROP TABLE {idDataBase}.{idTable}";
                SqliteCommand cmd = new SqliteCommand(qry, db);
                cmd.ExecuteReader();
                return "";

            }

            return $"The data base {idDataBase} doesn't contain the table {idTable}";
        }

        public static void DeleteDataBase(SqliteConnection db, string idDatabase)
        {
            string qry = $"DETACH DATABASE {idDatabase}";
            SqliteCommand cmd = new SqliteCommand(qry, db);
            cmd.ExecuteReader();
        }

        [GeneratedRegex(@"@[A-za-z0-9]+")]
        private static partial Regex MyRegex();
    }

    
}

