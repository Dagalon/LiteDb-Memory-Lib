using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Microsoft.Data.Sqlite;

namespace SqliteDB_Memory_Lib;

/// <summary>
/// Helper methods that simplify creating and managing SQLite databases in memory.
/// </summary>
public static partial class SqLiteLiteTools
{
    /// <summary>
    /// Creates or attaches an SQLite database to the provided connection.
    /// </summary>
    /// <param name="connection">Connection where the database will be available.</param>
    /// <param name="idDataBase">Optional identifier used to reference the database.</param>
    /// <param name="path">Optional path to an SQLite file on disk.</param>
    /// <param name="walMode">Indicates whether write-ahead logging should be activated.</param>
    /// <param name="dependency">Reserved parameter kept for backward compatibility.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Creates a table inside the provided database and optionally seeds it with data.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Identifier of the database that will hold the table.</param>
    /// <param name="idTable">Name of the table to create.</param>
    /// <param name="headers">Column definitions used to build the table.</param>
    /// <param name="values">Optional matrix with the data to insert.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Creates a table and populates it with data imported from a CSV file.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Identifier of the database that will hold the table.</param>
    /// <param name="idTable">Name of the table to create.</param>
    /// <param name="pathCsvValues">Path to the CSV file used as the data source.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Inserts rows into the target table by using the contents of a CSV file.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Identifier of the database that holds the table.</param>
    /// <param name="idTable">Name of the destination table.</param>
    /// <param name="pathCsvValues">Path to the CSV file used as the data source.</param>
    /// <param name="extraEnd">Extra SQL appended to the generated INSERT statement.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Inserts rows into the target table by using the provided in-memory matrix.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Identifier of the database that holds the table.</param>
    /// <param name="idTable">Name of the destination table.</param>
    /// <param name="fields">Ordered list of column names.</param>
    /// <param name="values">Matrix that contains the values to insert.</param>
    /// <param name="extraEnd">Extra SQL appended to the generated INSERT statement.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Executes a raw SELECT query and returns the rows as dictionaries.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="qry">Query to execute.</param>
    /// <returns>List of rows represented as dictionaries.</returns>
    public static List<Dictionary<string, object>> Select(SqliteConnection db, string qry)
    {
        return QueryExecutor.ExecuteQryReader(db, qry);
    }

    /// <summary>
    /// Creates a new SQLite connection using the provided path or an in-memory data source.
    /// </summary>
    /// <param name="path">Optional path to an SQLite file on disk.</param>
    /// <returns>A configured <see cref="SqliteConnection"/> instance.</returns>
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
    /// Attaches an external database file to the provided connection, creating the file when required.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="path">Path of the database file to attach.</param>
    /// <param name="aliasDataBase">Alias assigned to the attached database.</param>
    /// <param name="removeIfExist">Indicates whether the existing file must be deleted before attaching.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Enables write-ahead logging on the supplied SQLite connection.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Retrieves the list of database aliases attached to the connection.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <returns>Collection with the database aliases or <c>null</c> when unavailable.</returns>
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
    /// Retrieves the list of tables contained in the specified database alias.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Alias of the database to inspect.</param>
    /// <returns>A tuple with the operation result and the list of tables when successful.</returns>
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
    /// Executes a SQL script stored on disk that does not return rows.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="qryFilePath">Path to the SQL file.</param>
    /// <param name="parameters">Optional map of parameters to substitute.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Loads a SQL script and replaces the specified placeholders with the provided values.
    /// </summary>
    /// <param name="qryFilePath">Path to the SQL file.</param>
    /// <param name="parameters">Map of values used to replace placeholders.</param>
    /// <returns>A tuple with the operation result and the substituted script.</returns>
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
    /// Executes a SQL script stored on disk and returns the resulting rows.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="qryFilePath">Path to the SQL file.</param>
    /// <param name="parameters">Optional map of parameters to substitute.</param>
    /// <returns>A tuple with the operation result and the resulting rows.</returns>
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
    /// Persists the contents of an attached database to a new file on disk.
    /// </summary>
    /// <param name="db">Source SQLite connection.</param>
    /// <param name="idDataBase">Alias of the database to export.</param>
    /// <param name="idPathFile">Destination file path.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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
    /// Identifies the parameter placeholders present in the provided SQL script.
    /// </summary>
    /// <param name="qryFilePath">Optional path to the SQL file.</param>
    /// <param name="qry">Optional inline SQL script.</param>
    /// <returns>A tuple with the operation result and the ordered parameters.</returns>
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
    /// Drops a table from the specified database alias when it exists.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDataBase">Alias of the database that contains the table.</param>
    /// <param name="idTable">Name of the table to drop.</param>
    /// <returns>A tuple with the operation result and an informational message.</returns>
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
    /// Detaches a database alias from the connection.
    /// </summary>
    /// <param name="db">Target SQLite connection.</param>
    /// <param name="idDatabase">Alias of the database to detach.</param>
    /// <returns>A value that reports whether the operation succeeded.</returns>
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