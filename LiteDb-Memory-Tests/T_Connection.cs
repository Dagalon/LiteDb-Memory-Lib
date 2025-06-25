using LiteDb_Memory_Lib;
using LiteDB;

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
    
    [Test]
    public void T_Create_And_Remove_Data_Base()
    {
        var aliasDb = "Test_Db";
        var manager = ConnectionManager.Instance();
        
        // Create shared database 
        manager.CreateDatabase(aliasDb, isShared: true);

        // Create BsonDocument
        var customer = new BsonDocument
        {
            ["_id"] = ObjectId.NewObjectId(),
            ["Name"] = "David Garcia Lorite",
            ["role"] = "Developer"
        };
        
        // Create collection
        manager.CreateCollection(aliasDb, "personal_data",[customer]);
        var collection = manager.GetCollection<BsonDocument>(aliasDb,"personal_data");
       
        // Write to disk
        var folderPath = "D:\\GitHubRepository\\C#\\LiteDb-Memory-Lib\\LiteDb-Memory-Tests\\Data";
        var pathToKeep = Path.Combine(folderPath, "Test_Db_Shared.txt");
        manager.Close(aliasDb, pathToKeep);
        
        // Load again the database
        manager.CreateDatabase(aliasDb, pathToKeep);
    }
}