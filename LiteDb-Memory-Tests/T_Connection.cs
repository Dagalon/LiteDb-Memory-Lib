using LiteDb_Memory_Lib;

namespace LiteDb_Memory_Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void T_Create_Data_Base()
    {
        var alias_shared = "Test_Db_Shared";
        var alias = "Test_Db";
        
        var manager = ConnectionManager.Instance();
        
        // Create shared database 
        manager.CreateDatabase(alias_shared, isShared: true);
        
        // Create database
        manager.CreateDatabase(alias);
        
        manager.Close(alias_shared);
        manager.Close(alias);
    }
}