using LiteDb_Memory_Lib;
using LiteDB;

namespace LiteDb_Memory_Tests;


public class T_Insert_Document
{
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void T_Insert_One_Document()
    {
        var customer1 = new BsonDocument
        {
            ["_id"] = ObjectId.NewObjectId(),
            ["Name"] = "David Garcia Lorite",
            ["CreateDate"] = DateTime.Now,
            ["Phones"] = new BsonArray { "658576981" },
            ["IsActive"] = true,
            ["IsAdmin"] = new BsonValue(true),
            ["Address"] = new BsonDocument
            {
                ["Street"] = "Calle Solidaridad",
                ["City"] = "Madrid",
                ["State"] = "Spain",
                ["ZipCode"] = "28942"
            }
        };

        // Instance manager connections
        const string aliasDb = "Address";
        const string collection = "Customers";
        
        var manager = ConnectionManager.Instance();
        
        // Create shared database f
        manager.CreateDatabase(aliasDb);
        
        // Create collection
        manager.CreateCollection<BsonDocument>(aliasDb,collection, [customer1]);
        
        // Get collection names
        var collectionNames = manager.GetCollectionNames(aliasDb);
        Assert.That(collectionNames[0], Is.EqualTo(collection));
    }

}