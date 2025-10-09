using SqliteDB_Memory_Lib;

namespace SqliteDb_Memory_Tests;

public class Connection
{
    [SetUp]
    public void Setup()
    {
        
    }
    
    [Test]
    public void T_Create_Data_Base()
    {
        const string idDataBase = "TEST_DB";                              
        const string idAttachedDatabase = "TEST_DB_ATTACHED";

        var manager = ConnectionManager.GetInstance();
        var conn = manager.GetConnection();
        
        var db = SqLiteLiteTools.CreateDatabase(conn, idDataBase, null);
        var attachedDb = SqLiteLiteTools.AttachedDataBase(conn, null, idAttachedDatabase);

        var listDataBases = SqLiteLiteTools.GetListDataBase(conn);
        
        Assert.Multiple(() =>
        {
            Assert.That(listDataBases != null && listDataBases.Contains(idDataBase));
            Assert.That(listDataBases != null && listDataBases.Contains(idAttachedDatabase));
        });
        
        var deletedChercked = SqLiteLiteTools.DeleteDataBase(conn, idAttachedDatabase);
        listDataBases = SqLiteLiteTools.GetListDataBase(conn);

        Assert.That(listDataBases != null && !listDataBases.Contains(idAttachedDatabase));
    }

    [Test]
    public void T_Create_Table()
    {
        const string idDataBase = "TEST_DB";
        
        var manager = ConnectionManager.GetInstance();
        var conn = manager.GetConnection();

        var idTable = "TABLE_PERSONAL_DATA";
        
        //  create a database
        var checkDataBase = SqLiteLiteTools.CreateDatabase(conn, idDataBase, null);
        Assert.That(checkDataBase == EnumsSqliteMemory.Output.SUCCESS);
        var listDataBases = SqLiteLiteTools.GetListDataBase(conn);
        
        // data of the table
        var headers = new List<string> { "ID", "NAME", "FIRST_NAME", "AGE", "JOB" };
        var data = new object[,] { { 1, "Juan", "Garcia", 25, "Programmer" }, 
            { 2, "Pedro", "Moreno", 45, "Engineering" },
            {3, "Maria", "Lopez", 32, "Electricity"}
        };
        
        var checkTable = SqLiteLiteTools.CreateTable(conn, idDataBase, idTable, headers, data);
        Assert.That(checkTable == EnumsSqliteMemory.Output.SUCCESS);
        
        // execute query
        var qry = @"SELECT * FROM TABLE_PERSONAL_DATA";
        var results = SqLiteLiteTools.Select(conn, qry);
        Assert.That(checkTable == EnumsSqliteMemory.Output.SUCCESS);


    }


}