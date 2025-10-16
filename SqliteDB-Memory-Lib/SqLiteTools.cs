using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Microsoft.Data.Sqlite;

namespace SqliteDB_Memory_Lib;

/// <summary>
/// Provides helpers for creating and manipulating SQLite databases stored in memory or on disk.
/// </summary>
public static partial class SqLiteLiteTools
{
    /// <summary>
    /// Creates or attaches a database to the supplied connection and optionally enables WAL mode.
    /// </summary>
    /// <param name="connection">Connection that will host the database.</param>
    /// <param name="idDataBase">Identifier assigned to the database. When null, it is inferred from the file path.</param>
    /// <param name="path">Path to the database file or <c>null</c> for in-memory databases.</param>
    /// <param name="walMode">Indicates whether write-ahead logging should be enabled.</param>
    /// <param name="dependency">Optional dependency parameter kept for backward compatibility.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output CreateDatabase(SqliteConnection connection, string? idDataBase, string? path, bool walMode = false, object? dependency = null)
    {
        var listDataBase = GetListDataBase(connection);

        if (listDataBase != null && idDataBase != null && listDataBase.Contains(idDataBase))
        {
            return EnumsSqliteMemory.Output.SUCCESS;
        }

        if (string.IsNullOrEmpty(idDataBase))
        {

            if (string.IsNullOrEmpty(path))
            {
                return EnumsSqliteMemory.Output.PATH_AND_ID_IS_NULL_OR_EMPTY;
            }
                   
            idDataBase = Path.GetFileName(path).Split('.')[0];

        }
               
        if (path != null && KeeperRegisterIdDataBase.CheckPathDataBase(path))
        {
            return EnumsSqliteMemory.Output.THERE_EXISTS_DATABASE;
        }

        if (!string.IsNullOrEmpty(path))
        {
            KeeperRegisterIdDataBase.register(path, idDataBase);
        }

        AttachedDataBase(connection, path, idDataBase, false);
           
        if (walMode)
        {
            ActivateWalMode(connection);
        }

        return EnumsSqliteMemory.Output.SUCCESS;
    }

    /// <summary>
    /// Creates a table with the specified schema and optional seed data.
    /// </summary>
    /// <param name="db">Connection where the table will be created.</param>
    /// <param name="idDataBase">Identifier of the database that will host the table.</param>
    /// <param name="idTable">Name assigned to the new table.</param>
    /// <param name="headers">Collection of column names.</param>
    /// <param name="values">Optional matrix of values inserted after creation.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output CreateTable(SqliteConnection db, string idDataBase, string idTable, List<string> headers, object[,]? values)
    {
        if (string.IsNullOrEmpty(idDataBase))
        {
            idDataBase = "main";
        }

        try
        {
            DropTable(db, idDataBase, idTable);
            QueryExecutor.CreateTable(db, idDataBase, idTable, headers);
            if (values != null)
            {
                QueryExecutor.Insert(db, idDataBase, idTable, headers, values, "");
            }
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Creates a table and populates it with the contents of a CSV file.
    /// </summary>
    /// <param name="db">Connection where the table will be created.</param>
    /// <param name="idDataBase">Identifier of the database that will host the table.</param>
    /// <param name="idTable">Name assigned to the new table.</param>
    /// <param name="pathCsvValues">Path to the CSV file with the seed data.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output CreateTable(SqliteConnection db, string idDataBase, string idTable, string pathCsvValues)
    {
        if (string.IsNullOrEmpty(idDataBase))
        {
            idDataBase = "main";
        }

        if (string.IsNullOrEmpty(pathCsvValues))
        {
            return EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        if (!File.Exists(Path.GetFullPath(pathCsvValues)))
        {
            return EnumsSqliteMemory.Output.PATH_NOT_FOUND;
        }

        try
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
                var fields = csv.HeaderRecord.ToList();
                csv.Read();
           
                var values = new List<List<object>>();
                var firstRow = fields.Select(field => NetTypeToSqLiteType.StrTryParse(csv.GetField(field))).Select(fieldValue => fieldValue.Item1).ToList();

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
            
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Inserts data from a CSV file into the target table.
    /// </summary>
    /// <param name="db">Connection used to run the insert operations.</param>
    /// <param name="idDataBase">Identifier of the database that contains the table.</param>
    /// <param name="idTable">Name of the target table.</param>
    /// <param name="pathCsvValues">Path to the CSV file with the data to insert.</param>
    /// <param name="extraEnd">Optional SQL clause appended to the insert command.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output Insert(SqliteConnection db, string idDataBase, string idTable, string pathCsvValues, string extraEnd)
    {
        if (string.IsNullOrEmpty(pathCsvValues))
        {
            return EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        if (!File.Exists(Path.GetFullPath(pathCsvValues)))
        {
            return EnumsSqliteMemory.Output.PATH_NOT_FOUND;
        }

        try
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

                var insertResult = Insert(db, idDataBase, idTable, fieldsToInsert, arrayValues, extraEnd);
                return insertResult;
            }
            
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }
       
    /// <summary>
    /// Inserts the supplied values into the target table.
    /// </summary>
    /// <param name="db">Connection used to run the insert operations.</param>
    /// <param name="idDataBase">Identifier of the database that contains the table.</param>
    /// <param name="idTable">Name of the target table.</param>
    /// <param name="fields">Fields included in the insert statement.</param>
    /// <param name="values">Matrix of values that represent the rows to insert.</param>
    /// <param name="extraEnd">Optional SQL clause appended to the insert command.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output Insert(SqliteConnection db, string idDataBase, string idTable, List<String> fields,
        object[,] values, string extraEnd)
    {

        if (string.IsNullOrEmpty(idDataBase))
        {
            idDataBase = "main";
        }

        try
        {
            QueryExecutor.Insert(db, idDataBase, idTable, fields, values, extraEnd);
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Executes the provided SQL query and returns the results as dictionaries.
    /// </summary>
    /// <param name="db">Connection used to execute the query.</param>
    /// <param name="qry">SQL statement to run.</param>
    /// <returns>List of result rows represented as dictionaries.</returns>
    public static List<Dictionary<string, object>> Select(SqliteConnection db, string qry)
    {
        return QueryExecutor.ExecuteQryReader(db, qry);
    }

    /// <summary>
    /// Creates a new SQLite connection using in-memory or file-based storage depending on the provided path.
    /// </summary>
    /// <param name="path">Optional path to the SQLite database file.</param>
    /// <returns>The configured <see cref="SqliteConnection"/>.</returns>
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

    /// <summary>
    /// Attaches an external database file to the provided connection, creating the file if required.
    /// </summary>
    /// <param name="db">Connection that will host the attached database.</param>
    /// <param name="path">Path to the database file.</param>
    /// <param name="aliasDataBase">Alias assigned to the attached database.</param>
    /// <param name="removeIfExist">Indicates whether an existing file should be deleted before attachment.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output AttachedDataBase(SqliteConnection db, string? path, string? aliasDataBase, bool removeIfExist=false)
    {
        try
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
            else
            {
                attachedQry = $"ATTACH '{strConnection}' AS '{aliasDataBase}' ";
            }

            var cmd = new SqliteCommand(attachedQry, db);
            cmd.ExecuteNonQuery();
            
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Enables write-ahead logging (WAL) mode on the supplied connection.
    /// </summary>
    /// <param name="db">Connection that will be configured.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output ActivateWalMode(SqliteConnection db)
    {
        try
        {
            var cmd = new SqliteCommand("PRAGMA journal_mode = 'wal'", db);
            cmd.ExecuteNonQuery();
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Retrieves identifiers for all databases attached to the given connection.
    /// </summary>
    /// <param name="db">Connection inspected for attached databases.</param>
    /// <returns>List with the identifiers of the attached databases.</returns>
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

    /// <summary>
    /// Retrieves the list of tables stored inside the specified database.
    /// </summary>
    /// <param name="db">Connection that contains the database.</param>
    /// <param name="idDataBase">Identifier of the database whose tables will be listed.</param>
    /// <returns>Status of the operation and the table names when successful.</returns>
    public static (EnumsSqliteMemory.Output, List<string>?) GetListTables(SqliteConnection db, string idDataBase)
    {
        var dataBases = GetListDataBase(db);

        if (dataBases == null || !dataBases.Contains(idDataBase))
        {
            return (EnumsSqliteMemory.Output.DB_NOT_FOUND, null);
        }

        try
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
                return (EnumsSqliteMemory.Output.SUCCESS, tables);
            }

            tables.Add($"There is not tables in {idDataBase}");
            return (EnumsSqliteMemory.Output.SUCCESS, tables);
        }
        catch
        {
            return (EnumsSqliteMemory.Output.DB_NOT_FOUND, null);
        }
    }

    /// <summary>
    /// Executes a SQL command stored in a file without returning results.
    /// </summary>
    /// <param name="db">Connection used to execute the command.</param>
    /// <param name="qryFilePath">Path to the file that contains the SQL statement.</param>
    /// <param name="parameters">Optional dictionary with parameter replacements.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output ExecuteQryNotReader(SqliteConnection db, string qryFilePath, Dictionary<string, string>? parameters)
    {
        if (string.IsNullOrEmpty(qryFilePath))
        {
            return EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        if (!File.Exists(Path.GetFullPath(qryFilePath)))
        {
            return EnumsSqliteMemory.Output.PATH_NOT_FOUND;
        }

        try
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
            
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Substitutes parameter placeholders inside a SQL file with concrete values.
    /// </summary>
    /// <param name="qryFilePath">Path to the SQL template file.</param>
    /// <param name="parameters">Dictionary that maps placeholders to values.</param>
    /// <returns>Status of the operation and the resulting SQL string when successful.</returns>
    public static (EnumsSqliteMemory.Output, string?) SubstituteParameters(string qryFilePath, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(qryFilePath))
        {
            return (EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY, null);
        }

        if (!File.Exists(Path.GetFullPath(qryFilePath)))
        {
            return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
        }

        try
        {
            var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(fs, Encoding.UTF8);
            var qry = reader.ReadToEnd();
               
            reader.Close();
            fs.Close();

            var result = parameters.Keys.Aggregate(qry, (current, param) => current.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase));
            return (EnumsSqliteMemory.Output.SUCCESS, result);
        }
        catch
        {
            return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
        }
    }
    
    /// <summary>
    /// Executes a SQL query stored in a file and returns the results as dictionaries.
    /// </summary>
    /// <param name="db">Connection used to execute the query.</param>
    /// <param name="qryFilePath">Path to the SQL file that contains the query.</param>
    /// <param name="parameters">Optional dictionary with parameter replacements.</param>
    /// <returns>Status describing the outcome of the operation and the retrieved rows.</returns>
    public static (EnumsSqliteMemory.Output, List<Dictionary<string, object>>?) ExecuteQryReader(SqliteConnection db, string qryFilePath, Dictionary<string, string>? parameters)
    {
        if (string.IsNullOrEmpty(qryFilePath))
        {
            return (EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY, null);
        }

        if (!File.Exists(Path.GetFullPath(qryFilePath)))
        {
            return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
        }

        try
        {
            var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(fs, Encoding.UTF8);
            var qryToExecute = reader.ReadToEnd();
                   
            reader.Close();
            fs.Close();
                   
            var result = parameters is null ? QueryExecutor.ExecuteQryReader(db, qryToExecute) 
                : QueryExecutor.ExecuteQryReader(db, qryToExecute, parameters);
            
            return (EnumsSqliteMemory.Output.SUCCESS, result);
        }
        catch
        {
            return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
        }
    }

    /// <summary>
    /// Persists the specified database to a file on disk.
    /// </summary>
    /// <param name="db">Connection that holds the in-memory database.</param>
    /// <param name="idDataBase">Identifier of the database to back up.</param>
    /// <param name="idPathFile">Destination file path.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output SaveDataBase(SqliteConnection db, string idDataBase, string idPathFile)
    {
        if (string.IsNullOrEmpty(idDataBase))
        {
            idDataBase = "main";
        }

        if (string.IsNullOrEmpty(idPathFile))
        {
            return EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        try
        {
            using SqliteConnection destination = new SqliteConnection($"Data Source={idPathFile}; Version=3;");
            destination.Open();
            db.BackupDatabase(destination, "main", idDataBase);
            destination.Close();
            
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    /// <summary>
    /// Retrieves placeholder parameters defined in a SQL query or file.
    /// </summary>
    /// <param name="qryFilePath">Path to the SQL file to inspect.</param>
    /// <param name="qry">SQL string to inspect when not using a file.</param>
    /// <returns>Status describing the outcome and an array of placeholders.</returns>
    public static (EnumsSqliteMemory.Output, object[]?) GetListParameters(string qryFilePath, string qry)
    {
        var qryToExecute = "";  
        if (string.IsNullOrEmpty(qry))
        {
            if (string.IsNullOrEmpty(qryFilePath))
            {
                return (EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY, null);
            }

            if (!File.Exists(qryFilePath))
            {
                return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
            }

            try
            {
                var fs = new FileStream(Path.GetFullPath(qryFilePath), FileMode.Open , FileAccess.Read, FileShare.ReadWrite);
                var reader = new StreamReader(fs, Encoding.UTF8);
                qryToExecute = reader.ReadToEnd();
                   
                reader.Close();
                fs.Close();
            }
            catch
            {
                return (EnumsSqliteMemory.Output.PATH_NOT_FOUND, null);
            }
        }
        else
        {
            qryToExecute = qry;
        }

        if (!qryToExecute.Contains('@')) 
            return (EnumsSqliteMemory.Output.SUCCESS, new object[] { string.Empty });
            
        var matchExpression = MyRegex().Matches(qryToExecute);
        var result = matchExpression.OfType<Match>().ToList().Select(object (m) => m.Value).ToArray().Distinct().ToArray();
        
        return (EnumsSqliteMemory.Output.SUCCESS, result);
    }

    /// <summary>
    /// Drops a table from the specified database.
    /// </summary>
    /// <param name="db">Connection that hosts the database.</param>
    /// <param name="idDataBase">Identifier of the database that contains the table.</param>
    /// <param name="idTable">Name of the table to drop.</param>
    /// <returns>Status describing the outcome and an informational message.</returns>
    public static (EnumsSqliteMemory.Output, string?) DropTable(SqliteConnection db, string idDataBase, string idTable)
    {
        if (string.IsNullOrEmpty(idDataBase))
        {
            idDataBase = "main";
        }

        var tablesResult = GetListTables(db, idDataBase);
        if (tablesResult.Item1 != EnumsSqliteMemory.Output.SUCCESS)
        {
            return (tablesResult.Item1, $"Error accessing database {idDataBase}");
        }

        var listTables = tablesResult.Item2;
        if (listTables == null || !listTables.Contains(idTable))
            return (EnumsSqliteMemory.Output.COLLECTION_NOT_FOUND, $"The data base {idDataBase} doesn't contain the table {idTable}");

        try
        {
            var qry = $"DROP TABLE {idDataBase}.{idTable}";
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteReader();
            return (EnumsSqliteMemory.Output.SUCCESS, "");
        }
        catch
        {
            return (EnumsSqliteMemory.Output.DB_NOT_FOUND, "Error dropping table");
        }
    }

    /// <summary>
    /// Detaches an attached database from the connection.
    /// </summary>
    /// <param name="db">Connection that hosts the database.</param>
    /// <param name="idDatabase">Identifier of the database to detach.</param>
    /// <returns>Status describing the outcome of the operation.</returns>
    public static EnumsSqliteMemory.Output DeleteDataBase(SqliteConnection db, string idDatabase)
    {
        if (string.IsNullOrEmpty(idDatabase))
        {
            return EnumsSqliteMemory.Output.PATH_IS_NULL_OR_EMPTY;
        }

        try
        {
            var qry = $"DETACH DATABASE {idDatabase}";
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteReader();
            return EnumsSqliteMemory.Output.SUCCESS;
        }
        catch
        {
            return EnumsSqliteMemory.Output.DB_NOT_FOUND;
        }
    }

    [GeneratedRegex(@"@[A-za-z0-9]+")]
    private static partial Regex MyRegex();
}